using System.Collections.Generic;
using System.Linq;
using Godot;

namespace TheLastGate.Game;

// The game screen has no keyboard navigation, as in the current client, so Godot's keys for moving focus and pressing
// the focused button are taken off while it runs and put back for the launcher.
public sealed class NavigationKeys
{
    private static readonly string[] Actions =
    [
        "ui_focus_next", "ui_focus_prev", "ui_up", "ui_down", "ui_left", "ui_right", "ui_accept", "ui_select",
    ];

    private Dictionary<string, InputEvent[]>? taken;

    public void TurnOff()
    {
        if (taken != null)
        {
            return;
        }

        taken = Actions.ToDictionary(action => action, action => InputMap.ActionGetEvents(action).ToArray());
        foreach (var action in Actions)
        {
            InputMap.ActionEraseEvents(action);
        }
    }

    public void TurnOn()
    {
        if (taken == null)
        {
            return;
        }

        foreach (var (action, events) in taken)
        {
            foreach (var inputEvent in events)
            {
                InputMap.ActionAddEvent(action, inputEvent);
            }
        }

        taken = null;
    }
}
