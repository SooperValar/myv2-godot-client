using TheLastGate.Assets;
using TheLastGate.Windows;

namespace TheLastGate.Tests;

public class DisplaySwapsTests
{
    [Fact]
    public void SpritesFor_NothingSwitchedOn_SwapsNothing()
    {
        Assert.Empty(DisplaySwaps.SpritesFor([]));
    }

    [Fact]
    public void SpritesFor_ObviousHoleTraps_DrawsTheHoleInsteadOfTheHiddenTrap()
    {
        var sprites = DisplaySwaps.SpritesFor([DisplaySwap.ObviousHoleTraps]);

        Assert.Equal(new Dictionary<int, int> { [16859] = 34194 }, sprites);
    }

    [Fact]
    public void SpritesFor_TheSameSwapTwice_IsLikeOnce()
    {
        var sprites = DisplaySwaps.SpritesFor([DisplaySwap.ObviousHoleTraps, DisplaySwap.ObviousHoleTraps]);

        Assert.Equal(new Dictionary<int, int> { [16859] = 34194 }, sprites);
    }

    [Fact]
    public void Rows_HaveOneRowForEverySwap()
    {
        Assert.Equal(Enum.GetValues<DisplaySwap>(), DisplaySwaps.Rows.Select(row => row.Swap));
    }

    [Fact]
    public void Rows_HaveDistinctSettingKeys()
    {
        var keys = DisplaySwaps.Rows.Select(row => row.SettingKey).ToList();

        Assert.Equal(keys.Count, keys.Distinct().Count());
    }

    [Fact]
    public void Rows_NeverSwapASpriteIntoOneAnotherRowSwapsAgain()
    {
        var sources = DisplaySwaps.Rows.SelectMany(row => row.Sprites.Keys).ToList();
        var targets = DisplaySwaps.Rows.SelectMany(row => row.Sprites.Values);

        Assert.Equal(sources.Count, sources.Distinct().Count());
        Assert.DoesNotContain(targets, sources.Contains);
    }

    [Fact]
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

        Assert.Empty(missing);
    }
}
