using System;

namespace TheLastGate.Hud;

public static class Ranks
{
    private static readonly int[] PointsNeeded =
    [
        250, 1750, 7000, 21000, 52500, 115500, 231000, 429000, 750750, 1251250, 2002000, 3094000, 4641000, 6783000,
        9690000, 13566000, 18653250, 25236750, 33649000, 44275000, 57557500, 74002500, 94185000, 118755000,
    ];

    private static readonly string[] Names =
    [
        "Private", "Private First Class", "Lance Corporal", "Corporal", "Sergeant", "Staff Sergeant",
        "Master Sergeant", "First Sergeant", "Sergeant Major", "Second Lieutenant", "First Lieutenant", "Captain",
        "Major", "Lieutenant Colonel", "Colonel", "Brigadier General", "Major General", "Lieutenant General",
        "General", "Field Marshal", "Knight", "Baron", "Earl", "Marquess", "Warlord",
    ];

    private static readonly int[] BadgeSprites =
    [
        10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 6, 7, 8, 9,
    ];

    public static Rank For(int points)
    {
        var rank = Array.FindIndex(PointsNeeded, needed => points < needed);
        if (rank < 0)
        {
            rank = PointsNeeded.Length;
        }

        return new Rank(Names[rank], BadgeSprites[rank]);
    }

    public static double Progress(int points) => throw new NotImplementedException();
}
