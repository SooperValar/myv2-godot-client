using TheLastGate.Assets;
using TheLastGate.Items;
using TheLastGate.Net;
using TheLastGate.State;

namespace TheLastGate.Tests;

public class ItemTooltipsTests
{
    private const int Sword = 100;
    private const int Shield = 200;

    private static readonly LogLine Header = new("Steel Sword:", (int)TextColour.Lime);
    private static readonly LogLine OtherHeader = new("Bronze Sword:", (int)TextColour.Lime);
    private static readonly LogLine Description = new("A finely crafted steel sword.", (int)TextColour.Yellow);
    private static readonly LogLine Chatter = new("Bob: hello", (int)TextColour.Blue);

    private readonly TextLog chat = new();
    private readonly List<byte[]> sent = [];
    private readonly ItemTooltips tooltips;

    public ItemTooltipsTests() => tooltips = new ItemTooltips(chat) { SendLook = sent.Add };

    [Fact]
    public void SlotChanged_FetchesTheSlotQuietlyOnceItsUpdatesSettle()
    {
        tooltips.SlotChanged(TooltipSlotKind.Inventory, 5, Sword);

        Ticks(3);
        Assert.Empty(sent);
        Ticks(1);
        Assert.Equal([ItemLook(5), ItemLook(5)], sent);
    }

    [Fact]
    public void For_ShowsTheAnsweredLookAsTheSlotsTooltip()
    {
        FetchInventorySlot(5, Sword);

        Answer(Header, Description);

        var tooltip = tooltips.For(TooltipSlotKind.Inventory, 5);
        Assert.NotNull(tooltip);
        Assert.Equal([Header, Description], tooltip.Lines);
        Assert.Equal("Steel Sword", tooltip.Details.Name);
        Assert.Empty(chat.Lines);
    }

    [Fact]
    public void EndOfTick_FetchesWornSlotsBeforeTheInventory()
    {
        tooltips.SlotChanged(TooltipSlotKind.Inventory, 0, Sword);
        tooltips.SlotChanged(TooltipSlotKind.Worn, 8, Shield);

        Ticks(4);

        Assert.Equal([GearLook(8), GearLook(8)], sent);
    }

    [Fact]
    public void EndOfTick_FetchesTheHoveredSlotFirst()
    {
        tooltips.SlotChanged(TooltipSlotKind.Inventory, 3, Sword);
        tooltips.SlotChanged(TooltipSlotKind.Inventory, 9, Shield);
        tooltips.SetHovered(TooltipSlotKind.Inventory, 9);

        Ticks(4);

        Assert.Equal([ItemLook(9), ItemLook(9)], sent);
    }

    [Fact]
    public void EndOfTick_FetchesOneSlotAtATime()
    {
        tooltips.SlotChanged(TooltipSlotKind.Inventory, 3, Sword);
        tooltips.SlotChanged(TooltipSlotKind.Inventory, 9, Shield);

        Ticks(20);

        Assert.Equal(2, sent.Count);
    }

    [Fact]
    public void EndOfTick_FetchesNothingWhileAnItemIsHeld()
    {
        tooltips.HeldItemChanged(Shield);
        tooltips.SlotChanged(TooltipSlotKind.Inventory, 5, Sword);

        Ticks(20);
        Assert.Empty(sent);

        tooltips.HeldItemChanged(0);
        Ticks(1);
        Assert.Equal([ItemLook(5), ItemLook(5)], sent);
    }

    [Fact]
    public void EndOfTick_FetchesNothingWhenTooltipsAreTurnedOff()
    {
        tooltips.Enabled = false;
        tooltips.SlotChanged(TooltipSlotKind.Inventory, 5, Sword);

        Ticks(20);

        Assert.Empty(sent);
    }

    [Fact]
    public void EndOfTick_FetchesTheSameSlotAgainAtMostEveryFortyTicks()
    {
        FetchInventorySlot(5, Sword);
        Answer(Header, Description);
        tooltips.SlotChanged(TooltipSlotKind.Inventory, 5, Sword);
        sent.Clear();

        Ticks(38);
        Assert.Empty(sent);
        Ticks(1);
        Assert.Equal(2, sent.Count);
    }

    [Fact]
    public void For_KeepsTheOldTextWhileTheSameItemIsFetchedAgain()
    {
        FetchInventorySlot(5, Sword);
        Answer(Header, Description);

        tooltips.SlotChanged(TooltipSlotKind.Inventory, 5, Sword);

        Assert.NotNull(tooltips.For(TooltipSlotKind.Inventory, 5));
    }

    [Fact]
    public void For_ForgetsTheTextWhenADifferentItemArrives()
    {
        FetchInventorySlot(5, Sword);
        Answer(Header, Description);

        tooltips.SlotChanged(TooltipSlotKind.Inventory, 5, Shield);

        Assert.Null(tooltips.For(TooltipSlotKind.Inventory, 5));
    }

    [Fact]
    public void For_ShowsNothingForAnEmptySlot()
    {
        FetchInventorySlot(5, Sword);
        Answer(Header, Description);

        tooltips.SlotChanged(TooltipSlotKind.Inventory, 5, 0);

        Assert.Null(tooltips.For(TooltipSlotKind.Inventory, 5));
    }

    [Fact]
    public void EndOfTick_StopsAskingAboutAnItemThatNeverAnswers()
    {
        FetchInventorySlot(5, Sword);

        Ticks(31);
        tooltips.SlotChanged(TooltipSlotKind.Inventory, 8, Sword);
        Ticks(100);

        Assert.Equal(2, sent.Count);
    }

    [Fact]
    public void EndOfTick_RetriesAMismatchedAnswerOnceThenGivesUp()
    {
        FetchInventorySlot(5, Sword);
        AnswerDifferently();

        Ticks(50);
        Assert.Equal(4, sent.Count);

        AnswerDifferently();
        Ticks(100);
        Assert.Equal(4, sent.Count);
    }

    [Fact]
    public void Offer_ReleasesOtherChatCaughtWithTheAnswer()
    {
        FetchInventorySlot(5, Sword);

        tooltips.Offer(Header);
        tooltips.Offer(Description);
        tooltips.Offer(Chatter);
        tooltips.Offer(Header);
        tooltips.Offer(Description);
        tooltips.EndOfTick();

        Assert.Equal([Chatter], chat.Lines);
    }

    [Fact]
    public void EndOfTick_ReleasesWhatWasCaughtWhenItGivesUpWaiting()
    {
        FetchInventorySlot(5, Sword);

        tooltips.Offer(Header);
        Ticks(31);

        Assert.Equal([Header], chat.Lines);
    }

    [Fact]
    public void For_DiscardsAnAnswerForASlotThatChangedMeanwhile()
    {
        FetchInventorySlot(5, Sword);

        tooltips.SlotChanged(TooltipSlotKind.Inventory, 5, Shield);
        Answer(Header, Description);

        Assert.Null(tooltips.For(TooltipSlotKind.Inventory, 5));
    }

    [Fact]
    public void ManualLookSent_LetsTheReplyThroughAndPausesFetching()
    {
        tooltips.SlotChanged(TooltipSlotKind.Inventory, 5, Sword);

        tooltips.ManualLookSent();

        Assert.False(tooltips.Offer(Header));
        Ticks(19);
        Assert.Empty(sent);
        Ticks(1);
        Assert.Equal(2, sent.Count);
    }

    [Fact]
    public void HeldItemChanged_CarriesTheTextOfAnItemPickedUp()
    {
        FetchInventorySlot(5, Sword);
        Answer(Header, Description);

        tooltips.SlotChanged(TooltipSlotKind.Inventory, 5, 0);
        tooltips.HeldItemChanged(Sword);

        Assert.Equal([Header, Description], tooltips.Held?.Lines);
        Assert.Null(tooltips.For(TooltipSlotKind.Inventory, 5));
    }

    [Fact]
    public void HeldItemChanged_CarriesTheTextOfAnItemPutDown()
    {
        FetchInventorySlot(5, Sword);
        Answer(Header, Description);
        tooltips.SlotChanged(TooltipSlotKind.Inventory, 5, 0);
        tooltips.HeldItemChanged(Sword);
        tooltips.EndOfTick();

        tooltips.SlotChanged(TooltipSlotKind.Inventory, 7, Sword);
        tooltips.HeldItemChanged(0);

        Assert.Equal([Header, Description], tooltips.For(TooltipSlotKind.Inventory, 7)?.Lines);
        Assert.Null(tooltips.Held);
    }

    [Fact]
    public void Reset_ForgetsEverything()
    {
        FetchInventorySlot(5, Sword);
        Answer(Header, Description);

        tooltips.Reset();

        Assert.Null(tooltips.For(TooltipSlotKind.Inventory, 5));
    }

    [Theory]
    [MemberData(nameof(Packets))]
    public void IsManualLook_SpotsTheLooksAPlayerAsksFor(byte[] packet, bool manualLook)
    {
        Assert.Equal(manualLook, ItemTooltips.IsManualLook(packet));
    }

    public static TheoryData<byte[], bool> Packets() => new()
    {
        { ClientPackets.ThreeValues(ClientCommand.InventoryLook, 3, 0, 1), true },
        { InventoryPackets.For(InventoryAction.LookAtGear, 2, 1), true },
        { InventoryPackets.For(InventoryAction.UseItem, 2, 1), false },
        { MapCommandPackets.ForCharacter(ClientCommand.Look, 5), false },
    };

    private static byte[] ItemLook(int slot) => InventoryPackets.For(InventoryAction.LookAtItem, slot, 0);

    private static byte[] GearLook(int slot) => InventoryPackets.For(InventoryAction.LookAtGear, slot, 0);

    private void Ticks(int count)
    {
        for (var tick = 0; tick < count; tick++)
        {
            tooltips.EndOfTick();
        }
    }

    private void FetchInventorySlot(int slot, int sprite)
    {
        tooltips.SlotChanged(TooltipSlotKind.Inventory, slot, sprite);
        Ticks(4);
    }

    private void Answer(params LogLine[] reply)
    {
        foreach (var line in reply.Concat(reply))
        {
            tooltips.Offer(line);
        }

        tooltips.EndOfTick();
    }

    private void AnswerDifferently()
    {
        tooltips.Offer(Header);
        tooltips.Offer(OtherHeader);
        tooltips.EndOfTick();
    }
}
