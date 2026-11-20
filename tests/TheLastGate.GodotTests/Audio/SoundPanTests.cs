using GdUnit4;
using TheLastGate.Audio;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Audio;

[TestSuite]
public class SoundPanTests
{
    private const int DecimalPlaces = 4;

    [TestCase]
    public void Gains_LeaveBothSidesFullForACentredSound() => AssertThat(SoundPan.Gains(0)).IsEqual((1f, 1f));

    [TestCase(600, 1, 0.50119f)]
    [TestCase(2000, 1, 0.1f)]
    [TestCase(-600, 0.50119f, 1)]
    [TestCase(-2000, 0.1f, 1)]
    public void Gains_TurnTheFarSideDownByThePanInHundredthsOfADecibel(int serverPan, float left, float right)
    {
        var gains = SoundPan.Gains(serverPan);

        AssertThat(Rounded(gains.Left)).IsEqual(Rounded(left));
        AssertThat(Rounded(gains.Right)).IsEqual(Rounded(right));
    }

    [TestCase(0, 0, 1)]
    [TestCase(2000, -0.81818f, 0.55f)]
    [TestCase(-2000, 0.81818f, 0.55f)]
    public void Panner_GivesGodotsPanAndTheVolumeThatUndoesItsBoost(int serverPan, float pan, float volumeScale)
    {
        var panner = SoundPan.Panner(serverPan);

        AssertThat(Rounded(panner.Pan)).IsEqual(Rounded(pan));
        AssertThat(Rounded(panner.VolumeScale)).IsEqual(Rounded(volumeScale));
    }

    [TestCase(0)]
    [TestCase(150)]
    [TestCase(-150)]
    [TestCase(600)]
    [TestCase(-3500)]
    [TestCase(10000)]
    public void Panner_MatchesTheGainsThroughGodotsPanner(int serverPan)
    {
        var (pan, volumeScale) = SoundPan.Panner(serverPan);
        var (left, right) = SoundPan.Gains(serverPan);

        AssertThat(Rounded((1 - pan) * volumeScale)).IsEqual(Rounded(left));
        AssertThat(Rounded((1 + pan) * volumeScale)).IsEqual(Rounded(right));
    }

    private static float Rounded(float value) => MathF.Round(value, DecimalPlaces);
}
