![PepperDash Essentials Plugin Logo](/images/essentials-plugin-blue.png)

![PepperDash Essentials](https://img.shields.io/badge/PepperDash%20Essentials-≥%20v2.12.1-blue)
![.NET](https://img.shields.io/badge/.NET%20Framework-4.7.2-512BD4)
![Crestron](https://img.shields.io/badge/Crestron-4--Series-lightgrey)
![License](https://img.shields.io/badge/license-MIT-green)

# Essentials Plugin Template (c) 2025

## Overview

Use this repository as the starting point for a new Essentials plugin. For more information about plugins, refer to the Essentials Wiki [Plugins](https://pepperdash.github.io/Essentials/docs/Plugins.html) article.

This repo contains example classes for the three main categories of devices:

* `MakeModelDevice`: Used for most third party devices which require communication over a streaming mechanism such as a Com port, TCP/SSH/UDP socket, CEC, etc
* `MakeModelLogicDevice`: Used for devices that contain logic, but don't require any communication with third parties outside the program
* `MakeModelCrestronDevice`: Used for devices that represent a piece of Crestron hardware

There are matching factory classes for each of the three categories of devices. `MakeModelConfigObject` and `MakeModelBridgeJoinMap` are templates that should be modified for whichever categories of device the plugin uses.

This also illustrates how a plugin can contain multiple devices.

## Getting Started

1. **Create your repository.** Fork this repository into your own GitHub space, then create a new repository using it as the template.
2. **Build.** Open `epi-make-model.4Series.sln` in Visual Studio or VS Code and build, or run `dotnet build` from the repo root. Dependencies restore automatically (see [Prerequisites](#prerequisites)).
3. **Rename and customize.** Work through the [customization checklist](#renaming-and-customizing-the-template).
4. **Set up releases.** Update `.releaserc.json` (see [Building, Packaging and Releasing](#building-packaging-and-releasing)).
5. **Document the plugin.** Generate the plugin documentation at the end of this README (see [README Docs](#readme-docs-generated-plugin-documentation)).

## Prerequisites

* [.NET SDK](https://dotnet.microsoft.com/download) (builds the project and provides `dotnet tool` for the git hooks)
* Visual Studio or VS Code
* The [Essentials](https://github.com/PepperDash/Essentials) libraries are referenced with a NuGet `PackageReference` (`PepperDashEssentials`) in `src/epi-make-model.4Series.csproj`. They are restored automatically by Visual Studio, `dotnet restore` or `dotnet build`; `nuget.exe` is not required.

The git hooks and README docs tooling have their own requirements; see [Repository Automation](#repository-automation).

## Renaming and Customizing the Template

There is extensive inline documentation and examples in the source. In Visual Studio, the Task List lists every `TODO [ ]` item to complete. For renaming instructions in particular, see the XML `remarks` tags on the class definitions.

Checklist:

1. Rename the solution, project, namespace and classes to match the plugin. The template's class and file names are not fully consistent (for example, the join map class is `EssentialsPluginTemplateBridgeJoinMap` in `MakeModelBridgeJoinMap.cs`), so search for both `MakeModel` and `EssentialsPluginTemplate`.
2. Delete the device categories and factories the plugin does not need.
3. In each factory, set `MinimumEssentialsFrameworkVersion` and `TypeNames`.
4. Update the `PepperDashEssentials` package version in the csproj to match `MinimumEssentialsFrameworkVersion` (the pre-commit hook warns if they differ).
5. Modify `MakeModelConfigObject` and `MakeModelBridgeJoinMap` for the plugin's configuration and joins.
6. Update the package properties in the csproj (see [Package properties](#package-properties)).
7. Update `.releaserc.json` (see [Building, Packaging and Releasing](#building-packaging-and-releasing)).
8. Regenerate the README docs.

## Building, Packaging and Releasing

Building the project (Visual Studio, or `dotnet build`) produces two artifacts in `output/`:

* `<project>.<version>.cplz` - the plugin, for loading on a processor
* `<PackageId>.<version>.nupkg` - the NuGet package, which includes `LICENSE.md` and this `README.md`

Because this README is packed into the NuGet package, keep it accurate for plugin users.

### Package properties

To modify the name and other details of the package, edit the following properties in `src/epi-make-model.4Series.csproj`:

1. `PackageId` - This is the name that will be used to pull the package from NuGet once it's published
2. `PackageProjectUrl` - This should match the URL for the plugin repo
3. `AssemblyTitle` - This is the DLL file name that will show on a processor when the plugin is loaded

Shared values such as `Version`, `Copyright` and `PackageOutputPath` are defined in `src/Directory.Build.props`; the csproj values take precedence where both are set.

### Releases (PepperDash Internal)

Every push runs `.github/workflows/EssentialsPlugins-builds-caller.yml`, which calls the shared workflows in [PepperDash/workflow-templates](https://github.com/PepperDash/workflow-templates) to determine the version with [semantic-release](https://semantic-release.gitbook.io/) (`.releaserc.json`) and build the plugin when there is a new version.

* Versions are derived from commit messages. A commit scope of `force-patch` forces a patch release and `no-release` skips the release.
* `.releaserc.json` ships with placeholder pre-release settings (`replace-me-feature-branch`, `replace-me-prerelease`). Replace them with your pre-release branch and channel, or remove the entry if you do not use one.

## Repository Automation

### Git Hooks (Husky.Net)

This repo uses [Husky.Net](https://alirezanet.github.io/Husky.Net/) to run a `pre-commit` hook. The hook runs `.github/scripts/Update-ReadmeBadges.ps1`, which keeps the `.NET` and `PepperDash Essentials` badges at the top of this README in sync with the project:

* `.NET` badge - `TargetFramework` in `src/*.4Series.csproj`
* `PepperDash Essentials` badge - the highest `MinimumEssentialsFrameworkVersion` in `src/*Factory.cs`

If the badges change, `README.md` is re-staged automatically so the update is part of your commit. A warning is printed if the `PepperDashEssentials` package version in the csproj differs from `MinimumEssentialsFrameworkVersion`.

#### Requirements

* [.NET SDK](https://dotnet.microsoft.com/download) (provides `dotnet tool`)
* Windows: the built-in Windows PowerShell 5.1 is used. macOS/Linux: [PowerShell 7 (`pwsh`)](https://learn.microsoft.com/powershell/scripting/install/installing-powershell) must be on the `PATH`.

#### Setup

The hook is installed automatically the first time you do any of the following in a fresh clone:

* Open the solution in Visual Studio, or run `dotnet restore` / `dotnet build`
* Open the folder in VS Code and allow the "Install git hooks" automatic task

To install it manually:

```
dotnet tool restore
dotnet husky install
```

Verify with `git config core.hooksPath`, which should print `.husky`.

#### Usage

* Commit as usual; the hook runs on every `git commit`.
* Run it on demand: `dotnet husky run`
* Check without modifying files (useful for CI): `pwsh .github/scripts/Update-ReadmeBadges.ps1 -Check`
* Skip the hook for a single commit: `git commit --no-verify`
* Skip the automatic install on restore: set the `HUSKY=0` environment variable. It is also skipped when `CI=true`.

#### Renaming Projects and Factories

The script locates files by pattern (`*.4Series.csproj`, `*Factory.cs`), so renaming the project or factory classes does not require changes. Keep a `MinimumEssentialsFrameworkVersion = "x.y.z";` assignment in at least one factory, and keep the two badges in this README.

### README Docs (Generated Plugin Documentation)

The plugin's config example, supported types, join maps, feedbacks and public methods are generated into `<!-- START name -->` / `<!-- END name -->` sections under [Plugin Documentation](#plugin-documentation) at the end of this README. Generation runs locally on your current branch, so the result is committed with your changes (there is no CI job or separate branch, and it is not part of the pre-commit hook).

#### Requirements

* [Python 3](https://www.python.org/downloads/) on the `PATH` (`python3`, `python` or `py`)
* PowerShell: Windows PowerShell 5.1 (built in) or [PowerShell 7 (`pwsh`)](https://learn.microsoft.com/powershell/scripting/install/installing-powershell) on macOS/Linux
* Internet access on first run (the script downloads `metadata.py` from [PepperDash/workflow-templates](https://github.com/PepperDash/workflow-templates))

#### Running the Update

Run the commands from the repo root; the script always updates the repo root `README.md`.

| Where | How |
| --- | --- |
| VS Code | `Terminal > Run Task...` > **Update README docs** |
| macOS/Linux terminal | `pwsh -NoProfile -File .github/scripts/Update-ReadmeDocs.ps1` |
| Windows terminal | `powershell -NoProfile -ExecutionPolicy Bypass -File .github\scripts\Update-ReadmeDocs.ps1` |
| Copilot Chat | `/update-readme-docs` - runs the script, then reviews the generated sections against `src/` and cleans them up |

Then review `git diff README.md` and commit the result with your changes.

Options:

* `-Ref <branch-or-tag>` - download `metadata.py` from a specific `workflow-templates` ref instead of `main`
* `-ScriptPath <path>` - use a local copy of `metadata.py` (no download)

#### Controlling the Output

* The Config Example `type` is the first `TypeNames` entry of the first `*Factory.cs` (by file name), and the unused `uid` property is removed.
* Base Classes and Interfaces list the base classes and interfaces declared on the plugin's own device classes (factories and join maps are excluded). Each has its own section; Interfaces is empty when no device class declares one.
* Sections the generator gets wrong (for example placeholder config values or a join map it can't find) can be edited by hand. Add `<!-- SKIP -->` on the line after the `<!-- START name -->` marker and later runs leave that section alone.
* To hide a section that doesn't apply, keep its markers with only `<!-- SKIP -->` between them. Deleting the markers doesn't work; the generator adds them back.

## License

Provided under the MIT license; see [LICENSE.md](LICENSE.md).

## Plugin Documentation

The sections below are generated; see [README Docs](#readme-docs-generated-plugin-documentation).

<!-- START Minimum Essentials Framework Versions -->
<!-- SKIP -->
### Minimum Essentials Framework Versions

- 2.12.1
<!-- END Minimum Essentials Framework Versions -->

<!-- START Config Example -->
<!-- SKIP -->
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
<!-- SKIP -->
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

#### Serials

| Join | Type (RW) | Description |
| --- | --- | --- |
| 1 | R | Device Name |
<!-- END Join Maps -->

<!-- START Interfaces Implemented -->
<!-- END Interfaces Implemented -->

<!-- START Base Classes -->
### Base Classes

- `CrestronGenericBridgeableBaseDevice`
- `EssentialsBridgeableDevice`
<!-- END Base Classes -->

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
<!-- SKIP -->
<!-- END String Feedbacks -->
