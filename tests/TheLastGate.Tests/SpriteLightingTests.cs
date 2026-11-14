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

    [Fact]
    public void Brightness_DarkensWithEveryLevel()
    {
        for (var light = 1; light <= SpriteLighting.DarkestLight; light++)
        {
            Assert.True(SpriteLighting.Brightness(light) < SpriteLighting.Brightness(light - 1));
        }
    }
}
