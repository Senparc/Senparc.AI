# Provider Update Checklist

## Official verification

1. Confirm official endpoint format.
2. Confirm required auth fields (apiKey, apiSecret, appId, headers, signature, etc.).
3. Confirm whether provider is OpenAI-compatible or custom protocol.
4. Confirm capabilities: chat, embedding, image, speech.
5. Save official source links used for each claim.

## Code changes

1. Add `AiPlatform` enum value.
2. Add provider-specific `*Keys` model.
3. Update `ISenparcAiSetting` endpoint/key mapping.
4. Update `SenparcAiSettingBase` (`ApiKey`, `ModelName`, endpoint, `Set*`).
5. Update `AgentKernelHelper` Chat/Embedding switches.
6. Add/adjust `KernelBuilderExtension` methods.
7. If custom protocol, create standalone provider project.

## Tests

1. Setting-level mapping test.
2. Chat config route test.
3. Embedding config route test or explicit throw test.
4. Provider factory creation test (when custom provider package is added).

## Documentation

1. Update provider support matrix row.
2. Explain whether provider reuses OpenAI-compatible path.
3. Explicitly mark unsupported capabilities.
4. Attach official source links.
