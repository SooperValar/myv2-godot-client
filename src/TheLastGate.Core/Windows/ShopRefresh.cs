using System;
using TheLastGate.State;

namespace TheLastGate.Windows;

public sealed class ShopRefresh
{
    public Action<byte[]> Send { get; set; } = _ => { };

    public void EndOfTick(ShopKind? openShop, int shopNumber) => throw new NotImplementedException();
}
