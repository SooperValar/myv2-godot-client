using TheLastGate.World;

namespace TheLastGate.Tests;

public class AutoWalkTests
{
    [Theory]
    [InlineData(289, 272, WalkDirection.North)]
    [InlineData(289, 622, WalkDirection.West)]
    [InlineData(990, 622, WalkDirection.South)]
    [InlineData(990, 272, WalkDirection.East)]
    public void EachCornerButton_WalksItsWay(int screenX, int screenY, WalkDirection direction) =>
        Assert.Equal(direction, AutoWalk.ButtonAt(screenX, screenY));

    [Theory]
    [InlineData(289 + 11, 272)]
    [InlineData(289 + 8, 272 + 8)]
    public void AClickJustInsideTheButton_Counts(int screenX, int screenY) =>
        Assert.Equal(WalkDirection.North, AutoWalk.ButtonAt(screenX, screenY));

    [Theory]
    [InlineData(289 + 12, 272)]
    [InlineData(289 + 9, 272 + 8)]
    [InlineData(640, 360)]
    public void AClickAwayFromTheButtons_IsNotOne(int screenX, int screenY) =>
        Assert.Null(AutoWalk.ButtonAt(screenX, screenY));
}
