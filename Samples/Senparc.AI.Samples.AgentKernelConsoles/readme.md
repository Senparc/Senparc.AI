# Senparc.AI AgentKernel Console Sample

Console samples based on **Senparc.AI.AgentKernel** (Microsoft Agent Framework). The menu structure is aligned with `Senparc.AI.Samples.Consoles`.

## Implemented

| Menu | Description | Reference Tests |
| --- | --- | --- |
| Chat | Multi-turn chat + AgentSession | `AgentAiHandlerTests` |
| Completion | Single-turn TextCompletion | `RunTest` / `SingleLineTest` |
| Embedding [1] | Vector write + similarity search | `EmbeddingStoreTest` |
| Embedding [2] | RAG via TextSearchProvider | `KernelConfigExtensions.EmbeddingTests` |
| Image | Text-to-image | `KernelConfigExtensionsImageTests` |
| STT | Speech-to-text | `KernelConfigExtensionsSpeechTests` |
| TTS | Text-to-speech | `KernelConfigExtensionsSpeechTests` |
| MCP | LocalFunctionProxy / HostedServerTool (SSE) | `IWantToRunExtensionRunChatTests.EntityClassToolsTest` (tool-calling mode reference) |

## Not Yet Provided

The following menu items aligned with Consoles (Kernel) are not yet implemented in AgentKernel. Running them shows **"Not available yet"**:

- Planner task planning

## Configuration

1. Edit `appsettings.json` or copy it to `appsettings.Development.json` and fill in `SenparcAiSetting`.
2. The vector store defaults to `Memory`. It can be changed to Redis, Qdrant, or another type already supported by AgentKernel.
3. To use the MCP sample, configure `SenparcAiSetting.McpServers`.
4. The recommended default `ToolBindingMode` is `LocalFunctionProxy`, where the local proxy is exposed as an AIFunction and remains compatible with the current AgentKernel chat path.
5. When using `HostedServerTool`, `LocalSseUrl` usually needs `PublicBaseUrl` or the `MCP_PUBLIC_BASE_URL` environment variable to map it to a public URL. The model server cannot directly access `localhost`.
6. MCP sample logic has been abstracted into `Senparc.AI.AgentKernel.Mcp` (`McpToolsetBuilder` / `McpConfigurationExtensions`). The sample only keeps interaction and debug output logic, and reads `SenparcAiSetting.McpServers` by default.

## Run

```powershell
dotnet run --project Samples/Senparc.AI.Samples.AgentKernelConsoles
```
