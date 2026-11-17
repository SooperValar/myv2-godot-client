using System.Numerics;
using TheLastGate.World;

namespace TheLastGate.Tests;

public class GlideTrackTests
{
    private const long Start = 1_000;

    [Fact]
    public void ANewTrack_ShowsWhereItStarted()
    {
        var track = new GlideTrack(new Vector2(10, 20));

        Assert.Equal(new Vector2(10, 20), track.Shown(Start));
        Assert.Equal(new Vector2(10, 20), track.Shown(Start + 500));
    }

    [Fact]
    public void MoveTo_GlidesEvenlyOverTheDuration()
    {
        var track = new GlideTrack(Vector2.Zero);

        track.MoveTo(new Vector2(4, 2), Start, 50);

        Assert.Equal(Vector2.Zero, track.Shown(Start));
        Assert.Equal(new Vector2(2, 1), track.Shown(Start + 25));
        Assert.Equal(new Vector2(4, 2), track.Shown(Start + 50));
    }

    [Fact]
    public void MoveTo_HoldsStillOnceTheGlideEnds()
    {
        var track = new GlideTrack(Vector2.Zero);

        track.MoveTo(new Vector2(4, 2), Start, 50);

        Assert.Equal(new Vector2(4, 2), track.Shown(Start + 400));
    }

    [Fact]
    public void MoveTo_StartsFromWhereTheTrackIsShownNotWhereItWasHeading()
    {
        var track = new GlideTrack(Vector2.Zero);
        track.MoveTo(new Vector2(4, 0), Start, 50);

        track.MoveTo(new Vector2(8, 0), Start + 25, 50);

        Assert.Equal(new Vector2(2, 0), track.Shown(Start + 25));
        Assert.Equal(new Vector2(5, 0), track.Shown(Start + 50));
        Assert.Equal(new Vector2(8, 0), track.Shown(Start + 75));
    }

    [Fact]
    public void MoveTo_SnapsOverAJumpLongerThanTheLimit()
    {
        var track = new GlideTrack(Vector2.Zero);

        track.MoveTo(new Vector2(GlideTrack.JumpLimit + 1, 0), Start, 50);

        Assert.Equal(new Vector2(GlideTrack.JumpLimit + 1, 0), track.Shown(Start));
    }

    [Fact]
    public void MoveTo_GlidesAJumpExactlyAtTheLimit()
    {
        var track = new GlideTrack(Vector2.Zero);

        track.MoveTo(new Vector2(GlideTrack.JumpLimit, 0), Start, 50);

        Assert.Equal(new Vector2(GlideTrack.JumpLimit / 2, 0), track.Shown(Start + 25));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void MoveTo_SnapsWhenThereIsNoTimeToGlide(double duration)
    {
        var track = new GlideTrack(Vector2.Zero);

        track.MoveTo(new Vector2(4, 2), Start, duration);

        Assert.Equal(new Vector2(4, 2), track.Shown(Start));
    }
}
