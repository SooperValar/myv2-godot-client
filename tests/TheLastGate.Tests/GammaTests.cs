using TheLastGate.World;

namespace TheLastGate.Tests;

public class GammaTests
{
    [Fact]
    public void Range_RunsFromTheCurrentClientsNormalToItsBrightestInHundredths()
    {
        Assert.Equal(1f, Gamma.Normal);
        Assert.Equal(1f, Gamma.Lowest);
        Assert.Equal(1.2f, Gamma.Highest);
        Assert.Equal(0.01f, Gamma.Step);
    }

    [Theory]
    [InlineData(0.5f, 1f)]
    [InlineData(1.07f, 1.07f)]
    [InlineData(3f, 1.2f)]
    [InlineData(float.NaN, 1f)]
    public void Clamp_KeepsASavedGammaInsideTheRange(float saved, float gamma)
    {
        Assert.Equal(gamma, Gamma.Clamp(saved));
    }

    [Theory]
    [InlineData(1f, "1.00")]
    [InlineData(1.05f, "1.05")]
    [InlineData(1.2f, "1.20")]
    public void Text_ShowsTwoDecimalPlaces(float gamma, string text)
    {
        Assert.Equal(text, Gamma.Text(gamma));
    }
}
