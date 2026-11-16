using System.Collections.Generic;
using TheLastGate.Assets;
using TheLastGate.Net;
using TheLastGate.State;

namespace TheLastGate.Stats;

public static class SkillClicks
{
    private const int FirstClassSkill = 50;
    private const int SecondClassSkill = 51;
    private const int IdentifySkill = 52;
    private const int FirstLycanthropeSkill = 53;
    private const int SecondLycanthropeSkill = 54;

    private static readonly string[] AttributeHelp =
    [
        "Braveness improves most skills and spells. It also improves your critical hit rate.",
        "Willpower improves most support spells. It also improves the speed of casting spells and using skills, "
        + "and helps overwhelm spell suppression.",
        "Intuition improves most offensive spells. It also reduces the duration of skill exhaustion.",
        "Agility improves most combat skills. It also improves your movement speed and your attack speed.",
        "Strength improves most combat skills. It also improves your movement speed and the damage dealt by your "
        + "attacks.",
    ];

    public static byte[] Use(int skillNumber, int selectedCharacter, int firstAttribute) =>
        ClientPackets.ThreeValues(ClientCommand.Skill, skillNumber, selectedCharacter, firstAttribute);

    public static bool IsKnown(int skillNumber, bool learnt, uint kindred) => learnt || skillNumber switch
    {
        FirstClassSkill or SecondClassSkill => true,
        IdentifySkill => (kindred & Kindred.KnowsIdentify) != 0,
        FirstLycanthropeSkill or SecondLycanthropeSkill => (kindred & Kindred.Lycanthrope) != 0,
        _ => false,
    };

    public static IReadOnlyList<LogLine> Help(string name, string description, int[] attributes) =>
    [
        new(name, (int)TextColour.Orange),
        new(description, (int)TextColour.Yellow),
        new(
            $"Affected by: {StatLines.AttributeNames[attributes[0]]}, {StatLines.AttributeNames[attributes[1]]}, "
            + $"{StatLines.AttributeNames[attributes[2]]}",
            (int)TextColour.Orange),
    ];

    public static LogLine StatHelp(int slot) => new(
        slot switch
        {
            StatPlan.HitPointsSlot => "Your Hitpoints.",
            StatPlan.ManaSlot => "Your Mana.",
            _ => AttributeHelp[slot],
        },
        (int)TextColour.Yellow);
}
