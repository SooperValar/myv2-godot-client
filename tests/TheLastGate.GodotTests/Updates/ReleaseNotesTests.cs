using GdUnit4;
using TheLastGate.Updates;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Updates;

[TestSuite]
public class ReleaseNotesTests
{
    private const string ReleasePleaseBody = """
        ## [5.2.0](https://github.com/o/r/compare/v5.1.0...v5.2.0) (2026-11-17)


        ### Features

        * **hud:** Add world map help pages ([fb6ffec](https://github.com/o/r/commit/fb6ffec4))


        ### Bug Fixes

        * Stop the chat line losing focus ([4ed0f4b](https://github.com/o/r/commit/4ed0f4b))
        """;

    [TestCase]
    public void Parse_ReadsTheDateFromTheHeading() =>
        AssertThat(ReleaseNotes.Parse(ReleasePleaseBody).Date).IsEqual(new DateOnly(2026, 11, 17));

    [TestCase]
    public void Parse_ReadsSectionsInOrder()
    {
        var titles = ReleaseNotes.Parse(ReleasePleaseBody).Sections.Select(section => section.Title);

        AssertThat(titles).ContainsExactly("Features", "Bug Fixes");
    }

    [TestCase]
    public void Parse_StripsCommitLinksAndScopes()
    {
        var sections = ReleaseNotes.Parse(ReleasePleaseBody).Sections;

        AssertThat(sections[0].Items).ContainsExactly("Add world map help pages");
        AssertThat(sections[1].Items).ContainsExactly("Stop the chat line losing focus");
    }

    [TestCase]
    public void Parse_KeepsLinkTextOfOtherLinks()
    {
        var notes = ReleaseNotes.Parse("### Features\n\n* See [the map](https://x) now");

        AssertThat(notes.Sections[0].Items).ContainsExactly("See the map now");
    }

    [TestCase]
    public void Parse_NoHeadingMeansNoDate()
    {
        var notes = ReleaseNotes.Parse("### Features\n\n* Thing");

        AssertThat(notes.Date).IsNull();
        AssertThat(notes.Sections).HasSize(1);
    }

    [TestCase]
    public void Parse_EmptyTextHasNoSections()
    {
        var notes = ReleaseNotes.Parse("");

        AssertThat(notes.Date).IsNull();
        AssertThat(notes.Sections).IsEmpty();
    }

    [TestCase]
    public void Parse_BulletsBeforeAnySectionGoInAnUntitledSection()
    {
        var sections = ReleaseNotes.Parse("* Thing").Sections;

        AssertThat(sections).HasSize(1);
        AssertThat(sections[0].Title).IsEqual("");
        AssertThat(sections[0].Items).ContainsExactly("Thing");
    }
}
