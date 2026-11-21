using GdUnit4;
using TheLastGate.Game;
using TheLastGate.Net;
using TheLastGate.State;
using TheLastGate.Windows;
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

    private TickProcessor processor = null!;
    private List<byte[]> sent = [];
    private GameState state = new();

    [BeforeTest]
    public void StartASession()
    {
        state = new GameState();
        _ = new GameSession(state);
        processor = new TickProcessor(state);
        sent = [];
        state.AutoWalk.Send = sent.Add;
        Process(TestCommand.Of(ServerCommand.SetOrigin).UInt16(OriginX).UInt16(OriginY));
        sent.Clear();
    }

    [TestCase(WalkDirection.North, 20, 27)]
    [TestCase(WalkDirection.West, 27, 20)]
    [TestCase(WalkDirection.South, 34, 27)]
    [TestCase(WalkDirection.East, 27, 34)]
    public void Walking_MovesToTheTileSevenAwayOnTheNextTick(WalkDirection direction, int column, int row)
    {
        state.AutoWalk.Start(direction);

        Process();

        AssertThat(sent).HasSize(1);
        AssertThat(sent[0]).ContainsExactly(Move(column, row));
    }

    [TestCase]
    public void Walking_StepsAgainEveryFourTicks()
    {
        state.AutoWalk.Start(WalkDirection.North);

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
        state.AutoWalk.Start(WalkDirection.North);

        for (var tick = 0; tick < (stepsBeforeStopping + 10) * StepTicks; tick++)
        {
            Process();
        }

        AssertThat(sent).HasSize(stepsBeforeStopping);
        AssertThat(state.AutoWalk.Walking).IsFalse();
    }

    [TestCase]
    public void Stopping_SendsNoMoreSteps()
    {
        state.AutoWalk.Start(WalkDirection.North);
        Process();

        state.AutoWalk.Stop();
        for (var tick = 0; tick < StepTicks * 2; tick++)
        {
            Process();
        }

        AssertThat(sent).HasSize(1);
        AssertThat(state.AutoWalk.Walking).IsFalse();
    }

    [TestCase(23, false)]
    [TestCase(24, true)]
    public void ThePause_KeepsShopsAndTextWindowsShutUntilItEnds(int ticksAfterClosing, bool staysOpen)
    {
        Process(ShopCommands());
        state.CloseShop();
        for (var tick = 0; tick < ticksAfterClosing; tick++)
        {
            Process();
        }

        Process([.. ShopCommands(), TestCommand.Of(ServerCommand.ShowMessageOfTheDay).Byte(0)]);

        AssertThat(state.OpenShop != null).IsEqual(staysOpen);
        AssertThat(state.TextWindows.MessageOfTheDayOpen).IsEqual(staysOpen);
    }

    [TestCase]
    public void ThePause_ClosesTheTree()
    {
        Process(
            TestCommand.Of(ServerCommand.SetCharacterTokens)
                .UInt32(0)
                .UInt16(TwoTreePointsOneSpent)
                .UInt16(NoTreePoints));
        state.ToggleSkillTree(MapKeys.None);
        state.Pause.Start();

        Process();

        AssertThat(state.SkillTreeWindow.Open).IsNull();
    }

    [TestCase]
    public void ThePause_KeepsTheWaypointWindowShut()
    {
        state.Pause.Start();

        Process(TestCommand.Of(ServerCommand.Waypoints));

        AssertThat(state.WaypointWindow.IsOpen).IsFalse();
    }

    [TestCase]
    public void EachTick_CountsDownThePauseAfterClosingAWindow()
    {
        Process(TestCommand.Of(ServerCommand.ShowMessageOfTheDay).Byte(0));
        state.TextWindows.Close();

        Process(TestCommand.Of(ServerCommand.ShowMessageOfTheDay).Byte(0));

        AssertThat(state.TextWindows.MessageOfTheDayOpen).IsFalse();
    }

    [TestCase]
    public void TheServersSetTarget_ReachesTheUseQueue()
    {
        state.UseQueue.Enabled = true;
        state.UseQueue.Send = sent.Add;
        state.UseQueue.Add(10, 20, new PlayerTarget(0, 0, 0, Using, 1, 1));
        state.UseQueue.Add(11, 20, new PlayerTarget(0, 0, 0, Using, 1, 1));

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
        state.GiveMore.Gave(state.Player.HeldItem);

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
        state.GiveMore.Gave(state.Player.HeldItem);
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
        state.GiveMore.Gave(state.Player.HeldItem);

        Hold(0, 0);

        AssertThat(sent).IsEmpty();
    }

    [TestCase]
    public void AHandStillHoldingSomething_PicksUpNothing()
    {
        GiveMore();
        Process(InventoryItem(9, Potion, Plain));
        Hold(Potion, Plain);
        state.GiveMore.Gave(state.Player.HeldItem);

        Hold(Bread, Plain);

        AssertThat(sent).IsEmpty();
    }

    [TestCase]
    public void TurningGiveMoreOff_ForgetsTheGift()
    {
        GiveMore();
        Process(InventoryItem(9, Potion, Plain));
        Hold(Potion, Plain);
        state.GiveMore.Gave(state.Player.HeldItem);

        state.GiveMore.Enabled = false;
        state.GiveMore.Enabled = true;
        Hold(0, 0);

        AssertThat(sent).IsEmpty();
    }

    [TestCase]
    public void ThePause_LeavesTheOptionsOpen()
    {
        state.OptionsWindow.Toggle();
        state.Pause.Start();

        Process();

        AssertThat(state.OptionsWindow.IsOpen).IsTrue();
    }

    [TestCase]
    public void OpeningAShop_ClosesTheTextWindows()
    {
        state.TextWindows.Show(new WindowRequest(WindowKind.MessageOfTheDay, 0));

        Process(ShopCommands());

        AssertThat(state.TextWindows.AnyOpen).IsFalse();
    }

    [TestCase]
    public void OpeningAShop_ClosesTheTree()
    {
        Process(
            TestCommand.Of(ServerCommand.SetCharacterTokens)
                .UInt32(0)
                .UInt16(TwoTreePointsOneSpent)
                .UInt16(NoTreePoints));
        state.ToggleSkillTree(MapKeys.None);

        Process(ShopCommands());

        AssertThat(state.SkillTreeWindow.Open).IsNull();
    }

    [TestCase]
    public void OpeningAShop_ClosesTheWaypointWindow()
    {
        state.WaypointWindow.Open();

        Process(ShopCommands());

        AssertThat(state.OpenShop).IsNotNull();
        AssertThat(state.WaypointWindow.IsOpen).IsFalse();
    }

    [TestCase]
    public void ClosingTheShop_StartsThePause()
    {
        Process(ShopCommands());

        AssertThat(state.CloseShop()).IsTrue();

        AssertThat(state.Pause.Active).IsTrue();
    }

    [TestCase]
    public void TheServerClosingTheShop_StartsThePause()
    {
        Process(ShopCommands());

        Process(TestCommand.Of(ServerCommand.CloseShop));

        AssertThat(state.OpenShop).IsNull();
        AssertThat(state.Pause.Active).IsTrue();
    }

    [TestCase]
    public void Escape_ClosesTheShopWithAPauseAndTheTextWindowsWithout()
    {
        Process(ShopCommands());
        state.TextWindows.Show(new WindowRequest(WindowKind.MessageOfTheDay, 0));

        AssertThat(state.CloseWindowsByEscape()).IsTrue();

        AssertThat(state.OpenShop).IsNull();
        AssertThat(state.TextWindows.AnyOpen).IsFalse();
        AssertThat(state.Pause.Active).IsTrue();
    }

    [TestCase]
    public void ToggleSkillTree_ShutsOtherWindowsWithoutAPause()
    {
        Process(
            TestCommand.Of(ServerCommand.SetCharacterTokens)
                .UInt32(0)
                .UInt16(TwoTreePointsOneSpent)
                .UInt16(NoTreePoints));
        Process(ShopCommands());
        state.WaypointWindow.Open();
        state.TextWindows.Show(new WindowRequest(WindowKind.MessageOfTheDay, 0));

        state.ToggleSkillTree(MapKeys.None);

        AssertThat(state.SkillTreeWindow.Open).IsEqual(SkillTree.Class);
        AssertThat(state.OpenShop).IsNull();
        AssertThat(state.WaypointWindow.IsOpen).IsFalse();
        AssertThat(state.TextWindows.AnyOpen).IsFalse();
        AssertThat(state.Pause.Active).IsFalse();
    }

    [TestCase]
    public void TheServer_OpensTheWaypointWindow()
    {
        Process(TestCommand.Of(ServerCommand.Waypoints));

        AssertThat(state.WaypointWindow.IsOpen).IsTrue();
    }

    [TestCase(0, true, false, null)]
    [TestCase(99, false, true, null)]
    [TestCase(104, false, false, 4)]
    public void TheShowCommand_OpensTheTextWindowItNames(int window, bool messageOfTheDay, bool welcome, int? tutorial)
    {
        Process(TestCommand.Of(ServerCommand.ShowMessageOfTheDay).Byte((byte)window));

        AssertThat(state.TextWindows.MessageOfTheDayOpen).IsEqual(messageOfTheDay);
        AssertThat(state.TextWindows.WelcomeOpen).IsEqual(welcome);
        AssertThat(state.TextWindows.Tutorial ?? NoTutorial).IsEqual(tutorial ?? NoTutorial);
    }

    private void GiveMore()
    {
        state.GiveMore.Enabled = true;
        state.GiveMore.Send = sent.Add;
    }

    private void Hold(int sprite, int properties) =>
        Process(
            TestCommand.Of(ServerCommand.SetCharacterHeldItem)
                .UInt16((ushort)sprite)
                .UInt16((ushort)properties)
                .Byte(1));

    private static TestCommand InventoryItem(int slot, int sprite, int properties) =>
        TestCommand.Of(ServerCommand.SetCharacterItem)
            .UInt32((uint)slot)
            .UInt16((ushort)sprite)
            .UInt16((ushort)properties)
            .Bytes(1, 0);

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
}
