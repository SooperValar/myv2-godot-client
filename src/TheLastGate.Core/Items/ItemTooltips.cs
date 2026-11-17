using System;
using System.Collections.Generic;
using TheLastGate.State;

namespace TheLastGate.Items;

public enum TooltipSlotKind
{
    Inventory,
    Worn,
}

public sealed record ItemTooltip(IReadOnlyList<LogLine> Lines, ItemDetails Details);

public sealed class ItemTooltips
{
    private readonly TextLog chat;

    public ItemTooltips(TextLog chat) => this.chat = chat;

    public Action<byte[]> SendLook { get; set; } = _ => { };

    public bool Enabled { get; set; } = true;

    public ItemTooltip? Held => throw new NotImplementedException();

    public static bool IsManualLook(byte[] packet) => throw new NotImplementedException();

    public void Reset() => throw new NotImplementedException();

    public void SlotChanged(TooltipSlotKind kind, int slot, int sprite) => throw new NotImplementedException();

    public void HeldItemChanged(int sprite) => throw new NotImplementedException();

    public void ManualLookSent() => throw new NotImplementedException();

    public bool Offer(LogLine line) => throw new NotImplementedException();

    public void EndOfTick() => throw new NotImplementedException();

    public void SetHovered(TooltipSlotKind kind, int slot) => throw new NotImplementedException();

    public void ClearHovered() => throw new NotImplementedException();

    public ItemTooltip? For(TooltipSlotKind kind, int slot) => throw new NotImplementedException();
}
