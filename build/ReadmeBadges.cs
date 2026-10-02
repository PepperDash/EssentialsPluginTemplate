// README badge synchronizer, compiled at build time by RoslynCodeTaskFactory
// (see the UpdateReadmeBadges / CheckReadmeBadges targets in src/Directory.Build.targets).
//
// Rewrites the .NET badge from $(TargetFramework) and the PepperDash Essentials badge from the highest
// MinimumEssentialsFrameworkVersion assignment in the project's C# files; in check mode, fails instead.
//
// Must stay compatible with netstandard2.0 / C# 7.3 so it also compiles under Visual Studio's MSBuild.

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

public class SyncReadmeBadges : Task
{
    [Required] public string ReadmePath { get; set; }
    [Required] public string TargetFramework { get; set; }
    public string EssentialsPackageVersion { get; set; }
    public ITaskItem[] SourceFiles { get; set; }
    public bool CheckOnly { get; set; }

    private static readonly Regex MinVersionPattern =
        new Regex(@"(?m)^\s*MinimumEssentialsFrameworkVersion\s*=\s*""(\d+(?:\.\d+)+)""\s*;");

    public override bool Execute()
    {
        var utf8 = new UTF8Encoding(false);

        Version minVersion = null;
        string minText = null;
        foreach (var item in SourceFiles ?? new ITaskItem[0])
        {
            var code = File.ReadAllText(item.GetMetadata("FullPath"), utf8);
            foreach (Match m in MinVersionPattern.Matches(code))
            {
                var v = Version.Parse(m.Groups[1].Value);
                if (minVersion == null || v > minVersion) { minVersion = v; minText = m.Groups[1].Value; }
            }
        }
        if (minVersion == null)
        {
            Error("PDREADME001", "no MinimumEssentialsFrameworkVersion = \"x.y.z\"; assignment found in the project's C# files");
            return false;
        }

        // The factory minimum drives the badge; the referenced package must meet or exceed it
        if (!string.IsNullOrEmpty(EssentialsPackageVersion))
        {
            var pkg = Regex.Match(EssentialsPackageVersion.Trim(), @"^(\d+(?:\.\d+){1,3})(-[^+]+)?(\+.*)?$");
            if (!pkg.Success)
            {
                Log.LogMessage(MessageImportance.Normal,
                    "badges: PepperDashEssentials version '" + EssentialsPackageVersion + "' is not a plain version; skipping the minimum check");
            }
            else
            {
                var pkgVersion = Version.Parse(pkg.Groups[1].Value);
                // SemVer: a prerelease (2.13.0-beta) is lower than its release (2.13.0)
                var below = pkgVersion < minVersion || (pkgVersion == minVersion && pkg.Groups[2].Success);
                if (below)
                {
                    Error("PDREADME002", "PepperDashEssentials package " + EssentialsPackageVersion
                        + " is lower than MinimumEssentialsFrameworkVersion " + minText
                        + "; raise the PackageReference version or lower the factory minimum");
                    return false;
                }
            }
        }

        string fwLabel, fwMessage;
        var legacy = Regex.Match(TargetFramework, @"^net(\d)(\d)(\d)?$");
        var modern = Regex.Match(TargetFramework, @"^net(\d+\.\d+)");
        if (legacy.Success)
        {
            fwLabel = ".NET Framework";
            fwMessage = legacy.Groups[1].Value + "." + legacy.Groups[2].Value
                + (legacy.Groups[3].Success ? "." + legacy.Groups[3].Value : "");
        }
        else if (modern.Success) { fwLabel = ".NET"; fwMessage = modern.Groups[1].Value; }
        else { fwLabel = ".NET"; fwMessage = TargetFramework; }

        var replacements = new[]
        {
            new[] { @"!\[\.NET[^\]]*\]\(https://img\.shields\.io/badge/[^)]*\)",
                    "![.NET](" + BadgeUrl(fwLabel, fwMessage, "512BD4") + ")" },
            new[] { @"!\[PepperDash Essentials\]\(https://img\.shields\.io/badge/[^)]*\)",
                    "![PepperDash Essentials](" + BadgeUrl("PepperDash Essentials", "≥ v" + minText, "blue") + ")" },
        };

        var original = File.ReadAllText(ReadmePath, utf8);
        var updated = original;
        foreach (var r in replacements)
        {
            var pattern = new Regex(r[0]);
            if (!pattern.IsMatch(updated))
            {
                Error("PDREADME003", "badge not found in README.md: " + r[0]);
                return false;
            }
            var value = r[1];
            updated = pattern.Replace(updated, _ => value);
        }

        if (string.Equals(updated, original, StringComparison.Ordinal))
        {
            Log.LogMessage(MessageImportance.Normal, "badges: README.md up to date");
            return true;
        }
        if (CheckOnly)
        {
            Error("PDREADME004", "README.md badges are stale; run 'dotnet build' locally and commit README.md");
            return false;
        }

        File.WriteAllText(ReadmePath, updated, utf8);
        Log.LogMessage(MessageImportance.High,
            "badges: README.md updated (" + fwLabel + " " + fwMessage + ", Essentials >= v" + minText + ")");
        return true;
    }

    private void Error(string code, string message)
    {
        Log.LogError(null, code, null, ReadmePath, 0, 0, 0, 0, message.Replace("{", "{{").Replace("}", "}}"));
    }

    // shields.io static badge path: '-' and '_' are escaped by doubling, spaces as %20
    private static string Segment(string s)
    {
        return s.Replace("-", "--").Replace("_", "__").Replace(" ", "%20");
    }

    private static string BadgeUrl(string label, string message, string color)
    {
        return "https://img.shields.io/badge/" + Segment(label) + "-" + Segment(message) + "-" + color;
    }
}
