---
name: senparc-add-ai-platform
description: Upgrade or add AI provider support in Senparc.AI and Senparc.AI.AgentKernel with provider-specific parameter modeling, platform mapping, adapter integration, and unit tests. Use when a new model platform (domestic or international) must be onboarded, or when an existing provider needs parameter/schema corrections based on official docs.
---

# Senparc Add AI Platform

## Goal

Add or correct provider support with a stable, repeatable path:

1. Use official docs to define true minimum parameters.
2. Model provider-specific keys in `Senparc.AI`.
3. Bind provider mapping in `AgentKernel` Chat/Embedding configuration.
4. Add unit tests that validate mapping and platform behavior.
5. Update provider support documentation with source links.

## Workflow

### 1. Classify provider protocol first

Choose one path before coding:

1. Native official SDK/provider exists and is suitable:
   Use official provider package directly.
2. Provider is OpenAI-compatible:
   Reuse OpenAI-compatible adapter path; do not duplicate a full provider package.
3. Provider is custom protocol/auth (signature, special websocket/http schema):
   Create a dedicated provider package under `src/Senparc.AI.AgentKernel.Providers.<ProviderName>/`.

### 2. Add provider data model in `Senparc.AI`

Do all of the following:

1. Add `AiPlatform.<Provider>` enum value in `src/Senparc.AI/Enums.cs`.
2. Add `<Provider>Keys` in `src/Senparc.AI/Entities/Keys/`.
3. Update `src/Senparc.AI/Interfaces/ISenparcAiSetting.cs`:
   Add key property, endpoint property, and `Endpoint` switch mapping.
4. Update `src/Senparc.AI/Entities/SenparcAiSettingBase.cs`:
   Add `Use<Provider>` flag, key property, endpoint property, `ApiKey` switch mapping, `ModelName` switch mapping, `Set<Provider>()` helper.
5. Update schema if needed:
   `src/Senparc.AI/appsettings.schema.json`.

### 3. Add runtime wiring in `AgentKernel`

Update provider routing in:

1. `src/Senparc.AI.AgentKernel/Helpers/AgentKernelHelper.Config.Chat.cs`
2. `src/Senparc.AI.AgentKernel/Helpers/AgentKernelHelper.Config.Embedding.cs`
3. `src/Senparc.AI.AgentKernel/Kernels/KernelBuilderExtensions/KernelBuilderExtension.Chat.cs`
4. `src/Senparc.AI.AgentKernel/Kernels/KernelBuilderExtensions/KernelBuilderExtension.Embedding.cs` (if embedding supported)

Rule:

1. If official provider lacks embedding, throw explicit `SenparcAiException`.
2. If provider is OpenAI-compatible, call shared OpenAI-compatible helper.

### 4. Add tests

Minimum required tests:

1. Setting mapping test:
   `src/Senparc.AI.AgentKernel.Tests/Helpers/SenparcAiSettingAdditionalPlatformTests.cs`
2. Chat/Embedding support routing test:
   `src/Senparc.AI.AgentKernel.Tests/Helpers/AgentKernelHelper.PlatformSupportTests.cs`
3. Provider-factory test only if a new dedicated provider package is created:
   `src/Senparc.AI.AgentKernel.Tests/Helpers/ProviderFactoriesTests.cs`

### 5. Update docs with official sources

Update:

1. `documents/AgentKernel-Provider-Support-Matrix.md`
2. Add provider minimum params, integration path, unsupported capability notes.
3. Add official source URLs for every provider-specific parameter claim.

## Review Checklist

1. Provider parameters are not forced into a generic 3-field shape.
2. `OrganizationId` is only used by providers that actually need it.
3. `AiPlatform` mapping is complete for `Endpoint`, `ApiKey`, `ModelName`.
4. Chat/Embedding behavior matches official capability boundaries.
5. Unit tests pass for newly added provider branches.
6. Documentation has source links and a clear compatibility explanation.

## References

Read `references/provider-update-checklist.md` for the execution checklist template.
