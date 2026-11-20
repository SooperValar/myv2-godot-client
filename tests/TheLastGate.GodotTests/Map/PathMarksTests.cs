using System.Numerics;
using GdUnit4;
using TheLastGate.Map;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Map;

[TestSuite]
public class PathMarksTests
{
    [TestCase]
    public void Dashes_RunSixPixelsOnAndFourOffUpToTheEnd()
    {
        var dashes = PathMarks.Dashes(Vector2.Zero, new Vector2(25, 0));

        AssertThat(dashes)
            .ContainsExactly(
                (new Vector2(0, 0), new Vector2(6, 0)),
                (new Vector2(10, 0), new Vector2(16, 0)),
                (new Vector2(20, 0), new Vector2(25, 0)));
    }

    [TestCase]
    public void Dashes_SkipALineShorterThanAPixel()
    {
        AssertThat(PathMarks.Dashes(Vector2.Zero, new Vector2(0.5f, 0))).IsEmpty();
    }

    [TestCase]
    public void Chevrons_LeaveOutASegmentShorterThanTheirSpacing()
    {
        AssertThat(PathMarks.Chevrons(Vector2.Zero, new Vector2(47, 0))).IsEmpty();
    }

    [TestCase]
    public void Chevrons_PointAlongTheSegmentEvery48PixelsFromHalfwayIntoTheFirstGap()
    {
        var chevrons = PathMarks.Chevrons(Vector2.Zero, new Vector2(100, 0));

        AssertThat(chevrons)
            .ContainsExactly(
                (new Vector2(21, 3), new Vector2(27, 0), new Vector2(21, -3)),
                (new Vector2(69, 3), new Vector2(75, 0), new Vector2(69, -3)));
    }

    [TestCase]
    public void Chevrons_TurnWithTheSegment()
    {
        var chevrons = PathMarks.Chevrons(Vector2.Zero, new Vector2(0, 60));

        AssertThat(chevrons).HasSize(1);
        AssertThat(chevrons[0]).IsEqual((new Vector2(-3, 21), new Vector2(0, 27), new Vector2(3, 21)));
    }
}
