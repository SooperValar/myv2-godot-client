using Godot;
using TheLastGate.Stats;

namespace TheLastGate.Hud;

public partial class StatModeButton : ClickArea
{
    [Export] public StatListMode Mode { get; set; }

    public void ShowMarked(StatPanel panel)
    {
        var marked = panel.IsMarked(Mode);
        var both = panel.Mode == StatListMode.SkillsAndMeta;
        GetNode<CanvasItem>("Marker").Visible = marked && !both;
        if (GetNodeOrNull<CanvasItem>("BothMarker") is { } bothMarker)
        {
            bothMarker.Visible = marked && both;
        }
    }
}
