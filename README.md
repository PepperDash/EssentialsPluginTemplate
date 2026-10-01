![PepperDash Essentials Plugin Logo](/images/essentials-plugin-blue.png)

![PepperDash Essentials](https://img.shields.io/badge/PepperDash%20Essentials-≥%20v2.42.4-blue)
![.NET](https://img.shields.io/badge/.NET%20Framework-4.7.2-512BD4)
![Crestron](https://img.shields.io/badge/Crestron-4--Series-lightgrey)
![License](https://img.shields.io/badge/license-MIT-green)

# Essentials Plugin Template (c) 2026

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
4. Set the `PepperDashEssentials` package version in the csproj to `MinimumEssentialsFrameworkVersion` or higher (the build fails if it is lower; see [README Badges](#readme-badges)).
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

### README Badges

The `.NET` and `PepperDash Essentials` badges at the top of this README are kept in sync by an MSBuild target in `src/Directory.Build.targets`, so no extra tools are needed:

* `.NET` badge - `TargetFramework` of the plugin project
* `PepperDash Essentials` badge - the highest `MinimumEssentialsFrameworkVersion = "x.y.z";` assignment in the project's C# files (subfolders included), so factories can be renamed freely (for example `SonyBraviaDeviceFactory`)

Every local `dotnet build` (or Visual Studio build) rewrites the badges when they are stale; commit the `README.md` change with your code. The `PepperDashEssentials` `PackageReference` version in the csproj must be equal to or greater than `MinimumEssentialsFrameworkVersion` (a prerelease such as `2.13.0-beta` counts as lower than `2.13.0`); otherwise the build fails with `PDREADME002`.

In CI (`CI=true`) the target only checks: a stale badge fails the build with `PDREADME004`, so a release is never packaged with an out-of-date README.

* Check locally without modifying files: `dotnet build -p:ReadmeMode=Check`
* Skip the target: `dotnet build -p:SkipReadmeBadges=true`

Keep a `MinimumEssentialsFrameworkVersion = "x.y.z";` assignment in at least one factory, and keep the two badges in this README. Renaming the project or factory classes does not require changes.

### Git Hooks (Husky.Net)

This repo uses [Husky.Net](https://alirezanet.github.io/Husky.Net/) for one `commit-msg` hook. It runs `.husky/csx/commit-lint.csx`, which checks the message against [Conventional Commits](https://www.conventionalcommits.org/) using the type list from the shared `checkCommitMessage` workflow in PepperDash/workflow-templates (this template's CI does not currently run that check):

* Header: `<type>(<optional scope>): <subject>`, at most 100 characters
* Types: `feat`, `fix`, `chore`, `docs`, `style`, `refactor`, `perf`, `test`, `build`, `ci`, `revert`, `wip`
* A blank line between the header and the body
* Merge, `Revert "..."`, `fixup!`, `squash!` and `amend!` messages are allowed locally (squash `fixup!` commits before pushing)
* Breaking changes use a `BREAKING CHANGE:` footer; the `feat!:` form is not recognized by the release tooling

The only requirement is the [.NET SDK](https://dotnet.microsoft.com/download).

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
* Skip the hook for a single commit: `git commit --no-verify`
* Skip the automatic install on restore: set the `HUSKY=0` environment variable. It is also skipped when `CI=true`.

### README Docs (Generated Plugin Documentation)

The plugin's config example, supported types, join maps, feedbacks and public methods are generated into `<!-- START name -->` / `<!-- END name -->` sections under [Plugin Documentation](#plugin-documentation) at the end of this README. Generation is an MSBuild target (`UpdateReadmeDocs` in `src/Directory.Build.targets`, implemented in `build/ReadmeDocs.cs`) that you run on your branch, review, and commit with your changes. It needs only the .NET SDK and runs offline.

In CI (`CI=true`) the build regenerates the sections in memory and fails with `PDREADME005` if they differ from the committed README, so a release is never packaged with out-of-date docs. Skip that check with `-p:SkipReadmeDocsCheck=true`.

#### Running the Update

| Where | How |
| --- | --- |
| VS Code | `Terminal > Run Task...` > **Update README docs** |
| Terminal (repo root) | `dotnet msbuild -t:UpdateReadmeDocs` |
| Check only, no changes | `dotnet build -p:ReadmeMode=Check` |
| Copilot Chat | `/update-readme-docs` - runs the target, then reviews the generated sections against `src/` and cleans them up |

Then review `git diff README.md` and commit the result with your changes.

The generator is a C# port of `metadata.py` from [PepperDash/workflow-templates](https://github.com/PepperDash/workflow-templates) (used by the older `update-readme` workflow) and produces the same sections, with these differences: the output order no longer depends on the file system, each Minimum Essentials Framework Version is listed once, a join map is also found when its file is not named after the class, interface names are matched case-sensitively, and Base Classes are listed before Interfaces as plain items.

#### Controlling the Output

* The Config Example `type` is the first `TypeNames` entry of the first C# file (by file name) that sets `TypeNames`, and the unused `uid` property is removed. Factories and join maps are found by their content, not their file or class names, so renaming them (for example to `SonyBraviaDeviceFactory`) needs no changes here. The config class is the class whose name ends in `Config` or `ConfigObject` with the most properties.
* Base Classes and Interfaces list the base classes and interfaces declared on the plugin's own device classes (factories and join maps are excluded). Each has its own section; Interfaces is empty when no device class declares one.
* Sections the generator gets wrong (for example placeholder config values or a join map it can't find) can be edited by hand. Add `<!-- SKIP -->` on the line after the `<!-- START name -->` marker and later runs leave that section alone.
* To hide a section that doesn't apply, keep its markers with only `<!-- SKIP -->` between them. Deleting the markers doesn't work; the generator adds them back.

## License

Provided under the MIT license; see [LICENSE.md](LICENSE.md).

## Plugin Documentation

The sections below are generated; see [README Docs](#readme-docs-generated-plugin-documentation).

<!-- START Minimum Essentials Framework Versions -->
### Minimum Essentials Framework Versions

- 2.42.4
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
<!-- SKIP -->
<!-- END String Feedbacks -->
