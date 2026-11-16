using System.Collections.Generic;
using Godot;
using TheLastGate.State;

namespace TheLastGate.Hud;

[Tool]
public partial class ItemSlotView : ClickArea
{
    private const float HighlightBrightness = 2f;

    private int previewSprite;

    [Export]
    public int PreviewSprite
    {
        get => previewSprite;
        set
        {
            previewSprite = value;
            ShowPreview();
        }
    }

    public override void _Ready()
    {
        MouseEntered += () => SetHighlighted(true);
        MouseExited += () => SetHighlighted(false);
        ShowPreview();
    }

    public void Show(ItemSlot item, IReadOnlyList<int> overlays, bool blocked)
    {
        GetNode<ArchiveSprite>("Highlighted/Item").SpriteNumber = item.Sprite;
        var overlayNodes = GetNode<Control>("Highlighted/Overlays");
        while (overlayNodes.GetChildCount() < overlays.Count)
        {
            overlayNodes.AddChild(new ArchiveSprite { MouseFilter = MouseFilterEnum.Ignore });
        }

        for (var index = 0; index < overlayNodes.GetChildCount(); index++)
        {
            var overlay = overlayNodes.GetChild<ArchiveSprite>(index);
            overlay.Visible = index < overlays.Count;
            if (overlay.Visible)
            {
                overlay.SpriteNumber = overlays[index];
            }
        }

        GetNode<CanvasItem>("Blocked").Visible = blocked;
    }

    private void SetHighlighted(bool highlighted)
    {
        var brightness = highlighted ? HighlightBrightness : 1f;
        GetNode<CanvasItem>("Highlighted").Modulate = new Color(brightness, brightness, brightness);
    }

    private void ShowPreview()
    {
        if (Engine.IsEditorHint() && IsInsideTree())
        {
            Show(new ItemSlot(previewSprite, 0, 0, 0), [], false);
        }
    }
}
