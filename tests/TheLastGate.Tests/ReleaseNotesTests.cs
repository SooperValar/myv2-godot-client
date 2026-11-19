using TheLastGate.Updates;

namespace TheLastGate.Tests;

public class ReleaseNotesTests
{
    private const string ReleasePleaseBody = """
        ## [5.2.0](https://github.com/o/r/compare/v5.1.0...v5.2.0) (2026-11-17)


        ### Features

        * **hud:** Add world map help pages ([fb6ffec](https://github.com/o/r/commit/fb6ffec4))


        ### Bug Fixes

        * Stop the chat line losing focus ([4ed0f4b](https://github.com/o/r/commit/4ed0f4b))
        """;

    [Fact]
    public void Parse_ReadsTheDateFromTheHeading()
    {
        Assert.Equal(new DateOnly(2026, 11, 17), ReleaseNotes.Parse(ReleasePleaseBody).Date);
    }

    [Fact]
    public void Parse_ReadsSectionsInOrder()
    {
        var titles = ReleaseNotes.Parse(ReleasePleaseBody).Sections.Select(section => section.Title);

        Assert.Equal(["Features", "Bug Fixes"], titles);
    }

    [Fact]
    public void Parse_StripsCommitLinksAndScopes()
    {
        var sections = ReleaseNotes.Parse(ReleasePleaseBody).Sections;

        Assert.Equal(["Add world map help pages"], sections[0].Items);
        Assert.Equal(["Stop the chat line losing focus"], sections[1].Items);
    }

    [Fact]
    public void Parse_KeepsLinkTextOfOtherLinks()
    {
        var notes = ReleaseNotes.Parse("### Features\n\n* See [the map](https://x) now");

        Assert.Equal(["See the map now"], notes.Sections[0].Items);
    }

    [Fact]
    public void Parse_NoHeadingMeansNoDate()
    {
        var notes = ReleaseNotes.Parse("### Features\n\n* Thing");

        Assert.Null(notes.Date);
        Assert.Single(notes.Sections);
    }

    [Fact]
    public void Parse_EmptyTextHasNoSections()
    {
        var notes = ReleaseNotes.Parse("");

        Assert.Null(notes.Date);
        Assert.Empty(notes.Sections);
    }

    [Fact]
    public void Parse_BulletsBeforeAnySectionGoInAnUntitledSection()
    {
        var section = Assert.Single(ReleaseNotes.Parse("* Thing").Sections);

        Assert.Equal("", section.Title);
        Assert.Equal(["Thing"], section.Items);
    }
}
