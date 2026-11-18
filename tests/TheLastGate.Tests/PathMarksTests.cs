using System.Numerics;
using TheLastGate.Map;

namespace TheLastGate.Tests;

public class PathMarksTests
{
    [Fact]
    public void Dashes_RunSixPixelsOnAndFourOffUpToTheEnd()
    {
        var dashes = PathMarks.Dashes(Vector2.Zero, new Vector2(25, 0));

        Assert.Equal(
            [
                (new Vector2(0, 0), new Vector2(6, 0)),
                (new Vector2(10, 0), new Vector2(16, 0)),
                (new Vector2(20, 0), new Vector2(25, 0)),
            ],
            dashes);
    }

    [Fact]
    public void Dashes_SkipALineShorterThanAPixel()
    {
        Assert.Empty(PathMarks.Dashes(Vector2.Zero, new Vector2(0.5f, 0)));
    }

    [Fact]
    public void Chevrons_LeaveOutASegmentShorterThanTheirSpacing()
    {
        Assert.Empty(PathMarks.Chevrons(Vector2.Zero, new Vector2(47, 0)));
    }

    [Fact]
    public void Chevrons_PointAlongTheSegmentEvery48PixelsFromHalfwayIntoTheFirstGap()
    {
        var chevrons = PathMarks.Chevrons(Vector2.Zero, new Vector2(100, 0));

        Assert.Equal(
            [
                (new Vector2(21, 3), new Vector2(27, 0), new Vector2(21, -3)),
                (new Vector2(69, 3), new Vector2(75, 0), new Vector2(69, -3)),
            ],
            chevrons);
    }

    [Fact]
    public void Chevrons_TurnWithTheSegment()
    {
        var chevron = Assert.Single(PathMarks.Chevrons(Vector2.Zero, new Vector2(0, 60)));

        Assert.Equal((new Vector2(-3, 21), new Vector2(0, 27), new Vector2(3, 21)), chevron);
    }
}
