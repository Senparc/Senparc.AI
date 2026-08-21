using Azure.AI.Projects;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Specialized.Magentic;
using Microsoft.Extensions.AI;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.AgentKernel.Tests.BaseSupport;
using Senparc.CO2NET.Extensions;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

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
                Description = "You are an Orchestrator responsible for coordinating different speakers and completing the task. A different person should speak in each round.",
                ChatOptions = new ChatOptions()
                {
                    Instructions = "You are an Orchestrator responsible for coordinating all conversations. After one agent finishes speaking, decide the next speaking agent and when to end the conversation. Note: next_speaker must be different from the previous speaker.",
                    MaxOutputTokens = 1000
                }
            }).BuildKernelWithAgentSessionAsync();


            var iWRTeacher = await aiHandlerTeacher.IWantTo().ConfigChatModel("Jeffrey-2", new ChatClientAgentOptions()
            {
                Name = "Teacher",
                Description = "You are a teacher responsible for creating questions as requested. Note: do not generate any solution steps.",
                ChatOptions = new ChatOptions()
                {
                    Instructions = "You are a teacher responsible for creating questions as requested. Note: do not generate any solution steps.",
                    MaxOutputTokens = 1000
                }
            }).BuildKernelWithAgentSessionAsync();

            var iWRStudentA = await aiHandlerStudentA.IWantTo().ConfigChatModel("Jeffrey-3", new ChatClientAgentOptions()
            {
                Name = "StudentA",
                Description = "You are a student responsible for answering the teacher's question. Keep the answer concise and return only the result.",
                ChatOptions = new ChatOptions()
                {
                    Instructions = "Answer the question in no more than 20 words.",
                    MaxOutputTokens = 100
                }
            }).BuildKernelWithAgentSessionAsync();

            var iWRStudentB = await aiHandlerStudentB.IWantTo().ConfigChatModel("Jeffrey-4", new ChatClientAgentOptions()
            {
                Name = "StudentB",
                Description = "You are a student responsible for answering the teacher's question. Answer in a poetic tone.",
                ChatOptions = new ChatOptions()
                {
                    Instructions = "You are a student responsible for answering the teacher's question. Answer in a poetic tone, then translate it into English.",
                    MaxOutputTokens = 200
                }
            }).BuildKernelWithAgentSessionAsync();



            //AIAgent stuA = (iWRStudentB.Kernel.ChatClient as ChatClient).AsAIAgent("You are a student responsible for answering questions concisely", "StudentA", "You are a student responsible for answering questions concisely");
            //AIAgent stuB = (iWRStudentB.Kernel.ChatClient as ChatClient).AsAIAgent("You are a student responsible for answering questions concisely, then translating the answer into English", "StudentB", "You are a student responsible for answering questions concisely, then translating the answer into English");

#pragma warning disable IDE0059 // Assignment is not required
#pragma warning disable MAAIW001 // Type is for evaluation only and may be changed or removed in a future update. Suppress this diagnostic to continue.
            Workflow workflow = new MagenticWorkflowBuilder(iWRManager.Kernel.ChatClientAgent)
                .AddParticipants([
                    //stuA,stuB
                        iWRStudentB.Kernel.ChatClientAgent,
                        iWRStudentA.Kernel.ChatClientAgent,
                        iWRTeacher.Kernel.ChatClientAgent])
                .WithName("Homework")
                .WithDescription("Organize members for conversation")
                .RequirePlanSignoff(false)
                .WithMaxRounds(10)
                .WithMaxStalls(3)
                .WithMaxResets(2)
                .Build();

            // Mermaid diagram
            Console.WriteLine("ToMermaidString: \r\n" + workflow.ToMermaidString());


            var taskPrompt = @"Solve the problem with the following process:
1. Ask the Teacher to create a chickens-and-rabbits math problem with random numbers. The teacher may only provide the question and must not analyze the answer.
2. Let all students answer in sequence: StudentA, then StudentB.";

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
                        //Console.WriteLine("ExecutorInvokedEvent:" + invokedEvent.Data?.ToJson());
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
            List<ChatMessage> list = new List<ChatMessage>();
            Assert.IsNotNull(iWRStudentA.Kernel.AgentSession);



            Console.WriteLine("ChatMessages: " + list.Select(z => $"[{z.Role.Value}]: {z.Text}").ToJson(true));

#pragma warning restore MAAIW001 // Type is for evaluation only and may be changed or removed in a future update. Suppress this diagnostic to continue.
#pragma warning restore IDE0059 // Assignment is not required

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
                Name = "Group Host",
                ChatOptions = new ChatOptions()
                {
                    Instructions = "You are the Group Host, responsible for coordinating all conversations, after one agent finishes speaking, then decide the next speaking agent, and decide when to end the conversation",
                    MaxOutputTokens = 1000
                }
            }).BuildKernelWithAgentSessionAsync();

            var iWRTeacher = await aiHandlerTeacher.IWantTo().ConfigChatModel("Thacher", new ChatClientAgentOptions()
            {
                Name = "Teacher",
                ChatOptions = new ChatOptions()
                {
                    Instructions = "You are a teacher responsible for creating questions. Do not analyze them.",
                    MaxOutputTokens = 1000
                }
            }).BuildKernelWithAgentSessionAsync();

            var iWRStudentA = await aiHandlerStudentA.IWantTo().ConfigChatModel("StudentA", new ChatClientAgentOptions()
            {
                Name = "StudentA",
                ChatOptions = new ChatOptions()
                {
                    Instructions = "You are a student responsible for answering the teacher's question. Keep the answer concise and return only the result.",
                    MaxOutputTokens = 1000
                }
            }).BuildKernelWithAgentSessionAsync();

            var iWRStudentB = await aiHandlerStudentB.IWantTo().ConfigChatModel("StudentB", new ChatClientAgentOptions()
            {
                Name = "StudentB",
                ChatOptions = new ChatOptions()
                {
                    Instructions = "You are a student responsible for answering the teacher's question. Answer in a poetic tone.",
                    MaxOutputTokens = 1000
                }
            }).BuildKernelWithAgentSessionAsync();

#pragma warning disable IDE0059 // Assignment is not required
#pragma warning disable MAAIW001 // Type is for evaluation only and may be changed or removed in a future update. Suppress this diagnostic to continue.
            Workflow workflow = AgentWorkflowBuilder.BuildSequential([
                        iWRTeacher.Kernel.ChatClientAgent,
                        iWRStudentA.Kernel.ChatClientAgent,
                        iWRStudentB.Kernel.ChatClientAgent,
                        ]);

            var taskPrompt = @"Solve the problem with the following process:
1. Ask the Teacher to create a chickens-and-rabbits math problem with random numbers.
2. Have all students answer separately";

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

#pragma warning restore MAAIW001 // Type is for evaluation only and may be changed or removed in a future update. Suppress this diagnostic to continue.
#pragma warning restore IDE0059 // Assignment is not required

        }
    }
}
