using GdUnit4;
using TheLastGate.World;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.World;

[TestSuite]
public class WalkOffsetTests
{
    private const int OneStepEveryTick = 0;
    private const int OneStepEveryOtherTick = 10;

    [TestCase]
    public void WalkOffset_GoesUpInEvenStepsWhenEveryTickMovesAFrame()
    {
        AssertThat(WalkOffset.Distance(3, 8, OneStepEveryTick, true, 10)).IsEqual(12);
    }

    [TestCase]
    public void WalkOffset_CountsTheTicksBetweenFramesAtSlowerSpeeds()
    {
        AssertThat(WalkOffset.Distance(2, 8, OneStepEveryOtherTick, true, 10)).IsEqual(10);
    }

    [TestCase]
    public void WalkOffset_WrapsAroundTheServerCycle()
    {
        AssertThat(WalkOffset.Distance(2, 8, OneStepEveryOtherTick, true, 1)).IsEqual(8);
    }

    [TestCase]
    public void WalkOffset_GoesStraightByFramesWhileNotAdvancing()
    {
        AssertThat(WalkOffset.Distance(3, 8, OneStepEveryOtherTick, false, 10)).IsEqual(12);
        AssertThat(WalkOffset.Distance(3, 12, OneStepEveryOtherTick, false, 10)).IsEqual(8);
    }
}
