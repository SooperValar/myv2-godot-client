using System.Linq;
using Godot;
using TheLastGate.State;
using TheLastGate.Stats;

namespace TheLastGate.Hud;

public partial class SkillList : VBoxContainer
{
    private readonly SkillScroll scroll = new();
    private ScrollBar? scrollBar;

    [Export] public NodePath ScrollBarPath { get; set; } = new();

    public override void _Ready()
    {
        if (!ScrollBarPath.IsEmpty)
        {
            scrollBar = GetNode<ScrollBar>(ScrollBarPath);
            scrollBar.MaxValue = SkillScroll.LastRow;
            scrollBar.ValueChanged += value => scroll.ScrollTo((int)value);
        }
    }

    public override void _GuiInput(InputEvent inputEvent)
    {
        if (inputEvent is not InputEventMouseButton click)
        {
            return;
        }

        AcceptEvent();
        if (click.Pressed && click.ButtonIndex is MouseButton.WheelUp or MouseButton.WheelDown)
        {
            scroll.ScrollBy(click.ButtonIndex == MouseButton.WheelUp ? -1 : 1);
            scrollBar?.SetValueNoSignal(scroll.Row);
        }
    }

    public void Show(PlayerState player, Terminology terminology, StatPlan plan, bool showBase)
    {
        var order = SkillOrder.Sorted(
            terminology.Skills.Select((text, number) =>
                new SkillEntry(number, text.Name, text.SortKey, player.Skills[number][0] != 0)),
            player.Kindred);
        for (var row = 0; row < SkillScroll.VisibleRows; row++)
        {
            var number = order[scroll.PositionShownIn(row)];
            var text = terminology.Skills[number];
            var line = StatLines.Skill(number, text.Name, text.Show, player.Skills[number], plan, player.Points);
            GetChild<StatRow>(row).Show(line, StatPlan.SkillSlot(number), showBase);
        }
    }
}
