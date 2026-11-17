using Godot;
using TheLastGate.Items;
using TheLastGate.State;
using TheLastGate.Windows;

namespace TheLastGate.Hud;

// One page of the depot's 512 slots, with buttons for the eight pages.
public partial class DepotWindowView : Control
{
    // The current client's wheel area, relative to the blocker: it stops short of the depot's right edge.
    private static readonly Rect2 WheelArea = new(0, 1, 266, 279);

    private static readonly Vector2 PageBoxOffset = new(-1, -1);

    public static bool InWheelArea(Vector2 position) => WheelArea.HasPoint(position);

    public void ShowItems(GameState state, DepotPage page)
    {
        Visible = state.OpenShop == ShopKind.Depot;
        if (!Visible)
        {
            return;
        }

        var items = state.Shop.Depot[page.Current];
        var grid = GetNode("Blocker/Grid");
        for (var cell = 0; cell < DepotClicks.Cells; cell++)
        {
            var item = items[cell];
            var overlays = ItemOverlays.Stored(item);
            grid.GetChild<ItemSlotView>(cell).Show(new ItemSlot(item.Sprite, 0, 0, 0), overlays, false);
        }

        var button = GetNode<Control>($"Blocker/PageButton{page.Current}");
        GetNode<Control>("Blocker/PageBox").Position = button.Position + PageBoxOffset;
    }
}
