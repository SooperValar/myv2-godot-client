using System.Collections.Generic;
using Godot;
using TheLastGate.Assets;
using TheLastGate.State;

namespace TheLastGate.Hud;

public partial class BuffCellView : Control
{
    private const int NameFont = 3;
    private const int HarmfulNameFont = 0;
    private const int TimeFont = 1;
    private const int PowerFont = 2;
    private const int ShadowOffset = 1;
    private const int TimeGap = 2;

    private static readonly Color ShadowColour = new(0, 0, 0);

    private readonly ArchiveSprite sprite = new() { MouseFilter = MouseFilterEnum.Ignore };

    private readonly Label time = new()
    {
        MouseFilter = MouseFilterEnum.Ignore,
        VerticalAlignment = VerticalAlignment.Center,
    };

    private IFontSet? fonts;
    private BuffIcon icon;
    private bool harmful;
    private ItemTooltipRequest? tooltip;
    private BuffIcon tooltipFor;

    public BuffCellView()
    {
        MouseFilter = MouseFilterEnum.Pass;
        LayoutDirection = LayoutDirectionEnum.Ltr;
        Theme = ItemTooltipView.OwnerTheme;
        AddChild(sprite);
        time.AnchorBottom = 1;
        time.AddThemeColorOverride("font_shadow_color", ShadowColour);
        time.AddThemeConstantOverride("shadow_offset_x", ShadowOffset);
        time.AddThemeConstantOverride("shadow_offset_y", ShadowOffset);
        AddChild(time);
    }

    // Buffs fill the portrait area from its left and debuffs from its right, so each puts its time on the inner side.
    public void Show(BuffIcon shown, bool isHarmful, float iconWidth, IFontSet? fontSet)
    {
        icon = shown;
        harmful = isHarmful;
        fonts = fontSet;
        var cellWidth = CustomMinimumSize.X;
        sprite.Position = new Vector2(isHarmful ? cellWidth - iconWidth : 0, 0);
        time.OffsetLeft = isHarmful ? 0 : iconWidth + TimeGap;
        time.OffsetRight = isHarmful ? cellWidth - iconWidth - TimeGap : cellWidth;
        time.HorizontalAlignment = isHarmful ? HorizontalAlignment.Right : HorizontalAlignment.Left;
        sprite.SpriteNumber = shown.Sprite;
        sprite.SelfModulate = new Color(shown.Brightness, shown.Brightness, shown.Brightness);
        time.Text = shown.ShortTime;
        if (fonts == null)
        {
            return;
        }

        var font = fonts.For(shown.RunningOut ? TextColour.Red : TextColour.White);
        time.AddThemeFontOverride("font", font.Font);
        time.AddThemeFontSizeOverride("font_size", font.Size);
        time.AddThemeColorOverride("font_color", font.Tint);
    }

    public override string _GetTooltip(Vector2 atPosition) => icon.Name.Length > 0 ? icon.Name : "";

    public override GodotObject? _MakeCustomTooltip(string forText) =>
        fonts == null ? null : ItemTooltipView.Create(TooltipRequest, fonts, GetViewport());

    // Built again only when the shown text changes, because the tooltip rebuilds itself for every new request.
    private ItemTooltipRequest? TooltipRequest()
    {
        if (icon.Name.Length == 0)
        {
            return null;
        }

        if (tooltip == null || tooltipFor != icon with { Brightness = tooltipFor.Brightness })
        {
            List<LogLine> lines =
            [
                new(icon.Name, harmful ? HarmfulNameFont : NameFont),
                new($"Power {icon.Power}", PowerFont),
                new(icon.LongTime, TimeFont),
            ];
            tooltip = new ItemTooltipRequest(lines, []);
            tooltipFor = icon;
        }

        return tooltip;
    }
}
