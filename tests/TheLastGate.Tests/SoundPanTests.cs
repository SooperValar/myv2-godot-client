using TheLastGate.Audio;

namespace TheLastGate.Tests;

public class SoundPanTests
{
    [Fact]
    public void Gains_LeaveBothSidesFullForACentredSound()
    {
        Assert.Equal((1f, 1f), SoundPan.Gains(0));
    }

    [Theory]
    [InlineData(600, 1, 0.50119)]
    [InlineData(2000, 1, 0.1)]
    [InlineData(-600, 0.50119, 1)]
    [InlineData(-2000, 0.1, 1)]
    public void Gains_TurnTheFarSideDownByThePanInHundredthsOfADecibel(int serverPan, float left, float right)
    {
        var gains = SoundPan.Gains(serverPan);

        Assert.Equal(left, gains.Left, 4);
        Assert.Equal(right, gains.Right, 4);
    }

    [Theory]
    [InlineData(0, 0, 1)]
    [InlineData(2000, -0.81818, 0.55)]
    [InlineData(-2000, 0.81818, 0.55)]
    public void Panner_GivesGodotsPanAndTheVolumeThatUndoesItsBoost(int serverPan, float pan, float volumeScale)
    {
        var panner = SoundPan.Panner(serverPan);

        Assert.Equal(pan, panner.Pan, 4);
        Assert.Equal(volumeScale, panner.VolumeScale, 4);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(150)]
    [InlineData(-150)]
    [InlineData(600)]
    [InlineData(-3500)]
    [InlineData(10000)]
    public void Panner_MatchesTheGainsThroughGodotsPanner(int serverPan)
    {
        var (pan, volumeScale) = SoundPan.Panner(serverPan);
        var (left, right) = SoundPan.Gains(serverPan);

        Assert.Equal(left, (1 - pan) * volumeScale, 4);
        Assert.Equal(right, (1 + pan) * volumeScale, 4);
    }
}
