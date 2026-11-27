using System.Linq;
using TheLastGate.Characters;

namespace TheLastGate.State;

// The MyV2 server sends only the numbers (SV_META); its meta_stats() in svr_do.c uses the same slots.
public static class MyV2MetaStats
{
    public const int FirstOffense = 7;
    public const int FirstDefense = 48;

    // Never shown: the planner reads them.
    public const int SpeedStepSlot = 86;
    public const int WeaponSkillSlot = 87;

    private const string PerSecond = "/s";
    private const string Percent = "%";
    private const string PassiveHeading = "  Passive Stats:";
    private const string ActiveHeading = "  Active Stats:";

    private static readonly Meta[] Stats =
    [
        new(0, "Compatibility", "The sum of your five base attributes. Bless, Protect and Enhance cast on you are held to it.", LogFont.Pink),
        new(1, "Spell Bonus", "How much stronger your race and the moon make your spells. A good moon adds 0.15, a new moon takes 0.15 off.", LogFont.Pink, "x", true),
        new(2, "Mana Cost Multiplier", "What a spell costs you, after Concentrate.", LogFont.Pink, Percent, true),
        new(3, "Attack Speed", "Blows and casts a second: your Speed plus attack speed bonuses. Fast mode is quicker and costs endurance.", LogFont.Green, PerSecond, true),
        new(4, "Weapon Value", "Added to every blow you land, before the target's Armor Value is taken off.", LogFont.Orange),
        new(5, "Hit Score", "Your weapon skill, against the target's Parry Score. Each 4 points ahead hits more often.", LogFont.Orange),
        new(6, "Parry Score", "Your weapon skill, against the attacker's Hit Score. Lower from behind, from the side, or while not fighting back.", LogFont.Teal),
        new(7, PassiveHeading, "", LogFont.White),
        new(8, "Est. Melee Hit Dmg", "What an average blow does before armor, lucky hits and critical hits included.", LogFont.Orange, "", true),
        new(9, "Est. Melee DPS", "Est. Melee Hit Dmg times Attack Speed, before armor and misses.", LogFont.Orange, PerSecond, true),
        new(10, "Melee Ceiling Damage", "The most a normal blow does before armor: Weapon Value plus 6 plus your Strength Bonus.", LogFont.Orange),
        new(11, "Melee Floor Damage", "The least a normal blow does before armor: Weapon Value plus 1.", LogFont.Orange),
        new(12, "Strength Bonus", "Up to this much extra damage on each blow, from half your Strength.", LogFont.Orange),
        new(13, "Lucky Hit +1d6", "The chance of a blow doing 1 to 6 extra damage.", LogFont.Orange, Percent, true),
        new(14, "Lucky Hit +2d6", "The chance of a blow doing 2 to 12 extra damage.", LogFont.Orange, Percent, true),
        new(15, "Critical Chance", "The chance of a two-handed axe blow doing double damage. Grows with Strength.", LogFont.Orange, Percent, true),
        new(16, "Critical Multiplier", "What a critical hit does to a blow's damage.", LogFont.Orange, Percent),
        new(17, "Surround Hit", "Your Surround Hit skill: hitting the enemies around you at once.", LogFont.Orange),
        new(18, ActiveHeading, "", LogFont.White),
        new(19, "Bless Effect", "Attributes your Bless gives when cast on yourself.", LogFont.Pink, "Attribs"),
        new(20, "Enhance Effect", "Weapon Value your Enhance Weapon gives when cast on yourself.", LogFont.Pink, "WV"),
        new(21, "Protect Effect", "Armor Value your Protection gives when cast on yourself.", LogFont.Pink, "AV"),
        new(22, "M.Shield Effect", "Armor Value your Magic Shield starts with. It wears down as it lasts.", LogFont.Pink, "AV"),
        new(23, "M.Shield Duration", "How long your Magic Shield lasts.", LogFont.Pink, "Seconds", true),
        new(24, "Heal Effect", "Hit points your Heal gives back.", LogFont.Pink, "HP"),
        new(25, "Curse Effect", "Attributes your Curse takes off, before the target's Immunity.", LogFont.Pink, "Attribs"),
        new(26, "Stun Duration", "How long your Stun holds, before the target's Immunity.", LogFont.Pink, "Seconds", true),
        new(27, "Blast Damage", "What your Blast does, before the target's Immunity and armor. Others nearby take three quarters.", LogFont.Pink),
        new(28, "Blast Mana Cost", "Mana a Blast costs you.", LogFont.Pink),
        new(29, "Ghost Comp Strength", "How strong your Ghost Companion comes out, with your race, the moon, a staff and Intuition.", LogFont.Pink),
        new(48, PassiveHeading, "", LogFont.White),
        new(49, "Armor Value", "Taken off every blow that hits you. A melee blow then does a quarter of what is left, a spell three quarters.", LogFont.Teal),
        new(50, "Health Regen Rate", "Hit points back per second while standing still.", LogFont.Green, PerSecond, true),
        new(51, "Endurance Regen Rate", "Endurance back per second while standing still.", LogFont.Green, PerSecond, true),
        new(52, "Mana Regen Rate", "Mana back per second while standing still.", LogFont.Pink, PerSecond, true),
        new(53, "Effective Immunity", "Half your Immunity: taken off the power of every harmful spell cast at you.", LogFont.Teal),
        new(54, "Effective Resistance", "Your Resistance against spells cast at you.", LogFont.Teal),
        new(55, "Deflect Chance", "The chance of a two-handed sword turning a melee blow that would hit you. Grows with Agility.", LogFont.Teal, Percent, true),
        new(56, "Thorns Score", "Up to this much damage to whoever lands a melee blow on you.", LogFont.Teal),
        new(57, "Walk Speed", "Steps a second: your Speed plus walk speed bonuses.", LogFont.Green, PerSecond, true),
        new(58, "Speed", "Actions a second for picking up, using and giving; walk and attack speed start from it. Agility, Strength, the speed mode and Speed bonuses set it.", LogFont.Green, PerSecond, true),
        new(SpeedStepSlot, "Speed Step", "", LogFont.White, Listed: false),
        new(WeaponSkillSlot, "Weapon Skill Number", "", LogFont.White, Listed: false),
    ];

    public static int[] OffenseSlots { get; } = Slots(FirstOffense, FirstDefense);

    public static int[] DefenseSlots { get; } = Slots(FirstDefense, Terminology.MetaStatCount);

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

    private static int[] Slots(int first, int end) =>
        Stats.Where(stat => stat.Listed && stat.Number >= first && stat.Number < end).Select(stat => stat.Number).ToArray();

    private sealed record Meta(
        int Number,
        string Name,
        string Description,
        LogFont Font,
        string Affix = "",
        bool Hundredths = false,
        bool Listed = true);
}
