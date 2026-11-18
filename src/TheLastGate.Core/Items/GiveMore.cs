using System;
using TheLastGate.State;

namespace TheLastGate.Items;

public sealed class GiveMore
{
    public bool Enabled { get; set; }

    public Action<byte[]> Send { get; set; } = _ => { };

    public void Gave(ItemSlot given) => throw new NotImplementedException();

    public void HeldItemChanged(PlayerState player) => throw new NotImplementedException();
}
