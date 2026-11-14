using TheLastGate.Net;
using TheLastGate.State;

namespace TheLastGate.Tests;

public class ItemCommandsTests
{
    [Fact]
    public void SetCharacterItem_FillsAnInventorySlot()
    {
        var state = Process(
            TestCommand.Of(ServerCommand.SetCharacterItem).UInt32(59).UInt16(20000).UInt16(0xFFFB).Bytes(7, 1));

        Assert.Equal(new ItemSlot(20000, -5, 7, 1), state.Player.Inventory[59]);
    }

    [Fact]
    public void SetCharacterItem_IgnoresASlotPastTheInventory()
    {
        var state = Process(
            TestCommand.Of(ServerCommand.SetCharacterItem).UInt32(60).UInt16(20000).UInt16(1).Bytes(7, 1));

        Assert.All(state.Player.Inventory, slot => Assert.Equal(default, slot));
    }

    [Fact]
    public void SetCharacterWorn_FillsAWornSlotWithNoFlags()
    {
        var state = Process(
            TestCommand.Of(ServerCommand.SetCharacterWorn).UInt32(19).UInt16(0x8001).UInt16(300).Byte(2));

        Assert.Equal(new ItemSlot(-32767, 300, 2, 0), state.Player.Worn[19]);
    }

    [Fact]
    public void SetCharacterWorn_IgnoresASlotPastTheTwentieth()
    {
        var state = Process(TestCommand.Of(ServerCommand.SetCharacterWorn).UInt32(20).UInt16(1).UInt16(1).Byte(1));

        Assert.All(state.Player.Worn, slot => Assert.Equal(default, slot));
    }

    [Theory]
    [InlineData((ushort)5, 5)]
    [InlineData((ushort)0x0105, 5)]
    [InlineData((ushort)0x00FF, -1)]
    public void SetCharacterSpell_KeepsTheLowByteOfActiveAsASignedValue(ushort active, int expected)
    {
        var state = Process(TestCommand.Of(ServerCommand.SetCharacterSpell).UInt32(39).UInt16(1234).UInt16(active));

        Assert.Equal(new BuffSlot(1234, expected), state.Player.Buffs[39]);
    }

    [Fact]
    public void SetCharacterSpell_IgnoresASlotPastTheFortieth()
    {
        var state = Process(TestCommand.Of(ServerCommand.SetCharacterSpell).UInt32(40).UInt16(1234).UInt16(1));

        Assert.All(state.Player.Buffs, slot => Assert.Equal(default, slot));
    }

    [Fact]
    public void SetCharacterHeldItem_SetsTheItemOnTheCursor()
    {
        var state = Process(TestCommand.Of(ServerCommand.SetCharacterHeldItem).UInt16(4321).UInt16(0x8000).Byte(3));

        Assert.Equal(new ItemSlot(4321, -32768, 3, 0), state.Player.HeldItem);
    }

    private static GameState Process(params TestCommand[] commands)
    {
        var state = new GameState();
        new TickProcessor(state).Process(TestCommand.Payload(commands));
        return state;
    }
}
