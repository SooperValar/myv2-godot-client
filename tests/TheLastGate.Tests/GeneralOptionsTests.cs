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
                GeneralOption.ExpandedQuickStats,
                GeneralOption.GlideMovement,
                GeneralOption.PickTileDrawnUnderMouse,
            ],
            GeneralOptions.Rows.Select(row => row.Option));
        Assert.Equal([false, false, true, true], GeneralOptions.Rows.Select(row => row.GodotOnly));
    }

    [Theory]
    [InlineData(GeneralOption.EscapeClosesWindowsFirst, "Escape closes windows before canceling movement.", true)]
    [InlineData(GeneralOption.ExpandedQuickStats, "Show Hit/Parry Score with Weapon/Armor value.", true)]
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
