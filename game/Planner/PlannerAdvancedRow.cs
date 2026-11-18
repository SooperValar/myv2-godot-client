using System;
using System.Linq;
using Godot;

namespace TheLastGate.Planner;

// One row of the planner's Advanced window: a tick and its name, and for the rows that have them, what one copy does,
// how many copies there are and how many the skill tree adds.
[Tool]
public partial class PlannerAdvancedRow : Control
{
    private static readonly Color Gold = Color.Color8(243, 219, 106);
    private static readonly Color Hint = Color.Color8(72, 81, 81);

    // A disabled tick is drawn faded, as the current client's are.
    private static readonly Color Faded = new(1, 1, 1, 0.6f);

    private CheckBox Tick => GetNode<CheckBox>("Tick");

    private LineEdit Count => GetNode<LineEdit>("Count");

    public event Action? Toggled;

    public event Action<int>? CountChosen;

    public override void _Ready()
    {
        if (Engine.IsEditorHint())
        {
            return;
        }

        Tick.Toggled += _ => Toggled?.Invoke();
        Count.TextChanged += TypeCount;
        Count.EditingToggled += editing =>
        {
            if (!editing)
            {
                CountChosen?.Invoke(int.TryParse(Count.Text, out var copies) ? copies : 0);
            }
        };
    }

    public void Show(string name, bool ticked, bool enabled) => ShowTick(name, ticked, enabled, enabled ? Gold : Hint);

    // A row the planner always counts, such as the skill tree's.
    public void ShowAlwaysOn(string name) => ShowTick(name, true, false, Gold);

    // A line under a row, such as a spell's warning.
    public void ShowNote(string text)
    {
        Tick.Visible = false;
        var label = GetNode<Label>("Name");
        label.Text = text;
        label.AddThemeColorOverride("font_color", Hint);
    }

    public void ShowEffect(string effect) => GetNode<Label>("Effect").Text = effect;

    // The count being typed is left alone until the typing ends.
    public void ShowCount(int copies, bool enabled, string help)
    {
        Count.Visible = true;
        Count.Editable = enabled;
        Count.TooltipText = help;
        if (!Count.IsEditing())
        {
            Count.Text = copies.ToString();
        }
    }

    public void ShowTree(string text) => GetNode<Label>("Tree").Text = text;

    private void ShowTick(string name, bool ticked, bool enabled, Color colour)
    {
        Tick.Visible = true;
        Tick.SetPressedNoSignal(ticked);
        Tick.Disabled = !enabled;
        Tick.Modulate = enabled ? Colors.White : Faded;
        var label = GetNode<Label>("Name");
        label.Text = name;
        label.AddThemeColorOverride("font_color", colour);
    }

    // Only digits can be typed. A count takes effect as it is typed; an empty box or 0 waits for the typing to end,
    // which makes it 1.
    private void TypeCount(string text)
    {
        var digits = new string(text.Where(char.IsAsciiDigit).ToArray());
        if (digits != text)
        {
            var caret = Count.CaretColumn;
            Count.Text = digits;
            Count.CaretColumn = Math.Min(caret, digits.Length);
        }

        if (int.TryParse(digits, out var copies) && copies > 0)
        {
            CountChosen?.Invoke(copies);
        }
    }
}
