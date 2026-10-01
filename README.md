![PepperDash Essentials Pluign Logo](/images/essentials-plugin-blue.png)

![PepperDash Essentials](https://img.shields.io/badge/PepperDash%20Essentials-≥%20v2.42.4-blue)
![.NET](https://img.shields.io/badge/.NET%20Framework-4.7.2-512BD4)
![Crestron](https://img.shields.io/badge/Crestron-4--Series-lightgrey)
![License](https://img.shields.io/badge/license-MIT-green)

# Essentials Plugin Template (c) 2026

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

## README Badges

The `.NET` and `PepperDash Essentials` badges at the top of this README are kept in sync by an MSBuild target in `src/Directory.Build.targets`, so no extra tools are needed:

* `.NET` badge - `TargetFramework` of the plugin project
* `PepperDash Essentials` badge - the highest `MinimumEssentialsFrameworkVersion` in the project's `*Factory.cs` files (subfolders included)

Every local `dotnet build` (or Visual Studio build) rewrites the badges when they are stale; commit the `README.md` change with your code. The `PepperDashEssentials` `PackageReference` version in the csproj must be equal to or greater than `MinimumEssentialsFrameworkVersion` (a prerelease such as `2.13.0-beta` counts as lower than `2.13.0`); otherwise the build fails with `PDREADME002`.

In CI (`CI=true`) the target only checks: a stale badge fails the build with `PDREADME004`, so a release is never packaged with an out-of-date README.

* Check locally without modifying files: `dotnet build -p:ReadmeMode=Check`
* Skip the target: `dotnet build -p:SkipReadmeBadges=true`

Keep a `MinimumEssentialsFrameworkVersion = "x.y.z";` assignment in at least one factory, and keep the two badges in this README. Renaming the project or factory classes does not require changes.

## Git Hooks (Husky.Net)

This repo uses [Husky.Net](https://alirezanet.github.io/Husky.Net/) for one `commit-msg` hook. It runs `.husky/csx/commit-lint.csx`, which checks the message against [Conventional Commits](https://www.conventionalcommits.org/) using the type list from the shared `checkCommitMessage` workflow in PepperDash/workflow-templates (this template's CI does not currently run that check):

* Header: `<type>(<optional scope>): <subject>`, at most 100 characters
* Types: `feat`, `fix`, `chore`, `docs`, `style`, `refactor`, `perf`, `test`, `build`, `ci`, `revert`, `wip`
* A blank line between the header and the body
* Merge, `Revert "..."`, `fixup!`, `squash!` and `amend!` messages are allowed locally (squash `fixup!` commits before pushing)
* Breaking changes use a `BREAKING CHANGE:` footer; the `feat!:` form is not recognized by the release tooling

The only requirement is the [.NET SDK](https://dotnet.microsoft.com/download).

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
* Skip the hook for a single commit: `git commit --no-verify`
* Skip the automatic install on restore: set the `HUSKY=0` environment variable. It is also skipped when `CI=true`.

## Build Instructions (PepperDash Internal) 

## Generating Nuget Package

A nuget package is automatically generated when the plugin is build. To modify the name and other details of the package, edit the following properties in the .csproj file:

1. `PackageId` - This is the name that will be used to pull the package from Nuget once it's published
2. `PackgeProjectUrl` - This should match the URL for the plugin repo
3. `AssemblyTitle` - This is the dll file name that is will show on a processor when the plugin is loaded