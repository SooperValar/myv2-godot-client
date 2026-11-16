using Godot;
using TheLastGate.Hotbar;

namespace TheLastGate.Hud;

[Tool]
public partial class HotbarButtonView : ClickArea
{
    private int slot;

    [Export]
    public int Slot
    {
        get => slot;
        set
        {
            slot = value;
            ShowKey();
        }
    }

    public override void _Ready() => ShowKey();

    public void ShowName(string name) => GetNode<Label>("Name").Text = name;

    private void ShowKey()
    {
        if (IsInsideTree())
        {
            GetNode<Label>("Key").Text = HotbarButtons.KeyLabel(slot);
        }
    }
}
