using System;
using System.Diagnostics;
using TheLastGate.State;

namespace TheLastGate.Net.Handlers;

internal static class LookCommands
{
    private const int TrashWindowBit = 0x4000;
    private const int DepotWindowBit = 0x8000;

    private const int NameBytesSent = 15;
    private const int ShopItemsPerCommand = 2;
    private const uint SoulstoneBit = 1u << 30;
    private const uint EnchantedBit = 1u << 31;
    private const uint PriceMask = SoulstoneBit - 1;

    public static void Apply(GameState state, ReadOnlySpan<byte> command)
    {
        var look = state.PendingLook;
        var reader = new PacketReader(command);
        switch ((ServerCommand)reader.ReadByte())
        {
            case ServerCommand.Look1:
                look.Worn[0] = reader.ReadUInt16();
                look.Worn[2] = reader.ReadUInt16();
                look.Worn[3] = reader.ReadUInt16();
                look.Worn[5] = reader.ReadUInt16();
                look.Worn[6] = reader.ReadUInt16();
                look.Worn[7] = reader.ReadUInt16();
                look.Worn[8] = reader.ReadUInt16();
                look.IsAutoLook = reader.ReadByte() != 0;
                break;
            case ServerCommand.Look2:
                look.Worn[9] = reader.ReadUInt16();
                look.Sprite = reader.ReadUInt16();
                look.Points = (int)reader.ReadUInt32();
                look.HitPoints = (int)reader.ReadUInt32();
                look.Worn[10] = reader.ReadUInt16();
                break;
            case ServerCommand.Look3:
                look.Endurance = reader.ReadUInt16();
                look.ActiveHitPoints = reader.ReadUInt16();
                look.ActiveEndurance = reader.ReadUInt16();
                look.Number = reader.ReadUInt16();
                look.Id = reader.ReadUInt16();
                look.Mana = reader.ReadUInt16();
                look.ActiveMana = reader.ReadUInt16();
                break;
            case ServerCommand.Look4:
                look.Worn[1] = reader.ReadUInt16();
                look.Worn[4] = reader.ReadUInt16();
                look.Flags = (LookFlags)reader.ReadByte();
                look.HeldItemPrice = (int)reader.ReadUInt32();
                look.Worn[11] = reader.ReadUInt16();
                look.Worn[12] = reader.ReadUInt16();
                look.Worn[13] = reader.ReadUInt16();
                break;
            case ServerCommand.Look5:
                reader.ReadBytes(NameBytesSent).CopyTo(look.NameField);
                Finish(state, look);
                break;
            case ServerCommand.Look6:
                ReadShopItems(state, ref reader);
                break;
            default:
                throw new UnreachableException();
        }
    }

    private static void Finish(GameState state, CharacterLook look)
    {
        if (look.Flags.HasFlag(LookFlags.Shop))
        {
            return;
        }

        if (!look.IsAutoLook)
        {
            state.Look = look.Copy();
            state.ReportLookShown(state.Look);
        }

        state.Names.Remember(look.Number, look.Id, look.Name);
    }

    private static void ReadShopItems(GameState state, ref PacketReader reader)
    {
        var look = state.PendingLook;
        var firstSlot = reader.ReadByte();
        Span<int> sprites = stackalloc int[ShopItemsPerCommand];
        Span<uint> prices = stackalloc uint[ShopItemsPerCommand];
        for (var offset = 0; offset < ShopItemsPerCommand; offset++)
        {
            sprites[offset] = reader.ReadUInt16();
            prices[offset] = reader.ReadUInt32();
        }

        var catalysts = reader.ReadBytes(ShopItemsPerCommand);
        for (var offset = 0; offset < ShopItemsPerCommand && firstSlot + offset < CharacterLook.ShopSlots; offset++)
        {
            look.ShopItems[firstSlot + offset] = new ShopItem(
                sprites[offset],
                (int)(prices[offset] & PriceMask),
                (prices[offset] & SoulstoneBit) != 0,
                (prices[offset] & EnchantedBit) != 0,
                catalysts[offset]);
        }

        // The server sends the shop's kind in the first catalyst byte of the pair that reaches the last slot.
        if (firstSlot is >= CharacterLook.ShopSlots - ShopItemsPerCommand and <= CharacterLook.ShopSlots)
        {
            OpenShop(state, IsTrash(look.Number) ? ShopKind.Trash : ShopKindOf(catalysts[0]), look.Copy());
        }
    }

    private static void OpenShop(GameState state, ShopKind kind, CharacterLook shop)
    {
        state.Shop = shop;
        state.OpenShop = kind;
        state.ReportShopOpened(kind);
    }

    // MyV2 opens its trash window as a shop whose number carries 0x4000; its depot window carries 0x8000.
    private static bool IsTrash(int shopNumber) => (shopNumber & (TrashWindowBit | DepotWindowBit)) == TrashWindowBit;

    private static ShopKind ShopKindOf(byte kindByte) => kindByte switch
    {
        101 => ShopKind.BlackStronghold,
        102 => ShopKind.Casino,
        103 => ShopKind.Contract,
        104 => ShopKind.Experience,
        _ => ShopKind.Merchant,
    };
}
