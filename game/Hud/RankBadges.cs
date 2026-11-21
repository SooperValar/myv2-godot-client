using TheLastGate.State;

namespace TheLastGate.Hud;

public static class RankBadges
{
    private static readonly int[] Sprites =
    [
        10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 6, 7, 8, 9,
    ];

    public static int Sprite(int points) => Sprites[Ranks.Number(points)];
}
