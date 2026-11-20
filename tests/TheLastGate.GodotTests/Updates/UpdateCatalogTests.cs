using GdUnit4;
using TheLastGate.Updates;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Updates;

[TestSuite]
public class UpdateCatalogTests
{
    private static UpdateCatalog Catalog(string current, params string[] versions) =>
        new(Version.Parse(current), versions.Select(Release));

    private static AvailableRelease Release(string version) => new(Version.Parse(version), $"notes {version}", 100);

    private static string[] VersionsOf(IEnumerable<AvailableRelease> releases) =>
        releases.Select(release => release.Version.ToString()).ToArray();

    [TestCase]
    public void Newest_IsHighestAboveCurrent() =>
        AssertThat(Catalog("1.0.0", "1.0.0", "1.1.0", "1.2.0").Newest?.Version).IsEqual(Version.Parse("1.2.0"));

    [TestCase]
    public void Newest_IsNullWhenNothingIsNewer()
    {
        var catalog = Catalog("1.2.0", "1.0.0", "1.1.0", "1.2.0");

        AssertThat(catalog.Newest).IsNull();
        AssertThat(catalog.NewerThanCurrent).IsEmpty();
    }

    [TestCase]
    public void NewerThanCurrent_IsNewestFirst() =>
        AssertThat(VersionsOf(Catalog("1.0.0", "1.0.0", "1.1.0", "1.2.0").NewerThanCurrent))
            .ContainsExactly("1.2.0", "1.1.0");

    [TestCase]
    public void RollbackChoices_AreOlderOnlyNewestFirst() =>
        AssertThat(VersionsOf(Catalog("1.1.0", "1.0.0", "1.1.0", "1.2.0").RollbackChoices)).ContainsExactly("1.0.0");

    [TestCase]
    public void RollbackChoices_CountBackFromCurrent()
    {
        var versions = Enumerable.Range(0, 15).Select(minor => $"1.{minor}.0").ToArray();

        var choices = VersionsOf(Catalog("1.12.0", versions).RollbackChoices);

        AssertThat(choices.Length).IsEqual(UpdateCatalog.RollbackChoiceLimit);
        AssertThat(choices[0]).IsEqual("1.11.0");
        AssertThat(choices[^1]).IsEqual("1.2.0");
    }

    [TestCase]
    public void UndoneBy_ListsReleasesAboveTargetUpToCurrent()
    {
        var undone = Catalog("1.2.0", "1.0.0", "1.1.0", "1.2.0").UndoneBy(Version.Parse("1.0.0"));

        AssertThat(VersionsOf(undone)).ContainsExactly("1.2.0", "1.1.0");
    }

    [TestCase]
    public void Catalog_CountsDuplicateVersionsOnce() =>
        AssertThat(VersionsOf(Catalog("1.0.0", "1.1.0", "1.1.0").NewerThanCurrent)).ContainsExactly("1.1.0");

    [TestCase]
    public void Catalog_CurrentVersionMissingFromFeedStillSplitsByOrder()
    {
        var catalog = Catalog("1.1.5", "1.0.0", "1.1.0", "1.2.0");

        AssertThat(catalog.Newest?.Version).IsEqual(Version.Parse("1.2.0"));
        AssertThat(VersionsOf(catalog.RollbackChoices)).ContainsExactly("1.1.0", "1.0.0");
        AssertThat(VersionsOf(catalog.UndoneBy(Version.Parse("1.0.0")))).ContainsExactly("1.1.0");
    }
}
