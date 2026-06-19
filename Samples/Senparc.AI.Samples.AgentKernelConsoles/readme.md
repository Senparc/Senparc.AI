# Senparc.AI.Samples.AgentKernelConsoles

This console sample is based on **Senparc.AI.AgentKernel** and Microsoft Agent Framework. Its menu structure is aligned with `Senparc.AI.Samples.Consoles`.

## Implemented

| Menu | Description | Reference test |
| --- | --- | --- |
| Chat | Multi-turn conversation with AgentSession | `AgentAiHandlerTests` |
| Completion | Single-turn TextCompletion | `RunTest` / `SingleLineTest` |
| Embedding [1] | Vector write and similarity retrieval | `EmbeddingStoreTest` |
| Embedding [2] | RAG with TextSearchProvider | `EmbeddingTest` |
| Image | Text-to-image generation | `KernelConfigExtensionsImageTests` |
| STT | Speech-to-text | `KernelConfigExtensionsSpeechTests` |
| TTS | Text-to-speech | `KernelConfigExtensionsSpeechTests` |

## Not Yet Available

The following Kernel-aligned menu items are not implemented in AgentKernel yet. Running them shows **"Not available yet"**:

- Planner task planning
- PluginFromObject / Function Calling

## Configuration

1. Edit `appsettings.json`, or copy it to `appsettings.Development.json`, and fill in `SenparcAiSetting`.
2. The vector store defaults to `Memory`. You can change it to Redis, Qdrant, or another AgentKernel-supported type in configuration.

## Run

```bash
cd Samples/Senparc.AI.Samples.AgentKernelConsoles
dotnet run
```
