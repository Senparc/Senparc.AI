# Senparc.AI.AgentKernel Harness Release

## Scope

`Senparc.AI.AgentKernel` now exposes the native Microsoft Agent Framework Harness through:

- `AgentKernelHarness`
- `AgentKernelHarnessOptions`
- `BuildHarnessAgentAsync(...)`
- `SerializeSessionAsync(...)`

The integration uses `Microsoft.Agents.AI.Harness` and does not modify `Senparc.AI.Kernel`.

The default options are least privilege:

- File access disabled
- File memory disabled
- Skills scanning disabled
- Hosted Web Search disabled
- Todo, plan/execute modes, context compaction, function invocation, and tool approval remain enabled

Harness requests also use AgentKernel's shared model compatibility boundary:

- `BuildHarnessAgentAsync(...)` sanitizes Harness `ChatOptions` for models that reject sampling parameters such as `Temperature` and `TopP`.
- `AgentKernelHarness.RunStreaming(...)` always uses the native MAF streaming transport.
- A provider that cannot stream is treated as a real provider error; AgentKernel does not silently replace streaming with a buffered response.

## Current Package Status

The package is published as:

```text
Senparc.AI.AgentKernel 0.1.14-preview5
```

The preview suffix is intentional because the current MAF Harness dependency is:

```text
Microsoft.Agents.AI.Harness 1.8.0-preview.260528.1
```

Do not publish this AgentKernel version as a stable NuGet version until the corresponding Harness dependency is stable and the real-model/tool approval tests have passed.

## Local Release Build

Run from the Senparc.AI repository:

```bash
dotnet restore src/Senparc.AI.AgentKernel/Senparc.AI.AgentKernel.csproj
dotnet build src/Senparc.AI.AgentKernel/Senparc.AI.AgentKernel.csproj --no-restore -c Release -m:1
```

The project has `GeneratePackageOnBuild` enabled for Release. The package is generated under:

```text
BuildOutPut/Senparc.AI.AgentKernel.0.1.14-preview5.nupkg
```

Verify the package dependency before publishing:

```bash
unzip -p BuildOutPut/Senparc.AI.AgentKernel.0.1.14-preview5.nupkg '*.nuspec' \
  | rg 'Microsoft.Agents.AI.Harness|Senparc.AI.AgentKernel'
unzip -t BuildOutPut/Senparc.AI.AgentKernel.0.1.14-preview5.nupkg
```

## Installing the Local Package in NCF

The NCF repository references the preview version in `src/Senparc.AI.AgentKernel.props`.

Restore with the local package folder before building NCF:

```bash
dotnet restore tools/NcfSimulatedSite/Tests/Senparc.Areas.Admin.Tests/Senparc.Areas.Admin.Tests.csproj \
  --source "<path-to-Senparc.AI>/BuildOutPut" \
  --source https://api.nuget.org/v3/index.json
```

Then build without restoring:

```bash
dotnet build tools/NcfSimulatedSite/Senparc.Areas.Admin/Senparc.Areas.Admin.csproj \
  --no-restore -c Debug -m:1
```

If NuGet cannot write to the user cache, run the restore from a shell with permission to write:

```text
/Users/jeffreysu/.nuget/packages
```

Do not replace the package with a ProjectReference when validating the real release artifact.

## Public API Example

```csharp
var iWantToRun = agentHandler
    .IWantTo(setting)
    .ConfigChatModel("MyHarness", new ChatClientAgentOptions())
    .BuildKernel();

var harness = await iWantToRun.BuildHarnessAgentAsync();
var response = await harness.RunAsync(
    [new ChatMessage(ChatRole.User, "整理这项任务并执行必要工具")]);

var sessionState = await harness.SerializeSessionAsync();
```

For resume, persist `sessionState.GetRawText()` and pass the parsed JSON as `serializedSession` to `BuildHarnessAgentAsync`.

## Release Gates

Before publishing:

1. Build AgentKernel in Release with `--no-restore -m:1`.
2. Confirm the `.nupkg` contains `Microsoft.Agents.AI.Harness`.
3. Build NCF using the local `.nupkg`, not a stale cached package.
4. Run AgentKernel unit tests with the repository's Microsoft Testing Platform configuration.
5. Run NCF Admin Chat tests and a real authenticated model/tool Harness test.
6. Verify tool approvals, session resume after process restart, and least-privilege file settings.
7. Publish the preview package first. Promote to stable only after the MAF Harness dependency and E2E gates are stable.
