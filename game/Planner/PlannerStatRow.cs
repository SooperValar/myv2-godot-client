using System;
using Godot;

namespace TheLastGate.Planner;

// One stat row of the planner: name, base, current, the - and + glyphs, the preview "total +bought +gain" and the
// next point's cost.
public partial class PlannerStatRow : Control
{
    private static readonly Color Gold = Color.Color8(243, 219, 106);
    private static readonly Color Unchanged = Gold;
    private static readonly Color Changed = Color.Color8(255, 255, 255);
    private static readonly Color Hovered = Color.Color8(255, 255, 255);
    private static readonly Color Disabled = Color.Color8(72, 81, 81);

    private PlannerRow? shown;

    // The row's stat plan slot, and true to raise it or false to lower it.
    public event Action<int, bool>? GlyphClicked;

    public override void _Ready()
    {
        WireGlyph("Lower", false);
        WireGlyph("Raise", true);
    }

    public void Show(PlannerRow row)
    {
        shown = row;
        GetNode<Label>("Name").Text = row.Name;
        GetNode<Label>("Base").Text = row.Base.ToString();
        GetNode<Label>("Current").Text = row.Current.ToString();
        var total = GetNode<Label>("Preview/Total");
        total.Text = row.Preview.ToString();
        total.AddThemeColorOverride("font_color", row.Preview != row.Current ? Changed : Unchanged);
        ShowPart("Preview/Bought", row.DirectRaise, $" +{row.DirectRaise}");
        ShowPart("Preview/Gain", row.AttributeGain, $" {row.AttributeGain:+0;-0}");
        GetNode<Label>("Cost").Text = row.Cost?.ToString() ?? "-";
        GetNode<CanvasItem>("Band").Visible = row.DirectRaise != 0;
        GetNode<CanvasItem>("Marker").Visible = row.UsesRaisedAttribute;
        ShowGlyph("Lower");
        ShowGlyph("Raise");
    }

    private void ShowPart(string path, int amount, string text)
    {
        var part = GetNode<Label>(path);
        part.Visible = amount != 0;
        part.Text = text;
    }

    private bool GlyphEnabled(string name) =>
        shown != null && (name == "Raise" ? shown.CanRaise : shown.CanLower);

    private void ShowGlyph(string name)
    {
        var glyph = GetNode<Control>(name);
        var hovered = glyph.GetGlobalRect().HasPoint(glyph.GetGlobalMousePosition());
        var colour = !GlyphEnabled(name) ? Disabled : hovered ? Hovered : Gold;
        glyph.GetNode<Label>("Glyph").AddThemeColorOverride("font_color", colour);
    }

    private void WireGlyph(string name, bool raise)
    {
        var glyph = GetNode<Control>(name);
        glyph.MouseEntered += () => ShowGlyph(name);
        glyph.MouseExited += () => ShowGlyph(name);
        glyph.GuiInput += input =>
        {
            if (input is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: false } && shown != null
                && GlyphEnabled(name))
            {
                GlyphClicked?.Invoke(shown.Slot, raise);
            }
        };
    }
}
