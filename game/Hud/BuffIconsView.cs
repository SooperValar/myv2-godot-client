using System.Collections.Generic;
using System.Linq;
using Godot;
using TheLastGate.Assets;
using TheLastGate.Profiling;

namespace TheLastGate.Hud;

[Tool]
public partial class BuffIconsView : VFlowContainer, IHudContextUser
{
    private IFontSet? fonts;
    private int[] previewSprites = [];

    public BuffIconsView() => AddToGroup(HudContext.Group);

    [Export] public bool Harmful { get; set; }

    [Export] public Vector2 CellSize { get; set; } = new(38, 23);

    [Export] public float IconWidth { get; set; } = 20;

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

    public void ShowContext(HudContext context) => fonts = context.Fonts;

    public void Show(IReadOnlyList<BuffIcon> icons)
    {
        using var timing = Timings.Measure("bufficons.show");
        while (GetChildCount() < icons.Count)
        {
            AddChild(new BuffCellView { CustomMinimumSize = CellSize, Size = CellSize });
        }

        for (var index = 0; index < GetChildCount(); index++)
        {
            var cell = GetChild<BuffCellView>(index);
            cell.Visible = index < icons.Count;
            if (cell.Visible)
            {
                cell.Show(icons[index], Harmful, IconWidth, fonts);
            }
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
