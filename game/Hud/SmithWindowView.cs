using Godot;
using TheLastGate.Items;
using TheLastGate.State;
using TheLastGate.Windows;

namespace TheLastGate.Hud;

// The weapon or armour smith. Claystone in the middle moves that slot up and adds a second material slot below it;
// a whetstone brings in the two outer buttons.
public partial class SmithWindowView : Control
{
    public static readonly (string Node, int Slot)[] ItemSlots =
    [
        ("Slot0", 0),
        ("Slot1", 1),
        ("UpperSlot1", 1),
        ("Slot2", 2),
        ("Slot3", 3),
    ];

    public void ShowItems(GameState state)
    {
        Visible = state.OpenShop is ShopKind.WeaponSmith or ShopKind.ArmourSmith;
        if (!Visible)
        {
            return;
        }

        var items = state.Player.SmithItems;
        var twoMaterials = SmithClicks.UsesTwoMaterials(items);
        var outerButtons = SmithClicks.ShowsOuterButtons(items);
        GetNode<CanvasItem>("Frame").Visible = !twoMaterials;
        GetNode<CanvasItem>("TwoMaterialFrame").Visible = twoMaterials;
        GetNode<CanvasItem>("WeaponWhetstone").Visible = outerButtons && state.OpenShop == ShopKind.WeaponSmith;
        GetNode<CanvasItem>("ArmourWhetstone").Visible = outerButtons && state.OpenShop == ShopKind.ArmourSmith;
        GetNode<CanvasItem>("Slot1").Visible = !twoMaterials;
        GetNode<CanvasItem>("UpperSlot1").Visible = twoMaterials;
        GetNode<CanvasItem>("Slot2").Visible = twoMaterials;
        GetNode<CanvasItem>($"Button{SmithClicks.FirstButton}").Visible = outerButtons;
        GetNode<CanvasItem>($"Button{SmithClicks.LastButton}").Visible = outerButtons;

        foreach (var (node, slot) in ItemSlots)
        {
            var item = items[slot];
            var overlays = ItemOverlays.Stored(new DepotItem(item.Sprite, item.Stack, item.Flags, 0));
            GetNode<ItemSlotView>(node).Show(item with { Properties = 0 }, overlays, false);
        }
    }
}
