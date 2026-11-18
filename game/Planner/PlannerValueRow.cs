using Godot;

namespace TheLastGate.Planner;

// A planner row with one value: the server's, and when the plan changes it, the planned value and the difference.
public partial class PlannerValueRow : Control
{
    public void Show(string name, string value)
    {
        GetNode<Label>("Name").Text = name;
        GetNode<Label>("Value").Text = value;
        GetNode<CanvasItem>("Planned").Visible = false;
    }

    public void ShowPlanned(string name, int current, int planned)
    {
        Show(name, current.ToString());
        if (planned == current)
        {
            return;
        }

        GetNode<CanvasItem>("Planned").Visible = true;
        GetNode<Label>("Planned/Shown").Text = planned.ToString();
        GetNode<Label>("Planned/Difference").Text = $" {planned - current:+0;-0}";
    }
}
