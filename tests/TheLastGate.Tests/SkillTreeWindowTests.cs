using TheLastGate.Net;
using TheLastGate.State;
using TheLastGate.Windows;
using TheLastGate.World;

namespace TheLastGate.Tests;

public class SkillTreeWindowTests
{
    private const int FirstNodeLearned = 1 << 15;
    private const int LastNodeLearned = 1 << 4;
    private const int OneUnspent = 1;
    private const int TwoPointsOneSpent = FirstNodeLearned | OneUnspent;
    private const int AllSpent = FirstNodeLearned;
    private const int NoPoints = 0;

    private readonly GameState state = new();
    private readonly SkillTreeWindow window = new();

    [Theory]
    [InlineData(FirstNodeLearned, 0, true)]
    [InlineData(FirstNodeLearned, 1, false)]
    [InlineData(LastNodeLearned, 11, true)]
    [InlineData(LastNodeLearned, 10, false)]
    [InlineData(0b1111, 11, false)]
    public void IsLearned_ReadsTheNodesBitFromTheTop(int treeValue, int node, bool learned)
    {
        Assert.Equal(learned, SkillTrees.IsLearned(treeValue, node));
    }

    [Theory]
    [InlineData(TwoPointsOneSpent, 1, 2)]
    [InlineData(AllSpent, 0, 1)]
    [InlineData(NoPoints, 0, 0)]
    [InlineData(0xFFFF, 15, 27)]
    public void UnspentAndTotal_CountTheLowBitsAndTheLearnedNodes(int treeValue, int unspent, int total)
    {
        Assert.Equal(unspent, SkillTrees.Unspent(treeValue));
        Assert.Equal(total, SkillTrees.Total(treeValue));
    }

    [Fact]
    public void Terminology_StartsWithPlaceholdersUntilTheServerSendsTheTrees()
    {
        var terminology = new Terminology();

        Assert.Equal(6601, terminology.Trees[0][0].Icon);
        Assert.Equal("p01", terminology.Trees[0][0].Name);
        Assert.Equal(6720, terminology.Trees[1][11].Icon);
        Assert.Equal("c12", terminology.Trees[1][11].Name);
        Assert.Equal("", terminology.Trees[1][11].Description1);
    }

    [Fact]
    public void Toggle_OpensTheClassTreeAndClosesItAgain()
    {
        window.Toggle(MapKeys.None, TwoPointsOneSpent, NoPoints);
        Assert.Equal(SkillTree.Class, window.Open);

        window.Toggle(MapKeys.None, TwoPointsOneSpent, NoPoints);
        Assert.Null(window.Open);
    }

    [Theory]
    [InlineData(MapKeys.Shift)]
    [InlineData(MapKeys.Ctrl)]
    [InlineData(MapKeys.Alt)]
    public void Toggle_OpensTheContractTreeWithAModifierHeld(MapKeys keys)
    {
        window.Toggle(keys, TwoPointsOneSpent, AllSpent);

        Assert.Equal(SkillTree.Contract, window.Open);
    }

    [Fact]
    public void Toggle_SwitchesBetweenTheTrees()
    {
        window.Toggle(MapKeys.Shift, TwoPointsOneSpent, AllSpent);

        window.Toggle(MapKeys.None, TwoPointsOneSpent, AllSpent);

        Assert.Equal(SkillTree.Class, window.Open);
    }

    [Fact]
    public void Toggle_ClosesInsteadOfOpeningATreeWithoutPoints()
    {
        window.Toggle(MapKeys.None, TwoPointsOneSpent, NoPoints);

        window.Toggle(MapKeys.Shift, TwoPointsOneSpent, NoPoints);

        Assert.Null(window.Open);
    }

    [Fact]
    public void Close_SaysWhetherTheTreeWasOpen()
    {
        window.Toggle(MapKeys.None, TwoPointsOneSpent, NoPoints);

        Assert.True(window.Close());
        Assert.Null(window.Open);
        Assert.False(window.Close());
    }

    [Theory]
    [InlineData(MapButton.Left, MapKeys.None, 5)]
    [InlineData(MapButton.Left, MapKeys.Shift, 29)]
    [InlineData(MapButton.Left, MapKeys.Ctrl, 29)]
    [InlineData(MapButton.Right, MapKeys.None, 17)]
    [InlineData(MapButton.Right, MapKeys.Shift, 17)]
    public void Click_LearnsUnlearnsOrDescribesAClassNode(MapButton button, MapKeys keys, int value)
    {
        window.Toggle(MapKeys.None, TwoPointsOneSpent, NoPoints);

        Assert.Equal(ClientPackets.OneValue(ClientCommand.Tree, value), window.Click(5, button, keys));
    }

    [Theory]
    [InlineData(MapButton.Left, MapKeys.None, 41)]
    [InlineData(MapButton.Left, MapKeys.Alt, 65)]
    [InlineData(MapButton.Right, MapKeys.None, 53)]
    public void Click_LearnsUnlearnsOrDescribesAContractNode(MapButton button, MapKeys keys, int value)
    {
        window.Toggle(MapKeys.Shift, TwoPointsOneSpent, AllSpent);

        Assert.Equal(ClientPackets.OneValue(ClientCommand.Tree, value), window.Click(5, button, keys));
    }

    [Fact]
    public void Click_SendsNothingForOtherButtonsOrAClosedTree()
    {
        Assert.Null(window.Click(5, MapButton.Left, MapKeys.None));

        window.Toggle(MapKeys.None, TwoPointsOneSpent, NoPoints);

        Assert.Null(window.Click(5, MapButton.None, MapKeys.None));
    }

    [Theory]
    [InlineData(NoPoints, false)]
    [InlineData(AllSpent, true)]
    [InlineData(OneUnspent, true)]
    public void ButtonShows_OnlyWithClassTreePoints(int classPoints, bool shows)
    {
        Assert.Equal(shows, SkillTreeWindow.ButtonShows(classPoints));
    }

    [Fact]
    public void ButtonGlows_WhileATreeThatIsNotOpenHasPointsToSpend()
    {
        Assert.True(window.ButtonGlows(TwoPointsOneSpent, AllSpent));
        Assert.True(window.ButtonGlows(AllSpent, OneUnspent));
        Assert.False(window.ButtonGlows(AllSpent, AllSpent));

        window.Toggle(MapKeys.None, TwoPointsOneSpent, AllSpent);
        Assert.False(window.ButtonGlows(TwoPointsOneSpent, AllSpent));
        Assert.True(window.ButtonGlows(TwoPointsOneSpent, OneUnspent));
    }

    [Theory]
    [InlineData(0, 15)]
    [InlineData(4, 8)]
    [InlineData(8, 0)]
    [InlineData(13, 10)]
    [InlineData(16, 15)]
    public void GlowLight_SweepsEverySixteenMilliseconds(long milliseconds, int light)
    {
        Assert.Equal(light, SkillTreeWindow.GlowLight(milliseconds));
    }

    [Fact]
    public void Counter_ShowsUnspentOverTotal()
    {
        Assert.Equal("1/2", SkillTreeWindow.Counter(TwoPointsOneSpent));
    }

    [Fact]
    public void ShownText_PrintsTheServersDoubledPercentOnce()
    {
        Assert.Equal("2% more damage dealt", SkillTreeWindow.ShownText("2%% more damage dealt"));
    }

    [Fact]
    public void ToggleSkillTree_ShutsOtherWindowsWithoutAPause()
    {
        SetTreePoints(TwoPointsOneSpent, NoPoints);
        state.WaypointWindow.Open();
        state.TextWindows.Show(new WindowRequest(WindowKind.MessageOfTheDay, 0));
        OpenShop();

        state.ToggleSkillTree(MapKeys.None);

        Assert.Equal(SkillTree.Class, state.SkillTreeWindow.Open);
        Assert.Null(state.OpenShop);
        Assert.False(state.WaypointWindow.IsOpen);
        Assert.False(state.TextWindows.AnyOpen);
        Assert.False(state.Pause.Active);
    }

    [Fact]
    public void ToggleSkillTree_UsesTheContractTreeWithAModifier()
    {
        SetTreePoints(TwoPointsOneSpent, AllSpent);

        state.ToggleSkillTree(MapKeys.Ctrl);

        Assert.Equal(SkillTree.Contract, state.SkillTreeWindow.Open);
    }

    [Fact]
    public void Escape_ClosesTheTreeWithoutAPause()
    {
        SetTreePoints(TwoPointsOneSpent, NoPoints);
        state.ToggleSkillTree(MapKeys.None);

        Assert.True(state.CloseWindowsByEscape());

        Assert.Null(state.SkillTreeWindow.Open);
        Assert.False(state.Pause.Active);
    }

    [Fact]
    public void OpeningAShop_ClosesTheTree()
    {
        SetTreePoints(TwoPointsOneSpent, NoPoints);
        state.ToggleSkillTree(MapKeys.None);

        OpenShop();

        Assert.Null(state.SkillTreeWindow.Open);
    }

    [Fact]
    public void ThePause_ClosesTheTree()
    {
        SetTreePoints(TwoPointsOneSpent, NoPoints);
        state.ToggleSkillTree(MapKeys.None);
        state.Pause.Start();

        Process();

        Assert.Null(state.SkillTreeWindow.Open);
    }

    private void SetTreePoints(int classPoints, int contractPoints) =>
        Process(
            TestCommand.Of(ServerCommand.SetCharacterTokens)
                .UInt32(0)
                .UInt16((ushort)classPoints)
                .UInt16((ushort)contractPoints));

    private void OpenShop() => Process(ShopCommands());

    private static TestCommand[] ShopCommands() =>
    [
        TestCommand.Of(ServerCommand.Look1),
        TestCommand.Of(ServerCommand.Look2),
        TestCommand.Of(ServerCommand.Look3).UInt16(0).UInt16(0).UInt16(0).UInt16(1201).UInt16(7),
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

    private void Process(params TestCommand[] commands) =>
        new TickProcessor(state).Process(TestCommand.Payload(commands));
}
