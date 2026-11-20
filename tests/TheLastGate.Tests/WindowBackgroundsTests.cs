using TheLastGate.Windows;

namespace TheLastGate.Tests;

public class WindowBackgroundsTests
{
    [Fact]
    public void Rows_HaveOneRowForEveryWindowThatFades()
    {
        Assert.Equal(
            Enum.GetValues<WindowBackground>().Where(window => window != WindowBackground.None),
            WindowBackgrounds.Rows.Select(row => row.Window));
    }

    [Fact]
    public void Rows_HaveDistinctSettingKeys()
    {
        var keys = WindowBackgrounds.Rows.Select(row => row.SettingKey).ToList();

        Assert.Equal(keys.Count, keys.Distinct().Count());
    }

    [Theory]
    [InlineData(-20, 0)]
    [InlineData(0, 0)]
    [InlineData(55, 55)]
    [InlineData(100, 100)]
    [InlineData(250, 100)]
    public void Clamp_KeepsOpacityBetweenClearAndSolid(int opacity, int expected)
    {
        Assert.Equal(expected, WindowBackgrounds.Clamp(opacity));
    }

    [Theory]
    [InlineData(100, "100%")]
    [InlineData(55, "55%")]
    [InlineData(0, "0%")]
    public void Text_ShowsOpacityAsAPercentage(int opacity, string expected)
    {
        Assert.Equal(expected, WindowBackgrounds.Text(opacity));
    }
}
