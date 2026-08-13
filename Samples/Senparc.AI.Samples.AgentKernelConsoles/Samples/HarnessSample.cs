/*----------------------------------------------------------------
    Copyright (C) 2026 Senparc

    文件名：HarnessSample.cs
    文件功能描述：演示 Microsoft Agent Framework HarnessAgent 的创建与命令行交互。


    创建标识：Senparc - 20260808

----------------------------------------------------------------*/
#pragma warning disable MAAI001 // Microsoft.Agents.AI.Harness 1.8 中的部分 API 仍标记为实验性。

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Senparc.AI.AgentKernel;
using Senparc.AI.AgentKernel.Handlers;
using Senparc.AI.Interfaces;
using Senparc.CO2NET.Extensions;
using OpenAIChatClient = OpenAI.Chat.ChatClient;

namespace Senparc.AI.Samples.AgentKernelConsoles.Samples;

/// <summary>
/// HarnessAgent 命令行示例。
/// </summary>
public class HarnessSample
{
    private const int DefaultMaxContextWindowTokens = 32_768;
    private const int DefaultMaxOutputTokens = 2_048;

    private readonly IAiHandler _aiHandler;

    public HarnessSample(IAiHandler aiHandler)
    {
        _aiHandler = aiHandler;
    }

    public async Task RunAsync()
    {
        var agentHandler = GetHandler(_aiHandler);
        agentHandler.AgentKernelHelper.ResetHttpClient(enableLog: SampleSetting.EnableHttpClientLog);

        Console.WriteLine("HarnessSample 开始运行（Microsoft Agent Framework HarnessAgent）");
        Console.WriteLine("准确扩展方法为 IChatClient.AsHarnessAgent(...)。本示例默认关闭文件、Skills 和 Hosted Web Search 权限。");
        Console.WriteLine();

        Console.WriteLine("请输入 Agent 专属指令，留空则使用默认：");
        const string defaultInstructions = "你是一名严谨的中文助手。复杂任务先规划，再逐项执行并清晰汇报结果。";
        Console.WriteLine($"默认：{defaultInstructions}");
        var instructions = Console.ReadLine();
        instructions = instructions.IsNullOrEmpty() ? defaultInstructions : instructions;

        var maxContextWindowTokens = ReadPositiveInt(
            $"模型上下文窗口 Token 上限（默认 {DefaultMaxContextWindowTokens}）：",
            DefaultMaxContextWindowTokens);
        var maxOutputTokens = ReadPositiveInt(
            $"模型单次输出 Token 上限（默认 {DefaultMaxOutputTokens}）：",
            DefaultMaxOutputTokens);

        if (maxOutputTokens >= maxContextWindowTokens)
        {
            Console.WriteLine("输出 Token 上限必须小于上下文窗口上限，已返回主菜单。");
            return;
        }

        var iWantToRun = agentHandler.IWantTo(SampleSetting.CurrentSetting)
            .ConfigModel(ConfigModel.Chat, "HarnessSample")
            .BuildKernel();

        var chatClient = iWantToRun.Kernel.ChatClient switch
        {
            OpenAIChatClient client => client.AsIChatClient(),
            IChatClient client => client,
            var client => throw new NotSupportedException(
                $"HarnessSample 暂不支持 ChatClient 类型：{client?.GetType().FullName ?? "null"}")
        };

        AIAgent agent = chatClient.AsHarnessAgent(
            maxContextWindowTokens,
            maxOutputTokens,
            new HarnessAgentOptions
            {
                Name = "SenparcAgentKernelHarness",
                Description = "Senparc.AI AgentKernel Harness command-line sample",
                ChatOptions = new ChatOptions
                {
                    Instructions = instructions
                },

                // Harness 1.8 默认会为以下能力创建本地目录或添加特定模型工具。
                // Sample 采用最小权限默认值；需要时请在受控目录中显式开启。
                DisableFileAccess = true,
                DisableFileMemory = true,
                DisableAgentSkillsProvider = true,
                DisableWebSearch = true
            });

        var session = await agent.CreateSessionAsync();
        var modeProvider = agent.GetService<AgentModeProvider>();
        var todoProvider = agent.GetService<TodoProvider>();

        Console.WriteLine();
        Console.WriteLine("Harness 已启动。可用命令：");
        Console.WriteLine("  /mode                 查看当前模式");
        Console.WriteLine("  /mode plan|execute    切换规划/执行模式");
        Console.WriteLine("  /todos                查看 Harness Todo 列表");
        Console.WriteLine("  exit                  返回主菜单");
        Console.WriteLine("工具执行若需要权限，命令行会逐项请求确认。");

        while (true)
        {
            Console.WriteLine();
            Console.Write("人类：");
            var input = Console.ReadLine();

            if (input is null || input.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            if (input.IsNullOrEmpty())
            {
                continue;
            }

            if (await TryHandleCommandAsync(input, session, modeProvider, todoProvider))
            {
                continue;
            }

            Console.Write("机器：");
            try
            {
                await RunAgentTurnAsync(agent, session, [new ChatMessage(ChatRole.User, input)]);
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine($"发生错误：{ex}");
            }
        }
    }

    private static AgentAiHandler GetHandler(IAiHandler handler)
    {
        return handler as AgentAiHandler
            ?? throw new InvalidOperationException("当前示例需要 AgentAiHandler，请确认已调用 AddSenparcAI。");
    }

    private static int ReadPositiveInt(string prompt, int defaultValue)
    {
        Console.Write(prompt);
        var input = Console.ReadLine();
        if (input.IsNullOrEmpty())
        {
            return defaultValue;
        }

        if (int.TryParse(input, out var value) && value > 0)
        {
            return value;
        }

        Console.WriteLine($"输入无效，使用默认值 {defaultValue}。");
        return defaultValue;
    }

    private static async Task<bool> TryHandleCommandAsync(
        string input,
        AgentSession session,
        AgentModeProvider? modeProvider,
        TodoProvider? todoProvider)
    {
        if (input.Equals("/todos", StringComparison.OrdinalIgnoreCase))
        {
            if (todoProvider is null)
            {
                Console.WriteLine("TodoProvider 不可用。");
                return true;
            }

            var todos = await todoProvider.GetAllTodosAsync(session);
            if (todos.Count == 0)
            {
                Console.WriteLine("当前没有 Todo。");
                return true;
            }

            Console.WriteLine("Todo 列表：");
            foreach (var item in todos)
            {
                Console.WriteLine($"  [{(item.IsComplete ? "x" : " ")}] #{item.Id} {item.Title} {item.Description}");
            }

            return true;
        }

        if (!input.Equals("/mode", StringComparison.OrdinalIgnoreCase)
            && !input.StartsWith("/mode ", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (modeProvider is null)
        {
            Console.WriteLine("AgentModeProvider 不可用。");
            return true;
        }

        var parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 1)
        {
            Console.WriteLine($"当前模式：{modeProvider.GetMode(session)}");
            return true;
        }

        try
        {
            modeProvider.SetMode(session, parts[1]);
            Console.WriteLine($"已切换到 {parts[1]} 模式。");
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine(ex.Message);
        }

        return true;
    }

    private static async Task RunAgentTurnAsync(
        AIAgent agent,
        AgentSession session,
        IList<ChatMessage> messages)
    {
        IList<ChatMessage>? nextMessages = messages;

        while (nextMessages is not null)
        {
            var approvalRequests = new List<ToolApprovalRequestContent>();

            await foreach (var update in agent.RunStreamingAsync(nextMessages, session))
            {
                if (!update.Text.IsNullOrEmpty())
                {
                    Console.Write(update.Text);
                }

                foreach (var content in update.Contents)
                {
                    if (content is ToolApprovalRequestContent request)
                    {
                        approvalRequests.Add(request);
                    }
                }
            }

            Console.WriteLine();

            if (approvalRequests.Count == 0)
            {
                nextMessages = null;
                continue;
            }

            var responses = new List<ChatMessage>(approvalRequests.Count);
            foreach (var request in approvalRequests)
            {
                var toolName = request.ToolCall is FunctionCallContent functionCall
                    ? functionCall.Name
                    : request.ToolCall.GetType().Name;

                Console.Write($"工具 {toolName} 请求执行，是否允许？[y/N] ");
                var approved = Console.ReadLine()?.Equals("y", StringComparison.OrdinalIgnoreCase) == true;
                var response = request.CreateResponse(
                    approved,
                    approved ? "用户通过命令行确认执行" : "用户拒绝执行");
                responses.Add(new ChatMessage(ChatRole.User, [response]));
            }

            nextMessages = responses;
        }
    }
}
