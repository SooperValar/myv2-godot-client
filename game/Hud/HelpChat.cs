using System;
using System.Collections.Generic;
using TheLastGate.State;

namespace TheLastGate.Hud;

public sealed record HelpRequest(
    HelpPageNumber Tab,
    int? Quest = null,
    bool AllQuests = false,
    WeaponList? Weapons = null);

// The chat lines MyV2's Help window answers instead of the server: /help, /quest and the list commands below (# works
// as well as /). /who stays in chat.
public static class HelpChat
{
    private static readonly Dictionary<string, HelpRequest> Tabs = new()
    {
        ["areas"] = new HelpRequest(HelpPageNumber.Areas),
        ["kills"] = new HelpRequest(HelpPageNumber.KillQuests),
        ["ranks"] = new HelpRequest(HelpPageNumber.Ranks),
        ["armor"] = new HelpRequest(HelpPageNumber.Armor),
        ["moddedarmor"] = new HelpRequest(HelpPageNumber.Armor),
        ["weapon"] = new HelpRequest(HelpPageNumber.Weapons),
        ["dagger"] = new HelpRequest(HelpPageNumber.Weapons, Weapons: WeaponList.Daggers),
        ["sword"] = new HelpRequest(HelpPageNumber.Weapons, Weapons: WeaponList.Swords),
        ["twohand"] = new HelpRequest(HelpPageNumber.Weapons, Weapons: WeaponList.TwoHanded),
        ["axe"] = new HelpRequest(HelpPageNumber.Weapons, Weapons: WeaponList.Axes),
        ["greataxe"] = new HelpRequest(HelpPageNumber.Weapons, Weapons: WeaponList.Greataxes),
        ["rings"] = new HelpRequest(HelpPageNumber.Rings),
        ["belt"] = new HelpRequest(HelpPageNumber.Belts),
        ["specialw"] = new HelpRequest(HelpPageNumber.SpecialItems),
        ["specialarmor"] = new HelpRequest(HelpPageNumber.SpecialItems),
    };

    public static HelpRequest? Request(string line)
    {
        var words = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0 || words[0].Length < 2 || words[0][0] is not ('/' or '#'))
        {
            return null;
        }

        var command = words[0][1..].ToLowerInvariant();
        if (command == "help" && words.Length == 1)
        {
            return new HelpRequest(HelpPageNumber.Commands);
        }

        if (words.Length == 1 && Tabs.TryGetValue(command, out var tab))
        {
            return tab;
        }

        if (command != "quest")
        {
            return null;
        }

        var all = Array.Exists(words, word => word.Equals("all", StringComparison.OrdinalIgnoreCase));
        foreach (var word in words[1..])
        {
            if (int.TryParse(word, out var number) && number > 0)
            {
                return new HelpRequest(HelpPageNumber.Quests, number, all);
            }
        }

        return new HelpRequest(HelpPageNumber.Quests, AllQuests: all);
    }
}
