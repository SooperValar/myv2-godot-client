using TheLastGate.Items;
using TheLastGate.Net;
using TheLastGate.State;

namespace TheLastGate.Tests;

public class ItemLookWiringTests
{
    private const int ChunkLength = 15;
    private const ushort Sword = 100;
    private readonly TickProcessor processor;

    private readonly List<byte[]> sent = [];
    private readonly GameState state = new();

    public ItemLookWiringTests()
    {
        state.ItemLooks.SendLook = sent.Add;
        processor = new TickProcessor(state);
    }

    [Fact]
    public void AnInventoryUpdate_FetchesTheSlotInTheBackground()
    {
        Process(InventoryItem(5, Sword));
        Process();
        Process();
        Process();

        Assert.Equal([ItemLook(5), ItemLook(5)], sent);
    }

    [Fact]
    public void AWornUpdate_FetchesTheGearSlotInTheBackground()
    {
        Process(TestCommand.Of(ServerCommand.SetCharacterWorn).UInt32(8).UInt16(Sword).UInt16(0));
        Process();
        Process();
        Process();

        var gearLook = InventoryPackets.For(InventoryAction.LookAtGear, 8, 0);
        Assert.Equal([gearLook, gearLook], sent);
    }

    [Fact]
    public void HoldingAnItem_StopsBackgroundFetches()
    {
        Process(
            InventoryItem(5, Sword),
            TestCommand.Of(ServerCommand.SetCharacterHeldItem).UInt16(Sword).UInt16(0));
        for (var tick = 0; tick < 10; tick++)
        {
            Process();
        }

        Assert.Empty(sent);
    }

    private static TestCommand InventoryItem(uint slot, ushort sprite) =>
        TestCommand.Of(ServerCommand.SetCharacterItem).UInt32(slot).UInt16(sprite).UInt16(0);

    private static TestCommand Chunk(ServerCommand commandId, string text) =>
        TestCommand.Of(commandId).Text(text, ChunkLength);

    private static byte[] ItemLook(int slot) => InventoryPackets.For(InventoryAction.LookAtItem, slot, 0);

    private void FetchSlotFive()
    {
        Process(InventoryItem(5, Sword));
        Process();
        Process();
        Process();
    }

    private void Process(params TestCommand[] commands) => processor.Process(TestCommand.Payload(commands));
}
