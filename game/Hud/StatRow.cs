using System.Globalization;
using Godot;
using TheLastGate.Stats;

namespace TheLastGate.Hud;

[Tool]
public partial class StatRow : Control
{
    private string previewName = "";

    [Export]
    public string PreviewName
    {
        get => previewName;
        set
        {
            previewName = value;
            ShowPreview();
        }
    }

    public override void _Ready() => ShowPreview();

    public void Show(StatLine line, bool showBase)
    {
        var name = GetNode<HudLabel>("Name");
        name.Text = line.Name;
        name.Colour = line.Colour;
        ShowNumber("Base", showBase ? line.Base : null);
        ShowNumber("Current", line.Current);
        GetNode<HudLabel>("Current").Colour = line.Colour;
        GetNode<CanvasItem>("Raise").Visible = line.CanRaise;
        ShowNumber("Cost", line.Cost);
    }

    private void ShowNumber(string name, int? value)
    {
        var label = GetNode<Label>(name);
        label.Visible = value != null;
        label.Text = value?.ToString(CultureInfo.InvariantCulture) ?? "";
    }

    private void ShowPreview()
    {
        if (Engine.IsEditorHint() && IsInsideTree() && previewName != "")
        {
            GetNode<Label>("Name").Text = previewName;
        }
    }
}
