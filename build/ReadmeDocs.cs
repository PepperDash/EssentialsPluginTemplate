// README plugin documentation generator, compiled at build time by RoslynCodeTaskFactory
// (see the UpdateReadmeDocs / CheckReadmeDocs targets in src/Directory.Build.targets).
//
// Port of metadata.py from PepperDash/workflow-templates (commit 681f68e), followed by the template's
// post-processing (Config Example type/uid, Base Classes and Interfaces). Output matches metadata.py
// except that:
// - it is deterministic: source files are read in sorted folder order and each file's Supported Types
//   keep their declared order (metadata.py used os.walk order and an unordered set)
// - each distinct Minimum Essentials Framework Version is listed once, including several in one file
// - join access (R, W or R/W) follows JoinCapabilities (metadata.py listed every join as R)
// - a join map is also found by its class declaration when its file is not named <ClassName>.cs
// - factories are recognized by their Essentials base class, whatever they are named
// - interfaces are matched case-sensitively (IpTableObjectBase is a base class)
// - Base Classes come before Interfaces, as plain list items
//
// Must stay compatible with netstandard2.0 / C# 7.3 so it also compiles under Visual Studio's MSBuild.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

public class GenerateReadmeDocs : Task
{
    /// <summary>README.md to update or check.</summary>
    [Required] public string ReadmePath { get; set; }

    /// <summary>The project's own C# files; RelPath metadata (relative to the project) sets the read order.</summary>
    [Required] public ITaskItem[] SourceFiles { get; set; }

    /// <summary>Fail when README.md is stale instead of rewriting it.</summary>
    public bool CheckOnly { get; set; }

    private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);

    public override bool Execute()
    {
        var sources = SourceFiles
            .Select(i => new SourceFile(
                RelativePath(i),
                ReadmeDocs.NormalizeNewlines(File.ReadAllText(i.GetMetadata("FullPath"), Utf8))))
            .ToList();
        sources.Sort((a, b) => ReadmeDocs.WalkOrder(a.RelPath, b.RelPath));

        var original = File.ReadAllText(ReadmePath, Utf8);
        var generated = ReadmeDocs.Generate(ReadmeDocs.NormalizeNewlines(original), sources,
            message => Log.LogMessage(MessageImportance.Normal, "readme-docs: " + message));

        if (string.Equals(generated, ReadmeDocs.NormalizeNewlines(original), StringComparison.Ordinal))
        {
            Log.LogMessage(MessageImportance.High, "readme-docs: README.md up to date");
            return true;
        }
        if (CheckOnly)
        {
            Log.LogError(null, "PDREADME005", null, ReadmePath, 0, 0, 0, 0,
                "README.md plugin documentation is stale; run 'dotnet msbuild -t:UpdateReadmeDocs' locally, review and commit README.md");
            return false;
        }

        // Keep the line endings the README was checked out with
        if (original.Contains("\r\n")) generated = generated.Replace("\n", "\r\n");
        File.WriteAllText(ReadmePath, generated, Utf8);
        Log.LogMessage(MessageImportance.High, "readme-docs: README.md updated; review the diff, then commit it with your changes");
        return true;
    }

    private static string RelativePath(ITaskItem item)
    {
        var rel = item.GetMetadata("RelPath");
        return string.IsNullOrEmpty(rel) ? item.ItemSpec : rel;
    }
}

public class SourceFile
{
    public SourceFile(string relPath, string content)
    {
        RelPath = relPath.Replace('\\', '/');
        Content = content;
    }

    public string RelPath { get; private set; }
    public string Content { get; private set; }
    public string FileName { get { return RelPath.Substring(RelPath.LastIndexOf('/') + 1); } }
}

public static class ReadmeDocs
{
    public static string NormalizeNewlines(string text)
    {
        return text.Replace("\r\n", "\n").Replace('\r', '\n');
    }

    /// <summary>os.walk order with sorted entries: a folder's files before its subfolders, each sorted ordinally.</summary>
    public static int WalkOrder(string a, string b)
    {
        var pa = a.Replace('\\', '/').Split('/');
        var pb = b.Replace('\\', '/').Split('/');
        for (var i = 0; ; i++)
        {
            var aIsFile = i == pa.Length - 1;
            var bIsFile = i == pb.Length - 1;
            if (aIsFile != bIsFile) return aIsFile ? -1 : 1;
            var c = string.CompareOrdinal(pa[i], pb[i]);
            if (c != 0 || aIsFile) return c;
        }
    }

    public static string Generate(string readme, IList<SourceFile> sources, Action<string> log)
    {
        // ---- metadata.py ----
        var supportedTypes = new List<string>();
        var minimumVersions = new List<string>();
        var publicMethods = new List<string>();
        var boolFeedbacks = new List<string>();
        var intFeedbacks = new List<string>();
        var stringFeedbacks = new List<string>();
        foreach (var source in sources)
        {
            // De-duplicated per file only, as in metadata.py
            supportedTypes.AddRange(ExtractSupportedTypes(source.Content).Distinct());
            // Listed once per distinct value (metadata.py took the first per file and repeated it for every factory)
            foreach (Match version in MinimumVersionPattern.Matches(source.Content))
                if (!minimumVersions.Contains(version.Groups[1].Value)) minimumVersions.Add(version.Groups[1].Value);
            foreach (Match m in PublicMethodPattern.Matches(source.Content)) publicMethods.Add(m.Value.Trim());
            var uncommented = LineComment.Replace(source.Content, "");
            AddFeedbacks(BoolFeedbackPattern, uncommented, boolFeedbacks);
            AddFeedbacks(IntFeedbackPattern, uncommented, intFeedbacks);
            AddFeedbacks(StringFeedbackPattern, uncommented, stringFeedbacks);
        }

        var joins = new List<JoinInfo>();
        foreach (var joinMap in FindJoinMapClasses(sources))
            joins.AddRange(ParseJoinMap(joinMap, sources, log));

        string configExample = "";
        var classDefs = ParseAllClasses(sources);
        var configClasses = classDefs.Keys.Where(c => c.EndsWith("Config", StringComparison.Ordinal) || c.EndsWith("ConfigObject", StringComparison.Ordinal)).ToList();
        if (configClasses.Count == 0)
        {
            log("no config classes found");
        }
        else
        {
            var main = configClasses[0];
            foreach (var c in configClasses)
                if (classDefs[c].Count > classDefs[main].Count) main = c;
            configExample = "### Config Example\n\n```json\n" + Json.Write(SampleConfig(main, classDefs, supportedTypes), 0) + "\n```\n";
        }

        readme = UpdateSection(readme, "Minimum Essentials Framework Versions", MarkdownList(minimumVersions, "Minimum Essentials Framework Versions"));
        if (configExample.Length > 0) readme = UpdateSection(readme, "Config Example", configExample);
        readme = UpdateSection(readme, "Supported Types", MarkdownList(supportedTypes, "Supported Types"));
        readme = UpdateSection(readme, "Join Maps", JoinMapChart(joins));
        // Base Classes and Interfaces Implemented bodies are replaced by the post-processing below
        readme = UpdateSection(readme, "Base Classes", "");
        readme = UpdateSection(readme, "Interfaces Implemented", "");
        readme = BaseClassesBeforeInterfaces(readme);
        readme = UpdateSection(readme, "Public Methods", MarkdownList(publicMethods, "Public Methods"));
        readme = UpdateSection(readme, "Bool Feedbacks", MarkdownList(boolFeedbacks, "Bool Feedbacks"));
        readme = UpdateSection(readme, "Int Feedbacks", MarkdownList(intFeedbacks, "Int Feedbacks"));
        readme = UpdateSection(readme, "String Feedbacks", MarkdownList(stringFeedbacks, "String Feedbacks"));

        // ---- template post-processing ----
        var byName = sources.OrderBy(s => s.FileName, StringComparer.OrdinalIgnoreCase).ToList();
        readme = FixConfigExample(readme, FindConfigType(byName));
        var baseTypes = new List<string>();
        var interfaceTypes = new List<string>();
        CollectDeclaredTypes(byName, baseTypes, interfaceTypes);
        readme = SetSectionBody(readme, "Base Classes", DeclaredTypesBody("Base Classes", baseTypes));
        readme = SetSectionBody(readme, "Interfaces Implemented", DeclaredTypesBody("Interfaces", interfaceTypes));
        return readme;
    }

    // ---------------------------------------------------------------- metadata.py extraction

    private static readonly Regex LineComment = new Regex("//.*");
    private static readonly Regex BlockComment = new Regex(@"/\*.*?\*/", RegexOptions.Singleline);
    private static readonly Regex TypeNamesPattern = new Regex(@"TypeNames\s*=\s*new\s*List<string>\(\)\s*\{([^}]+)\}");
    private static readonly Regex MinimumVersionPattern = new Regex(@"^\s*MinimumEssentialsFrameworkVersion\s*=\s*""([^""]+)""\s*;", RegexOptions.Multiline);
    private static readonly Regex PublicMethodPattern = new Regex(@"public\s+\w+\s+\w+\s*\([^)]*\)\s*");
    private static readonly Regex BoolFeedbackPattern = FeedbackPattern("BoolFeedback");
    private static readonly Regex IntFeedbackPattern = FeedbackPattern("IntFeedback");
    private static readonly Regex StringFeedbackPattern = FeedbackPattern("StringFeedback");

    private static Regex FeedbackPattern(string type)
    {
        return new Regex(@"public\s+" + type + @"\s+(\w+)(?:\s*\{[^}]*\}|\s*;|\s*=)");
    }

    private static IEnumerable<string> ExtractSupportedTypes(string content)
    {
        foreach (Match m in TypeNamesPattern.Matches(LineComment.Replace(content, "")))
            foreach (var item in m.Groups[1].Value.Split(','))
            {
                var type = item.Trim().Trim('"');
                if (type.Length > 0) yield return type;
            }
    }

    private static void AddFeedbacks(Regex pattern, string content, List<string> target)
    {
        foreach (Match m in pattern.Matches(content))
        {
            var name = m.Groups[1].Value.Trim();
            if (name.Length > 0) target.Add(name);
        }
    }

    private static string MarkdownList(List<string> items, string title)
    {
        if (items.Count == 0) return "";
        var sb = new StringBuilder("### " + title + "\n\n");
        foreach (var item in items) sb.Append("- ").Append(item).Append('\n');
        return sb.Append('\n').ToString();
    }

    // ---------------------------------------------------------------- join maps

    private static readonly Regex ClassWithBasesPattern = new Regex(
        @"^\s*(?:\[[^\]]+\]\s*)*(?:public\s+|private\s+|protected\s+)?(?:partial\s+)?class\s+([A-Za-z_]\w*)(?:\s*:\s*([^\{]+))?\s*\{",
        RegexOptions.Multiline);

    private static readonly Regex JoinPattern = new Regex(
        @"\[JoinName\(""(?<join_name>[^""]+)""\)\]\s*" +
        @"public\s+JoinDataComplete\s+(?<property_name>\w+)\s*=\s*" +
        @"new\s+JoinDataComplete\s*\(\s*" +
        @"(?<join_params>.*?)\)\s*;",
        RegexOptions.Singleline);

    private static readonly Regex JoinDataPattern = new Regex(@"new\s+JoinData\s*(?:\(\s*\))?\s*\{(.*?)\}", RegexOptions.Singleline);
    private static readonly Regex JoinMetadataPattern = new Regex(@"new\s+JoinMetadata\s*(?:\(\s*\))?\s*\{(.*?)\}", RegexOptions.Singleline);

    private class JoinInfo
    {
        public string Number;
        public string Type;
        public string Description;
        public string Access;
    }

    private static List<string> FindJoinMapClasses(IList<SourceFile> sources)
    {
        // Python dict semantics: first-seen order, last definition wins
        var order = new List<string>();
        var bases = new Dictionary<string, List<string>>();
        foreach (var source in sources)
            foreach (Match m in ClassWithBasesPattern.Matches(source.Content))
            {
                var name = m.Groups[1].Value;
                if (!bases.ContainsKey(name)) order.Add(name);
                bases[name] = m.Groups[2].Success
                    ? m.Groups[2].Value.Split(',').Select(b => b.Trim()).ToList()
                    : new List<string>();
            }
        return order.Where(n => bases[n].Contains("JoinMapBaseAdvanced")).ToList();
    }

    private static IEnumerable<JoinInfo> ParseJoinMap(string className, IList<SourceFile> sources, Action<string> log)
    {
        // metadata.py reads the join map from <ClassName>.cs; fall back to the file that declares the
        // class, so a join map still works when its file is not renamed along with the class
        var declares = new Regex(@"\bclass\s+" + Regex.Escape(className) + @"\b");
        var file = sources.FirstOrDefault(s => s.FileName == className + ".cs")
            ?? sources.FirstOrDefault(s => declares.IsMatch(s.Content));
        if (file == null)
        {
            log("join map file not found: " + className + ".cs; skipping");
            yield break;
        }

        var content = BlockComment.Replace(LineComment.Replace(file.Content, ""), "");
        foreach (Match match in JoinPattern.Matches(content))
        {
            var joinName = match.Groups["join_name"].Value;
            var joinParams = match.Groups["join_params"].Value;
            string number = null, description = null, type = null, access = "R";

            var data = JoinDataPattern.Match(joinParams);
            if (data.Success)
            {
                var m = Regex.Match(data.Groups[1].Value, @"JoinNumber\s*=\s*(\d+)");
                if (m.Success) number = m.Groups[1].Value;
            }
            var metadata = JoinMetadataPattern.Match(joinParams);
            if (metadata.Success)
            {
                var d = Regex.Match(metadata.Groups[1].Value, @"Description\s*=\s*""([^""]+)""");
                if (d.Success) description = d.Groups[1].Value;
                var t = Regex.Match(metadata.Groups[1].Value, @"JoinType\s*=\s*eJoinType\.(\w+)");
                if (t.Success) type = t.Groups[1].Value;
                var c = Regex.Match(metadata.Groups[1].Value, @"JoinCapabilities\s*=\s*([^,\r\n}]+)");
                if (c.Success) access = JoinAccess(c.Groups[1].Value) ?? access;
            }

            if (joinName.Length > 0 && number != null && type != null)
                yield return new JoinInfo { Number = number, Type = type, Description = description, Access = access };
            else
                log("incomplete join information for '" + joinName + "'; skipping");
        }
    }

    // ToSIMPL is feedback SIMPL reads (R), FromSIMPL is a command SIMPL writes (W); null for None
    private static string JoinAccess(string capabilities)
    {
        var read = Regex.IsMatch(capabilities, @"\b(?:ToSIMPL|ToFromSIMPL)\b");
        var write = Regex.IsMatch(capabilities, @"\b(?:FromSIMPL|ToFromSIMPL)\b");
        if (read && write) return "R/W";
        if (read) return "R";
        if (write) return "W";
        return null;
    }

    private static string JoinMapChart(List<JoinInfo> joins)
    {
        if (joins.Count == 0) return "";
        var sb = new StringBuilder("### Join Maps\n\n");
        foreach (var kind in new[] { "Digital", "Analog", "Serial" })
        {
            // Unrecognized join types (e.g. DigitalSerial) are listed as Digital
            var ofKind = joins.Where(j => j.Type == kind || (kind == "Digital" && j.Type != "Analog" && j.Type != "Serial")).ToList();
            if (ofKind.Count == 0) continue;
            sb.Append("#### ").Append(kind).Append("s\n\n");
            sb.Append("| Join | Type (RW) | Description |\n");
            sb.Append("| --- | --- | --- |\n");
            foreach (var j in ofKind)
                sb.Append("| ").Append(j.Number).Append(" | ").Append(j.Access).Append(" | ").Append(j.Description ?? "None").Append(" |\n");
            sb.Append('\n');
        }
        return sb.ToString();
    }

    // ---------------------------------------------------------------- config example

    private static readonly Regex ClassPattern = new Regex(
        @"^\s*(?:\[[^\]]+\]\s*)*(?:public\s+|private\s+|protected\s+)?(?:partial\s+)?class\s+([A-Za-z_]\w*)(?:\s*:\s*[^\{]+)?\s*\{",
        RegexOptions.Multiline);

    private static readonly Regex PropertyPattern = new Regex(
        @"^\s*(?:\[[^\]]*\]\s*)*(?:public|private|protected)\s+(?:static\s+|virtual\s+|override\s+|abstract\s+|readonly\s+)?" +
        @"([A-Za-z0-9_<>,\s\[\]\?]+?)\s+([A-Za-z_]\w*)\s*\{[^}]*?\}",
        RegexOptions.Multiline | RegexOptions.Singleline);

    private static readonly Regex JsonPropertyPattern = new Regex(@"\[JsonProperty\(""([^""]+)""\)\]");

    private class PropertyDef
    {
        public string JsonName;
        public string Type;
    }

    private class ClassDefs
    {
        public readonly List<string> Keys = new List<string>();
        private readonly Dictionary<string, List<PropertyDef>> _map = new Dictionary<string, List<PropertyDef>>();

        public List<PropertyDef> this[string name]
        {
            get { return _map[name]; }
            set
            {
                if (!_map.ContainsKey(name)) Keys.Add(name);
                _map[name] = value;
            }
        }

        public bool Contains(string name) { return _map.ContainsKey(name); }
    }

    private static ClassDefs ParseAllClasses(IList<SourceFile> sources)
    {
        var defs = new ClassDefs();
        foreach (var source in sources)
        {
            var content = source.Content;
            foreach (Match classMatch in ClassPattern.Matches(content))
            {
                var body = ClassBody(content, classMatch.Index + classMatch.Length);
                var properties = new List<PropertyDef>();
                foreach (Match prop in PropertyPattern.Matches(body))
                {
                    var json = JsonPropertyPattern.Match(prop.Value);
                    properties.Add(new PropertyDef
                    {
                        JsonName = json.Success ? json.Groups[1].Value : prop.Groups[2].Value,
                        Type = prop.Groups[1].Value.Trim(),
                    });
                }
                defs[classMatch.Groups[1].Value] = properties;
            }
        }
        return defs;
    }

    private static string ClassBody(string content, int start)
    {
        var depth = 1;
        var index = start;
        while (depth > 0 && index < content.Length)
        {
            if (content[index] == '{') depth++;
            else if (content[index] == '}') depth--;
            index++;
        }
        return content.Substring(start, Math.Max(0, index - 1 - start));
    }

    private static Json.Object SampleConfig(string configClass, ClassDefs defs, List<string> supportedTypes)
    {
        var typeName = configClass.Substring(0, Math.Max(0, configClass.Length - 6));
        if (!supportedTypes.Contains(typeName) && supportedTypes.Count > 0) typeName = supportedTypes[0];
        var config = new Json.Object();
        config.Set("key", "GeneratedKey");
        config.Set("uid", 1);
        config.Set("name", "GeneratedName");
        config.Set("type", typeName);
        config.Set("group", "Group");
        config.Set("properties", SampleValue(configClass, defs, new HashSet<string>()));
        return config;
    }

    private static readonly string[] CollectionPrefixes = { "List<", "IList<", "IEnumerable<", "ObservableCollection<" };

    private static object SampleValue(string propertyType, ClassDefs defs, HashSet<string> processing)
    {
        var type = propertyType.Trim().TrimEnd('?');
        switch (type)
        {
            case "int": case "long": case "float": case "double": case "decimal": return 0;
            case "string": return "SampleString";
            case "bool": return true;
            case "DateTime": return "2021-01-01T00:00:00Z";
        }
        if (CollectionPrefixes.Any(p => type.StartsWith(p, StringComparison.Ordinal)))
            return new List<object> { SampleValue(GenericArguments(type), defs, processing) };
        if (type.StartsWith("Dictionary<", StringComparison.Ordinal))
        {
            var args = GenericArguments(type).Split(',');
            var key = SampleValue(args[0].Trim(), defs, processing);
            var value = args.Length > 1 ? SampleValue(args[1].Trim(), defs, processing) : "SampleValue";
            var dict = new Json.Object();
            dict.Set(Json.KeyString(key), value);
            return dict;
        }
        if (defs.Contains(type))
        {
            if (processing.Contains(type)) return new Json.Object();
            processing.Add(type);
            var obj = new Json.Object();
            foreach (var prop in defs[type]) obj.Set(prop.JsonName, SampleValue(prop.Type, defs, processing));
            processing.Remove(type);
            return obj;
        }
        return "SampleValue";
    }

    private static string GenericArguments(string type)
    {
        var start = type.IndexOf('<') + 1;
        return type.Substring(start, Math.Max(0, type.Length - 1 - start));
    }

    // ---------------------------------------------------------------- README sections

    private static string UpdateSection(string readme, string title, string content)
    {
        var start = "<!-- START " + title + " -->";
        var end = "<!-- END " + title + " -->";
        var match = Regex.Match(readme, Regex.Escape(start) + "(.*?)" + Regex.Escape(end), RegexOptions.Singleline | RegexOptions.IgnoreCase);
        if (match.Success)
        {
            if (match.Groups[1].Value.Contains("<!-- SKIP -->")) return readme;
            return readme.Substring(0, match.Index) + start + "\n" + content.TrimEnd() + "\n" + end + readme.Substring(match.Index + match.Length);
        }
        if (!readme.EndsWith("\n", StringComparison.Ordinal)) readme += "\n";
        return readme + start + "\n" + content.TrimEnd() + "\n" + end + "\n";
    }

    // Swaps the two marker blocks (with their contents) when Interfaces comes first
    private static string BaseClassesBeforeInterfaces(string readme)
    {
        var interfaces = Regex.Match(readme, @"(?s)<!-- START Interfaces Implemented -->.*?<!-- END Interfaces Implemented -->");
        var baseClasses = Regex.Match(readme, @"(?s)<!-- START Base Classes -->.*?<!-- END Base Classes -->");
        if (!interfaces.Success || !baseClasses.Success || baseClasses.Index < interfaces.Index) return readme;
        var between = interfaces.Index + interfaces.Length;
        return readme.Substring(0, interfaces.Index) + baseClasses.Value
            + readme.Substring(between, baseClasses.Index - between)
            + interfaces.Value + readme.Substring(baseClasses.Index + baseClasses.Length);
    }

    private static string SetSectionBody(string text, string name, string body)
    {
        var m = Regex.Match(text, "(?s)(<!-- START " + Regex.Escape(name) + " -->)(.*?)(<!-- END " + Regex.Escape(name) + " -->)");
        if (!m.Success || m.Groups[2].Value.Contains("<!-- SKIP -->")) return text;
        return text.Substring(0, m.Index) + m.Groups[1].Value + body + m.Groups[3].Value + text.Substring(m.Index + m.Length);
    }

    // ---------------------------------------------------------------- template post-processing

    // Config Example "type" = first TypeNames entry of the first file (by name) that sets TypeNames,
    // so factories can have any class or file name (e.g. SonyBraviaDeviceFactory)
    private static string FindConfigType(List<SourceFile> byName)
    {
        var typeNames = new Regex(@"TypeNames\s*=\s*new\s+List<string>\s*\(\s*\)\s*\{\s*""([^""]+)""");
        foreach (var source in byName)
        {
            var m = typeNames.Match(LineComment.Replace(source.Content, ""));
            if (m.Success) return m.Groups[1].Value;
        }
        return null;
    }

    // Applied even to <!-- SKIP --> sections, so "type" always follows the factories: the unused "uid"
    // is removed and "type" is set
    private static string FixConfigExample(string readme, string configType)
    {
        var section = Regex.Match(readme, @"(?s)<!-- START Config Example -->.*?<!-- END Config Example -->");
        if (!section.Success) return readme;
        var fixedText = Regex.Replace(section.Value, @"(?m)^[ \t]*""uid"":\s*\d+,?[ \t]*\n", "");
        if (configType != null)
        {
            var replacement = "\"type\": \"" + configType + "\"";
            fixedText = new Regex(@"""type"":\s*""[^""]*""").Replace(fixedText, _ => replacement, 1);
        }
        return readme.Substring(0, section.Index) + fixedText + readme.Substring(section.Index + section.Length);
    }

    // Base classes and interfaces declared by the plugin's own classes (not factories or join maps)
    private static void CollectDeclaredTypes(List<SourceFile> byName, List<string> baseTypes, List<string> interfaceTypes)
    {
        var classPattern = new Regex(@"(?m)^[ \t]*(?:(?:public|internal|abstract|sealed|static|partial)[ \t]+)*class[ \t]+(\w+)[ \t\r\n]*:([^{]+)\{");
        foreach (var source in byName)
        {
            var code = LineComment.Replace(BlockComment.Replace(source.Content, ""), "");
            foreach (Match m in classPattern.Matches(code))
            {
                var bases = SplitTopLevel(Regex.Split(m.Groups[2].Value, @"\swhere\s")[0]);
                if (bases.Contains("JoinMapBaseAdvanced") || IsFactory(m.Groups[1].Value, bases)) continue;
                foreach (var b in bases)
                {
                    var target = Regex.IsMatch(b, "^I[A-Z]") ? interfaceTypes : baseTypes;
                    if (!target.Contains(b)) target.Add(b);
                }
            }
        }
    }

    // Factories are recognized by their Essentials base class, whatever they are named
    private static bool IsFactory(string className, List<string> bases)
    {
        return className.EndsWith("Factory", StringComparison.Ordinal)
            || bases.Any(b => Regex.IsMatch(b, @"^Essentials\w*Factory\s*<"));
    }

    private static List<string> SplitTopLevel(string list)
    {
        var parts = new List<string>();
        var depth = 0;
        var current = new StringBuilder();
        foreach (var ch in list)
        {
            if (ch == '<') depth++;
            else if (ch == '>') depth--;
            if (ch == ',' && depth == 0) { parts.Add(current.ToString().Trim()); current.Clear(); }
            else current.Append(ch);
        }
        if (current.ToString().Trim().Length > 0) parts.Add(current.ToString().Trim());
        return parts;
    }

    private static string DeclaredTypesBody(string title, List<string> items)
    {
        if (items.Count == 0) return "\n";
        return "\n### " + title + "\n\n" + string.Join("\n", items.Select(i => "- " + i)) + "\n";
    }
}

/// <summary>Minimal writer matching Python's json.dumps(value, indent=4).</summary>
public static class Json
{
    public class Object
    {
        public readonly List<KeyValuePair<string, object>> Items = new List<KeyValuePair<string, object>>();

        // Python dict assignment: an existing key keeps its position
        public void Set(string key, object value)
        {
            var i = Items.FindIndex(p => p.Key == key);
            if (i >= 0) Items[i] = new KeyValuePair<string, object>(key, value);
            else Items.Add(new KeyValuePair<string, object>(key, value));
        }
    }

    public static string KeyString(object key)
    {
        if (key is bool) return (bool)key ? "true" : "false";
        if (key is int) return ((int)key).ToString(System.Globalization.CultureInfo.InvariantCulture);
        return key as string ?? "SampleValue";
    }

    public static string Write(object value, int level)
    {
        var obj = value as Object;
        if (obj != null)
        {
            if (obj.Items.Count == 0) return "{}";
            var inner = new string(' ', (level + 1) * 4);
            return "{\n" + string.Join(",\n", obj.Items.Select(p => inner + Quote(p.Key) + ": " + Write(p.Value, level + 1)))
                + "\n" + new string(' ', level * 4) + "}";
        }
        var list = value as List<object>;
        if (list != null)
        {
            if (list.Count == 0) return "[]";
            var inner = new string(' ', (level + 1) * 4);
            return "[\n" + string.Join(",\n", list.Select(v => inner + Write(v, level + 1)))
                + "\n" + new string(' ', level * 4) + "]";
        }
        if (value is bool) return (bool)value ? "true" : "false";
        if (value is int) return ((int)value).ToString(System.Globalization.CultureInfo.InvariantCulture);
        return Quote((string)value);
    }

    // ensure_ascii=True escaping
    private static string Quote(string s)
    {
        var sb = new StringBuilder("\"");
        foreach (var c in s)
        {
            switch (c)
            {
                case '"': sb.Append("\\\""); break;
                case '\\': sb.Append("\\\\"); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                case '\b': sb.Append("\\b"); break;
                case '\f': sb.Append("\\f"); break;
                default:
                    if (c < 0x20 || c > 0x7E) sb.Append("\\u").Append(((int)c).ToString("x4"));
                    else sb.Append(c);
                    break;
            }
        }
        return sb.Append('"').ToString();
    }
}
