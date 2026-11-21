using GdUnit4;
using TheLastGate.Hud;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Hud;

[TestSuite]
public class RankBadgesTests
{
    [TestCase(-1, 10)]
    [TestCase(0, 10)]
    [TestCase(249, 10)]
    [TestCase(250, 11)]
    [TestCase(1750, 12)]
    [TestCase(750749, 18)]
    [TestCase(750750, 19)]
    [TestCase(44274999, 29)]
    [TestCase(44275000, 30)]
    [TestCase(57557500, 6)]
    [TestCase(74002500, 7)]
    [TestCase(118754999, 8)]
    [TestCase(118755000, 9)]
    [TestCase(int.MaxValue, 9)]
    public void Sprite_IsTheBadgeForTheRank(int points, int sprite)
    {
        AssertThat(RankBadges.Sprite(points)).IsEqual(sprite);
    }
}
