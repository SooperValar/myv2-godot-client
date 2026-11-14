using TheLastGate.World;

namespace TheLastGate.Tests;

public class NameplateTextTests
{
    private const int Id = 41;

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(false, false, false)]
    public void IsShown_WhenEitherNamesOrPercentagesAreOn(bool showNames, bool showPercent, bool shown)
    {
        Assert.Equal(shown, NameplateText.IsShown(showNames, showPercent));
    }

    [Theory]
    [InlineData(64, true, true, "Guard 64%")]
    [InlineData(0, true, true, "Guard")]
    [InlineData(64, true, false, "Guard")]
    [InlineData(64, false, true, "64%")]
    [InlineData(0, false, true, "")]
    [InlineData(64, false, false, "")]
    public void For_CombinesTheNameAndPercentageAsTheOptionsSay(
        int healthPercent,
        bool showNames,
        bool showPercent,
        string text)
    {
        Assert.Equal(text, NameplateText.For("Guard", Id, healthPercent, showNames, showPercent));
    }

    [Fact]
    public void For_ShowsAPercentageAfterAnEmptyNameLikeTheCurrentClient()
    {
        Assert.Equal(" 64%", NameplateText.For("", Id, 64, true, true));
    }

    [Fact]
    public void For_ShowsOnlyTheNameForACharacterWithoutAnId()
    {
        Assert.Equal("Guard", NameplateText.For("Guard", 0, 64, false, true));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    [InlineData(5, 5)]
    [InlineData(9, 9)]
    public void FontFor_SwapsTheFirstTwoColours(int textColour, int font)
    {
        Assert.Equal(font, NameplateText.FontFor(textColour));
    }

    [Theory]
    [InlineData("Guard 64%", -22)]
    [InlineData("Guard", -12)]
    [InlineData("", 0)]
    public void LeftOffset_CentresAtFivePixelsALetterLikeTheCurrentClient(string text, int offset)
    {
        Assert.Equal(offset, NameplateText.LeftOffset(text));
    }
}
