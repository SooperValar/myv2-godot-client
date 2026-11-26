using System;
using Godot;

namespace TheLastGate.Planner;

// A planner row with one value: the server's, and when the plan changes it, the planned value and the difference.
public partial class PlannerValueRow : Control
{
    public event Action? NameClicked;

    // A combat stat's name can be clicked to focus it while the planned values are shown.
    public void AllowFocus(bool allowed) =>
        GetNode<Control>("NameClick").MouseFilter = allowed ? MouseFilterEnum.Stop : MouseFilterEnum.Ignore;

    public override void _Ready()
    {
        GetNode<Control>("NameClick").GuiInput += input =>
        {
            if (input is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: false })
            {
                NameClicked?.Invoke();
            }
        };
    }

    public void ShowOutline(bool outlined) => GetNode<CanvasItem>("Outline").Visible = outlined;

    public void Show(string name, string value)
    {
        GetNode<Label>("Name").Text = name;
        GetNode<Label>("Value").Text = value;
        GetNode<CanvasItem>("Planned").Visible = false;
    }

    public void ShowPlanned(string name, int current, int planned) =>
        ShowPlanned(name, current, planned, value => value.ToString());

    public void ShowPlanned(string name, int current, int planned, Func<int, string> format)
    {
        Show(name, format(current));
        if (planned == current)
        {
            return;
        }

        GetNode<CanvasItem>("Planned").Visible = true;
        GetNode<Label>("Planned/Shown").Text = format(planned);
        GetNode<Label>("Planned/Difference").Text = planned > current
            ? " +" + format(planned - current)
            : " -" + format(current - planned);
    }
}
