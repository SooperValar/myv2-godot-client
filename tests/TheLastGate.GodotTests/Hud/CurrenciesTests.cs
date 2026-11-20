using GdUnit4;
using TheLastGate.Hud;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Hud;

[TestSuite]
public class CurrenciesTests
{
    [TestCase(123456, "1234G 56S")]
    [TestCase(100, "1G  0S")]
    [TestCase(5, "0G  5S")]
    [TestCase(0, "0G  0S")]
    [TestCase(-250, "-2G -50S")]
    public void MoneyText_SplitsGoldAndSilverWithTwoPlacesForSilver(int gold, string expected)
    {
        AssertThat(Currencies.MoneyText(gold)).IsEqual(expected);
    }

    [TestCase]
    public void RowName_IsHiddenUntilThereArePoints()
    {
        AssertThat(Currencies.RowName("Casino", 0)).IsEqual("???");
        AssertThat(Currencies.RowName("Casino", 7)).IsEqual("Casino");
    }
}
