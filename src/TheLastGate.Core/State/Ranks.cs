using System;

namespace TheLastGate.State;

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

    public static int Number(int points)
    {
        var rank = Array.FindIndex(PointsNeeded, needed => points < needed);
        return rank < 0 ? PointsNeeded.Length : rank;
    }

    public static string Name(int points) => Names[Number(points)];

    public static double Progress(int points)
    {
        var rank = Number(points);
        if (rank == PointsNeeded.Length)
        {
            return 1;
        }

        var start = rank == 0 ? 0 : PointsNeeded[rank - 1];
        return Math.Max(0, (double)(points - start) / (PointsNeeded[rank] - start));
    }
}
