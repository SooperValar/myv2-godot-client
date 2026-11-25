using GdUnit4;
using TheLastGate.Game;
using TheLastGate.Net;
using TheLastGate.State;
using TheLastGate.World;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Game;

[TestSuite]
public class GameSessionTests
{
    private const int OriginX = 500;
    private const int OriginY = 400;
    private const int StepTicks = 4;
    private const int ShopNumber = 1201;
    private const int TwoTreePointsOneSpent = (1 << 15) | 1;
    private const int NoTreePoints = 0;
    private const int Using = 4;
    private const int Idle = 0;
    private const int NoTutorial = -1;
    private const int Potion = 2500;
    private const int Bread = 2600;
    private const int Plain = 0;
    private const int Enchanted = 9;
    private const int MessageChunkLength = 15;

    private TickProcessor processor = null!;
    private List<byte[]> sent = [];
    private GameSession session = null!;
    private GameState state = new();

    [BeforeTest]
    public void StartASession()
    {
        state = new GameState();
        session = new GameSession(state);
        processor = new TickProcessor(state);
        sent = [];
        session.AutoWalk.Send = sent.Add;
        Process(TestCommand.Of(ServerCommand.SetOrigin).UInt16(OriginX).UInt16(OriginY));
        sent.Clear();
    }

    [TestCase(WalkDirection.North, 20, 27)]
    [TestCase(WalkDirection.West, 27, 20)]
    [TestCase(WalkDirection.South, 34, 27)]
    [TestCase(WalkDirection.East, 27, 34)]
    public void Walking_MovesToTheTileSevenAwayOnTheNextTick(WalkDirection direction, int column, int row)
    {
        session.AutoWalk.Start(direction);

        Process();

        AssertThat(sent).HasSize(1);
        AssertThat(sent[0]).ContainsExactly(Move(column, row));
    }

    [TestCase]
    public void Walking_StepsAgainEveryFourTicks()
    {
        session.AutoWalk.Start(WalkDirection.North);

        for (var tick = 0; tick < StepTicks; tick++)
        {
            Process();
        }

        AssertThat(sent).HasSize(1);
        Process();
        AssertThat(sent).HasSize(2);
    }

    [TestCase]
    public void Walking_StopsByItselfAfterAThousandSteps()
    {
        const int stepsBeforeStopping = 1002;
        session.AutoWalk.Start(WalkDirection.North);

        for (var tick = 0; tick < (stepsBeforeStopping + 10) * StepTicks; tick++)
        {
            Process();
        }

        AssertThat(sent).HasSize(stepsBeforeStopping);
        AssertThat(session.AutoWalk.Walking).IsFalse();
    }

    [TestCase]
    public void Stopping_SendsNoMoreSteps()
    {
        session.AutoWalk.Start(WalkDirection.North);
        Process();

        session.AutoWalk.Stop();
        for (var tick = 0; tick < StepTicks * 2; tick++)
        {
            Process();
        }

        AssertThat(sent).HasSize(1);
        AssertThat(session.AutoWalk.Walking).IsFalse();
    }

    [TestCase]
    public void TheServersSetTarget_ReachesTheUseQueue()
    {
        session.UseQueue.Enabled = true;
        session.UseQueue.Send = sent.Add;
        session.UseQueue.Add(10, 20, ClientCommand.Use, new PlayerTarget(0, 0, 0, Using, 1, 1));
        session.UseQueue.Add(11, 20, ClientCommand.Use, new PlayerTarget(0, 0, 0, Using, 1, 1));

        Process(SetTarget(Using, 1, 1));
        Process(SetTarget(Idle, 0, 0));

        AssertThat(sent).HasSize(1);
        AssertThat(sent[0]).ContainsExactly(MapCommandPackets.ForTile(ClientCommand.Use, 10, 20));
    }

    [TestCase]
    public void AnEmptyHand_PicksUpTheFirstItemLikeTheOneGiven()
    {
        GiveMore();
        Process(InventoryItem(4, Bread, Plain), InventoryItem(7, Potion, Enchanted), InventoryItem(9, Potion, Plain));
        Hold(Potion, Plain);
        session.GiveMore.Gave(state.Player.HeldItem);

        Hold(0, 0);

        AssertThat(sent).HasSize(1);
        AssertThat(sent[0]).ContainsExactly(InventoryPackets.For(InventoryAction.SwapWithInventorySlot, 9, 0));
    }

    [TestCase]
    public void TheGiftIsForgottenOnceTheHandEmpties()
    {
        GiveMore();
        Process(InventoryItem(9, Potion, Plain));
        Hold(Potion, Plain);
        session.GiveMore.Gave(state.Player.HeldItem);
        Hold(0, 0);
        Hold(Bread, Plain);

        Hold(0, 0);

        AssertThat(sent).HasSize(1);
    }

    [TestCase]
    public void WithNothingLikeIt_NothingIsPickedUp()
    {
        GiveMore();
        Process(InventoryItem(4, Bread, Plain));
        Hold(Potion, Plain);
        session.GiveMore.Gave(state.Player.HeldItem);

        Hold(0, 0);

        AssertThat(sent).IsEmpty();
    }

    [TestCase]
    public void AHandStillHoldingSomething_PicksUpNothing()
    {
        GiveMore();
        Process(InventoryItem(9, Potion, Plain));
        Hold(Potion, Plain);
        session.GiveMore.Gave(state.Player.HeldItem);

        Hold(Bread, Plain);

        AssertThat(sent).IsEmpty();
    }

    [TestCase]
    public void TurningGiveMoreOff_ForgetsTheGift()
    {
        GiveMore();
        Process(InventoryItem(9, Potion, Plain));
        Hold(Potion, Plain);
        session.GiveMore.Gave(state.Player.HeldItem);

        session.GiveMore.Enabled = false;
        session.GiveMore.Enabled = true;
        Hold(0, 0);

        AssertThat(sent).IsEmpty();
    }

    [TestCase]
    public void ThePause_LeavesTheOptionsOpen()
    {
        session.OptionsWindow.Toggle();
        session.Pause.Start();

        Process();

        AssertThat(session.OptionsWindow.IsOpen).IsTrue();
    }

    [TestCase]
    public void OpeningAShop_ClosesTheTextWindows()
    {
        session.TextWindows.Show(new WindowRequest(WindowKind.MessageOfTheDay, 0));

        Process(ShopCommands());

        AssertThat(session.TextWindows.AnyOpen).IsFalse();
    }

    [TestCase]
    public void OpeningAShop_ClosesTheWaypointWindow()
    {
        session.WaypointWindow.Show([new WaypointRow(1, "Aston", "", 0, true, false)]);

        Process(ShopCommands());

        AssertThat(state.OpenShop).IsNotNull();
        AssertThat(session.WaypointWindow.IsOpen).IsFalse();
    }

    [TestCase]
    public void ClosingTheShop_StartsThePause()
    {
        Process(ShopCommands());

        AssertThat(session.CloseShop()).IsTrue();

        AssertThat(session.Pause.Active).IsTrue();
    }

    [TestCase]
    public void Escape_ClosesTheShopWithAPauseAndTheTextWindowsWithout()
    {
        Process(ShopCommands());
        session.TextWindows.Show(new WindowRequest(WindowKind.MessageOfTheDay, 0));

        AssertThat(session.CloseWindowsByEscape()).IsTrue();

        AssertThat(state.OpenShop).IsNull();
        AssertThat(session.TextWindows.AnyOpen).IsFalse();
        AssertThat(session.Pause.Active).IsTrue();
    }

    [TestCase]
    public void CloseShop_SaysWhenNoShopWasOpen()
    {
        AssertThat(session.CloseShop()).IsFalse();
        AssertThat(session.Pause.Active).IsFalse();
    }

    [TestCase]
    public void Escape_ClosesTextWindowsAloneWithoutAPause()
    {
        session.TextWindows.Show(new WindowRequest(WindowKind.MessageOfTheDay, 0));

        AssertThat(session.CloseWindowsByEscape()).IsTrue();

        AssertThat(session.TextWindows.AnyOpen).IsFalse();
        AssertThat(session.Pause.Active).IsFalse();
    }

    [TestCase]
    public void Escape_SaysWhenNothingWasOpen()
    {
        AssertThat(session.CloseWindowsByEscape()).IsFalse();
    }

    [TestCase]
    public void Escape_ClosesTheWaypointWindowWithoutAPause()
    {
        session.WaypointWindow.Show([new WaypointRow(1, "Aston", "", 0, true, false)]);

        AssertThat(session.CloseWindowsByEscape()).IsTrue();

        AssertThat(session.WaypointWindow.IsOpen).IsFalse();
        AssertThat(session.Pause.Active).IsFalse();
    }

    [TestCase]
    public void Escape_ClosesTheOptionsWithoutAPause()
    {
        session.OptionsWindow.Toggle();

        AssertThat(session.CloseWindowsByEscape()).IsTrue();

        AssertThat(session.OptionsWindow.IsOpen).IsFalse();
        AssertThat(session.Pause.Active).IsFalse();
    }

    [TestCase]
    public void Escape_ClosesThePlanner()
    {
        session.PlannerWindow.Toggle();

        AssertThat(session.CloseWindowsByEscape()).IsTrue();

        AssertThat(session.PlannerWindow.IsOpen).IsFalse();
    }

    [TestCase]
    public void Escape_ClosesTheWorldMap()
    {
        session.WorldMapWindow.OpenAtPlayer();

        AssertThat(session.CloseWindowsByEscape()).IsTrue();

        AssertThat(session.WorldMapWindow.IsOpen).IsFalse();
    }

    [TestCase]
    public void Escape_ClosesTheSessionStats()
    {
        session.SessionStatsOpen = true;

        AssertThat(session.CloseWindowsByEscape()).IsTrue();

        AssertThat(session.SessionStatsOpen).IsFalse();
    }

    private void GiveMore()
    {
        session.GiveMore.Enabled = true;
        session.GiveMore.Send = sent.Add;
    }

    private void Hold(int sprite, int properties) =>
        Process(
            TestCommand.Of(ServerCommand.SetCharacterHeldItem)
                .UInt16((ushort)sprite)
                .UInt16((ushort)properties));

    private static TestCommand InventoryItem(int slot, int sprite, int properties) =>
        TestCommand.Of(ServerCommand.SetCharacterItem)
            .UInt32((uint)slot)
            .UInt16((ushort)sprite)
            .UInt16((ushort)properties);

    private static TestCommand SetTarget(int action, int x, int y) =>
        TestCommand.Of(ServerCommand.SetTarget)
            .UInt16(0)
            .UInt16(0)
            .UInt16(0)
            .UInt16((ushort)action)
            .UInt16((ushort)x)
            .UInt16((ushort)y);

    private static byte[] Move(int column, int row) =>
        MapCommandPackets.ForTile(ClientCommand.Move, OriginX + column, OriginY + row);

    private static TestCommand[] ShopCommands() =>
    [
        TestCommand.Of(ServerCommand.Look1),
        TestCommand.Of(ServerCommand.Look2),
        TestCommand.Of(ServerCommand.Look3).UInt16(0).UInt16(0).UInt16(0).UInt16(ShopNumber).UInt16(7),
        TestCommand.Of(ServerCommand.Look4).UInt16(0).UInt16(0).Byte((byte)LookFlags.Shop).UInt32(250),
        TestCommand.Of(ServerCommand.Look5).Text("Merchant", 15),
        .. Enumerable.Range(0, CharacterLook.ShopSlots / 2)
            .Select(pair => TestCommand.Of(ServerCommand.Look6)
                .Byte((byte)(pair * 2))
                .UInt16(1000)
                .UInt32(10)
                .UInt16(1001)
                .UInt32(20)
                .Bytes(0, 0)),
    ];

    private void Process(params TestCommand[] commands) => processor.Process(TestCommand.Payload(commands));

    [TestCase]
    public void EachTick_CountsTheTickAndWorksOutTheItemFrames()
    {
        const byte absoluteSetMap = 0x80;
        const byte itemSpriteAndStatus = 0b0001_1000;
        const int itemTile = 10 * WorldMap.Size + 10;
        const int firstLoopStatus = 2;

        Process(
            TestCommand.Of(absoluteSetMap)
                .Byte(itemSpriteAndStatus)
                .UInt16(itemTile)
                .UInt16(Potion)
                .Byte(firstLoopStatus));

        AssertThat(session.MapFrames.TickCount).IsEqual(2);
        AssertThat(session.MapFrames.ItemSprite(itemTile)).IsEqual(Potion + 2);
    }
}
