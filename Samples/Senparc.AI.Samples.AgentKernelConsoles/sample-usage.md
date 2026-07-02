# Senparc.AI AgentKernel Console Sample Usage

Open the solution and build this console project, or run it directly:

```powershell
dotnet run --project Samples/Senparc.AI.Samples.AgentKernelConsoles
```

## Configuration

1. Edit `appsettings.json`. For local development, use `appsettings.Development.json` to override sensitive values.
2. Fill in the `ApiKey`, `Endpoint`, and model names for the matching platform under `SenparcAiSetting`.
3. For Embedding or RAG, configure `ModelName.Embedding`, `EmbeddingDimensions`, and `VectorDB`.
4. To use the MCP sample, configure `SenparcAiSetting.McpServers`.
5. The recommended default `ToolBindingMode` is `LocalFunctionProxy`, where the local proxy is exposed as an AIFunction and remains compatible with the current AgentKernel chat path.
6. If switching to `HostedServerTool`, `LocalSseUrl` usually needs `PublicBaseUrl` or the `MCP_PUBLIC_BASE_URL` environment variable for public URL mapping. The `/sse` path is preserved automatically.
7. MCP standard logic has been abstracted into `Senparc.AI.AgentKernel.Mcp` (`McpToolsetBuilder` / `McpConfigurationExtensions`) for direct reuse in business code. It reads `SenparcAiSetting.McpServers` by default.

## Run

### Windows / macOS / Linux

```bash
dotnet run --project Samples/Senparc.AI.Samples.AgentKernelConsoles
```

Or run from the output directory:

```bash
dotnet Senparc.AI.Samples.AgentKernelConsoles.dll
```

## Differences From The Kernel Console Sample

- This sample references **Senparc.AI.AgentKernel** and is based on Microsoft Agent Framework.
- Conversation context is maintained through **AgentSession**.
- It currently covers Chat, Completion, Embedding, RAG, Image, STT, TTS, and MCP (LocalFunctionProxy / HostedServerTool).
- Capabilities not yet implemented in AgentKernel, such as Planner and Plugin, show **"Not available yet"**. Use `Senparc.AI.Samples.Consoles` for the Kernel version.
