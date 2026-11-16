using TheLastGate.Hud;

namespace TheLastGate.Tests;

public class CurrenciesTests
{
    [Theory]
    [InlineData(123456, "1234G 56S")]
    [InlineData(100, "1G  0S")]
    [InlineData(5, "0G  5S")]
    [InlineData(0, "0G  0S")]
    [InlineData(-250, "-2G -50S")]
    public void MoneyText_SplitsGoldAndSilverWithTwoPlacesForSilver(int gold, string expected)
    {
        Assert.Equal(expected, Currencies.MoneyText(gold));
    }

    [Fact]
    public void RowName_IsHiddenUntilThereArePoints()
    {
        Assert.Equal("???", Currencies.RowName("Casino", 0));
        Assert.Equal("Casino", Currencies.RowName("Casino", 7));
    }
}
