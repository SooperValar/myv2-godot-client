using TheLastGate.Characters;

namespace TheLastGate.State;

// The MyV2 server never sends skill names, so the client knows them, as merc.exe does.
public static class MyV2Skills
{
    public const int Listed = 37;

    private const int Braveness = 0;
    private const int Willpower = 1;
    private const int Intuition = 2;
    private const int Agility = 3;
    private const int Strength = 4;

    private const int BaseValue = 0;
    private const int Limit = 2;
    private const int FixedLimit = 1;
    private const int NotShown = 0;

    private static readonly Skill[] Skills =
    [
        new(0, 'C', "Hand to Hand", "Fighting without weapons.", [Braveness, Agility, Strength]),
        new(1, 'C', "Karate", "Fighting without weapons and doing damage.", [Braveness, Agility, Strength]),
        new(2, 'C', "Dagger", "Fighting with daggers or similar weapons.", [Braveness, Agility, Intuition]),
        new(3, 'C', "Sword", "Fighting with swords or similar weapons.", [Braveness, Agility, Strength]),
        new(4, 'C', "Axe", "Fighting with axes or similar weapons.", [Braveness, Strength, Strength]),
        new(5, 'C', "Staff", "Fighting with staffs or similar weapons.", [Agility, Strength, Strength]),
        new(6, 'C', "Two-Handed", "Fighting with two-handed weapons.", [Agility, Strength, Strength]),
        new(7, 'G', "Lock-Picking", "Opening doors without keys.", [Intuition, Willpower, Agility]),
        new(8, 'G', "Stealth", "Moving without being seen or heard.", [Intuition, Willpower, Agility]),
        new(9, 'G', "Perception", "Seeing and hearing.", [Intuition, Willpower, Agility]),
        new(10, 'M', "Swimming", "Moving through water without drowning.", [Intuition, Willpower, Agility]),
        new(11, 'R', "Magic Shield", "Spell: Create a magic shield (Cost: 25 Mana).", [Braveness, Intuition, Willpower]),
        new(12, 'G', "Bartering", "Getting good prices from merchants.", [Braveness, Intuition, Willpower]),
        new(13, 'G', "Repair", "Repairing items.", [Intuition, Willpower, Agility]),
        new(14, 'R', "Light", "Spell: Create light (Cost: 5 Mana).", [Braveness, Intuition, Willpower]),
        new(15, 'R', "Recall", "Spell: Teleport to temple (Cost: 15 Mana).", [Braveness, Intuition, Willpower]),
        new(16, 'R', "Guardian Angel", "Spell: Avoid loss of HPs and items on death.", [Braveness, Intuition, Willpower]),
        new(17, 'R', "Protection", "Spell: Enhance Armor of target (Cost: 15 Mana).", [Braveness, Intuition, Willpower]),
        new(18, 'R', "Enhance Weapon", "Spell: Enhance Weapon of target (Cost: 15 Mana).", [Braveness, Intuition, Willpower]),
        new(19, 'R', "Stun", "Spell: Make target motionless (Cost: 20 Mana).", [Braveness, Intuition, Willpower]),
        new(20, 'R', "Curse", "Spell: Decrease attributes of target (Cost: 35 Mana).", [Braveness, Intuition, Willpower]),
        new(21, 'R', "Bless", "Spell: Increase attributes of target (Cost: 35 Mana).", [Braveness, Intuition, Willpower]),
        new(22, 'R', "Identify", "Spell: Read stats of item/character. (Cost: 25 Mana)", [Braveness, Intuition, Willpower]),
        new(23, 'G', "Resistance", "Resist against magic.", [Intuition, Willpower, Strength]),
        new(24, 'R', "Blast", "Spell: Inflict injuries to target (Cost: varies).", [Intuition, Willpower, Strength]),
        new(25, 'R', "Dispel Magic", "Spell: Removes curse magic from target (Cost: 25 Mana).", [Braveness, Intuition, Willpower]),
        new(26, 'R', "Heal", "Spell: Heal injuries (Cost: 25 Mana).", [Braveness, Intuition, Willpower]),
        new(27, 'R', "Ghost Companion", "Spell: Create a ghost to attack an enemy.", [Braveness, Intuition, Willpower]),
        new(28, 'B', "Regenerate", "Regenerate Hitpoints faster.", [Strength, Strength, Strength]),
        new(29, 'B', "Rest", "Regenerate Endurance faster.", [Agility, Agility, Agility]),
        new(30, 'B', "Meditate", "Regenerate Mana faster.", [Intuition, Willpower, Willpower]),
        new(31, 'G', "Sense Magic", "Find out who casts what at you.", [Braveness, Intuition, Willpower]),
        new(32, 'G', "Immunity", "Partial immunity against negative magic.", [Braveness, Agility, Strength]),
        new(33, 'G', "Surround Hit", "Hit all your enemies at once.", [Braveness, Agility, Strength]),
        new(34, 'G', "Concentrate", "Reduces mana cost for all spells.", [Willpower, Willpower, Willpower]),
        new(35, 'G', "Warcry", "Frighten all enemies in hearing distance.", [Braveness, Braveness, Strength]),
        new(36, 'C', "Greataxe", "Fighting with greataxes (two-handed axes).", [Strength, Strength, Agility]),
    ];

    public static void Fill(SkillText[] texts)
    {
        foreach (var skill in Skills)
        {
            var text = texts[skill.Number];
            text.SortKey = skill.SortKey;
            NulTerminatedText.Write(text.NameField, skill.Name);
            NulTerminatedText.Write(text.DescriptionField, skill.Description);
            skill.Attributes.CopyTo(text.Attributes, 0);
        }
    }

    // merc.exe shows a skill not learnt as unused, and one that can never go past 1 in red without a raise button.
    public static void ShowFor(SkillText[] texts, int[][] values)
    {
        for (var number = 0; number < texts.Length && number < values.Length; number++)
        {
            texts[number].Show = number >= Listed || values[number][BaseValue] == 0
                ? NotShown
                : values[number][Limit] <= FixedLimit ? SkillShows.Red : SkillShows.Known;
        }
    }

    private sealed record Skill(int Number, char SortKey, string Name, string Description, int[] Attributes);
}
