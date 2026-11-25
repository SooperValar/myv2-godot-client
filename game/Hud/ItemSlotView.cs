using System;
using System.Collections.Generic;
using System.Globalization;
using Godot;
using TheLastGate.Assets;
using TheLastGate.Hotkeys;
using TheLastGate.Items;
using TheLastGate.State;
using TheLastGate.World;

namespace TheLastGate.Hud;

[Tool]
public partial class ItemSlotView : ClickArea, IHudContextUser
{
    private const float HighlightBrightness = 2f;
    private const string TintShaderPath = "res://World/sprite_tint.gdshader";

    private static readonly Dictionary<int, ShaderMaterial> OutlineMaterials = [];
    private IFontSet? fonts;

    private int previewSprite;

    public ItemSlotView() => AddToGroup(HudContext.Group);

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

    public Func<ItemTooltipRequest?>? TooltipRequest { get; set; }

    public void ShowContext(HudContext context) => fonts = context.Fonts;

    public override string _GetTooltip(Vector2 atPosition) => TooltipRequest?.Invoke() != null ? "item" : "";

    public override GodotObject? _MakeCustomTooltip(string forText)
    {
        if (TooltipRequest == null || this.fonts is not { } fonts)
        {
            return null;
        }

        return ItemTooltipView.Create(TooltipRequest, fonts, GetViewport());
    }

    public override void _Ready()
    {
        Theme ??= ItemTooltipView.OwnerTheme;
        MouseEntered += () => SetHighlighted(true);
        MouseExited += () => SetHighlighted(false);
        ShowPreview();
    }

    public void Show(ItemSlot item, IReadOnlyList<ItemOverlay> overlays, bool blocked, ItemMark mark = default)
    {
        var itemSprite = GetNode<ArchiveSprite>("Highlighted/Item");
        itemSprite.SpriteNumber = item.Sprite;
        itemSprite.Material = item.Sprite != 0 ? OutlineMaterial(mark.Tier) : null;
        var letter = GetNode<Label>("MarkLetter");
        letter.Visible = item.Sprite != 0 && mark.Letter != '\0';
        letter.Text = mark.Letter.ToString();
        var number = GetNode<Label>("MarkNumber");
        number.Visible = item.Sprite != 0 && mark.Number != null;
        number.Text = mark.Number?.ToString(CultureInfo.InvariantCulture) ?? "";
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
                overlay.SpriteNumber = overlays[index].Sprite;
                overlay.Position = new Vector2(overlays[index].OffsetX, 0);
            }
        }

        GetNode<CanvasItem>("Blocked").Visible = blocked;
    }

    private static ShaderMaterial? OutlineMaterial(int tier)
    {
        if (tier == TierOutline.None)
        {
            return null;
        }

        if (!OutlineMaterials.TryGetValue(tier, out var material))
        {
            material = new ShaderMaterial { Shader = GD.Load<Shader>(TintShaderPath) };
            material.SetShaderParameter("outline_color", TierOutline.For(tier));
            OutlineMaterials[tier] = material;
        }

        return material;
    }

    public void ShowHotbarKey(int? button)
    {
        var hotbarKey = GetNode<Label>("HotbarKey");
        hotbarKey.Visible = button != null;
        if (button is { } slot)
        {
            hotbarKey.Text = BindingRules.HotbarKeyName(slot);
        }
    }

    public void SetHighlighted(bool highlighted)
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
