using TheLastGate.Assets;
using TheLastGate.State;

namespace TheLastGate.Stats;

public sealed class SkillShortcut
{
    private const uint ClassesWithShortcutKeys = Kindred.Mercenary | Kindred.Templar | Kindred.Harakim;

    private const string ShortcutKeysHint =
        " Right-click on one of the shortcut keys in the bottom right to set a shortcut.";

    private bool explainedShortcutKeys;

    public int? Selected { get; private set; }

    public LogLine Toggle(int skillNumber, string name, uint kindred)
    {
        if (Selected == skillNumber)
        {
            Selected = null;
            return Line($"{name} no longer selected for shortcut.");
        }

        Selected = skillNumber;
        var selected = $"{name} selected for shortcut.";
        if (explainedShortcutKeys || (kindred & ClassesWithShortcutKeys) == 0)
        {
            return Line(selected);
        }

        explainedShortcutKeys = true;
        return Line(selected + ShortcutKeysHint);
    }

    private static LogLine Line(string text) => new(text, (int)TextColour.Lime);
}
