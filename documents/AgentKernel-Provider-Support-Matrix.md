# Senparc.AI / AgentKernel Provider Support Matrix

## 1. Goals

This document describes:

- Minimum required parameters for each provider (per official documentation)
- How each provider is integrated in `Senparc.AI` and `Senparc.AI.AgentKernel`
- Why OpenAI-compatible platforms can share the same adapter layer

## 2. Environment Notes

The current development environment cannot reach `api.nuget.org` (DNS failure). This implementation therefore uses:

- Existing available dependencies where possible
- Custom provider packages (`HuggingFace`, `FastAPI`)
- OpenAI-compatible routing for platforms that support it

When network access is restored, Anthropic/Gemini can be switched to official NuGet providers (see “Recommended Official Packages” below).

## 3. Provider Parameters and Integration

| Provider | Official Parameters (Minimum) | Current Implementation | Notes |
|---|---|---|---|
| OpenAI | `apiKey` (optional `organization`) | Native OpenAI client | Existing implementation; unchanged |
| Azure OpenAI | `endpoint` + `apiKey` + `deployment` | Native AzureOpenAI client | Existing implementation; unchanged |
| NeuCharAI | Azure OpenAI–compatible parameters | Reuses AzureOpenAI path | Existing implementation; unchanged |
| HuggingFace | `HF_TOKEN`; OpenAI-compatible routing and `base_url` supported | New standalone project `Senparc.AI.AgentKernel.Providers.HuggingFace` | Default endpoint: `https://router.huggingface.co/v1` |
| FastAPI | No single auth format mandated by the framework; typically API Key / OAuth2 / Bearer | New standalone project `Senparc.AI.AgentKernel.Providers.FastAPI` | Integrated as an OpenAI-compatible gateway |
| DeepSeek | Officially compatible with OpenAI/Anthropic API shapes | Reuses OpenAI-compatible path | Only `apiKey` / `endpoint` / `model` need to be swapped |
| Qwen (DashScope) | Official OpenAI-compatible docs; swap `apiKey` / `base_url` / `model` | Reuses OpenAI-compatible path | No duplicate adapter layer |
| Kimi (Moonshot) | Official OpenAI-compatible docs; swap `apiKey` / `base_url` / `model` | Reuses OpenAI-compatible path | No duplicate adapter layer |
| XunFei (iFLYTEK MaaS) | Official docs provide OpenAI protocol access with `apiKey` + `base_url` | Reuses OpenAI-compatible path (`XunFeiKeys`) | Current integration targets MaaS OpenAI-compatible Chat API; Embedding is not enabled by default |
| Anthropic | `x-api-key` + `anthropic-version` (not a native OpenAI API) | Chat via compatible path; Embedding explicitly unsupported | `AnthropicKeys` adds `AnthropicVersion` field |
| Gemini | API Key (Google AI) or Vertex parameters (Project/Location) | Compatible entry point retained for now | `GeminiKeys` adds `UseVertexAI` / `ProjectId` / `Location` to align with official parameter structure |

### XunFei integration note

XunFei currently has multiple API lines. This release uses the **MaaS OpenAI-compatible route** for `AiPlatform.XunFei`, because it can be cleanly integrated into the existing OpenAI-compatible adapter layer.

For XunFei Spark HTTP/WebSocket documentation using URL-signature authentication (`appid + apiKey + apiSecret`), a dedicated custom adapter can be added in a future release if required by business scenarios.

## 4. Recommended Official Packages (switch when network is available)

- Anthropic (Agent Framework): `Microsoft.Agents.AI.Anthropic`
- OpenAI (Agent Framework): `Microsoft.Agents.AI.OpenAI`
- Gemini (Google official .NET SDK): `Google.GenAI` (supports `IChatClient` adapter)

## 5. Key Code Changes in This Release

- New standalone provider packages:
  - `src/Senparc.AI.AgentKernel.Providers.HuggingFace/`
  - `src/Senparc.AI.AgentKernel.Providers.FastAPI/`
- `AgentKernel` updated to accept generic `IChatClient` / `IEmbeddingGenerator`.
- `Senparc.AI` parameter models changed from a unified three-field layout to provider-specific requirements:
  - Anthropic / Gemini / Qwen / Kimi / FastAPI no longer require `OrganizationId`
  - Anthropic adds `AnthropicVersion`
  - Gemini adds Vertex-related fields
- Anthropic Embedding throws at configuration time to avoid implying official support.
- XunFei added as OpenAI-compatible Chat platform (`XunFeiKeys` with provider-specific endpoint + apiKey).

## 6. Reference Links

- Microsoft Agent Framework - Anthropic provider:
  - https://learn.microsoft.com/en-us/agent-framework/agents/providers/anthropic
- Microsoft Agent Framework - Custom provider:
  - https://learn.microsoft.com/en-us/agent-framework/agents/providers/custom
- NuGet - Microsoft.Agents.AI.Anthropic:
  - https://www.nuget.org/packages/Microsoft.Agents.AI.Anthropic/
- Google Gemini API key:
  - https://ai.google.dev/gemini-api/docs/api-key
- Google Gemini API reference:
  - https://ai.google.dev/api
- NuGet - Google.GenAI:
  - https://www.nuget.org/packages/Google.GenAI
- DeepSeek API docs:
  - https://api-docs.deepseek.com/
- Qwen / DashScope OpenAI-compatible:
  - https://www.alibabacloud.com/help/en/model-studio/compatibility-of-openai-with-dashscope
- Kimi API overview:
  - https://platform.kimi.ai/docs/api/overview
- Kimi API quickstart:
  - https://platform.kimi.ai/docs/guide/start-using-kimi-api
- XunFei MaaS Coding Plan (supports OpenAI protocol):
  - https://www.xfyun.cn/doc/spark/CodingPlan.html
- XunFei inference service HTTP protocol (OpenAI Python example):
  - https://www.xfyun.cn/doc/spark/%E7%B2%BE%E8%B0%83%E6%9C%8D%E5%8A%A1API-http.html
- XunFei general authentication (appid / APIKey / APISecret signature mode):
  - https://www.xfyun.cn/doc/spark/general_url_authentication.html
- HuggingFace Inference Providers:
  - https://huggingface.co/docs/inference-providers/index
  - https://huggingface.co/docs/inference-providers/en/guides/first-api-call
- FastAPI security docs:
  - https://fastapi.tiangolo.com/tutorial/security/
