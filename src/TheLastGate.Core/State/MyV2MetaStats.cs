using System.Linq;
using TheLastGate.Characters;

namespace TheLastGate.State;

// The MyV2 server sends only the numbers (SV_META); its meta_stats() in svr_do.c uses the same slots.
public static class MyV2MetaStats
{
    public const int FirstOffense = 7;
    public const int FirstDefense = 48;

    private const string PerSecond = "/s";
    private const string Percent = "%";

    private static readonly Meta[] Stats =
    [
        new(0, "Weapon Value", "Added to every blow you land, before the target's Armor Value is taken off.", LogFont.Orange),
        new(1, "Armor Value", "Taken off every blow that hits you. A melee blow then does a quarter of what is left, a spell three quarters.", LogFont.Teal),
        new(2, "Attack Speed", "Blows you swing per second, from your Speed.", LogFont.Orange, PerSecond, true),
        new(3, "Min Damage", "The least a normal blow does before armor: Weapon Value plus 1.", LogFont.Orange),
        new(4, "Max Damage", "The most a normal blow does before armor: Weapon Value plus 6 plus your Strength bonus.", LogFont.Orange),
        new(5, "Hit Score", "Your weapon skill, against the target's Parry Score. Each 4 points ahead hits more often.", LogFont.Orange),
        new(6, "Parry Score", "Your weapon skill, against the attacker's Hit Score. Lower from behind, from the side, or while not fighting back.", LogFont.Teal),
        new(7, "Average Damage", "What an average blow does before armor, lucky hits and critical hits included.", LogFont.Orange, "", true),
        new(8, "Damage/Second", "Average Damage times Attack Speed, before armor and misses.", LogFont.Orange, PerSecond, true),
        new(9, "Strength Bonus", "Up to this much extra damage on each blow, from half your Strength.", LogFont.Orange),
        new(10, "Lucky Hit +1d6", "The chance of a blow doing 1 to 6 extra damage.", LogFont.Orange, Percent, true),
        new(11, "Lucky Hit +2d6", "The chance of a blow doing 2 to 12 extra damage.", LogFont.Orange, Percent, true),
        new(12, "Critical Hit", "The chance of a two-handed axe blow doing double damage. Grows with Strength.", LogFont.Orange, Percent, true),
        new(48, "Deflect", "The chance of a two-handed sword turning a melee blow that would hit you. Grows with Agility.", LogFont.Teal, Percent, true),
        new(49, "Thorns", "Up to this much damage to whoever lands a melee blow on you.", LogFont.Teal),
        new(50, "Hit Point Regen", "Hit points back per second while standing still.", LogFont.Teal, PerSecond, true),
        new(51, "Endurance Regen", "Endurance back per second while standing still.", LogFont.Teal, PerSecond, true),
        new(52, "Mana Regen", "Mana back per second while standing still.", LogFont.Teal, PerSecond, true),
        new(53, "Resistance", "Against spells cast at you.", LogFont.Teal),
        new(54, "Immunity", "Against the strength of harmful spells on you.", LogFont.Teal),
    ];

    public static int OffenseRows => Stats.Count(stat => stat.Number >= FirstOffense && stat.Number < FirstDefense);

    public static int DefenseRows => Stats.Count(stat => stat.Number >= FirstDefense);

    public static void Fill(MetaStat[] stats)
    {
        foreach (var stat in Stats)
        {
            var meta = stats[stat.Number];
            NulTerminatedText.Write(meta.NameField, stat.Name);
            NulTerminatedText.Write(meta.DescriptionField, stat.Description);
            NulTerminatedText.Write(meta.AffixField, stat.Affix);
            meta.Font = (int)stat.Font;
            meta.Flag = stat.Hundredths ? 1 : 0;
        }
    }

    private sealed record Meta(
        int Number,
        string Name,
        string Description,
        LogFont Font,
        string Affix = "",
        bool Hundredths = false);
}
