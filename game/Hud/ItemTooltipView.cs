using System;
using System.Collections.Generic;
using Godot;
using TheLastGate.Assets;
using TheLastGate.Items;
using TheLastGate.Profiling;

namespace TheLastGate.Hud;

// Godot builds a custom tooltip once, so this one watches its slot and rebuilds itself when what it should show
// changes, such as the item's text arriving.
public partial class ItemTooltipView : PanelContainer
{
    private const string ScenePath = "res://Hud/item_tooltip.tscn";
    private const float WrapWidth = 272;
    private const int BadgeGap = 6;
    private const int ChangeGap = 4;
    private const int TableColumnGap = 12;
    private const int TableRowGap = 2;
    private const int SeparatorSpace = 5;

    private static readonly Color SeparatorColour = Color.Color8(110, 110, 128, 128);

    private IFontSet? fonts;
    private ItemTooltipRequest? shown;
    private Func<ItemTooltipRequest?>? source;
    private Viewport? shownIn;

    // Godot wraps a custom tooltip in a TooltipPanel under the control that owns it, so that control's theme styles the
    // panel. The tooltip draws its own background and border.
    public static Theme OwnerTheme { get; } = MakeOwnerTheme();

    public override void _Process(double delta) => Update();

    public static ItemTooltipView Create(Func<ItemTooltipRequest?> tooltipSource, IFontSet fontSet, Viewport ownerViewport)
    {
        var view = GD.Load<PackedScene>(ScenePath).Instantiate<ItemTooltipView>();
        view.Watch(tooltipSource, fontSet, ownerViewport);
        return view;
    }

    private void Watch(Func<ItemTooltipRequest?> tooltipSource, IFontSet fontSet, Viewport slotViewport)
    {
        source = tooltipSource;
        fonts = fontSet;
        shownIn = slotViewport;
        Update();
    }

    private static Theme MakeOwnerTheme()
    {
        var theme = new Theme();
        theme.SetStylebox("panel", "TooltipPanel", new StyleBoxEmpty());
        return theme;
    }

    // A value that equipping the held item would change is followed by the change.
    private static Control MakeText(TooltipText text, IFontSet fonts, bool wraps = false)
    {
        var label = MakeLabel(text, fonts, wraps);
        if (text.Change is not { } change)
        {
            return label;
        }

        var row = new HBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
        row.AddThemeConstantOverride("separation", ChangeGap);
        row.AddChild(label);
        row.AddChild(MakeLabel(new TooltipText(change.Text, change.Colour), fonts));
        return row;
    }

    private static Label MakeLabel(TooltipText text, IFontSet fonts, bool wraps = false)
    {
        var font = fonts.For(text.Colour);
        var label = new Label { Text = text.Text, MouseFilter = MouseFilterEnum.Ignore };
        label.AddThemeFontOverride("font", font.Font);
        label.AddThemeFontSizeOverride("font_size", font.Size);
        label.AddThemeColorOverride("font_color", font.Tint);
        if (wraps && font.Font.GetStringSize(text.Text, HorizontalAlignment.Left, -1, font.Size).X > WrapWidth)
        {
            label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            label.CustomMinimumSize = new Vector2(WrapWidth, 0);
        }

        return label;
    }

    private static HSeparator MakeSeparator()
    {
        var separator = new HSeparator { MouseFilter = MouseFilterEnum.Ignore };
        separator.AddThemeStyleboxOverride("separator", new StyleBoxLine { Color = SeparatorColour, Thickness = 1 });
        separator.AddThemeConstantOverride("separation", SeparatorSpace);
        return separator;
    }

    private static GridContainer MakeTable(TooltipTable table, IFontSet fonts)
    {
        var grid = new GridContainer { Columns = table.Header.Count, MouseFilter = MouseFilterEnum.Ignore };
        grid.AddThemeConstantOverride("h_separation", TableColumnGap);
        grid.AddThemeConstantOverride("v_separation", TableRowGap);
        foreach (var cell in table.Header)
        {
            grid.AddChild(MakeLabel(cell, fonts));
        }

        foreach (var row in table.Rows)
        {
            foreach (var cell in row)
            {
                grid.AddChild(MakeText(cell, fonts));
            }
        }

        return grid;
    }

    private static HBoxContainer MakeBadges(TooltipBadges badges, IFontSet fonts)
    {
        var row = new HBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
        row.AddThemeConstantOverride("separation", BadgeGap);
        row.AddChild(MakeLabel(new TooltipText(badges.Label, TextColour.Dim), fonts));
        foreach (var badge in badges.Badges)
        {
            row.AddChild(MakeText(badge, fonts));
        }

        return row;
    }

    private void Update()
    {
        using var timing = Timings.Measure("tooltip.update");
        if (source == null || fonts == null)
        {
            return;
        }

        if (source() is { } request && request != shown)
        {
            ShowParts(GetNode<VBoxContainer>("Columns/Parts"), request.Parts(), fonts);
            var equipped = request.EquippedParts();
            ShowParts(GetNode<VBoxContainer>("Columns/Equipped"), equipped, fonts);
            GetNode<Control>("Columns/Equipped").Visible = equipped.Count > 0;
            GetNode<Control>("Columns/Divider").Visible = equipped.Count > 0;
            shown = request;
            CallDeferred(MethodName.FitPopup);
        }
    }

    // Godot sizes and places its tooltip popup only when it opens, so a tooltip that grows (Shift adds the worn
    // items) or shrinks again has to refit the popup itself and keep it inside the screen.
    private void FitPopup()
    {
        var popup = GetWindow();
        if (shownIn == null || popup == GetTree().Root)
        {
            return;
        }

        popup.Size = Vector2I.Zero;
        var screen = shownIn.GetVisibleRect();
        var left = (int)screen.Position.X;
        var top = (int)screen.Position.Y;
        var right = Math.Max(left, (int)screen.End.X - popup.Size.X);
        var bottom = Math.Max(top, (int)screen.End.Y - popup.Size.Y);
        popup.Position = new Vector2I(Math.Clamp(popup.Position.X, left, right), Math.Clamp(popup.Position.Y, top, bottom));
    }

    private static void ShowParts(VBoxContainer rows, IReadOnlyList<TooltipPart> parts, IFontSet fontSet)
    {
        foreach (var child in rows.GetChildren())
        {
            rows.RemoveChild(child);
            child.QueueFree();
        }

        foreach (var part in parts)
        {
            var node = part switch
            {
                TooltipLine line => MakeText(line.Text, fontSet, line.Wraps),
                TooltipSeparator => MakeSeparator(),
                TooltipTable table => MakeTable(table, fontSet),
                TooltipBadges badges => MakeBadges(badges, fontSet),
                _ => new Control(),
            };
            rows.AddChild(node);
        }
    }
}
