# Senparc.AI.Samples.AgentKernelConsoles

基于 **Senparc.AI.AgentKernel**（Microsoft Agent Framework）的控制台示例，菜单结构与 `Senparc.AI.Samples.Consoles` 对齐。

## 已实现

| 菜单 | 说明 | 参考测试 |
|------|------|----------|
| Chat | 多轮对话 + AgentSession；可选严格流式、严格单次响应与兼容入口 | `AgentAiHandlerTests` |
| Completion | 单次 TextCompletion | `RunTest` / `SingleLineTest` |
| Embedding [1] | 向量写入 + 相似检索 | `EmbeddingStoreTest` |
| Embedding [2] | RAG + TextSearchProvider | `EmbeddingTest` |
| Image | 文生图（TextToImage） | `KernelConfigExtensionsImageTests` |
| STT | 语音转文字（SpeechToText） | `KernelConfigExtensionsSpeechTests` |
| TTS | 文本转语音（TextToSpeech） | `KernelConfigExtensionsSpeechTests` |
| MCP | LocalFunctionProxy / HostedServerTool（SSE） | `IWantToRunExtensionRunChatTests.EntityClassToolsTest`（工具调用模式参考） |
| A2A | LocalFunctionProxy（A2A Agent -> AIFunction） | `A2AToolsetBuilderTests` |
| Harness Agent | 上下文压缩、Todo、plan/execute 模式、工具审批 | [Microsoft Agent Framework Harness Samples](https://github.com/microsoft/agent-framework/tree/main/dotnet/samples/02-agents/Harness) |

## 尚未提供

以下能力与 Consoles（Kernel）对齐的菜单项在 AgentKernel 中暂未实现，运行后会提示 **「尚未提供」**：

- Planner 任务计划
- PluginFromObject / Function Calling

## 配置

1. 编辑 `appsettings.json`（或复制为 `appsettings.Development.json`）填写 `SenparcAiSetting`
2. 向量库默认 `Memory`，可在配置中改为 Redis / Qdrant 等（需 AgentKernel 已支持的类型）
3. 如需 MCP 示例，请配置 `SenparcAiSetting.McpServers`
4. 如需 A2A 示例，请配置 `SenparcAiSetting.A2AAgents`
5. `ToolBindingMode` 默认建议使用 `LocalFunctionProxy`（本地代理为 AIFunction，兼容当前 AgentKernel Chat 路径）
6. 若使用 MCP 的 `HostedServerTool`，`LocalSseUrl` 通常需要配合 `PublicBaseUrl`（或环境变量 `MCP_PUBLIC_BASE_URL`）映射为公网 URL；模型服务端无法直接访问 `localhost`
7. A2A 若使用 `LocalBaseUrl`，可配合 `PublicBaseUrl`（或环境变量 `A2A_PUBLIC_BASE_URL`）映射为公网 URL
8. MCP 示例已抽象到 `Senparc.AI.AgentKernel.Mcp`（`McpToolsetBuilder` / `McpConfigurationExtensions`），A2A 示例已抽象到 `Senparc.AI.AgentKernel.A2A`（`A2AToolsetBuilder` / `A2AConfigurationExtensions`），Sample 仅保留交互与调试输出逻辑，默认读取 `SenparcAiSetting` 下对应配置
9. Harness 示例复用当前选择的 Chat 配置，不需要增加 ApiKey；进入菜单后需按模型实际规格填写上下文窗口和单次输出 Token 上限
10. Chat 示例默认使用严格流式 `RunChatStreamingAsync`；该入口保留 IWantTo 的 Prompt 替换、模型参数净化和 `HttpClient` 传输链路，并会让上游异常原样抛出。它适合 A2A、Workflow 等需要区分“调用失败”和“正常文本回复”的服务端场景；如需兼容旧行为，可在 Chat 菜单选择 `RunChatAsync`。

## Harness Agent

菜单 `[11]` 演示的是 `Microsoft.Agents.AI.Harness` 中的 `HarnessAgent`。准确的扩展方法名是 `AsHarnessAgent(...)`，不是 `AsHarness()`：

```csharp
IChatClient chatClient = /* 由 AgentKernel 当前模型配置创建 */;
AIAgent agent = chatClient.AsHarnessAgent(
    maxContextWindowTokens,
    maxOutputTokens,
    new HarnessAgentOptions { /* ... */ });
```

它在普通 `IChatClient` 上组合函数调用循环、逐次服务调用的聊天历史持久化和上下文压缩，并可提供 Todo、plan/execute 模式、文件能力、Skills、Web Search、后台 Agent、工具审批等能力。当前项目使用的 MAF 核心版本是 `1.8.0`，所以 Sample 引用与之匹配的 `Microsoft.Agents.AI.Harness` `1.8.0-preview.260528.1`，没有借此升级整个 AgentKernel 依赖图。

命令行支持：

- `/mode`：查看当前模式
- `/mode plan`、`/mode execute`：切换规划或执行模式
- `/todos`：查看 Harness 管理的 Todo
- `exit`：退出 Harness，返回 Sample 主菜单

为保证 Sample 的默认权限最小且兼容不同模型服务，文件访问、文件记忆、Skills 扫描和 Hosted Web Search 默认关闭。若要开启文件能力，请把 `FileAccessStore` / `FileMemoryStore` 显式限制到专用目录，并保留人工审批；不要直接把仓库或用户目录作为可写根目录。

微软官方示例中的响应式 `HarnessConsole` 当前是 Sample 源码而不是独立 NuGet 包，因此本项目实现了轻量命令行循环，并处理了 `ToolApprovalRequestContent` / `ToolApprovalResponseContent` 审批往返。

参考资料：

- [Microsoft Agent Framework 文档](https://learn.microsoft.com/agent-framework/overview/agent-framework-overview)
- [Microsoft.Agents.AI.Harness NuGet 包](https://www.nuget.org/packages/Microsoft.Agents.AI.Harness/1.8.0-preview.260528.1)
- [微软官方 Harness 命令行 Samples](https://github.com/microsoft/agent-framework/tree/main/dotnet/samples/02-agents/Harness)

## 运行

```bash
cd Samples/Senparc.AI.Samples.AgentKernelConsoles
dotnet run
```

启动后输入 `11` 进入 Harness Agent。
