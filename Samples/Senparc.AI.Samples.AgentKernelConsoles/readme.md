# Senparc.AI.Samples.AgentKernelConsoles

Console sample based on **Senparc.AI.AgentKernel** (Microsoft Agent Framework), with a menu structure aligned with `Senparc.AI.Samples.Consoles`.

## Implemented

| Menu | Description | Reference Test |
|------|------|----------|
| Chat | Multi-turn conversation with AgentSession; supports strict streaming, strict single-response, and compatibility entry points | `AgentAiHandlerTests` |
| Completion | Single TextCompletion | `RunTest` / `SingleLineTest` |
| Embedding [1] | Vector writes and similarity search | `EmbeddingStoreTest` |
| Embedding [2] | RAG + TextSearchProvider | `EmbeddingTest` |
| Image | Text-to-image generation (TextToImage) | `KernelConfigExtensionsImageTests` |
| STT | Speech to text (SpeechToText) | `KernelConfigExtensionsSpeechTests` |
| TTS | Text to speech (TextToSpeech) | `KernelConfigExtensionsSpeechTests` |
| MCP | LocalFunctionProxy / HostedServerTool (SSE) | `IWantToRunExtensionRunChatTests.EntityClassToolsTest` (tool-call mode reference) |
| A2A | LocalFunctionProxy (A2A Agent -> AIFunction) | `A2AToolsetBuilderTests` |
| Harness Agent | Context compression, Todo, plan/execute modes, and tool approval | [Microsoft Agent Framework Harness Samples](https://github.com/microsoft/agent-framework/tree/main/dotnet/samples/02-agents/Harness) |

## Not Yet Available

The following menu items align with the Consoles (Kernel) sample but are not yet implemented in AgentKernel. Running them displays **"Not available yet"**:

- Planner task planning
- PluginFromObject / Function Calling

## Configuration

1. Edit `appsettings.json`, or copy it to `appsettings.Development.json`, and configure `SenparcAiSetting`.
2. The vector database defaults to `Memory`. It can be changed to Redis, Qdrant, or another type supported by AgentKernel.
3. To use the MCP sample, configure `SenparcAiSetting.McpServers`.
4. To use the A2A sample, configure `SenparcAiSetting.A2AAgents`.
5. The recommended default `ToolBindingMode` is `LocalFunctionProxy`, where the local proxy is exposed as an AIFunction and remains compatible with the current AgentKernel Chat path.
6. When using MCP `HostedServerTool`, `LocalSseUrl` usually must be mapped to a public URL through `PublicBaseUrl` or the `MCP_PUBLIC_BASE_URL` environment variable because the model service cannot access `localhost` directly.
7. When A2A uses `LocalBaseUrl`, it can be mapped to a public URL through `PublicBaseUrl` or the `A2A_PUBLIC_BASE_URL` environment variable.
8. The MCP sample logic is abstracted into `Senparc.AI.AgentKernel.Mcp` (`McpToolsetBuilder` / `McpConfigurationExtensions`), and the A2A sample logic is abstracted into `Senparc.AI.AgentKernel.A2A` (`A2AToolsetBuilder` / `A2AConfigurationExtensions`). The sample retains only interaction and debug-output logic and reads the corresponding `SenparcAiSetting` configuration by default.
9. The Harness sample reuses the currently selected Chat configuration and requires no additional ApiKey. In its menu, enter the context-window and per-response Token limits for the actual model specifications.
10. The Chat sample uses strict streaming `RunChatStreamingAsync` by default. This entry point preserves IWantTo Prompt replacement, model-parameter sanitization, and the `HttpClient` transport chain while rethrowing upstream exceptions unchanged. It is suitable for server-side scenarios such as A2A and Workflows that must distinguish call failures from normal text responses. Select `RunChatAsync` from the Chat menu for legacy-compatible behavior.

## Harness Agent

Menu item `[11]` demonstrates `HarnessAgent` from `Microsoft.Agents.AI.Harness`. The exact extension method name is `AsHarnessAgent(...)`, not `AsHarness()`:

```csharp
IChatClient chatClient = /* Created from the current AgentKernel model configuration */;
AIAgent agent = chatClient.AsHarnessAgent(
    maxContextWindowTokens,
    maxOutputTokens,
    new HarnessAgentOptions { /* ... */ });
```

It adds a function-call loop, chat-history persistence across service calls, and context compression to a regular `IChatClient`. It can also provide Todo, plan/execute modes, file capabilities, Skills, Web Search, background Agents, and tool approval. The project currently uses MAF core version `1.8.0`, so the sample references the matching `Microsoft.Agents.AI.Harness` version `1.8.0-preview.260528.1` without upgrading the entire AgentKernel dependency graph.

The command line supports:

- `/mode`: view the current mode.
- `/mode plan`, `/mode execute`: switch to plan or execute mode.
- `/todos`: view the Todos managed by Harness.
- `exit`: exit Harness and return to the sample's main menu.

To keep the sample's default permissions minimal and maintain compatibility across model services, file access, file memory, Skills scanning, and Hosted Web Search are disabled by default. To enable file capabilities, explicitly restrict `FileAccessStore` and `FileMemoryStore` to dedicated directories and retain human approval. Do not use the repository or user directory directly as a writable root.

The interactive `HarnessConsole` in Microsoft's official examples is currently sample source code rather than a standalone NuGet package. This project therefore implements a lightweight command-line loop and handles the `ToolApprovalRequestContent` / `ToolApprovalResponseContent` approval round trip.

References:

- [Microsoft Agent Framework documentation](https://learn.microsoft.com/agent-framework/overview/agent-framework-overview)
- [Microsoft.Agents.AI.Harness NuGet package](https://www.nuget.org/packages/Microsoft.Agents.AI.Harness/1.8.0-preview.260528.1)
- [Official Microsoft Harness command-line samples](https://github.com/microsoft/agent-framework/tree/main/dotnet/samples/02-agents/Harness)

## Run

```bash
cd Samples/Senparc.AI.Samples.AgentKernelConsoles
dotnet run
```

After startup, enter `11` to open Harness Agent.
