using System;
using TheLastGate.State;

namespace TheLastGate.Net.Handlers;

internal static class ItemCommands
{
    public static void Apply(PlayerState player, ReadOnlySpan<byte> command)
    {
        var reader = new PacketReader(command);
        switch ((ServerCommand)reader.ReadByte())
        {
            case ServerCommand.SetCharacterItem:
            {
                var slot = reader.ReadUInt32();
                Store(
                    player.Inventory,
                    slot,
                    new ItemSlot(reader.ReadInt16(), reader.ReadInt16(), reader.ReadByte(), reader.ReadByte()));
                break;
            }
            case ServerCommand.SetCharacterWorn:
            {
                var slot = reader.ReadUInt32();
                Store(player.Worn, slot, new ItemSlot(reader.ReadInt16(), reader.ReadInt16(), reader.ReadByte(), 0));
                break;
            }
            case ServerCommand.SetCharacterSpell:
            {
                var slot = reader.ReadUInt32();
                var sprite = reader.ReadInt16();
                // The current client keeps this in a signed char, so only the low byte counts.
                var active = (sbyte)reader.ReadInt16();
                Store(player.Buffs, slot, new BuffSlot(sprite, active));
                break;
            }
            case ServerCommand.SetCharacterHeldItem:
                player.HeldItem = new ItemSlot(reader.ReadInt16(), reader.ReadInt16(), reader.ReadByte(), 0);
                break;
        }
    }

    private static void Store<TSlot>(TSlot[] slots, uint slot, TSlot value)
    {
        if (slot < slots.Length)
        {
            slots[slot] = value;
        }
    }
}
