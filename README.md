![PepperDash Essentials Pluign Logo](/images/essentials-plugin-blue.png)

![PepperDash Essentials](https://img.shields.io/badge/PepperDash%20Essentials-≥%20v3.0.0-blue)
![.NET](https://img.shields.io/badge/.NET-8-512BD4)
![Crestron](https://img.shields.io/badge/Crestron-4--Series-lightgrey)
![License](https://img.shields.io/badge/license-MIT-green)

# Essentials Plugin Template (c) 2025

## License

Provided under MIT license

## Overview

Fork this repo when creating a new plugin for Essentials. For more information about plugins, refer to the Essentials Wiki [Plugins](https://pepperdash.github.io/Essentials/docs/Plugins.html) article.

This repo contains example classes for the three main categories of devices:
* `MakeModelDevice`: Used for most third party devices which require communication over a streaming mechanism such as a Com port, TCP/SSh/UDP socket, CEC, etc
* `MakeModelLogicDevice`:  Used for devices that contain logic, but don't require any communication with third parties outside the program
* `MakeModelCrestronDevice`:  Used for devices that represent a piece of Crestron hardware

There are matching factory classes for each of the three categories of devices.  The `MakeModelConfigObject` should be used as a template and modified for any of the categories of device.  Same goes for the `MakeModeleBridgeJoinMap`.

This also illustrates how a plugin can contain multiple devices.

## Cloning Instructions

After forking this repository into your own GitHub space, you can create a new repository using this one as the template.  Then you must install the necessary dependencies as indicated below.

## Dependencies

* Install the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) and PowerShell 7+ (`pwsh`). `global.json` selects an installed stable .NET 8 SDK, rolling forward to its latest installed feature band/patch.
* The [Essentials](https://github.com/PepperDash/Essentials) dependency is pinned directly in `src/epi-make-model.4Series.csproj` to **3.0.0-rc.19**, a prerelease, available from [nuget.org](https://www.nuget.org/packages/PepperDashEssentials/3.0.0-rc.19). No authenticated feed is required.
* The project uses literal `net8`, matching the [release-tagged Essentials v3 project](https://github.com/PepperDash/Essentials/blob/v3.0.0-rc.19/src/PepperDash.Essentials/PepperDash.Essentials.csproj). The .NET 8 SDK resolves this to .NET 8; NuGet uses the canonical `net8.0` asset folder. Keep the `.4Series` solution/project names for existing automation.
* Deployment requires a .NET 8-capable Crestron 4-Series/VC-4 environment running the matching Essentials v3 runtime. A successful build does not verify firmware compatibility or plugin loading on hardware.

### Installing Dependencies

Run from the repository root; `dotnet restore` installs the pinned dependencies:

```powershell
dotnet restore epi-make-model.4Series.sln
dotnet build epi-make-model.4Series.sln --no-restore --configuration Debug
dotnet pack src/epi-make-model.4Series.csproj --no-build --configuration Debug
dotnet build epi-make-model.4Series.sln --no-restore --configuration Release
dotnet pack src/epi-make-model.4Series.csproj --no-build --configuration Release
pwsh -NoProfile -File .github/scripts/Test-ReadmeBadges.ps1
pwsh -NoProfile -File .github/scripts/Update-ReadmeBadges.ps1 -Check
```

Builds generate a CPLZ and NuGet package in `output/`. The CPLZ includes the plugin output, not host-provided Essentials/Crestron dependencies (`ExcludeAssets=runtime`). Inspect both archives before deployment. `SERIES4` is defined for both configurations.

The local `Validate plugin` workflow runs badge tests/checks and restore/build/pack in Debug and Release with the .NET 8 SDK, without publishing or requiring a semantic-release version. Existing release automation remains separate.

### RC-to-GA follow-up

All three factories use the numeric runtime minimum `3.0.0`, not a prerelease string. The [v3 runtime gate](https://github.com/PepperDash/Essentials/blob/v3.0.0-rc.19/src/PepperDash.Essentials.Core/Global/Global.cs#L233-L270) strips the host version to major/minor/build and parses the factory minimum as `System.Version`: RC19 therefore compares as `3.0.0`. This excludes v2 but cannot distinguish individual v3 RCs. The exact build dependency remains pinned to `3.0.0-rc.19`; verify deployment against that runtime rather than assuming every RC is compatible.

This is an initial prerelease migration, not a promise of automatic updates:

* [ ] When upstream CI publishes the official Essentials v3 NuGet package, verify its availability on nuget.org, then replace the pinned `3.0.0-rc.19` dependency with `3.0.0` (or a verified newer official v3 release).
* [ ] Restore, build both configurations, run badge regression tests and `-Check`, and pack using the commands above. Regenerate badges if the numeric runtime minimum changes.
* [ ] Inspect CPLZ/NuGet contents for accidental host dependencies and verify plugin loading, all three example device categories, communications, and bridge feedback on the intended Essentials runtime/hardware before deployment.

### Instructions for Renaming Solution and Files

See the Task List in Visual Studio for a guide on how to start using the template.  There is extensive inline documentation and examples as well.

For renaming instructions in particular, see the XML `remarks` tags on class definitions

## Git Hooks (Husky.Net)

This repo uses [Husky.Net](https://alirezanet.github.io/Husky.Net/) to run a `pre-commit` hook. The hook runs `.github/scripts/Update-ReadmeBadges.ps1`, which keeps the `.NET` and `PepperDash Essentials` badges at the top of this README in sync with the project:

* `.NET` badge - `TargetFramework` in `src/*.4Series.csproj`
* `PepperDash Essentials` badge - the highest `MinimumEssentialsFrameworkVersion` in `src/*Factory.cs`

If the badges change, `README.md` is re-staged automatically so the update is part of your commit. A warning is printed if the `PepperDashEssentials` package version in the csproj differs from `MinimumEssentialsFrameworkVersion`. With this RC migration, the literal `3.0.0-rc.19` package versus numeric `3.0.0` runtime minimum intentionally triggers that warning; it does not make `-Check` fail when badges are current.

### Requirements

* [.NET SDK](https://dotnet.microsoft.com/download) (provides `dotnet tool`)
* [PowerShell 7+ (`pwsh`)](https://learn.microsoft.com/powershell/scripting/install/installing-powershell) on the `PATH` (Windows, macOS and Linux). Windows PowerShell 5.1 is not used.

### Setup

The hook is installed automatically the first time you do any of the following in a fresh clone:

* Open the solution in Visual Studio, or run `dotnet restore` / `dotnet build`
* Open the folder in VS Code and allow the "Install git hooks" automatic task

To install it manually:

```
dotnet tool restore
dotnet husky install
```

Verify with `git config core.hooksPath`, which should print `.husky`.

### Usage

* Commit as usual; the hook runs on every `git commit`.
* Run it on demand: `dotnet husky run`
* Check without modifying files (useful for CI): `pwsh -NoProfile -File .github/scripts/Update-ReadmeBadges.ps1 -Check`
* The hook refuses to run if `README.md`, the `.4Series.csproj`, or a `*Factory.cs` file has unstaged or untracked changes, so a partial commit never picks up unrelated edits. Stage or stash them and commit again.
* Skip the hook for a single commit: `git commit --no-verify`
* Skip the automatic install on restore: set the `HUSKY=0` environment variable. It is also skipped when `CI=true`.

### Renaming

The script locates files by pattern (`*.4Series.csproj`, `*Factory.cs`), so renaming the project or factory classes does not require changes. Keep a `MinimumEssentialsFrameworkVersion = "x.y.z";` assignment in at least one factory, and keep the two badges in this README.

## Build Instructions (PepperDash Internal) 

## Generating Nuget Package

A nuget package is automatically generated when the plugin is build. To modify the name and other details of the package, edit the following properties in the .csproj file:

1. `PackageId` - This is the name that will be used to pull the package from Nuget once it's published
2. `PackgeProjectUrl` - This should match the URL for the plugin repo
3. `AssemblyTitle` - This is the dll file name that is will show on a processor when the plugin is loaded