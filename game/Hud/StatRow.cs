using System.Globalization;
using Godot;
using TheLastGate.Assets;
using TheLastGate.Stats;

namespace TheLastGate.Hud;

[Tool]
public partial class StatRow : Control
{
    private static readonly string[] StatLabels = ["Base", "Current", "Raise", "Lower", "Cost"];
    private static readonly string[] MetaLabels = ["MetaValue", "Affix"];

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

    public StatLine Line { get; private set; }

    public int Slot { get; private set; }

    public int? Meta { get; private set; }

    public override void _Ready() => ShowPreview();

    public void Show(StatLine line, int slot, bool showBase)
    {
        Line = line;
        Slot = slot;
        Meta = null;
        SetVisible(MetaLabels, false);
        var name = GetNode<HudLabel>("Name");
        name.Visible = true;
        name.Text = line.Name;
        name.Colour = line.Colour;
        ShowNumber("Base", showBase ? line.Base : null);
        ShowNumber("Current", line.Current);
        GetNode<HudLabel>("Current").Colour = line.Colour;
        GetNode<CanvasItem>("Raise").Visible = line.CanRaise;
        GetNode<CanvasItem>("Lower").Visible = line.CanLower;
        ShowNumber("Cost", line.Cost);
    }

    public void ShowMeta(MetaLine? line, int meta)
    {
        Meta = meta;
        SetVisible(StatLabels, false);
        GetNode<CanvasItem>("Name").Visible = line != null;
        SetVisible(MetaLabels, line != null);
        if (line is { } shown)
        {
            ShowText("Name", shown.Name, shown.Colour);
            ShowText("MetaValue", shown.Value, shown.Colour);
            ShowText("Affix", shown.Affix, shown.Colour);
        }
    }

    private void SetVisible(string[] names, bool visible)
    {
        foreach (var name in names)
        {
            GetNode<CanvasItem>(name).Visible = visible;
        }
    }

    private void ShowText(string name, string text, TextColour colour)
    {
        var label = GetNode<HudLabel>(name);
        label.Text = text;
        label.Colour = colour;
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
