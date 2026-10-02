# Plan: Consolidate the README generators into one package

**Status:** proposal, not started · **Date:** 2026-10-02 · **Related:** config schema generator (`pd-schemagen` in [PepperDash/schemaTest](https://github.com/PepperDash/schemaTest))

## Summary

PepperDash keeps two separate README generators for Essentials plugins:

| Generator | Where | Runs |
| --- | --- | --- |
| `build/ReadmeDocs.cs` (C#) | This template | Locally in MSBuild (`UpdateReadmeDocs`), and as a check in the template's release build |
| `.github/scripts/metadata.py` (Python) | [PepperDash/workflow-templates](https://github.com/PepperDash/workflow-templates) | In `update-readme.yml`, which writes the result to a `robot-docs` branch |

They have already drifted apart (see [Output differences](#output-differences)).

This plan makes **`ReadmeDocs.cs` the only README generator**. It moves to its own repo and is published to nuget.org in two forms:
- an **MSBuild package** for plugin builds;
- a **dotnet tool** for the shared workflow.

Afterwards the template's copy is gone and `metadata.py` is deleted. As with the config schema work, we start with the **README only**. Other build logic (CPLZ packaging, git hooks, config schema targets) can move into the package later.

## Goals

- **One source file** for README generation, maintained in one repo and released with a version number.
- **The same output** whether the README is generated locally or by the `update-readme` workflow.
- **No changes in caller repos.** About 100 PepperDash repos call `update-readme.yml`; the code search caps at 100 results, so there may be more. Its `workflow_call` interface stays exactly as it is.
- **Plugins adopt it with one reference:** a single `PackageReference` replaces copying targets and generator files.

## Non-goals

- Moving the CPLZ, git-hook or config-schema targets. That's a follow-up.
- Changing what the README sections contain, beyond the reviewed differences in Phase 0.
- Changing the `robot-docs` flow in `update-readme.yml`, which only commits to a branch and is reviewed before merging.

## Current state

- **`build/ReadmeDocs.cs`:** about 830 lines, compiled by `RoslynCodeTaskFactory` on every build. It must stay compatible with netstandard2.0 / C# 7.3 so Visual Studio's MSBuild can load it.
- **`SyncReadmeBadges`:** an inline task in `src/Directory.Build.targets`, about 130 lines, that keeps the `.NET` and `PepperDash Essentials` badges current.
- **The README block in `src/Directory.Build.targets`:**
  - targets `UpdateReadmeBadges`, `CheckReadmeBadges`, `UpdateReadmeDocs` and `CheckReadmeDocs`;
  - properties `ReadmeMode`, `ReadmePath`, `SkipReadmeBadges` and `SkipReadmeDocsCheck`;
  - error codes `PDREADME001`–`PDREADME005`.
- **`update-readme.yml` (workflow-templates `main`):**
  - checks out the caller repo and workflow-templates `@main`;
  - sets up Python and runs `metadata.py` against the repo;
  - force-pushes `README.md` to the `robot-docs` branch.
- **Callers:** each plugin has `.github/workflows/essentialsplugins-updatereadme-caller.yml`, and the org starter workflow is `PepperDash/.github/workflow-templates/EssentialsPlugins-UpdateReadme-Caller.yml`. They call `@main`, so any change to `update-readme.yml` reaches every caller immediately.
- **The #49 branch:** `refactor/workflow-modularization-49` in workflow-templates also changes `update-readme.yml`.

### Output differences

`ReadmeDocs.cs` is a port of `metadata.py` (commit `681f68e`) and differs from it in these ways, as recorded in its header:

1. **Deterministic order.** Files are read in sorted folder order. `metadata.py` used `os.walk` order and an unordered set.
2. **Minimum Essentials Framework Versions:** each distinct version is listed once, including several versions in one file.
3. **The Config Example is always regenerated, even with `<!-- SKIP -->`.**
   - Values come from `<example><code>` blocks.
   - Realistic `key`, `name` and `group` values.
   - The `type` is the first `TypeNames` entry, and `uid` is removed.
4. **Join access** (`R`, `W` or `R/W`) follows `JoinCapabilities`. `metadata.py` lists every join as `R`.
5. **A join map is found by its class declaration,** even when the file isn't named after the class.
6. **Factories are recognized by their Essentials base class,** whatever their name.
7. **Interfaces are matched case-sensitively.**
8. **Base Classes are listed before Interfaces,** as plain list items.

Most of these are fixes. **Item 3 is the risky one:** a repo with a hand-written Config Example under `<!-- SKIP -->` would lose it.

## Architecture

```
essentials-readme-docs/                         (new repo)
├── src/ReadmeDocs.Core/       netstandard2.0   ReadmeDocs.cs (moved unchanged) + badge logic
├── src/ReadmeDocs.Tasks/      netstandard2.0   MSBuild tasks: GenerateReadmeDocs, SyncReadmeBadges
│   └── build/PepperDash.Essentials.ReadmeDocs.targets   ← README block from src/Directory.Build.targets
├── src/ReadmeDocs.Tool/       net8.0           pd-readmedocs CLI (PackAsTool)
├── tests/                                      golden files, parity, package tests
└── .github/workflows/readmedocs.yml            PR: test (ubuntu + windows); tag readmedocs-v*: publish
```

| Package (nuget.org) | Used by | What it is |
| --- | --- | --- |
| `PepperDash.Essentials.ReadmeDocs` | Plugin builds | An MSBuild task package. Plugins reference it with `PrivateAssets="all"`. The targets come with the package and run in-process, so nothing extra starts on each build. |
| `PepperDash.Essentials.ReadmeDocs.Tool` (`pd-readmedocs`) | `update-readme.yml` | A dotnet tool: `pd-readmedocs <repoRoot> [--check]` rewrites or checks `README.md` |

Both packages contain the same compiled `ReadmeDocs.Core`, built from one source.

**Why an MSBuild package rather than only a dotnet tool, as `pd-schemagen` is:**
- The badges are refreshed before every local build. A tool would start a process on every build.
- The generator reads MSBuild items (`@(Compile)`, the `PepperDashEssentials` version) directly.
- A package carries the targets with it, so plugins don't copy them.
- The task is compiled once into the package instead of by Roslyn on every build.

## Phases

### Phase 0: Decisions (before any code)

| # | Decision | Recommendation |
| --- | --- | --- |
| D1 | Repo and package names | `PepperDash/essentials-readme-docs`; `PepperDash.Essentials.ReadmeDocs` and `.Tool` |
| D2 | License, and nuget.org key scope | Confirm the license. Confirm the org `NUGET_API_KEY` can push new package IDs. These are the same open questions as for `pd-schemagen`. |
| D3 | **Config Example and `<!-- SKIP -->`** | Make it a setting, not a fork of the code. The workflow tool keeps the old behavior and **leaves a skipped Config Example alone**. The template's MSBuild path keeps regenerating it. Revisit once callers use `<example>` blocks. |
| D4 | How the tool finds source files when it isn't run by MSBuild | Find the `ProgramLibrary` csproj and read `*.cs` under it, excluding `bin`, `obj` and test projects. Confirm with the parity tests. |
| D5 | Should the tool update badges? | No. The workflow never did, and badges belong to the build. |

### Phase 1: New repo (about 3–4 days)

1. **Create the repo and move the code.**
   - Move `build/ReadmeDocs.cs` into `ReadmeDocs.Core`, unchanged apart from namespace and wrapping.
   - Turn the inline `SyncReadmeBadges` into a compiled class.
   - Keep netstandard2.0 / C# 7.3.
2. **`ReadmeDocs.Tasks`.**
   - Port the README block from `src/Directory.Build.targets` into `build/*.targets`, keeping the target names, properties, defaults (`Check` mode when `CI=true`) and error codes.
   - Pack the task DLL under `tasks/`, with `BuildOutputInPackage=false` and `DevelopmentDependency=true`.
3. **`ReadmeDocs.Tool`.**
   - Command line: `pd-readmedocs <repoRoot> [--check] [--readme <path>] [--regenerate-skipped-config-example]`.
   - Exit codes: 0 for success, 5 when the README is stale (the same meaning as `PDREADME005`), and 1 for errors.
4. **Tests.**
   - **Golden files:** the template's sources and README produce the committed README byte for byte.
   - **Parity:**
     - Run `metadata.py` and `pd-readmedocs` against about 10 real plugins, chosen to cover: net472 and net8; with and without `<!-- SKIP -->`; several join maps; join maps not named after their file; factories with custom names.
     - Every difference must map to an item in [Output differences](#output-differences), or it's a bug.
     - Commit the reviewed difference report.
   - **Packages:**
     - A sample net472 plugin and a sample net8 plugin build through the packed `.nupkg`, in `Update` and `Check` mode.
     - Check the error codes on purposely broken inputs.
     - Do one **manual check in Visual Studio**, because that's where the netstandard2.0 rule matters.
5. **CI and release.**
   - `readmedocs.yml` runs the tests on Ubuntu and Windows.
   - A `readmedocs-v*` tag packs both packages and pushes them to nuget.org with `--skip-duplicate`.
6. **Docs.** Move the template README's "Build targets", "Build properties", "Error codes" and "The documentation generator" content into the package README, and link to it from the template.

### Phase 2: Switch the template (about 1 day)

1. Delete `build/ReadmeDocs.cs`, the inline `SyncReadmeBadges` task and the README block in `src/Directory.Build.targets`.
2. Add `<PackageReference Include="PepperDash.Essentials.ReadmeDocs" Version="x.y.z" PrivateAssets="all" />`, in `src/Directory.Build.props` or the csproj.
3. Keep the "Update README docs" VS Code task and `.github/skills/update-readme-docs/SKILL.md`. The target names don't change.
4. Update the README's Reference and Explanation sections: the generator is now a package, and "The two implementations are maintained separately" no longer applies.
5. **Acceptance:** `README.md` is byte-identical before and after, in Debug, Release and `CI=true`. `PDREADME004` and `PDREADME005` still fire on stale input.

### Phase 3: workflow-templates (about half a day; one small PR)

This is the only workflow change, and it's needed because the second generator lives in workflow-templates.

1. In `update-readme.yml`, replace "Set up Python", "Install Python Dependencies" and "Run README Update Script" with these steps:
   ```yaml
   - uses: actions/setup-dotnet@v5
     with:
       dotnet-version: '8.0.x'
   - name: Run README Update Script
     working-directory: repo
     run: |
       dotnet tool install --global PepperDash.Essentials.ReadmeDocs.Tool --version x.y.z
       pd-readmedocs .
   ```
   - **Pin the tool version.** Output then changes only when someone deliberately bumps it.
   - **Leave the rest as it is:** the `workflow_call` inputs, the change detection and the `robot-docs` push.
2. Delete `.github/scripts/metadata.py`. The "Checkout Workflow Repository" step stays only if another step still needs it.
3. Update `docs/workflow-details.md` and `docs/workflows.md`.
4. Coordinate with #49: whichever lands second rebases its `update-readme.yml` changes.
5. **Merge only after the Phase 1 parity report is signed off.** Every caller that uses `@main` picks this up at once.

### Phase 4: Rollout (spot checks over about a week)

1. After Phase 3 merges, each caller's next `update-readme` run updates its `robot-docs` branch. Nothing reaches `main` without review.
2. Spot-check about 10 `robot-docs` diffs. Fix any regressions in the tool, release a patch, and bump the pin.
3. Plugins can add the MSBuild package for local generation and checks, one plugin at a time, separately from the workflow.

## Risks

| Risk | Mitigation |
| --- | --- |
| One change in output hits about 100 repos at once (`robot-docs` churn) | Parity report before Phase 3; the D3 setting keeps skipped Config Examples; a pinned tool version; `robot-docs` is reviewed before merging |
| Hand-written Config Examples are overwritten | D3: the workflow leaves skipped Config Examples alone by default |
| The package doesn't load in Visual Studio's MSBuild | netstandard2.0 task DLL; manual Visual Studio check in Phase 1 |
| `update-readme.yml` callers pin `@main` | Merge Phase 3 only after sign-off; tool version pinned inside the workflow |
| Conflicts with the #49 branch | Coordinate the order; the Phase 3 diff is small |
| The first restore needs nuget.org | CI already restores from nuget.org; local builds restore on the first build |
| nuget.org versions can't be deleted (only unlisted) | Release `0.x` versions first, and tag only after tests pass on both operating systems |

## Effort

| Phase | Estimate |
| --- | --- |
| 0: Decisions | One review meeting |
| 1: New repo, including parity tests | 3–4 days |
| 2: Template switch | About 1 day |
| 3: workflow-templates PR | About half a day |
| 4: Rollout spot checks | Spread over about a week |

## Follow-ups

- Move the CPLZ, git-hook and config-schema targets into the same package (a shared "plugin build" package), so a plugin's `Directory.Build.targets` becomes nearly empty.
- Once callers use `<example>` blocks, change the workflow default (D3) to always regenerate the Config Example.
