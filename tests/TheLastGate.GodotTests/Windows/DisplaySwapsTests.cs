using GdUnit4;
using TheLastGate.Assets;
using TheLastGate.Windows;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Windows;

[TestSuite]
public class DisplaySwapsTests
{
    [TestCase]
    public void SpritesFor_NothingSwitchedOn_SwapsNothing()
    {
        AssertThat(DisplaySwaps.SpritesFor([])).IsEmpty();
    }

    [TestCase]
    public void SpritesFor_ObviousHoleTraps_DrawsTheHoleInsteadOfTheHiddenTrap()
    {
        var sprites = DisplaySwaps.SpritesFor([DisplaySwap.ObviousHoleTraps]);

        AssertThat(sprites).IsEqual(new Dictionary<int, int> { [16859] = 34194 });
    }

    [TestCase]
    public void SpritesFor_TheSameSwapTwice_IsLikeOnce()
    {
        var sprites = DisplaySwaps.SpritesFor([DisplaySwap.ObviousHoleTraps, DisplaySwap.ObviousHoleTraps]);

        AssertThat(sprites).IsEqual(new Dictionary<int, int> { [16859] = 34194 });
    }

    [TestCase]
    public void Rows_HaveOneRowForEverySwap()
    {
        AssertThat(DisplaySwaps.Rows.Select(row => row.Swap)).ContainsExactly(Enum.GetValues<DisplaySwap>());
    }

    [TestCase]
    public void Rows_HaveDistinctSettingKeys()
    {
        var keys = DisplaySwaps.Rows.Select(row => row.SettingKey).ToList();

        AssertThat(keys.Distinct().Count()).IsEqual(keys.Count);
    }

    [TestCase]
    public void Rows_NeverSwapASpriteIntoOneAnotherRowSwapsAgain()
    {
        var sources = DisplaySwaps.Rows.SelectMany(row => row.Sprites.Keys).ToList();
        var targets = DisplaySwaps.Rows.SelectMany(row => row.Sprites.Values);

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

        var missing = DisplaySwaps.Rows
            .SelectMany(row => row.Sprites.Values)
            .Where(sprite => !bmpLibrary.Contains(sprite) && !pngLibrary.Contains(sprite));

        AssertThat(missing).IsEmpty();
    }
}
