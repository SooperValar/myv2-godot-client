using TheLastGate.World;

namespace TheLastGate.Tests;

public class CharacterShadowTests
{
    [Theory]
    [InlineData(1999, false)]
    [InlineData(2000, true)]
    [InlineData(16335, true)]
    [InlineData(16336, false)]
    [InlineData(17360, false)]
    [InlineData(17361, true)]
    [InlineData(0, false)]
    public void Casts_OnlyForTheCurrentClientsCharacterSpriteRanges(int spriteNumber, bool casts)
    {
        Assert.Equal(casts, CharacterShadow.Casts(spriteNumber));
    }

    [Theory]
    [InlineData(64, 96, -32, 24)]
    [InlineData(32, 64, -16, 16)]
    [InlineData(70, 100, -32, 24)]
    [InlineData(20, 20, 0, 0)]
    public void For_SquashesTheSpriteToAQuarterOfItsWholeTilesUnderTheFeet(
        int width,
        int height,
        int left,
        int shadowHeight)
    {
        Assert.Equal(new CharacterShadow(left, -10, shadowHeight), CharacterShadow.For(width, height));
    }
}
