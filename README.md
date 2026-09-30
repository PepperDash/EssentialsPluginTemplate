![PepperDash Essentials Pluign Logo](/images/essentials-plugin-blue.png)

![PepperDash Essentials](https://img.shields.io/badge/PepperDash%20Essentials-≥%20v2.12.1-blue)
![.NET](https://img.shields.io/badge/.NET%20Framework-4.7.2-512BD4)
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

The [Essentials](https://github.com/PepperDash/Essentials) libraries are required. They referenced via nuget. You must have nuget.exe installed and in the `PATH` environment variable to use the following command. Nuget.exe is available at [nuget.org](https://dist.nuget.org/win-x86-commandline/latest/nuget.exe).

### Installing Dependencies

Dependencies will be automatically installed when

### Instructions for Renaming Solution and Files

See the Task List in Visual Studio for a guide on how to start using the template.  There is extensive inline documentation and examples as well.

For renaming instructions in particular, see the XML `remarks` tags on class definitions

## Git Hooks (Husky.Net)

This repo uses [Husky.Net](https://alirezanet.github.io/Husky.Net/) to run a `pre-commit` hook. The hook runs `.github/scripts/Update-ReadmeBadges.ps1`, which keeps the `.NET` and `PepperDash Essentials` badges at the top of this README in sync with the project:

* `.NET` badge - `TargetFramework` in `src/*.4Series.csproj`
* `PepperDash Essentials` badge - the highest `MinimumEssentialsFrameworkVersion` in `src/*Factory.cs`

If the badges change, `README.md` is re-staged automatically so the update is part of your commit. A warning is printed if the `PepperDashEssentials` package version in the csproj differs from `MinimumEssentialsFrameworkVersion`.

### Requirements

* [.NET SDK](https://dotnet.microsoft.com/download) (provides `dotnet tool`)
* Windows: the built-in Windows PowerShell 5.1 is used. macOS/Linux: [PowerShell 7 (`pwsh`)](https://learn.microsoft.com/powershell/scripting/install/installing-powershell) must be on the `PATH`.

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
* Check without modifying files (useful for CI): `pwsh .github/scripts/Update-ReadmeBadges.ps1 -Check`
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