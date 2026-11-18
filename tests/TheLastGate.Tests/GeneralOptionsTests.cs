using TheLastGate.Windows;

namespace TheLastGate.Tests;

public class GeneralOptionsTests
{
    [Fact]
    public void Rows_ListTheCurrentClientsOptionsFirstThenTheGodotOnes()
    {
        Assert.Equal(
            [
                GeneralOption.EscapeClosesWindowsFirst,
                GeneralOption.GiveMore,
                GeneralOption.UseQueue,
                GeneralOption.EnterToTalk,
                GeneralOption.ExpandedQuickStats,
                GeneralOption.SkillPlanner,
                GeneralOption.AdaptivePacing,
                GeneralOption.ItemTooltips,
                GeneralOption.ItemTooltipsAlways,
                GeneralOption.GlideMovement,
                GeneralOption.PickTileDrawnUnderMouse,
            ],
            GeneralOptions.Rows.Select(row => row.Option));
        Assert.Equal(
            [false, false, false, false, false, false, false, false, false, true, true],
            GeneralOptions.Rows.Select(row => row.GodotOnly));
    }

    [Theory]
    [InlineData(GeneralOption.EscapeClosesWindowsFirst, "Escape closes windows before canceling movement.", true)]
    [InlineData(
        GeneralOption.GiveMore,
        "Holding Ctrl+Shift when giving an item will attempt to reload that item.",
        false)]
    [InlineData(GeneralOption.UseQueue, "Holding Ctrl+Shift when using an item queues that use. Limit 5.", false)]
    [InlineData(
        GeneralOption.EnterToTalk,
        "Press Enter to activate chat mode before typing. Press Enter again to send, or ESC to cancel.",
        false)]
    [InlineData(GeneralOption.ExpandedQuickStats, "Show Hit/Parry Score with Weapon/Armor value.", true)]
    [InlineData(
        GeneralOption.AdaptivePacing,
        "Adaptive network smoothing: lower delay on steady connections, more buffering when the connection is uneven.",
        false)]
    [InlineData(GeneralOption.ItemTooltips, "Show item tooltips for inventory and equipped gear.", true)]
    [InlineData(
        GeneralOption.ItemTooltipsAlways,
        "Always show tooltips on hover. When off, hold the Item Tooltip hotkey to show them.",
        true)]
    public void Rows_UseTheCurrentClientsWordsAndDefaults(GeneralOption option, string label, bool isOn)
    {
        var row = GeneralOptions.Rows.Single(row => row.Option == option);

        Assert.Equal(label, row.Label);
        Assert.Equal(isOn, row.Default);
    }

    [Theory]
    [InlineData(GeneralOption.GlideMovement)]
    [InlineData(GeneralOption.PickTileDrawnUnderMouse)]
    public void Rows_LeaveTheGodotOptionsOffToBehaveLikeTheCurrentClient(GeneralOption option)
    {
        var row = GeneralOptions.Rows.Single(row => row.Option == option);

        Assert.False(row.Default);
        Assert.NotEmpty(row.Label);
    }

    [Fact]
    public void Rows_PutAlwaysShowingTooltipsUnderTheTooltipOption()
    {
        Assert.Equal(
            [GeneralOption.ItemTooltipsAlways],
            GeneralOptions.Rows.Where(row => row.Under != null).Select(row => row.Option));
        Assert.Equal(
            GeneralOption.ItemTooltips,
            GeneralOptions.Rows.Single(row => row.Option == GeneralOption.ItemTooltipsAlways).Under);
    }

    [Theory]
    [InlineData(GeneralOption.ItemTooltips, false, true)]
    [InlineData(GeneralOption.ItemTooltipsAlways, true, true)]
    [InlineData(GeneralOption.ItemTooltipsAlways, false, false)]
    public void IsShown_HidesARowWhileTheOptionItIsUnderIsOff(GeneralOption option, bool tooltipsOn, bool shown)
    {
        var row = GeneralOptions.Rows.Single(row => row.Option == option);

        Assert.Equal(shown, GeneralOptions.IsShown(row, other => other == GeneralOption.ItemTooltips && tooltipsOn));
    }

    [Theory]
    [InlineData(true, true, false, true)]
    [InlineData(true, false, true, true)]
    [InlineData(true, false, false, false)]
    [InlineData(false, true, true, false)]
    [InlineData(false, false, true, false)]
    public void ShowsItemTooltips_AlwaysOrWhileTheKeyIsHeld(bool tooltips, bool always, bool keyHeld, bool shows)
    {
        Assert.Equal(shows, GeneralOptions.ShowsItemTooltips(tooltips, always, keyHeld));
    }

    [Theory]
    [InlineData(true, true, false)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(false, false, true)]
    public void EscapeSendsReset_UnlessItClosedAWindowFirst(bool closesWindowsFirst, bool closedAWindow, bool resets)
    {
        Assert.Equal(resets, GeneralOptions.EscapeSendsReset(closesWindowsFirst, closedAWindow));
    }
}
