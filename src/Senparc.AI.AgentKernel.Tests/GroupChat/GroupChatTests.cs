using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Specialized.Magentic;
using Microsoft.Extensions.AI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.AgentKernel.Tests.BaseSupport;
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI.AgentKernel.Tests.GroupChat
{
    [TestClass]
    public class GroupChatTests : KernelTestBase
    {
        [TestMethod]
        public async Task GroupChatTest()
        {
            var aiHandlerManager = new AgentAiHandler(KernelTestBase._senparcAiSetting);
            var aiHandlerTeacher = new AgentAiHandler(KernelTestBase._senparcAiSetting);
            var aiHandlerStudentA = new AgentAiHandler(KernelTestBase._senparcAiSetting);
            var aiHandlerStudentB = new AgentAiHandler(KernelTestBase._senparcAiSetting);


            var iWRManager = await aiHandlerTeacher.IWantTo().ConfigChatModel("Manager", new ChatClientAgentOptions()
            {
                Name = "Teacher",
                ChatOptions = new ChatOptions()
                {
                    Instructions = "你是管理员，负责协调所有对话，并且决定终止对话",
                    MaxOutputTokens = 1000
                }
            }).BuildKernelWithAgentSessionAsync();

            var iWRTeacher = await aiHandlerTeacher.IWantTo().ConfigChatModel("Thacher", new ChatClientAgentOptions()
            {
                Name = "Teacher",
                ChatOptions = new ChatOptions()
                {
                    Instructions = "你是一位老师，负责出题，并且评估回答结果进行打分，0~100分",
                    MaxOutputTokens = 1000
                }
            }).BuildKernelWithAgentSessionAsync();

            var iWRStudentA= await aiHandlerTeacher.IWantTo().ConfigChatModel("StudentA", new ChatClientAgentOptions()
            {
                Name = "StudentA",
                ChatOptions = new ChatOptions()
                {
                    Instructions = "你是一位学生，负责回答老师的问题，使用英文回答问题。",
                    MaxOutputTokens = 1000
                }
            }).BuildKernelWithAgentSessionAsync();

            var iWRStudentB = await aiHandlerTeacher.IWantTo().ConfigChatModel("StudentB", new ChatClientAgentOptions()
            {
                Name = "StudentB",
                ChatOptions = new ChatOptions()
                {
                    Instructions = "你是一位学生，负责回答老师的问题，回答问题需要使用诗词的口吻",
                    MaxOutputTokens = 1000
                }
            }).BuildKernelWithAgentSessionAsync();



#pragma warning disable IDE0059 // 不需要赋值
#pragma warning disable MAAIW001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
            Workflow workflow = new MagenticWorkflowBuilder(iWRManager.Kernel.ChatClientAgent)
                .AddParticipants([iWRTeacher.Kernel.ChatClientAgent, iWRStudentA.Kernel.ChatClientAgent, iWRStudentB.Kernel.ChatClientAgent])
                .WithName("Home work")
                .WithDescription("老师负责出一个题，学生分别答题，最后老师评分。")
                .RequirePlanSignoff(false)
                .WithMaxRounds(10)
                .WithMaxStalls(3)
                .WithMaxResets(2)
                .Build();

            var taskPrompt = "老师出一个鸡兔同笼的数学题，让学生回答";

            await using StreamingRun run = await InProcessExecution.RunStreamingAsync(workflow, new List<ChatMessage> { new(ChatRole.User, taskPrompt) });

            await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

            string? lastResponseId = null;
            WorkflowOutputEvent? finalOutput = null;

            await foreach (WorkflowEvent workflowEvent in run.WatchStreamAsync())
            {
                switch (workflowEvent)
                {
                    case AgentResponseUpdateEvent updateEvent:
                        // Stream per-participant deltas. Group by ResponseId / MessageId / ExecutorId so
                        // each new contiguous response prints its executor header once.
                        string responseId = updateEvent.Update.ResponseId
                            ?? updateEvent.Update.MessageId
                            ?? updateEvent.ExecutorId;
                        if (!string.Equals(responseId, lastResponseId, StringComparison.Ordinal))
                        {
                            if (lastResponseId is not null)
                            {
                                Console.WriteLine();
                            }
                            Console.Write($"- {updateEvent.ExecutorId}: ");
                            lastResponseId = responseId;
                        }
                        Console.Write(updateEvent.Update.Text);
                        break;

                    case MagenticPlanCreatedEvent planCreated:
                        Console.WriteLine($"\n[Magentic Initial Plan]\n{planCreated.FullTaskLedger.Text}");
                        break;

                    case MagenticReplannedEvent replanned:
                        Console.WriteLine($"\n[Magentic Replanned]\n{replanned.FullTaskLedger.Text}");
                        break;

                    case MagenticProgressLedgerUpdatedEvent progressUpdated:
                        MagenticProgressLedger ledger = progressUpdated.ProgressLedger;
                        Console.WriteLine(
                            $"\n[Magentic Progress Ledger] satisfied={ledger.IsRequestSatisfied}, " +
                            $"inLoop={ledger.IsInLoop}, progressing={ledger.IsProgressBeingMade}, " +
                            $"nextSpeaker={ledger.NextSpeaker}, instruction={ledger.InstructionOrQuestion}");
                        break;

                    case WorkflowOutputEvent outputEvent when outputEvent.Is<List<ChatMessage>>():
                        finalOutput = outputEvent;
                        break;

                    case WorkflowErrorEvent workflowError:
                        Console.Error.WriteLine(workflowError.Exception?.ToString() ?? "Unknown workflow error.");
                        break;

                    case ExecutorFailedEvent executorFailed:
                        Console.Error.WriteLine(
                            $"Executor '{executorFailed.ExecutorId}' failed: " +
                            (executorFailed.Data?.ToString() ?? "unknown error"));
                        break;
                }
            }

            if (finalOutput?.As<List<ChatMessage>>() is { } transcript)
            {
                Console.WriteLine("\n\n=== Final Conversation Transcript ===\n");
                foreach (ChatMessage message in transcript)
                {
                    Console.WriteLine($"{message.AuthorName ?? message.Role.ToString()}: {message.Text}");
                }
            }


#pragma warning restore MAAIW001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
#pragma warning restore IDE0059 // 不需要赋值

        }
    }
}
