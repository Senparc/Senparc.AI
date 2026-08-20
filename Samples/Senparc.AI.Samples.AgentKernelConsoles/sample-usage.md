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
5. To use the A2A sample, configure `SenparcAiSetting.A2AAgents`.
6. The recommended default `ToolBindingMode` is `LocalFunctionProxy`, where the local proxy is exposed as an AIFunction and remains compatible with the current AgentKernel chat path.
7. If MCP uses `HostedServerTool`, `LocalSseUrl` usually needs `PublicBaseUrl` or the `MCP_PUBLIC_BASE_URL` environment variable for public URL mapping. The `/sse` path is preserved automatically.
8. If A2A uses `LocalBaseUrl`, configure `PublicBaseUrl` or the `A2A_PUBLIC_BASE_URL` environment variable for public URL mapping. The path is preserved automatically.
9. MCP standard logic is abstracted into `Senparc.AI.AgentKernel.Mcp` (`McpToolsetBuilder` / `McpConfigurationExtensions`), and A2A standard logic is abstracted into `Senparc.AI.AgentKernel.A2A` (`A2AToolsetBuilder` / `A2AConfigurationExtensions`).
10. Harness Agent reuses the current Chat model configuration. After entering menu item `[11]`, provide the context-window and per-response Token limits for the actual model specifications.

## Run

### Windows / macOS / Linux

```bash
dotnet run --project Samples/Senparc.AI.Samples.AgentKernelConsoles
```

Or run from the output directory:

```bash
dotnet Senparc.AI.Samples.AgentKernelConsoles.dll
```

## Harness Agent

Enter `11` at the main menu to start Harness. The exact API is `IChatClient.AsHarnessAgent(...)` from `Microsoft.Agents.AI.Harness`. The sample uses Harness `1.8.0-preview.260528.1`, matching AgentKernel MAF `1.8.0`.

Available commands: `/mode`, `/mode plan`, `/mode execute`, `/todos`, and `exit`.

The sample disables Harness file access, file memory, Skills scanning, and Hosted Web Search by default. It retains context compression, Todo, plan/execute modes, and human tool approval. To enable file capabilities, explicitly configure `FileAccessStore` and `FileMemoryStore` to point only to dedicated directories.

Official references: [MAF Harness Samples](https://github.com/microsoft/agent-framework/tree/main/dotnet/samples/02-agents/Harness) and [Harness NuGet](https://www.nuget.org/packages/Microsoft.Agents.AI.Harness/1.8.0-preview.260528.1).

## Differences From The Kernel Console Sample

- This sample references **Senparc.AI.AgentKernel** and is based on Microsoft Agent Framework.
- Conversation context is maintained through **AgentSession**.
- It currently covers Chat, Completion, Embedding, RAG, Image, STT, TTS, MCP (LocalFunctionProxy / HostedServerTool), A2A (LocalFunctionProxy), and Harness Agent.
- Capabilities not yet implemented in AgentKernel, such as Planner and Plugin, show **"Not available yet"**. Use `Senparc.AI.Samples.Consoles` for the Kernel version.
