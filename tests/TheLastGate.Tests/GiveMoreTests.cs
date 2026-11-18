using TheLastGate.Net;
using TheLastGate.State;

namespace TheLastGate.Tests;

public class GiveMoreTests
{
    private const int Potion = 2500;
    private const int Bread = 2600;
    private const int Plain = 0;
    private const int Enchanted = 9;
    private readonly TickProcessor processor;
    private readonly List<byte[]> sent = [];

    private readonly GameState state = new();

    public GiveMoreTests()
    {
        processor = new TickProcessor(state);
        state.GiveMore.Enabled = true;
        state.GiveMore.Send = sent.Add;
    }

    [Fact]
    public void AnEmptyHand_PicksUpTheFirstItemLikeTheOneGiven()
    {
        Play(InventoryItem(4, Bread, Plain), InventoryItem(7, Potion, Enchanted), InventoryItem(9, Potion, Plain));
        Hold(Potion, Plain);
        state.GiveMore.Gave(state.Player.HeldItem);

        Hold(0, 0);

        Assert.Equal([InventoryPackets.For(InventoryAction.SwapWithInventorySlot, 9, 0)], sent);
    }

    [Fact]
    public void WithNothingLikeIt_NothingIsPickedUp()
    {
        Play(InventoryItem(4, Bread, Plain));
        Hold(Potion, Plain);
        state.GiveMore.Gave(state.Player.HeldItem);

        Hold(0, 0);

        Assert.Empty(sent);
    }

    [Fact]
    public void TheGiftIsForgottenOnceTheHandEmpties()
    {
        Play(InventoryItem(9, Potion, Plain));
        Hold(Potion, Plain);
        state.GiveMore.Gave(state.Player.HeldItem);
        Hold(0, 0);
        Hold(Bread, Plain);

        Hold(0, 0);

        Assert.Single(sent);
    }

    [Fact]
    public void AHandStillHoldingSomething_PicksUpNothing()
    {
        Play(InventoryItem(9, Potion, Plain));
        Hold(Potion, Plain);
        state.GiveMore.Gave(state.Player.HeldItem);

        Hold(Bread, Plain);

        Assert.Empty(sent);
    }

    [Fact]
    public void TurningTheOptionOff_ForgetsTheGift()
    {
        Play(InventoryItem(9, Potion, Plain));
        Hold(Potion, Plain);
        state.GiveMore.Gave(state.Player.HeldItem);

        state.GiveMore.Enabled = false;
        state.GiveMore.Enabled = true;
        Hold(0, 0);

        Assert.Empty(sent);
    }

    private void Hold(int sprite, int properties) =>
        Play(
            TestCommand.Of(ServerCommand.SetCharacterHeldItem)
                .UInt16((ushort)sprite)
                .UInt16((ushort)properties)
                .Byte(1));

    private void Play(params TestCommand[] commands) => processor.Process(TestCommand.Payload(commands));

    private static TestCommand InventoryItem(int slot, int sprite, int properties) =>
        TestCommand.Of(ServerCommand.SetCharacterItem)
            .UInt32((uint)slot)
            .UInt16((ushort)sprite)
            .UInt16((ushort)properties)
            .Bytes(1, 0);
}
