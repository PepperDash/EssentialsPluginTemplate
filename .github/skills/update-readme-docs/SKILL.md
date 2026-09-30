---
name: update-readme-docs
description: 'Update the plugin documentation sections in README.md (config example, supported types, join maps, feedbacks, methods). Use when: documenting a plugin, refreshing README after code changes, cleaning up generated README sections.'
---

# Update README plugin docs

Regenerates the marker-delimited documentation sections in `README.md` on the current branch, then reviews them.

## Procedure

1. Run `pwsh -NoProfile -File .github/scripts/Update-ReadmeDocs.ps1` (Windows: `powershell -NoProfile -ExecutionPolicy Bypass -File ...`). Requires Python 3. Do not switch branches.
2. Read `git diff README.md` and compare each generated section against the source in `src/`:
   - **Minimum Essentials Framework Versions**: collapse duplicates to one entry per distinct value and mark `<!-- SKIP -->` (the generator re-adds duplicates otherwise).
   - **Supported Types**: must match every `TypeNames` entry in the `*Factory.cs` files.
   - **Config Example**: `type` is set by the script to the first `TypeNames` entry of the first `*Factory.cs` (by file name); do not change it by hand. The script also removes the unused `uid` property. Must use the real config class(es), JSON property names and value types (nested objects such as `control` are emitted as `"SampleValue"`). Replace `SampleString`/`SampleValue`/`GeneratedKey` placeholders with realistic values, using the `<!-- SKIP -->` marker (see below). Take example values from the `<example>` blocks in `MakeModelConfigObject.cs`.
   - **Join Maps**: if empty, the generator could not find the join map file (it looks for `<ClassName>.cs`). Write the table by hand from the `JoinDataComplete` definitions and mark the section `<!-- SKIP -->`.
   - **Base Classes / Interfaces**: the script lists the base classes (Base Classes) and interfaces (Interfaces) declared by the plugin's device classes, excluding factories and join maps. Verify against `src/`; do not edit by hand unless you add `<!-- SKIP -->`. Interfaces uses the generator's `Interfaces Implemented` markers and is empty when there are none.
   - **Public Methods / Feedbacks**: remove noise (non-public API, base classes of factories, template-only members) by hand and mark `<!-- SKIP -->`; for a section that does not apply, keep the markers with only `<!-- SKIP -->` between them (deleting the markers does not work, the generator re-adds them).
3. Ensure a blank line precedes each `<!-- START ... -->` marker and the file ends with a newline.
4. Do not edit anything outside the `<!-- START -->`/`<!-- END -->` blocks.
5. Report which sections were regenerated, hand-edited (now `<!-- SKIP -->`), or removed. Do not commit unless asked.

## Rules

- `<!-- SKIP -->` inside a section makes the generator leave it untouched on later runs. Use it only for sections you curated by hand, and say so in the summary.
- Never fabricate joins, feedbacks, or config properties; derive everything from `src/`.
