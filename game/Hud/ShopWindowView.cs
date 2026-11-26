using System.Linq;
using Godot;
using TheLastGate.Assets;
using TheLastGate.Items;
using TheLastGate.State;
using TheLastGate.Windows;

namespace TheLastGate.Hud;

// A merchant's or a reward shop's 62 items, a grave's loot in two pages, or what waits in the trash window.
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
            or ShopKind.Experience or ShopKind.Trash or ShopKind.Bank or ShopKind.Grave;

    public static int SlotFor(ShopKind? kind, int gravePage, int cell) =>
        kind == ShopKind.Grave ? GravePages.SlotFor(gravePage, cell) : ShopClicks.SlotFor(cell);

    public void ShowItems(GameState state, int gravePage)
    {
        Visible = Shows(state.OpenShop);
        if (!Visible)
        {
            return;
        }

        GetNode<CanvasItem>("TrashButton").Visible = state.OpenShop == ShopKind.Trash;
        var items = state.Shop.ShopItems;
        ShowPageButton(state.OpenShop == ShopKind.Grave, gravePage, GravePages.SecondPageCount(items));
        var grid = GetNode("Grid");
        for (var cell = 0; cell < ShopClicks.Cells; cell++)
        {
            var slot = SlotFor(state.OpenShop, gravePage, cell);
            var item = slot < items.Length ? items[slot] : default;
            grid.GetChild<ItemSlotView>(cell)
                .Show(new ItemSlot(item.Sprite, 0, 0, 0), [], false, ItemMark.Shop(state.ItemTiers, slot));
        }
    }

    // On the backpack page the button is lit only when the second page holds something.
    private void ShowPageButton(bool shown, int page, int secondPageCount)
    {
        var button = GetNode<Control>("PageButton");
        button.Visible = shown;
        if (!shown)
        {
            return;
        }

        var text = button.GetNode<HudLabel>("Text");
        text.Text = GravePages.ButtonText(page, secondPageCount);
        text.Colour = page == 0 && secondPageCount == 0 ? TextColour.Dim : TextColour.Yellow;
    }

    // The price follows the mouse and the held keys, so it is updated every frame.
    public void ShowText(GameState state, int? hoveredCell, int gravePage, bool buyingTen, bool trashArmed)
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
        else if (hoveredCell is { } cell && SlotFor(kind, gravePage, cell) is var slot && slot < items.Length)
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
