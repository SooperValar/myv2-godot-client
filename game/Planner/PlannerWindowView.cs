using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using TheLastGate.Assets;
using TheLastGate.Hud;
using TheLastGate.Windows;

namespace TheLastGate.Planner;

// The current client's skill planner window, at its fixed place on the screen and framed by the game's window sprite:
// the points left and planned, Commit, Reset and Close, the stat rows and the skills list. The HUD tells it what to
// show and does what its buttons ask.
[Tool]
public partial class PlannerWindowView : Panel
{
    private const int FrameSprite = 18128;
    private const string StatRowScenePath = "res://Planner/planner_stat_row.tscn";
    private const int PreviewSkillRows = 12;

    private static readonly string[] StatRowNames =
        ["Braveness", "Willpower", "Intuition", "Agility", "Strength", "Hitpoints", "Mana"];

    private static readonly Color LightBackground = Color.Color8(42, 15, 10, 240);
    private static readonly Color DarkBackground = Color.Color8(23, 0, 5);
    private Theme? sceneTheme;

    private PackedScene? statRowScene;

    private Container SkillRows => GetNode<Container>("Right/SkillScroll/SkillRows");

    public event Action? CommitPressed;

    public event Action? ResetPressed;

    public event Action? ClosePressed;

    // A row's stat plan slot, and true to raise it or false to lower it.
    public event Action<int, bool>? RowClicked;

    public override void _Ready()
    {
        ApplyFrameSprite();
        ApplyFont();
        statRowScene = GD.Load<PackedScene>(StatRowScenePath);
        if (Engine.IsEditorHint())
        {
            EnsureSkillRows(PreviewSkillRows);
            return;
        }

        foreach (var name in StatRowNames)
        {
            GetNode<PlannerStatRow>("Left/" + name).GlyphClicked += (slot, raise) => RowClicked?.Invoke(slot, raise);
        }

        GetNode<Button>("Header/Commit").Pressed += () => CommitPressed?.Invoke();
        GetNode<Button>("Header/Reset").Pressed += () => ResetPressed?.Invoke();
        GetNode<Button>("Header/Close").Pressed += () => ClosePressed?.Invoke();
    }

    public void ShowWindow(bool shown, bool darkMode, int pointsLeft, int planned, PlannerSheet? sheet)
    {
        Visible = shown;
        ((StyleBoxFlat)GetThemeStylebox("panel")).BgColor = darkMode ? DarkBackground : LightBackground;
        GetNode<Label>("Header/Points").Text = PlannerWindow.Header(pointsLeft, planned);
        if (sheet == null)
        {
            return;
        }

        var statRows = sheet.Attributes.Concat(sheet.Vitals).ToArray();
        for (var index = 0; index < StatRowNames.Length; index++)
        {
            GetNode<PlannerStatRow>("Left/" + StatRowNames[index]).Show(statRows[index]);
        }

        ShowSkills(sheet.Skills);
    }

    private void ShowSkills(IReadOnlyList<PlannerRow> skills)
    {
        EnsureSkillRows(skills.Count);
        var rows = SkillRows.GetChildren().Cast<PlannerStatRow>().ToArray();
        for (var index = 0; index < rows.Length; index++)
        {
            rows[index].Visible = index < skills.Count;
            if (index < skills.Count)
            {
                rows[index].Show(skills[index]);
            }
        }
    }

    private void EnsureSkillRows(int count)
    {
        var skillRows = SkillRows;
        while (skillRows.GetChildCount() < count && statRowScene != null)
        {
            var row = statRowScene.Instantiate();
            if (row is PlannerStatRow statRow)
            {
                statRow.GlyphClicked += (slot, raise) => RowClicked?.Invoke(slot, raise);
            }

            skillRows.AddChild(row);
        }
    }

    // The text is in the HUD's font, so a font set swapped in the options reaches the planner too. Every label keeps
    // the planner's own colour, which tints the font set's white letters.
    public void ApplyFont()
    {
        if (HudView.Containing(this)?.Fonts is not { } fonts)
        {
            return;
        }

        sceneTheme ??= Theme;
        var font = fonts.For(TextColour.White);
        var themed = (Theme)sceneTheme.Duplicate();
        themed.DefaultFont = font.Font;
        themed.DefaultFontSize = font.Size;
        Theme = themed;
    }

    // The window sprite lives in the sprite archives, so it is put on when the HUD has them.
    public void ApplyFrameSprite()
    {
        if (HudView.Containing(this)?.Sprites?.GetTexture(FrameSprite) is not { } texture)
        {
            return;
        }

        foreach (var frame in FindChildren("Frame", nameof(NinePatchRect), true, false).Cast<NinePatchRect>())
        {
            frame.Texture = texture;
        }
    }
}
