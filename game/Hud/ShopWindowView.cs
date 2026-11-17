using Godot;
using TheLastGate.Items;
using TheLastGate.State;
using TheLastGate.Windows;

namespace TheLastGate.Hud;

// A merchant's or a reward shop's 62 items, or a corpse's loot.
public partial class ShopWindowView : Control
{
    public static bool Shows(ShopKind? kind) =>
        kind is ShopKind.Merchant or ShopKind.BlackStronghold or ShopKind.Casino or ShopKind.Contract
            or ShopKind.Experience;

    public void ShowItems(GameState state)
    {
        Visible = Shows(state.OpenShop);
        if (!Visible)
        {
            return;
        }

        var items = state.Shop.ShopItems;
        var grid = GetNode("Grid");
        for (var cell = 0; cell < ShopClicks.Cells; cell++)
        {
            var item = cell < items.Length ? items[cell] : default;
            grid.GetChild<ItemSlotView>(cell).Show(new ItemSlot(item.Sprite, 0, 0, 0), ItemOverlays.Shop(item), false);
        }
    }

    // The price follows the mouse and the held keys, so it is updated every frame.
    public void ShowText(GameState state, int? hoveredCell, bool buyingTen)
    {
        if (!Visible || state.OpenShop is not { } kind)
        {
            return;
        }

        var items = state.Shop.ShopItems;
        var player = state.Player;
        string? price = null;
        if (player.HeldItem.Sprite != 0)
        {
            price = ShopText.Sell(kind, state.Shop.HeldItemPrice);
        }
        else if (hoveredCell is { } cell && ShopClicks.SlotFor(cell) is var slot && slot < items.Length)
        {
            price = ShopText.Price(kind, items[slot].Price, buyingTen);
        }

        var balances = new ShopBalances(
            player.Gold,
            player.BlackStrongholdPoints,
            player.Tokens,
            player.OsirisPoints,
            player.Points);
        GetNode<Label>("Price").Text = price ?? "";
        GetNode<Label>("Balance").Text = ShopText.Balance(kind, items, balances) ?? "";
    }
}
