/// Conventional Commits linter for the commit-msg hook (run by Husky.Net: dotnet husky exec).
/// The type list follows CONVENTIONAL_REGEX in PepperDash/workflow-templates
/// .github/workflows/essentialsplugins-checkCommitMessage.yml; keep them in sync. Local-only
/// allowances: Revert "...", fixup!/squash!/amend! and "Merge pull request" headers.
/// Breaking changes use a "BREAKING CHANGE:" footer; "type!:" is rejected because the default
/// (angular) semantic-release preset does not recognize it.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

var types = "feat|fix|chore|docs|style|refactor|perf|test|build|ci|revert|wip";
var conventional = new Regex(@"^(" + types + @")(\([^()\r\n]+\))?: \S");
var merge = new Regex(@"^Merge (branch|remote-tracking branch|commit|pull request) .+");
var gitRevert = new Regex(@"^Revert "".+""$");
var autosquash = new Regex(@"^(fixup|squash|amend)! ");
const int MaxHeader = 100;

// git strips '#' comment lines and leading blank lines before storing the message
// (Husky's script host has no System.Linq reference, hence the loop)
var lines = new List<string>();
foreach (var line in File.ReadAllLines(Args[0]))
{
    if (line.StartsWith("#")) continue;
    if (lines.Count == 0 && string.IsNullOrWhiteSpace(line)) continue;
    lines.Add(line);
}

var errors = new List<string>();
var header = lines.Count > 0 ? lines[0] : "";

if (header.Length == 0)
{
    errors.Add("commit message is empty");
}
else if (!merge.IsMatch(header) && !gitRevert.IsMatch(header) && !autosquash.IsMatch(header))
{
    if (!conventional.IsMatch(header))
        errors.Add("header must be '<type>(<optional scope>): <subject>' with type one of: " + types.Replace("|", ", "));
    if (header.Length > MaxHeader)
        errors.Add($"header is {header.Length} characters; the limit is {MaxHeader}");
    if (lines.Count > 1 && !string.IsNullOrWhiteSpace(lines[1]))
        errors.Add("leave a blank line between the header and the body");
}

if (errors.Count == 0)
    return 0;

Console.ForegroundColor = ConsoleColor.Red;
Console.WriteLine("commit-lint: invalid commit message");
Console.ResetColor();
Console.WriteLine("  " + header);
foreach (var e in errors)
    Console.WriteLine("  - " + e);
Console.WriteLine("  e.g. 'feat(display): add input routing' or 'fix: handle null config'");
return 1;
