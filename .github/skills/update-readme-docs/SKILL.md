---
name: update-readme-docs
description: 'Update the plugin documentation sections in README.md (config example, supported types, join maps, feedbacks, methods). Use when: documenting a plugin, refreshing README after code changes, cleaning up generated README sections.'
---

# Update README plugin docs

Regenerates the marker-delimited documentation sections in `README.md` on the current branch, then reviews them.

## Procedure

1. From the repo root, run `dotnet msbuild -t:UpdateReadmeDocs` (the `UpdateReadmeDocs` target in `src/Directory.Build.targets`, implemented in `build/ReadmeDocs.cs`). Requires only the .NET SDK. Do not switch branches.
2. Read `git diff README.md` and compare each generated section against the source in `src/`:
   - **Minimum Essentials Framework Versions**: one entry per distinct `MinimumEssentialsFrameworkVersion` in the factories; do not edit by hand or mark it `<!-- SKIP -->`, or it stops following version bumps.
   - **Supported Types**: must match every `TypeNames` entry in the plugin's factory classes (any file name).
   - **Config Example**: `type` is set by the target to the first `TypeNames` entry of the first C# file (by file name) that sets `TypeNames`; do not change it by hand. The target also removes the unused `uid` property. Must use the real config class(es), JSON property names and value types (nested objects such as `control` are emitted as `"SampleValue"`). Replace `SampleString`/`SampleValue`/`GeneratedKey` placeholders with realistic values, using the `<!-- SKIP -->` marker (see below). Take example values from the `<example>` blocks in the config class (`MakeModelPropertiesConfig.cs` in the template).
   - **Join Maps**: generated from classes deriving from `JoinMapBaseAdvanced`, whatever their file name. If a join is missing (for example a `JoinDataComplete` without `JoinNumber` or `JoinType`), write the table by hand from the definitions and mark the section `<!-- SKIP -->`.
   - **Base Classes / Interfaces**: the target lists the base classes (Base Classes) and interfaces (Interfaces) declared by the plugin's device classes, excluding factories and join maps. Verify against `src/`; do not edit by hand unless you add `<!-- SKIP -->`. Interfaces uses the generator's `Interfaces Implemented` markers and is empty when there are none.
   - **Public Methods / Feedbacks**: remove noise (non-public API, base classes of factories, template-only members) by hand and mark `<!-- SKIP -->`; for a section that does not apply, keep the markers with only `<!-- SKIP -->` between them (deleting the markers does not work, the generator re-adds them).
3. Ensure a blank line precedes each `<!-- START ... -->` marker and the file ends with a newline.
4. Do not edit anything outside the `<!-- START -->`/`<!-- END -->` blocks.
5. Report which sections were regenerated, hand-edited (now `<!-- SKIP -->`), or removed. Do not commit unless asked.

## Rules

- `<!-- SKIP -->` inside a section makes the generator leave it untouched on later runs. Use it only for sections you curated by hand, and say so in the summary.
- Never fabricate joins, feedbacks, or config properties; derive everything from `src/`.
