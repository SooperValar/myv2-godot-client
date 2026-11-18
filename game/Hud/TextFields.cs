using Godot;

namespace TheLastGate.Hud;

// The text fields of the HUD's windows, such as a count or a map note's name. While one is being typed in, its keys
// belong to it: hotkeys and the chat's keys wait, Escape stops the typing, and a click anywhere else ends it. The chat
// line has its own rules for which keys run hotkeys.
public static class TextFields
{
    public static Control? BeingTypedIn(Viewport viewport) => viewport.GuiGetFocusOwner() switch
    {
        ChatLine => null,
        LineEdit field when field.IsEditing() => field,
        TextEdit { Editable: true } field => field,
        _ => null,
    };

    // True when the key was Escape and the typing was stopped.
    public static bool StopOnEscape(Control field, InputEventKey key)
    {
        if (key.Keycode != Key.Escape)
        {
            return false;
        }

        field.ReleaseFocus();
        return true;
    }

    public static void StopOnClickOutside(Viewport viewport, InputEventMouseButton press)
    {
        if (press.Pressed && BeingTypedIn(viewport) is { } field && !field.GetGlobalRect().HasPoint(press.Position))
        {
            field.ReleaseFocus();
        }
    }
}
