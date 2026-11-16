using System;
using System.Collections.Generic;
using Godot;
using TheLastGate.Assets;
using TheLastGate.Characters;

namespace TheLastGate.Hud;

public partial class HotbarMenu : PopupMenu
{
    private readonly List<int> shownSkills = [];
    private int slot;

    public event Action<int, int?>? SkillChosen;

    public override void _EnterTree() => ApplyFont();

    public override void _Ready() => IndexPressed += Choose;

    public void ApplyFont()
    {
        if (HudView.Containing(this)?.Fonts is not { } fonts)
        {
            return;
        }

        var font = fonts.For(TextColour.Yellow);
        AddThemeFontOverride("font", font.Font);
        AddThemeFontSizeOverride("font_size", font.Size);
        AddThemeColorOverride("font_color", font.Tint);
        AddThemeColorOverride("font_hover_color", font.Tint);
    }

    public void Open(int hotbarSlot, IEnumerable<(int Number, string Name)> skills, Vector2 position)
    {
        slot = hotbarSlot;
        shownSkills.Clear();
        Clear();
        AddItem(CharacterSettings.EmptyHotbarName);
        foreach (var (number, name) in skills)
        {
            shownSkills.Add(number);
            AddItem(name);
        }

        Popup(new Rect2I((Vector2I)position, Vector2I.Zero));
    }

    private void Choose(long index) => SkillChosen?.Invoke(slot, index == 0 ? null : shownSkills[(int)index - 1]);
}
