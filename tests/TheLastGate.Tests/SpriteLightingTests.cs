using TheLastGate.World;

namespace TheLastGate.Tests;

public class SpriteLightingTests
{
    [Fact]
    public void Brightness_IsFullAtLightZero()
    {
        Assert.Equal(1f, SpriteLighting.Brightness(0));
    }

    [Theory]
    [InlineData(1, 120f / 121f)]
    [InlineData(5, 120f / 145f)]
    [InlineData(15, 120f / 345f)]
    public void Brightness_FollowsTheCurrentClientsCurve(int light, float brightness)
    {
        Assert.Equal(brightness, SpriteLighting.Brightness(light));
    }

    [Theory]
    [InlineData(5, 1.1f, 620f / 645f)]
    [InlineData(15, 1.2f, 1120f / 1345f)]
    [InlineData(15, 1f, 120f / 345f)]
    public void Brightness_UsesTheWorldGammaAsTheCurrentClientUsedItsGamma(
        int light,
        float worldGamma,
        float brightness)
    {
        Assert.Equal(brightness, SpriteLighting.Brightness(light, worldGamma), 5);
    }

    [Fact]
    public void Brightness_LightsDarkTilesMoreWithAHigherWorldGamma()
    {
        Assert.True(SpriteLighting.Brightness(10, Gamma.Highest) > SpriteLighting.Brightness(10, Gamma.Normal));
        Assert.Equal(1f, SpriteLighting.Brightness(0, Gamma.Highest));
    }

    [Fact]
    public void Brightness_DarkensWithEveryLevel()
    {
        for (var light = 1; light <= SpriteLighting.DarkestLight; light++)
        {
            Assert.True(SpriteLighting.Brightness(light) < SpriteLighting.Brightness(light - 1));
        }
    }
}
