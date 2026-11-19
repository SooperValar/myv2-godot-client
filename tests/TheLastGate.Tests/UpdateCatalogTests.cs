using TheLastGate.Updates;

namespace TheLastGate.Tests;

public class UpdateCatalogTests
{
    private static UpdateCatalog Catalog(string current, params string[] versions) =>
        new(Version.Parse(current), versions.Select(Release));

    private static AvailableRelease Release(string version) => new(Version.Parse(version), $"notes {version}", 100);

    private static string[] VersionsOf(IEnumerable<AvailableRelease> releases) =>
        releases.Select(release => release.Version.ToString()).ToArray();

    [Fact]
    public void Newest_IsHighestAboveCurrent()
    {
        Assert.Equal(Version.Parse("1.2.0"), Catalog("1.0.0", "1.0.0", "1.1.0", "1.2.0").Newest?.Version);
    }

    [Fact]
    public void Newest_IsNullWhenNothingIsNewer()
    {
        var catalog = Catalog("1.2.0", "1.0.0", "1.1.0", "1.2.0");

        Assert.Null(catalog.Newest);
        Assert.Empty(catalog.NewerThanCurrent);
    }

    [Fact]
    public void NewerThanCurrent_IsNewestFirst()
    {
        Assert.Equal(["1.2.0", "1.1.0"], VersionsOf(Catalog("1.0.0", "1.0.0", "1.1.0", "1.2.0").NewerThanCurrent));
    }

    [Fact]
    public void RollbackChoices_AreOlderOnlyNewestFirst()
    {
        Assert.Equal(["1.0.0"], VersionsOf(Catalog("1.1.0", "1.0.0", "1.1.0", "1.2.0").RollbackChoices));
    }

    [Fact]
    public void RollbackChoices_CountBackFromCurrent()
    {
        var versions = Enumerable.Range(0, 15).Select(minor => $"1.{minor}.0").ToArray();

        var choices = VersionsOf(Catalog("1.12.0", versions).RollbackChoices);

        Assert.Equal(UpdateCatalog.RollbackChoiceLimit, choices.Length);
        Assert.Equal("1.11.0", choices[0]);
        Assert.Equal("1.2.0", choices[^1]);
    }

    [Fact]
    public void UndoneBy_ListsReleasesAboveTargetUpToCurrent()
    {
        var undone = Catalog("1.2.0", "1.0.0", "1.1.0", "1.2.0").UndoneBy(Version.Parse("1.0.0"));

        Assert.Equal(["1.2.0", "1.1.0"], VersionsOf(undone));
    }

    [Fact]
    public void Catalog_CountsDuplicateVersionsOnce()
    {
        Assert.Equal(["1.1.0"], VersionsOf(Catalog("1.0.0", "1.1.0", "1.1.0").NewerThanCurrent));
    }

    [Fact]
    public void Catalog_CurrentVersionMissingFromFeedStillSplitsByOrder()
    {
        var catalog = Catalog("1.1.5", "1.0.0", "1.1.0", "1.2.0");

        Assert.Equal(Version.Parse("1.2.0"), catalog.Newest?.Version);
        Assert.Equal(["1.1.0", "1.0.0"], VersionsOf(catalog.RollbackChoices));
        Assert.Equal(["1.1.0"], VersionsOf(catalog.UndoneBy(Version.Parse("1.0.0"))));
    }
}
