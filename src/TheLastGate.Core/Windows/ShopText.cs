using System;
using System.Collections.Generic;
using TheLastGate.State;

namespace TheLastGate.Windows;

public readonly record struct ShopBalances(
    int Money,
    int StrongholdPoints,
    int Tokens,
    int ContractPoints,
    int UnspentExperience);

public static class ShopText
{
    public static string? Price(ShopKind kind, int price, bool buyingTen) => throw new NotImplementedException();

    public static string? Sell(ShopKind kind, int heldItemPrice) => throw new NotImplementedException();

    public static string? Balance(ShopKind kind, IReadOnlyList<ShopItem> items, ShopBalances balances) =>
        throw new NotImplementedException();
}
