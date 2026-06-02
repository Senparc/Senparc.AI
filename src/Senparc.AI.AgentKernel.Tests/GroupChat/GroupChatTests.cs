using Azure;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Specialized.Magentic;
using Microsoft.Extensions.AI;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.AgentKernel.Tests.BaseSupport;
using Senparc.CO2NET.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.AI.AgentKernel.Tests.GroupChat
{
    [TestClass]
    public class GroupChatTests : KernelTestBase
    {
        [TestMethod]
        public async Task GroupChat_MagenticTest()
        {
            var aiHandlerManager = new AgentAiHandler(KernelTestBase._senparcAiSetting);
            var aiHandlerTeacher = new AgentAiHandler(KernelTestBase._senparcAiSetting);
            var aiHandlerStudentA = new AgentAiHandler(KernelTestBase._senparcAiSetting);
            var aiHandlerStudentB = new AgentAiHandler(KernelTestBase._senparcAiSetting);

            var iWRManager = await aiHandlerManager.IWantTo().ConfigChatModel("Jeffrey-1", new ChatClientAgentOptions()
            {
                Name = "MagenticManager",
                Description= "你是一位 Orchestrator，负责协调不同人讲话，并完成任务，每一轮需要有不同的人来发言",
                ChatOptions = new ChatOptions()
                {
                    Instructions = "你是一位 Orchestrator，负责协调所有对话，在一个智能体发言完毕后，再决定下一个发言的智能体，并且决定何时终止对话。请注意：next_speaker 必须和上一个发言人不同！",
                    MaxOutputTokens = 1000
                }
            }).BuildKernelWithAgentSessionAsync();


            var iWRTeacher = await aiHandlerTeacher.IWantTo().ConfigChatModel("Jeffrey-2", new ChatClientAgentOptions()
            {
                Name = "Teacher",
                ChatOptions = new ChatOptions()
                {
                    Instructions = "你是一位老师，负责按照要求出题。注意：你不能生成任何解题思路。",
                    MaxOutputTokens = 1000
                }
            }).BuildKernelWithAgentSessionAsync();

            var iWRStudentA = await aiHandlerStudentA.IWantTo().ConfigChatModel("Jeffrey-3", new ChatClientAgentOptions()
            {
                Name = "StudentA",
                ChatOptions = new ChatOptions()
                {
                    Instructions = "你是一位学生，负责回答老师的问题，回答需要尽量简洁，只回答结果。",
                    MaxOutputTokens = 1000
                }
            }).BuildKernelWithAgentSessionAsync();

            var iWRStudentB = await aiHandlerStudentB.IWantTo().ConfigChatModel("Jeffrey-4", new ChatClientAgentOptions()
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
                .AddParticipants([
                        iWRStudentB.Kernel.ChatClientAgent,
                        iWRStudentA.Kernel.ChatClientAgent,
                        /*iWRTeacher.Kernel.ChatClientAgent*/])
                .WithName("Homework")
                .WithDescription("组织成员进行对话")
                .RequirePlanSignoff(false)
                .WithMaxRounds(10)
                .WithMaxStalls(3)
                .WithMaxResets(2)
                .Build();

            // Mermaid diagram
            Console.WriteLine("ToMermaidString: \r\n" + workflow.ToMermaidString());


            var taskPrompt = @"请两位 StudentA和StudentB分别回答：为什么1+1=2？言简意赅，轮流回答。";

            await using StreamingRun run = await InProcessExecution.RunStreamingAsync(workflow, new List<ChatMessage> { new(ChatRole.User, taskPrompt) });

            var trySend = await run.TrySendMessageAsync(new TurnToken(emitEvents: true));
            Console.WriteLine($"Try send: {trySend}");

            string? lastResponseId = null;
            WorkflowOutputEvent? finalOutput = null;

            long? totalTokenCount = 0;

            await foreach (WorkflowEvent workflowEvent in run.WatchStreamAsync())
            {
                switch (workflowEvent)
                {
                    case ExecutorInvokedEvent invokedEvent:
                        Console.WriteLine("ExecutorInvokedEvent:" + invokedEvent.Data?.ToJson());
                        break;
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
                            Console.Write($"- {updateEvent.ExecutorId} [{SystemTime.Now}] ");
                            lastResponseId = responseId;
                        }
                        Console.Write(updateEvent.Update.Text);

                        totalTokenCount += updateEvent.AsResponse()?.Usage?.TotalTokenCount;

                        break;

                    case MagenticPlanCreatedEvent planCreated:
                        Console.WriteLine($"\n[Magentic Initial Plan]\n{planCreated.FullTaskLedger.Text}");
                        Console.WriteLine($"\n===== END of Magentic Initial Plan ======");
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

            Console.WriteLine("========");
            Console.WriteLine("TotalTokenCount: " + totalTokenCount);

#pragma warning restore MAAIW001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
#pragma warning restore IDE0059 // 不需要赋值

        }

        [TestMethod]
        public async Task GroupChat_SequentialTest()
        {
            var aiHandlerManager = new AgentAiHandler(KernelTestBase._senparcAiSetting);
            var aiHandlerTeacher = new AgentAiHandler(KernelTestBase._senparcAiSetting);
            var aiHandlerStudentA = new AgentAiHandler(KernelTestBase._senparcAiSetting);
            var aiHandlerStudentB = new AgentAiHandler(KernelTestBase._senparcAiSetting);

            var setting = KernelTestBase._senparcAiSetting;

            var iWRManager = await aiHandlerManager.IWantTo().ConfigChatModel("Manager", new ChatClientAgentOptions()
            {
                Name = "群主",
                ChatOptions = new ChatOptions()
                {
                    Instructions = "你是群主，负责协调所有对话，在一个智能体发言完毕后，再决定下一个发言的智能体，并且决定何时终止对话",
                    MaxOutputTokens = 1000
                }
            }).BuildKernelWithAgentSessionAsync();

            var iWRTeacher = await aiHandlerTeacher.IWantTo().ConfigChatModel("Thacher", new ChatClientAgentOptions()
            {
                Name = "Teacher",
                ChatOptions = new ChatOptions()
                {
                    Instructions = "你是一位老师，负责出题，不用进行分析",
                    MaxOutputTokens = 1000
                }
            }).BuildKernelWithAgentSessionAsync();

            var iWRStudentA = await aiHandlerStudentA.IWantTo().ConfigChatModel("StudentA", new ChatClientAgentOptions()
            {
                Name = "StudentA",
                ChatOptions = new ChatOptions()
                {
                    Instructions = "你是一位学生，负责回答老师的问题，回答需要尽量简洁，只回答结果。",
                    MaxOutputTokens = 1000
                }
            }).BuildKernelWithAgentSessionAsync();

            var iWRStudentB = await aiHandlerStudentB.IWantTo().ConfigChatModel("StudentB", new ChatClientAgentOptions()
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
            Workflow workflow = AgentWorkflowBuilder.BuildSequential([
                        iWRTeacher.Kernel.ChatClientAgent,
                        iWRStudentA.Kernel.ChatClientAgent,
                        iWRStudentB.Kernel.ChatClientAgent,
                        ]);

            var taskPrompt = @"请按照如下过程解决问题：
1. 请 Teacher 老师出一个鸡兔同笼的数学题，数字可以随机定义
2. 让所有学生 Students 分别回答";

            await using StreamingRun run = await InProcessExecution.RunStreamingAsync(workflow, new List<ChatMessage> { new(ChatRole.User, taskPrompt) });

            await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

            string? lastResponseId = null;
            WorkflowOutputEvent? finalOutput = null;

            long? totalTokenCount = 0;

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
                            Console.Write($"- {updateEvent.ExecutorId} [{SystemTime.Now}] ");
                            lastResponseId = responseId;
                        }
                        Console.Write(updateEvent.Update.Text);

                        totalTokenCount += updateEvent.AsResponse()?.Usage?.TotalTokenCount;

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

            Console.WriteLine("========");
            Console.WriteLine("TotalTokenCount: " + totalTokenCount);

#pragma warning restore MAAIW001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
#pragma warning restore IDE0059 // 不需要赋值

        }
    }
}
