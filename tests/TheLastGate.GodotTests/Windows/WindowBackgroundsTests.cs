using GdUnit4;
using TheLastGate.Windows;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Windows;

[TestSuite]
public class WindowBackgroundsTests
{
    [TestCase]
    public void Rows_HaveOneRowForEveryWindowThatFades()
    {
        AssertThat(WindowBackgrounds.Rows.Select(row => row.Window))
            .ContainsExactly(
                Enum.GetValues<WindowBackground>().Where(window => window != WindowBackground.None).ToArray());
    }

    [TestCase]
    public void Rows_HaveDistinctSettingKeys()
    {
        var keys = WindowBackgrounds.Rows.Select(row => row.SettingKey).ToList();

        AssertThat(keys.Distinct().Count()).IsEqual(keys.Count);
    }

    [TestCase(-20, 0)]
    [TestCase(0, 0)]
    [TestCase(55, 55)]
    [TestCase(100, 100)]
    [TestCase(250, 100)]
    public void Clamp_KeepsOpacityBetweenClearAndSolid(int opacity, int expected)
    {
        AssertThat(WindowBackgrounds.Clamp(opacity)).IsEqual(expected);
    }

    [TestCase(100, "100%")]
    [TestCase(55, "55%")]
    [TestCase(0, "0%")]
    public void Text_ShowsOpacityAsAPercentage(int opacity, string expected)
    {
        AssertThat(WindowBackgrounds.Text(opacity)).IsEqual(expected);
    }
}
