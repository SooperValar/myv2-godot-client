using System.Numerics;
using TheLastGate.World;

namespace TheLastGate.Tests;

public class MovementGlideTests
{
    private const long Start = 1_000;
    private const double TickLength = 50;
    private const int Goblin = 77;

    private static readonly Dictionary<int, Vector2> NoCharacters = [];

    [Fact]
    public void WorldPoint_LaysTilesOnTheDiamondGridPlusTheirOffset()
    {
        Assert.Equal(new Vector2(16 * 1025 + 14, 8 * 1 + 7), MovementGlide.WorldPoint(513, 512, 14, 7));
    }

    [Fact]
    public void WorldPoint_IsSeamlessWhenAStepCrossesIntoTheNextTile()
    {
        var lastStepInTile = MovementGlide.WorldPoint(513, 512, 14, 7);
        var firstStepInNextTile = MovementGlide.WorldPoint(514, 512, 0, 0);

        Assert.Equal(new Vector2(2, 1), firstStepInNextTile - lastStepInTile);
    }

    [Fact]
    public void Player_SnapsOnTheFirstRecord()
    {
        var glide = new MovementGlide();

        glide.Record(new Vector2(100, 50), NoCharacters, Start, 1, TickLength);

        Assert.Equal(new Vector2(100, 50), glide.Player(Start));
    }

    [Fact]
    public void Player_GlidesOverOneTickLength()
    {
        var glide = new MovementGlide();
        glide.Record(new Vector2(100, 50), NoCharacters, Start, 1, TickLength);

        glide.Record(new Vector2(104, 52), NoCharacters, Start + 50, 1, TickLength);

        Assert.Equal(new Vector2(102, 51), glide.Player(Start + 75));
        Assert.Equal(new Vector2(104, 52), glide.Player(Start + 100));
    }

    [Fact]
    public void Player_TakesTwiceAsLongWhenTwoTicksArriveTogether()
    {
        var glide = new MovementGlide();
        glide.Record(new Vector2(100, 50), NoCharacters, Start, 1, TickLength);

        glide.Record(new Vector2(104, 52), NoCharacters, Start + 50, 2, TickLength);

        Assert.Equal(new Vector2(102, 51), glide.Player(Start + 100));
        Assert.Equal(new Vector2(104, 52), glide.Player(Start + 150));
    }

    [Fact]
    public void Character_IsUnknownUntilRecorded()
    {
        Assert.Null(new MovementGlide().Character(Goblin, Start));
    }

    [Fact]
    public void Character_SnapsWhenItFirstAppearsAndGlidesAfterwards()
    {
        var glide = new MovementGlide();
        glide.Record(Vector2.Zero, new Dictionary<int, Vector2> { [Goblin] = new(10, 10) }, Start, 1, TickLength);
        Assert.Equal(new Vector2(10, 10), glide.Character(Goblin, Start));

        glide.Record(Vector2.Zero, new Dictionary<int, Vector2> { [Goblin] = new(14, 10) }, Start + 50, 1, TickLength);

        Assert.Equal(new Vector2(12, 10), glide.Character(Goblin, Start + 75));
    }

    [Fact]
    public void Character_IsForgottenOnceItLeavesTheScreen()
    {
        var glide = new MovementGlide();
        glide.Record(Vector2.Zero, new Dictionary<int, Vector2> { [Goblin] = new(10, 10) }, Start, 1, TickLength);
        glide.Record(Vector2.Zero, NoCharacters, Start + 50, 1, TickLength);
        Assert.Null(glide.Character(Goblin, Start + 50));

        glide.Record(Vector2.Zero, new Dictionary<int, Vector2> { [Goblin] = new(14, 10) }, Start + 100, 1, TickLength);

        Assert.Equal(new Vector2(14, 10), glide.Character(Goblin, Start + 100));
    }
}
