using Godot;
using TheLastGate.Hotbar;
using TheLastGate.Items;
using TheLastGate.State;

namespace TheLastGate.Hud;

public partial class InventoryGrid : GridContainer
{
    private readonly InventoryScroll scroll = new();
    private ScrollBar? scrollBar;

    [Export] public NodePath ScrollBarPath { get; set; } = new();

    public override void _Ready()
    {
        if (!ScrollBarPath.IsEmpty)
        {
            scrollBar = GetNode<ScrollBar>(ScrollBarPath);
            scrollBar.MaxValue = InventoryScroll.LastRow + 1;
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

    public int SlotShownIn(int cell) => scroll.SlotShownIn(cell);

    public void Show(PlayerState player, ItemTiers tiers, int[] hotbarActions)
    {
        for (var cell = 0; cell < InventoryScroll.VisibleSlots; cell++)
        {
            var slot = scroll.SlotShownIn(cell);
            var item = player.Inventory[slot];
            var slotView = GetChild<ItemSlotView>(cell);
            slotView.Show(item, [], false, ItemMark.Carried(tiers, slot));
            slotView.ShowHotbarKey(HotbarButtons.InventoryKeyButton(hotbarActions, slot));
        }
    }
}
