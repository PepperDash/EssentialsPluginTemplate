![PepperDash Essentials Plugin Logo](/images/essentials-plugin-blue.png)

![PepperDash Essentials](https://img.shields.io/badge/PepperDash%20Essentials-≥%20v2.42.4-blue)
![.NET](https://img.shields.io/badge/.NET%20Framework-4.7.2-512BD4)
![Crestron](https://img.shields.io/badge/Crestron-4--Series-lightgrey)
![License](https://img.shields.io/badge/license-MIT-green)

# Essentials Plugin Template (c) 2026

Use this repository as the starting point for a new [PepperDash Essentials](https://github.com/PepperDash/Essentials) plugin. For background on plugins, see the Essentials Wiki [Plugins](https://pepperdash.github.io/Essentials/docs/Plugins.html) article.

This README follows the [Diataxis](https://diataxis.fr/) structure:

* [Tutorial](#tutorial) - build your first plugin from the template, step by step
* [How-to guides](#how-to-guides) - recipes for specific tasks
* [Reference](#reference) - files, build properties, targets, error codes and rules
* [Explanation](#explanation) - how the template and its automation work, and why
* [Plugin Documentation](#plugin-documentation) - the generated documentation of this plugin

## Tutorial

In this tutorial you create a plugin for a fictional serial/TCP display, the "Samsung MDC", from the template, build it, document it and commit it. It takes about 15 minutes. You need the [.NET SDK](https://dotnet.microsoft.com/download), Git, and Visual Studio or VS Code.

### 1. Create the repository

On GitHub, create a new repository from this template (**Use this template**), or fork it. Clone it and open the folder:

```
git clone https://github.com/<your-org>/epi-samsung-mdc.git
cd epi-samsung-mdc
```

### 2. Build it once

```
dotnet build
```

The build restores the Essentials NuGet packages, installs the git hooks, and writes two files to `output/`: `epi-make-model.4Series.1.0.0-local.cplz` (the plugin) and a `.nupkg` (the NuGet package). Run `git config core.hooksPath`; it prints `.husky`, which confirms the commit-message hook is active.

### 3. Keep one device category

The template has three example devices. A display is controlled over a serial or TCP connection, so keep `MakeModelDevice` and its factory, and delete the other two device classes and their factories:

```
git rm src/MakeModelLogicDevice.cs src/MakeModelLogicDeviceFactory.cs
git rm src/MakeModelCrestronDevice.cs src/MakeModelCrestronDeviceFactory.cs
```

### 4. Name the device

1. Rename `MakeModelDevice` to `SamsungMdcDevice` and `MakeModelDeviceFactory` to `SamsungMdcDeviceFactory`, in both the class and the file name. Your editor's rename refactoring updates the references.
2. In `SamsungMdcDeviceFactory`, set the type name that configuration files will use:

   ```csharp
   TypeNames = new List<string>() { "samsungMdc" };
   ```

3. Build again with `dotnet build`. It succeeds, and the badges at the top of this README are unchanged because the Essentials version did not change.

### 5. Document the plugin

```
dotnet msbuild -t:UpdateReadmeDocs
git diff README.md
```

The [Plugin Documentation](#plugin-documentation) sections now list `samsungMdc` under Supported Types, and only the base classes of the device you kept.

### 6. Commit

```
git add -A
git commit -m "feat: add Samsung MDC display plugin"
```

The commit-msg hook checks the message and the commit succeeds. Try `git commit --allow-empty -m "added stuff"` to see it reject a message that does not follow the [commit message rules](#commit-message-rules).

You now have a building, documented plugin. Next, work through [Rename the template](#rename-the-template) to finish customizing it, and [Release a version](#release-a-version) to publish it.

## How-to guides

### Rename the template

Every name to replace is a form of "Make Model"; the [placeholder names](#placeholder-names) table lists each form and where it appears. Search the repository for `MakeModel`, `make-model`, `Make.Model` and `Make Model`, then:

1. Rename the solution and project files (`epi-make-model.4Series.sln`, `src/epi-make-model.4Series.csproj`, and the project path inside the `.sln`), the namespace and the classes. The XML `remarks` and `example` tags on each class show the intended rename, for example `MakeModelDevice` to `SamsungMdcDevice`.
2. Delete the device categories and factories the plugin does not need.
3. In each factory, set `MinimumEssentialsFrameworkVersion` and `TypeNames`.
4. Update `MakeModelPropertiesConfig` and `MakeModelBridgeJoinMap` for the plugin's configuration and joins.
5. Update the [package properties](#package-properties).
6. Update `.releaserc.json` (see [Release a version](#release-a-version)).
7. [Regenerate the plugin documentation](#regenerate-the-plugin-documentation).

In Visual Studio, the Task List shows every remaining `TODO [ ]` item.

### Change the minimum Essentials version

1. Set `MinimumEssentialsFrameworkVersion` in each factory to the lowest Essentials version the plugin is tested against.
2. Set the `PepperDashEssentials` `PackageReference` version in the csproj to the same version or higher.
3. Run `dotnet build`. The `PepperDash Essentials` badge updates; commit `README.md` with the change.
4. [Regenerate the plugin documentation](#regenerate-the-plugin-documentation) so the Minimum Essentials Framework Versions section matches.

### Regenerate the plugin documentation

1. Run `dotnet msbuild -t:UpdateReadmeDocs` from the repo root, or in VS Code run **Terminal > Run Task... > Update README docs**. In Copilot Chat, `/update-readme-docs` runs the target and then reviews the result.
2. Review `git diff README.md`. If the Config Example shows placeholders such as `SampleString` or `SampleValue`, add an `<example>` block for that property in the config class (see [Set the Config Example values](#set-the-config-example-values)) and run the target again.
3. Commit `README.md` with your changes.

### Edit a generated section by hand

1. Edit the content between the section's `<!-- START name -->` and `<!-- END name -->` markers.
2. Add `<!-- SKIP -->` on the line after the `<!-- START name -->` marker so later runs leave the section alone.

To hide a section that does not apply, keep its markers with only `<!-- SKIP -->` between them. Deleting the markers does not work; the generator adds them back.

Do not mark Minimum Essentials Framework Versions as `<!-- SKIP -->`, or it stops following version changes. The Config Example is always regenerated and ignores `<!-- SKIP -->`; set its values in the config class instead.

### Set the Config Example values

The Config Example takes each property's value from the first `<example>` block on that property whose `<code>` contains the property's JSON name, for example:

```csharp
/// <example>
/// <code>
/// "properties": {
///     "pollTimeMs": 60000
/// }
/// </code>
/// </example>
[JsonProperty("pollTimeMs")]
public long PollTimeMs { get; set; }
```

Without an example, `pollTimeMs`, `warningTimeoutMs` and `errorTimeoutMs` default to `30000`, `180000` and `300000`, and other properties get placeholders from their C# type. Run `dotnet msbuild -t:UpdateReadmeDocs` after changing an example.

### Check the README the way CI does

```
dotnet build -p:ReadmeMode=Check
```

The build fails with `PDREADME004` if the badges are stale and with `PDREADME005` if the plugin documentation is stale, and changes nothing. Fix it with `dotnet build` and `dotnet msbuild -t:UpdateReadmeDocs`.

### Release a version

1. Replace the placeholder pre-release entry in `.releaserc.json` (`replace-me-feature-branch`, `replace-me-prerelease`) with your pre-release branch and channel, or remove it if you only release from `main`.
2. Write [conventional commit messages](#commit-message-rules); they determine the next version.
3. Push. The build workflow releases when semantic-release finds a new version. Use the commit scope `force-patch` to force a patch release, or `no-release` to skip one.

### Install or skip the git hooks

* The hooks install automatically on `dotnet restore` or `dotnet build`, and when VS Code runs the "Install git hooks" task on folder open. To install them manually: `dotnet tool restore`, then `dotnet husky install`.
* Skip the hook for one commit: `git commit --no-verify`.
* Skip the automatic install: set the environment variable `HUSKY=0`.

### Package properties

Set these plugin-specific properties in the csproj:

1. `PackageId` - the name used to install the package from NuGet
2. `PackageProjectUrl` - the plugin repository's URL
3. `AssemblyTitle` - the DLL name shown on a processor when the plugin is loaded
4. `Description` and `PackageTags`

Set `Product` and `RepositoryUrl` in `src/Directory.Build.props`. Shared values (`Version`, `Authors`, `Company`, `Copyright`, `PackageOutputPath`) are also defined there.

## Reference

### Repository layout

| Path | Contents |
| --- | --- |
| `src/MakeModelDevice.cs`, `src/MakeModelDeviceFactory.cs` | Device that talks to third-party equipment over a stream (serial, TCP/SSH/UDP, CEC) |
| `src/MakeModelLogicDevice.cs`, `src/MakeModelLogicDeviceFactory.cs` | Device with logic only, no external communication |
| `src/MakeModelCrestronDevice.cs`, `src/MakeModelCrestronDeviceFactory.cs` | Device that represents Crestron hardware |
| `src/MakeModelPropertiesConfig.cs` | Device configuration class (`MakeModelPropertiesConfig`): the device's `properties` in the configuration file |
| `src/MakeModelBridgeJoinMap.cs` | EISC bridge join map (`MakeModelBridgeJoinMap`) |
| `src/Directory.Build.props` | Shared version, package and copyright properties |
| `src/Directory.Build.targets` | CPLZ packaging, git hook install, README badge and docs targets |
| `build/ReadmeDocs.cs` | Plugin documentation generator, compiled by the build |
| `.husky/` | Commit-msg hook and its linter (`csx/commit-lint.csx`) |
| `.releaserc.json` | semantic-release configuration |
| `.github/workflows/EssentialsPlugins-builds-caller.yml` | Build and release workflow |
| `.github/skills/update-readme-docs/` | Copilot skill that regenerates and reviews the plugin documentation |

### Placeholder names

The template names everything after a fictional "Make Model" device. Replace each form with your plugin's make and model; for a Samsung MDC display, `MakeModel` becomes `SamsungMdc` and `epi-make-model` becomes `epi-samsung-mdc`.

| Form | Where |
| --- | --- |
| `MakeModel` | Class and file names in `src/` (devices, factories, `MakeModelPropertiesConfig`, `MakeModelBridgeJoinMap`), and the namespace `PepperDash.Essentials.Plugins.MakeModel` (`RootNamespace` in the csproj and every `.cs` file) |
| `epi-make-model` | Solution and project file names, the GitHub repository name in `RepositoryUrl` (`src/Directory.Build.props`) and `PackageProjectUrl` (csproj) |
| `Make.Model` | `AssemblyTitle` and `PackageId` (both `PepperDash.Essentials.Plugins.Make.Model`) in the csproj |
| `Make Model` | `Product` (`src/Directory.Build.props`) and `Description` (csproj) |
| `examplePlugin…` | `TypeNames` in each factory, the configuration `type` values |

### Build outputs

| File | Description |
| --- | --- |
| `output/<project>.<version>.cplz` | The plugin, for loading on a processor |
| `output/<PackageId>.<version>.nupkg` | The NuGet package, including `LICENSE.md` and this `README.md` |

### Build targets

| Target | Runs | Effect |
| --- | --- | --- |
| `UpdateReadmeBadges` | Before every local build | Rewrites stale `.NET` and `PepperDash Essentials` badges. Incremental. |
| `CheckReadmeBadges` | Before the build when `ReadmeMode` is `Check` | Fails on stale badges |
| `UpdateReadmeDocs` | Only when invoked (`dotnet msbuild -t:UpdateReadmeDocs`) | Regenerates the [Plugin Documentation](#plugin-documentation) sections |
| `CheckReadmeDocs` | Before the build when `ReadmeMode` is `Check` | Fails on stale plugin documentation |
| `InstallGitHooks` | Before restore, unless `CI=true` or `HUSKY=0` | Runs `dotnet tool restore` and `dotnet husky install` |

The README targets run only for projects with `ProjectType` `ProgramLibrary`, and never in design-time builds.

### Build properties

| Property | Default | Effect |
| --- | --- | --- |
| `ReadmeMode` | `Check` when `CI=true`, otherwise `Update` | `Check` verifies the README without changing it |
| `SkipReadmeBadges` | not set | `true` skips the badge targets |
| `SkipReadmeDocsCheck` | not set | `true` skips `CheckReadmeDocs` |
| `ReadmePath` | `README.md` at the repo root | README file the targets update |
| `HUSKY` | not set | `0` skips the git hook install (environment variable) |

### Error codes

| Code | Meaning | Fix |
| --- | --- | --- |
| `PDREADME001` | No `MinimumEssentialsFrameworkVersion = "x.y.z";` assignment found in the project's C# files | Set it in at least one factory |
| `PDREADME002` | The `PepperDashEssentials` package version is lower than `MinimumEssentialsFrameworkVersion` (a prerelease such as `2.13.0-beta` counts as lower than `2.13.0`) | Raise the package version or lower the factory minimum |
| `PDREADME003` | A badge is missing from `README.md` | Restore the `.NET` and `PepperDash Essentials` badges at the top |
| `PDREADME004` | The badges are stale (check mode) | Run `dotnet build` and commit `README.md` |
| `PDREADME005` | The plugin documentation is stale (check mode) | Run `dotnet msbuild -t:UpdateReadmeDocs`, review and commit `README.md` |

### Badges

| Badge | Source |
| --- | --- |
| `.NET` | `TargetFramework` of the plugin project |
| `PepperDash Essentials` | The highest `MinimumEssentialsFrameworkVersion = "x.y.z";` assignment in the project's C# files, in any file and subfolder |

### Generated documentation sections

| Section | Source |
| --- | --- |
| Minimum Essentials Framework Versions | Each distinct `MinimumEssentialsFrameworkVersion` value |
| Config Example | The class whose name ends in `Config` or `ConfigObject` with the most properties. Values come from each property's `<example>` block (see [Set the Config Example values](#set-the-config-example-values)). `type` is the first `TypeNames` entry of the first C# file (by file name) that sets `TypeNames`; the unused `uid` property is removed. Always regenerated, even with `<!-- SKIP -->`. |
| Supported Types | Every `TypeNames` entry |
| Join Maps | Classes deriving from `JoinMapBaseAdvanced`, in any file. Type (RW) is `R`, `W` or `R/W` from each join's `JoinCapabilities` |
| Base Classes, Interfaces | Base classes and interfaces declared by the plugin's own classes, excluding factories and join maps |
| Public Methods | Public methods in the project's C# files |
| Bool, Int and String Feedbacks | Public `BoolFeedback`, `IntFeedback` and `StringFeedback` members |

A section containing `<!-- SKIP -->` is never changed, except the Config Example, which is always regenerated. Factories and join maps are found by their content, so renaming them (for example to `SonyBraviaDeviceFactory`) needs no changes.

### Commit message rules

* Header: `<type>(<optional scope>): <subject>`, at most 100 characters
* Types: `feat`, `fix`, `chore`, `docs`, `style`, `refactor`, `perf`, `test`, `build`, `ci`, `revert`, `wip`
* A blank line between the header and the body
* Breaking changes: a `BREAKING CHANGE:` footer. The `feat!:` form is rejected because the release tooling does not recognize it.
* Allowed as-is: merge commits, `Revert "..."`, and `fixup!`, `squash!` and `amend!` commits (squash these before pushing)

The type list follows the shared `checkCommitMessage` workflow in PepperDash/workflow-templates. This template's build workflow does not run that check.

### Requirements

* [.NET SDK](https://dotnet.microsoft.com/download): building, the git hooks and all README automation. Nothing else is required.
* Visual Studio or VS Code
* The Essentials libraries come from the `PepperDashEssentials` NuGet package and restore automatically; `nuget.exe` is not required.

## Explanation

### Device categories

Essentials plugins usually wrap one of three kinds of device, so the template includes one example of each: a device that talks to third-party equipment over a stream (`MakeModelDevice`), a device that only contains logic (`MakeModelLogicDevice`), and a device that represents Crestron hardware (`MakeModelCrestronDevice`). Each has its own factory, which tells Essentials which configuration `type` values create it and which Essentials version it needs. One plugin can contain several devices.

### Why the README is maintained by the build

This README is packed into the plugin's NuGet package, so the badges and the plugin documentation must match the code that was released. Both are produced by MSBuild targets rather than by a git hook or a CI job:

* A build target runs the same way in Visual Studio, VS Code, the command line and CI, and needs nothing beyond the .NET SDK.
* Git hooks can be skipped, and a CI job that commits to the repository creates extra commits after review. Generating locally means the change is reviewed with the code that caused it.
* CI runs the same targets in check mode, so a release cannot be packaged with a README that no longer matches the code.

The badges are rewritten on every build because they are purely mechanical. The plugin documentation is regenerated only when you ask for it, because it needs a review: the Config Example can contain placeholders for properties without an `<example>` block, and some sections are curated by hand with `<!-- SKIP -->`.

CI checks the README only in the release build, which runs when semantic-release finds a new version. Ordinary pushes are not checked.

### The documentation generator

`build/ReadmeDocs.cs` is a C# port of `metadata.py` from [PepperDash/workflow-templates](https://github.com/PepperDash/workflow-templates), which the older `update-readme` workflow runs in CI. It produces the same sections, with these differences: the output order no longer depends on the file system, each Minimum Essentials Framework Version is listed once, a join map is also found when its file is not named after the class, interface names are matched case-sensitively, and Base Classes are listed before Interfaces as plain items. The two implementations are maintained separately.

### Commit messages and versions

Releases are versioned by [semantic-release](https://semantic-release.gitbook.io/), which reads the commit messages since the last release: `fix` produces a patch release, `feat` a minor release, and a `BREAKING CHANGE:` footer a major release. The commit-msg hook catches malformed messages before they reach the shared history, where they would be ignored or produce the wrong version.

## License

Provided under the MIT license; see [LICENSE.md](LICENSE.md).

## Plugin Documentation

The sections below are generated from the source code; see [Regenerate the plugin documentation](#regenerate-the-plugin-documentation) and [Generated documentation sections](#generated-documentation-sections).

<!-- START Minimum Essentials Framework Versions -->
### Minimum Essentials Framework Versions

- 2.42.4
<!-- END Minimum Essentials Framework Versions -->

<!-- START Config Example -->
### Config Example

```json
{
    "key": "device-1",
    "name": "Example Device",
    "type": "examplePluginCrestronDevice",
    "group": "pluginDevices",
    "properties": {
        "control": {
            "method": "tcpIp",
            "tcpSshProperties": {
                "address": "172.22.0.101",
                "port": 23,
                "username": "admin",
                "password": "password",
                "autoReconnect": true,
                "autoReconnectIntervalMs": 10000
            }
        },
        "pollTimeMs": 30000,
        "warningTimeoutMs": 180000,
        "errorTimeoutMs": 300000,
        "DeviceDictionary": {
            "item1": {
                "name": "Item 1 Name",
                "value": 1
            }
        }
    }
}
```
<!-- END Config Example -->

<!-- START Supported Types -->
### Supported Types

- examplePluginCrestronDevice
- examplePluginDevice
- examplePluginLogicDevice
<!-- END Supported Types -->

<!-- START Join Maps -->
### Join Maps

#### Digitals

| Join | Type (RW) | Description |
| --- | --- | --- |
| 1 | R | Is Online |
| 2 | R/W | Connect (Held)/Disconnect (Release) & corresponding feedback |

#### Analogs

| Join | Type (RW) | Description |
| --- | --- | --- |
| 1 | R | Socket Status |
<!-- END Join Maps -->

<!-- START Base Classes -->
### Base Classes

- CrestronGenericBridgeableBaseDevice
- EssentialsBridgeableDevice
<!-- END Base Classes -->

<!-- START Interfaces Implemented -->
<!-- END Interfaces Implemented -->

<!-- START Public Methods -->
### Public Methods

- public void SendText(string text)
- public void SendBytes(byte[] bytes)
- public void Poll()
<!-- END Public Methods -->

<!-- START Bool Feedbacks -->
### Bool Feedbacks

- ConnectFeedback
- OnlineFeedback
<!-- END Bool Feedbacks -->

<!-- START Int Feedbacks -->
### Int Feedbacks

- StatusFeedback
<!-- END Int Feedbacks -->

<!-- START String Feedbacks -->

<!-- END String Feedbacks -->
