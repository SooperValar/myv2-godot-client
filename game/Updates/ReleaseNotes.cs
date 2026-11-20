using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace TheLastGate.Updates;

public sealed record ReleaseNotes(DateOnly? Date, IReadOnlyList<NotesSection> Sections)
{
    private const string VersionHeading = "## ";
    private const string SectionHeading = "### ";

    private static readonly Regex HeadingDate = new(@"\((\d{4}-\d{2}-\d{2})\)\s*$", RegexOptions.Compiled);
    private static readonly Regex CommitLink = new(@"\s*\(\[[0-9a-f]+\]\([^)]*\)\)\s*$", RegexOptions.Compiled);
    private static readonly Regex Scope = new(@"^\*\*[^*]+:\*\*\s*", RegexOptions.Compiled);
    private static readonly Regex Link = new(@"\[([^\]]*)\]\([^)]*\)", RegexOptions.Compiled);

    public static ReleaseNotes Parse(string markdown)
    {
        DateOnly? date = null;
        var sections = new List<NotesSection>();
        List<string>? items = null;

        foreach (var rawLine in markdown.Split('\n'))
        {
            var line = rawLine.Trim();
            if (line.StartsWith(VersionHeading, StringComparison.Ordinal))
            {
                date = DateIn(line) ?? date;
            }
            else if (line.StartsWith(SectionHeading, StringComparison.Ordinal))
            {
                items = StartSection(sections, line[SectionHeading.Length..].Trim());
            }
            else if (line.StartsWith("* ", StringComparison.Ordinal) || line.StartsWith("- ", StringComparison.Ordinal))
            {
                items ??= StartSection(sections, "");
                items.Add(ItemText(line[2..]));
            }
        }

        return new ReleaseNotes(date, sections);
    }

    private static List<string> StartSection(List<NotesSection> sections, string title)
    {
        var items = new List<string>();
        sections.Add(new NotesSection(title, items));
        return items;
    }

    private static DateOnly? DateIn(string heading)
    {
        var match = HeadingDate.Match(heading);
        return match.Success
            ? DateOnly.ParseExact(match.Groups[1].Value, "yyyy-MM-dd", CultureInfo.InvariantCulture)
            : null;
    }

    private static string ItemText(string item)
    {
        var withoutCommit = CommitLink.Replace(item, "");
        var withoutScope = Scope.Replace(withoutCommit, "");
        return Link.Replace(withoutScope, "$1").Trim();
    }
}
