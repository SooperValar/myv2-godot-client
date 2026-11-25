using System.Linq;
using Godot;
using TheLastGate.Assets;
using TheLastGate.Items;
using TheLastGate.State;
using TheLastGate.Windows;

namespace TheLastGate.Hud;

// A merchant's or a reward shop's 62 items, a corpse's loot, or what waits in the trash window.
public partial class ShopWindowView : Control
{
    private static readonly StyleBoxFlat ArmedButtonStyle = new()
    {
        BgColor = Color.Color8(150, 30, 30),
        BorderColor = Color.Color8(140, 107, 58),
        BorderWidthLeft = 1,
        BorderWidthTop = 1,
        BorderWidthRight = 1,
        BorderWidthBottom = 1,
    };

    public static bool Shows(ShopKind? kind) =>
        kind is ShopKind.Merchant or ShopKind.BlackStronghold or ShopKind.Casino or ShopKind.Contract
            or ShopKind.Experience or ShopKind.Trash or ShopKind.Bank;

    public void ShowItems(GameState state)
    {
        Visible = Shows(state.OpenShop);
        if (!Visible)
        {
            return;
        }

        GetNode<CanvasItem>("TrashButton").Visible = state.OpenShop == ShopKind.Trash;
        var items = state.Shop.ShopItems;
        var grid = GetNode("Grid");
        for (var cell = 0; cell < ShopClicks.Cells; cell++)
        {
            var item = cell < items.Length ? items[cell] : default;
            grid.GetChild<ItemSlotView>(cell)
                .Show(new ItemSlot(item.Sprite, 0, 0, 0), [], false, ItemMark.Shop(state.ItemTiers, cell));
        }
    }

    // The price follows the mouse and the held keys, so it is updated every frame.
    public void ShowText(GameState state, int? hoveredCell, bool buyingTen, bool trashArmed)
    {
        if (!Visible || state.OpenShop is not { } kind)
        {
            return;
        }

        if (kind == ShopKind.Trash)
        {
            ShowTrashText(state.Shop.ShopItems.Count(item => item.Sprite != 0), trashArmed);
            return;
        }

        GetNode<HudLabel>("Balance").Colour = TextColour.Yellow;
        GetNode<HudLabel>("Price").Colour = TextColour.Yellow;

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

    private void ShowTrashText(int itemCount, bool armed)
    {
        var plural = itemCount == 1 ? "" : "s";
        var balance = GetNode<HudLabel>("Balance");
        var price = GetNode<HudLabel>("Price");
        balance.Text = armed ? "This can't be undone!" : "Shift+click backpack items";
        price.Text = armed ? $"Click again: destroy {itemCount} item{plural}" : $"to put them here ({itemCount})";
        balance.Colour = armed ? TextColour.Red : TextColour.Yellow;
        price.Colour = balance.Colour;
        var button = GetNode<Control>("TrashButton");
        button.GetNode<HudLabel>("Text").Text = armed ? "Click again" : "Trash";
        var panel = button.GetNode<Panel>("Panel");
        if (armed)
        {
            panel.AddThemeStyleboxOverride("panel", ArmedButtonStyle);
        }
        else
        {
            panel.RemoveThemeStyleboxOverride("panel");
        }
    }
}
