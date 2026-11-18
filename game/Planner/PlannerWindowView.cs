using System;
using System.Linq;
using Godot;
using TheLastGate.Hud;
using TheLastGate.Windows;

namespace TheLastGate.Planner;

// The current client's skill planner window, at its fixed place on the screen and framed by the game's window sprite:
// the points left and planned, and Commit, Reset and Close. The HUD tells it what to show and does what its buttons ask.
[Tool]
public partial class PlannerWindowView : Panel
{
    private const int FrameSprite = 18128;

    private static readonly Color LightBackground = Color.Color8(42, 15, 10, 240);
    private static readonly Color DarkBackground = Color.Color8(23, 0, 5);

    public event Action? CommitPressed;

    public event Action? ResetPressed;

    public event Action? ClosePressed;

    public override void _Ready()
    {
        ApplyFrameSprite();
        if (Engine.IsEditorHint())
        {
            return;
        }

        GetNode<Button>("Header/Commit").Pressed += () => CommitPressed?.Invoke();
        GetNode<Button>("Header/Reset").Pressed += () => ResetPressed?.Invoke();
        GetNode<Button>("Header/Close").Pressed += () => ClosePressed?.Invoke();
    }

    public void ShowWindow(bool shown, bool darkMode, int pointsLeft, int planned)
    {
        Visible = shown;
        ((StyleBoxFlat)GetThemeStylebox("panel")).BgColor = darkMode ? DarkBackground : LightBackground;
        GetNode<Label>("Header/Points").Text = PlannerWindow.Header(pointsLeft, planned);
    }

    // The window sprite lives in the sprite archives, so it is put on when the HUD has them.
    public void ApplyFrameSprite()
    {
        if (HudView.Containing(this)?.Sprites?.GetTexture(FrameSprite) is not { } texture)
        {
            return;
        }

        foreach (var frame in FindChildren("Frame", nameof(NinePatchRect), true, false).Cast<NinePatchRect>())
        {
            frame.Texture = texture;
        }
    }
}
