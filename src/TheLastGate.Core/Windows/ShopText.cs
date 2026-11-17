using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TheLastGate.State;

namespace TheLastGate.Windows;

public readonly record struct ShopBalances(
    int Money,
    int StrongholdPoints,
    int Tokens,
    int ContractPoints,
    int UnspentExperience);

// The current client's shop lines, padded with spaces so the numbers line up in its fixed-width font.
public static class ShopText
{
    private const int SilverPerGold = 100;
    private const int BuyTenCount = 10;
    private const int NumberWidth = 9;
    private const int SilverWidth = 2;

    public static string? Price(ShopKind kind, int price, bool buyingTen)
    {
        if (price <= 0)
        {
            return null;
        }

        return kind switch
        {
            ShopKind.Merchant when buyingTen => $"Buy 10 for: {Money(price * BuyTenCount)}",
            ShopKind.Merchant => $"   Buy for: {Money(price)}",
            ShopKind.BlackStronghold => $"    Take reward for: {Padded(price)} Stronghold Pts",
            ShopKind.Casino => $"Take reward for: {Padded(price)} Tokens",
            ShopKind.Contract => $"  Take reward for: {Padded(price)} Contract Pts",
            ShopKind.Experience => $"         Buy for: {Padded(price)} Exp",
            _ => null,
        };
    }

    public static string? Sell(ShopKind kind, int heldItemPrice) =>
        kind == ShopKind.Merchant && heldItemPrice > 0 ? $"  Sell for: {Money(heldItemPrice)}" : null;

    // Shown only if something is for sale, so a corpse's loot window has no money line.
    public static string? Balance(ShopKind kind, IReadOnlyList<ShopItem> items, ShopBalances balances)
    {
        if (!items.Any(item => item.Price > 0))
        {
            return null;
        }

        return kind switch
        {
            ShopKind.Merchant => $"Your Money: {Money(balances.Money)}",
            ShopKind.BlackStronghold => $"Your Stronghold Pts: {Padded(balances.StrongholdPoints)}",
            ShopKind.Casino => $"    Your Tokens: {Padded(balances.Tokens)}",
            ShopKind.Contract => $"Your Contract Pts: {Padded(balances.ContractPoints)}",
            ShopKind.Experience => $"Your Unspent Exp: {Padded(balances.UnspentExperience)}",
            _ => null,
        };
    }

    private static string Money(int silver) =>
        $"{Padded(silver / SilverPerGold)}G {Whole(silver % SilverPerGold).PadLeft(SilverWidth)}S";

    private static string Padded(int value) => Whole(value).PadLeft(NumberWidth);

    private static string Whole(int value) => value.ToString(CultureInfo.InvariantCulture);
}
