# Senparc.AI.Samples.AgentKernelConsoles

基于 **Senparc.AI.AgentKernel**（Microsoft Agent Framework）的控制台示例，菜单结构与 `Senparc.AI.Samples.Consoles` 对齐。

## 已实现

| 菜单 | 说明 | 参考测试 |
|------|------|----------|
| Chat | 多轮对话 + AgentSession | `AgentAiHandlerTests` |
| Completion | 单次 TextCompletion | `RunTest` / `SingleLineTest` |
| Embedding [1] | 向量写入 + 相似检索 | `EmbeddingStoreTest` |
| Embedding [2] | RAG + TextSearchProvider | `EmbeddingTest` |

## 尚未提供

以下能力与 Consoles（Kernel）对齐的菜单项在 AgentKernel 中暂未实现，运行后会提示 **「尚未提供」**：

- Dall·E 绘图
- Planner 任务计划
- PluginFromObject / Function Calling
- STT（Speech to Text）

## 配置

1. 编辑 `appsettings.json`（或复制为 `appsettings.Development.json`）填写 `SenparcAiSetting`
2. 向量库默认 `Memory`，可在配置中改为 Redis / Qdrant 等（需 AgentKernel 已支持的类型）

## 运行

```bash
cd Samples/Senparc.AI.Samples.AgentKernelConsoles
dotnet run
```
