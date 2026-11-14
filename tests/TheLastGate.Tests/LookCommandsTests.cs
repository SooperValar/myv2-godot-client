using TheLastGate.Net;
using TheLastGate.State;

namespace TheLastGate.Tests;

public class LookCommandsTests
{
    private const int Number = 5825;
    private const int Id = 41;

    [Fact]
    public void Look_ShowsTheCharacterOnceTheNameArrives()
    {
        var state = new GameState();
        var shown = new List<CharacterLook>();
        state.LookShown += shown.Add;

        Process(state, LookCommands("Worldofslaying", flags: LookFlags.Shadow | LookFlags.Bloody));

        var look = Assert.Single(shown);
        Assert.Same(state.Look, look);
        Assert.Equal("Worldofslaying", look.Name);
        Assert.Equal(Number, look.Number);
        Assert.Equal(Id, look.Id);
        Assert.Equal(3021, look.Sprite);
        Assert.Equal(1_500_000, look.Points);
        Assert.Equal(70_000, look.HitPoints);
        Assert.Equal(400, look.Endurance);
        Assert.Equal(650, look.Mana);
        Assert.Equal(320, look.ActiveHitPoints);
        Assert.Equal(390, look.ActiveEndurance);
        Assert.Equal(640, look.ActiveMana);
        Assert.Equal(Enumerable.Range(0, 14).Select(slot => 100 + slot), look.Worn.Take(14));
        Assert.False(look.IsAutoLook);
        Assert.Equal(LookFlags.Shadow | LookFlags.Bloody, look.Flags);
        Assert.Equal(12345, look.HeldItemPrice);
    }

    [Fact]
    public void Look_ShowsNothingUntilTheNameArrives()
    {
        var state = new GameState();
        var shown = 0;
        state.LookShown += _ => shown++;

        Process(state, LookCommands("Worldofslaying")[..4]);

        Assert.Null(state.Look);
        Assert.Equal(0, shown);
    }

    [Fact]
    public void Look_KeepsTheShownLookWhenTheNextLookArrives()
    {
        var state = new GameState();
        Process(state, LookCommands("Worldofslaying"));

        Process(state, TestCommand.Of(ServerCommand.Look1).UInt16(999));

        Assert.Equal(100, state.Look!.Worn[0]);
    }

    [Fact]
    public void Look_RemembersTheNameUnderTheCharactersNumberAndId()
    {
        var state = new GameState();

        Process(state, LookCommands("Worldofslaying"));

        Assert.Equal("Worldofslaying", state.Names.NameOf(Number, Id));
        Assert.Equal("Worldofslaying", state.Names.NameOf(Number, 0));
        Assert.Equal("", state.Names.NameOf(Number, Id + 1));
        Assert.Equal("", state.Names.NameOf(Number + 1, Id));
    }

    [Fact]
    public void Look_ForgetsTheOldNameWhenTheNumberGoesToANewCharacter()
    {
        var state = new GameState();

        Process(state, LookCommands("Worldofslaying"));
        Process(state, LookCommands("Grolm", Id + 1));

        Assert.Equal("", state.Names.NameOf(Number, Id));
        Assert.Equal("Grolm", state.Names.NameOf(Number, Id + 1));
    }

    [Fact]
    public void AutoLook_RemembersTheNameWithoutShowingTheLook()
    {
        var state = new GameState();

        Process(state, LookCommands("Worldofslaying", autoLook: true));

        Assert.Null(state.Look);
        Assert.Equal("Worldofslaying", state.Names.NameOf(Number, Id));
    }

    [Fact]
    public void ShopLook_IsNeitherShownNorRemembered()
    {
        var state = new GameState();

        Process(state, LookCommands("Merchant", flags: LookFlags.Shop));

        Assert.Null(state.Look);
        Assert.Equal("", state.Names.NameOf(Number, Id));
    }

    private static TestCommand[] LookCommands(
        string name,
        int id = Id,
        bool autoLook = false,
        LookFlags flags = LookFlags.None) =>
    [
        TestCommand.Of(ServerCommand.Look1)
            .UInt16(100)
            .UInt16(102)
            .UInt16(103)
            .UInt16(105)
            .UInt16(106)
            .UInt16(107)
            .UInt16(108)
            .Byte(autoLook ? (byte)1 : (byte)0),
        TestCommand.Of(ServerCommand.Look2).UInt16(109).UInt16(3021).UInt32(1_500_000).UInt32(70_000).UInt16(110),
        TestCommand.Of(ServerCommand.Look3)
            .UInt16(400)
            .UInt16(320)
            .UInt16(390)
            .UInt16(Number)
            .UInt16((ushort)id)
            .UInt16(650)
            .UInt16(640),
        TestCommand.Of(ServerCommand.Look4)
            .UInt16(101)
            .UInt16(104)
            .Byte((byte)flags)
            .UInt32(12345)
            .UInt16(111)
            .UInt16(112)
            .UInt16(113),
        TestCommand.Of(ServerCommand.Look5).Text(name, 15),
    ];

    private static void Process(GameState state, params TestCommand[] commands) =>
        new TickProcessor(state).Process(TestCommand.Payload(commands));
}
