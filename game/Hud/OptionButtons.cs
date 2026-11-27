using TheLastGate.Characters;
using TheLastGate.Net;
using TheLastGate.State;

namespace TheLastGate.Hud;

public static class OptionButtons
{
    public static byte[]? Packet(OptionButton button) =>
        ModeOf(button) is { } mode ? ModePackets.For(mode) : null;

    public static bool Toggle(OptionButton button, CharacterSettings settings)
    {
        switch (button)
        {
            case OptionButton.HealthPercent:
                settings.ShowHealthPercent = !settings.ShowHealthPercent;
                return true;
            case OptionButton.StatBase:
                settings.ShowStatBase = !settings.ShowStatBase;
                return true;
            case OptionButton.HideWalls:
                settings.HideWalls = !settings.HideWalls;
                return true;
            case OptionButton.Names:
                settings.ShowNames = !settings.ShowNames;
                return true;
            case OptionButton.HealthBars:
                settings.ShowHealthBars = !settings.ShowHealthBars;
                return true;
            default:
                return false;
        }
    }

    public static bool IsMarked(OptionButton button, CharacterSettings settings, int mode) =>
        button switch
        {
            OptionButton.HealthPercent => settings.ShowHealthPercent,
            OptionButton.StatBase => settings.ShowStatBase,
            OptionButton.HideWalls => settings.HideWalls,
            OptionButton.Names => settings.ShowNames,
            OptionButton.HealthBars => settings.ShowHealthBars,
            _ => ModeOf(button) == (SpeedMode)mode,
        };

    public static string? Help(OptionButton button) =>
        button switch
        {
            OptionButton.Fast =>
                "F1/FAST: Makes you move faster, but uses more endurance. You will also be seen more easily.",
            OptionButton.Normal => "F2/NORMAL: Move at normal speed. Takes no endurance when not fighting.",
            OptionButton.Slow =>
                "F3/SLOW: Makes you move slower. You regain endurance while not fighting. Decreases chances of being seen.",
            OptionButton.HealthPercent =>
                "F4/HEALTH: Toggle whether the current health of all characters is displayed as a percent.",
            OptionButton.StatBase =>
                "F5/STATS: Toggle seeing stat bases on the skill list, displayed as blue numbers.",
            OptionButton.HideWalls => "F6/HIDE: Toggle whether walls and other high objects are displayed.",
            OptionButton.Names => "F7/NAMES: Toggle whether the name of all characters is displayed.",
            OptionButton.HealthBars =>
                "F8/BARS: Toggle whether the current health of all characters is displayed as bars.",
            OptionButton.Exit =>
                "F12/EXIT: Leave the game immediately. Will cost you 50% health and can result in death.",
            OptionButton.Help => "HELP: Commands, quests and more. /help opens it too.",
            _ => null,
        };

    private static SpeedMode? ModeOf(OptionButton button) =>
        button switch
        {
            OptionButton.Fast => SpeedMode.Fast,
            OptionButton.Normal => SpeedMode.Normal,
            OptionButton.Slow => SpeedMode.Slow,
            _ => null,
        };
}
