using System;
using System.Collections.Generic;
using TheLastGate.State;

namespace TheLastGate.Hud;

public sealed record HelpRequest(HelpPageNumber Tab, int? Quest = null, bool AllQuests = false);

// The chat lines MyV2's Help window answers instead of the server: /help, /quest, /areas, /kills and /ranks (# works as
// well as /). /who stays in chat.
public static class HelpChat
{
    private static readonly Dictionary<string, HelpPageNumber> Tabs = new()
    {
        ["areas"] = HelpPageNumber.Areas,
        ["kills"] = HelpPageNumber.KillQuests,
        ["ranks"] = HelpPageNumber.Ranks,
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
            return new HelpRequest(tab);
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
