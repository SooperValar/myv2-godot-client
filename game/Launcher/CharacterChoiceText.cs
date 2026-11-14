using System.Collections.Generic;
using TheLastGate.Characters;

namespace TheLastGate.Launcher;

public static class CharacterChoiceText
{
    public static readonly IReadOnlyDictionary<CharacterClass, string> ClassNames =
        new Dictionary<CharacterClass, string>
        {
            [CharacterClass.Templar] = "Templar",
            [CharacterClass.Mercenary] = "Mercenary",
            [CharacterClass.Harakim] = "Harakim",
            [CharacterClass.ArchTemplar] = "Arch-Templar",
            [CharacterClass.Skald] = "Skald",
            [CharacterClass.Warrior] = "Warrior",
            [CharacterClass.Seyan] = "Seyan",
            [CharacterClass.Sorcerer] = "Sorcerer",
            [CharacterClass.Summoner] = "Summoner",
            [CharacterClass.ArchHarakim] = "Arch-Harakim",
            [CharacterClass.Braver] = "Braver",
            [CharacterClass.Lycanthrope] = "Lycanthrope",
        };

    public static readonly IReadOnlyDictionary<CharacterClass, string> ClassDescriptions =
        new Dictionary<CharacterClass, string>
        {
            [CharacterClass.Templar] =
                "An adept at melee combat, these fighters can become heavy hitters or unbreakable tanks. They learn only a few utility spells.",
            [CharacterClass.Mercenary] =
                "Jack of all trades, master of none. These fighters can do a bit of everything, and can become capable in any situation.",
            [CharacterClass.Harakim] =
                "Spellcaster extraordinare, these fighters can summon capable companions and hit foes with a powerful long-range blast.",
        };

    public static readonly IReadOnlyDictionary<GameMode, string> GameModeNames = new Dictionary<GameMode, string>
    {
        [GameMode.Normal] = "Normal",
        [GameMode.Hard] = "Hard Mode",
        [GameMode.Roguelike] = "Roguelike",
    };

    public static readonly IReadOnlyDictionary<GameMode, string> GameModeDescriptions = new Dictionary<GameMode, string>
    {
        [GameMode.Normal] = "The normal The Last Gate experience.",
        [GameMode.Hard] = "Stricter gameplay experience. Good luck!",
        [GameMode.Roguelike] = "Progress through a series of trials with unique, randomized properties.",
    };

    public static readonly IReadOnlyDictionary<Season, string> SeasonDescriptions = new Dictionary<Season, string>
    {
        [Season.Legacy] = "No seasonal effects will be applied and the game will behave as normal.",
        [Season.Current] =
            "Your character will play in the current 'season'. You may only group with other players or interact with items found in the same 'season'.",
    };
}
