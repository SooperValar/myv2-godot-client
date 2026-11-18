using System.Diagnostics;
using Godot;

namespace TheLastGate.Planner;

// One line of the planner's Focus panel: its text, indented for the raises a route buys, and a value at the right.
public partial class PlannerFocusLine : Control
{
    private const int TextLeft = 2;
    private const int IndentedTextLeft = 10;

    private static readonly Color Heading = Color.Color8(255, 255, 255);
    private static readonly Color Normal = Color.Color8(243, 219, 106);
    private static readonly Color SkillRaise = Color.Color8(125, 220, 106);
    private static readonly Color AttributeRaise = Color.Color8(90, 180, 255);
    private static readonly Color OtherRoute = Color.Color8(168, 176, 176);

    public void Show(FocusLine line)
    {
        var colour = line.Tone switch
        {
            FocusTone.Heading => Heading,
            FocusTone.Normal => Normal,
            FocusTone.SkillRaise => SkillRaise,
            FocusTone.AttributeRaise => AttributeRaise,
            FocusTone.OtherRoute => OtherRoute,
            _ => throw new UnreachableException(),
        };
        var text = GetNode<Label>("Text");
        text.Text = line.Text;
        text.OffsetLeft = line.Indented ? IndentedTextLeft : TextLeft;
        text.AddThemeColorOverride("font_color", colour);
        var value = GetNode<Label>("Value");
        value.Text = line.Value ?? "";
        value.AddThemeColorOverride("font_color", colour);
    }
}
