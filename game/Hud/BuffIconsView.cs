using System.Collections.Generic;
using System.Linq;
using Godot;

namespace TheLastGate.Hud;

[Tool]
public partial class BuffIconsView : VFlowContainer
{
    private int[] previewSprites = [];

    [Export] public Vector2 CellSize { get; set; } = new(20, 23);

    [Export]
    public int[] PreviewSprites
    {
        get => previewSprites;
        set
        {
            previewSprites = value;
            ShowPreview();
        }
    }

    public override void _Ready() => ShowPreview();

    public void Show(IReadOnlyList<BuffIcon> icons)
    {
        while (GetChildCount() < icons.Count)
        {
            var cell = new Control
            {
                CustomMinimumSize = CellSize,
                MouseFilter = MouseFilterEnum.Ignore,
                LayoutDirection = LayoutDirectionEnum.Ltr,
            };
            cell.AddChild(new ArchiveSprite { MouseFilter = MouseFilterEnum.Ignore });
            AddChild(cell);
        }

        for (var index = 0; index < GetChildCount(); index++)
        {
            var cell = GetChild<Control>(index);
            cell.Visible = index < icons.Count;
            if (!cell.Visible)
            {
                continue;
            }

            var sprite = cell.GetChild<ArchiveSprite>(0);
            sprite.SpriteNumber = icons[index].Sprite;
            var brightness = icons[index].Brightness;
            sprite.SelfModulate = new Color(brightness, brightness, brightness);
        }
    }

    private void ShowPreview()
    {
        if (Engine.IsEditorHint() && IsInsideTree())
        {
            Show(previewSprites.Select(sprite => new BuffIcon(sprite, 1f)).ToList());
        }
    }
}
