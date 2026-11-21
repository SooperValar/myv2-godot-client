using GdUnit4;
using TheLastGate.Assets;
using TheLastGate.Windows;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Windows;

[TestSuite]
public class AccessibilityOptionsTests
{
    [TestCase]
    public void SpritesFor_NothingSwitchedOn_SwapsNothing()
    {
        AssertThat(AccessibilityOptions.SpritesFor([])).IsEmpty();
    }

    [TestCase]
    public void SpritesFor_ObviousHoleTraps_DrawsTheHoleInsteadOfTheHiddenTrap()
    {
        var sprites = AccessibilityOptions.SpritesFor([AccessibilityOption.ObviousHoleTraps]);

        AssertThat(sprites).IsEqual(new Dictionary<int, int> { [16859] = 34194 });
    }

    [TestCase]
    public void SpritesFor_PlainInfravision_SwapsNoSprites()
    {
        AssertThat(AccessibilityOptions.SpritesFor([AccessibilityOption.PlainInfravision])).IsEmpty();
    }

    [TestCase]
    public void SpritesFor_TheSameOptionTwice_IsLikeOnce()
    {
        var sprites = AccessibilityOptions.SpritesFor(
            [AccessibilityOption.ObviousHoleTraps, AccessibilityOption.ObviousHoleTraps]);

        AssertThat(sprites).IsEqual(new Dictionary<int, int> { [16859] = 34194 });
    }

    [TestCase]
    public void Rows_HaveOneRowForEveryOption()
    {
        AssertThat(AccessibilityOptions.Rows.Select(row => row.Option))
            .ContainsExactly(Enum.GetValues<AccessibilityOption>());
    }

    [TestCase]
    public void Rows_HaveDistinctSettingKeys()
    {
        var keys = AccessibilityOptions.Rows.Select(row => row.SettingKey).ToList();

        AssertThat(keys.Distinct().Count()).IsEqual(keys.Count);
    }

    [TestCase]
    public void Rows_NeverSwapASpriteIntoOneAnotherRowSwapsAgain()
    {
        var sources = AccessibilityOptions.Rows.SelectMany(row => row.Sprites.Keys).ToList();
        var targets = AccessibilityOptions.Rows.SelectMany(row => row.Sprites.Values);

        AssertThat(sources.Distinct().Count()).IsEqual(sources.Count);
        AssertThat(targets.Where(sources.Contains)).IsEmpty();
    }

    [TestCase]
    public void Rows_SwapOnlyToSpritesTheArchivesHold()
    {
        var archives = Path.Combine(RepositoryFolder.Find(), "data", DataLayout.Archives);
        using var bmpLibrary = new BmpLibrary(
            Path.Combine(archives, DataLayout.BmpIndexFile),
            Path.Combine(archives, DataLayout.BmpDataFile));
        using var pngLibrary = new PngLibrary(
            Path.Combine(archives, DataLayout.PngIndexFile),
            Path.Combine(archives, DataLayout.PngDataFile));

        var missing = AccessibilityOptions.Rows
            .SelectMany(row => row.Sprites.Values)
            .Where(sprite => !bmpLibrary.Contains(sprite) && !pngLibrary.Contains(sprite));

        AssertThat(missing).IsEmpty();
    }
}
