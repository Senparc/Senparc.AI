# Senparc.AI AgentKernel Console Sample Usage

Open the solution and build this console project, or run it directly:

```powershell
dotnet run --project Samples/Senparc.AI.Samples.AgentKernelConsoles
```

## Configuration

1. Edit `appsettings.json`. For local development, use `appsettings.Development.json` to override sensitive values.
2. Fill in the `ApiKey`, `Endpoint`, and model names for the matching platform under `SenparcAiSetting`.
3. For Embedding or RAG, configure `ModelName.Embedding`, `EmbeddingDimensions`, and `VectorDB`.

## Differences From The Kernel Console Sample

- This sample references **Senparc.AI.AgentKernel** and is based on Microsoft Agent Framework.
- Conversation context is maintained through **AgentSession**.
- It currently covers Chat, Completion, Embedding, RAG, Image, STT, and TTS.
- Capabilities not yet implemented in AgentKernel, such as Planner and Plugin, show **"Not available yet"**. Use `Senparc.AI.Samples.Consoles` for the Kernel version.
