using Godot;

namespace TheLastGate.Hud;

public partial class OptionButtonView : ClickArea
{
    [Export] public OptionButton Kind { get; set; }

    public void ShowMarked(bool marked)
    {
        if (GetNodeOrNull<CanvasItem>("Marker") is { } marker)
        {
            marker.Visible = marked;
        }
    }
}
