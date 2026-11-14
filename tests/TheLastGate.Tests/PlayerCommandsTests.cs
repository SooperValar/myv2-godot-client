using TheLastGate.Net;
using TheLastGate.State;

namespace TheLastGate.Tests;

public class PlayerCommandsTests
{
    [Fact]
    public void Process_SkipsCommandsWithNoHandlerYet()
    {
        var state = Process(
            TestCommand.Of(ServerCommand.CloseShop),
            TestCommand.Of(ServerCommand.SetCharacterMode).Byte(2));

        Assert.Equal(2, state.Player.Mode);
    }

    [Fact]
    public void SetCharacterName_JoinsTheThreePartsAndTakesTheRaceFromTheLast()
    {
        const string name = "Alexandria, Keeper of the Western Shores";

        var state = Process(
            TestCommand.Of(ServerCommand.SetCharacterName1).Text(name[..15], 15),
            TestCommand.Of(ServerCommand.SetCharacterName2).Text(name[15..30], 15),
            TestCommand.Of(ServerCommand.SetCharacterName3).Text(name[30..], 10).UInt32(110009));

        Assert.Equal(name, state.Player.Name);
        Assert.Equal(110009, state.Player.Race);
    }

    [Fact]
    public void SetCharacterName_ReportsTheNameOnceTheLastPartArrives()
    {
        var state = new GameState();
        var reports = 0;
        state.Player.NameReceived += () => reports++;
        var processor = new TickProcessor(state);

        processor.Process(TestCommand.Payload(TestCommand.Of(ServerCommand.SetCharacterName1).Text("Ishtar", 15)));
        Assert.Equal(0, reports);

        processor.Process(TestCommand.Payload(TestCommand.Of(ServerCommand.SetCharacterName3).Text("", 10).UInt32(76)));
        Assert.Equal(1, reports);
    }

    [Fact]
    public void SetCharacterHitPoints_ReadsSixUnsignedValues()
    {
        var state = Process(Powers(ServerCommand.SetCharacterHitPoints));

        Assert.Equal(new[] { 1, 2, 3, 4, 5, 65535 }, state.Player.HitPoints);
    }

    [Theory]
    [InlineData(ServerCommand.SetCharacterEndurance)]
    [InlineData(ServerCommand.SetCharacterMana)]
    public void SetCharacterEnduranceAndMana_ReadSixSignedValues(ServerCommand commandId)
    {
        var player = Process(Powers(commandId)).Player;

        var values = commandId == ServerCommand.SetCharacterEndurance ? player.Endurance : player.Mana;
        Assert.Equal(new[] { 1, 2, 3, 4, 5, -1 }, values);
    }

    [Fact]
    public void SetCharacterAttribute_StoresTheSixValuesOfOneAttribute()
    {
        var state = Process(TestCommand.Of(ServerCommand.SetCharacterAttribute).Bytes(4, 10, 20, 30, 40, 1, 44));

        Assert.Equal(new[] { 10, 20, 30, 40, 1, 44 }, state.Player.Attributes[4]);
    }

    [Fact]
    public void SetCharacterAttribute_IgnoresAnAttributePastTheFifth()
    {
        var state = Process(TestCommand.Of(ServerCommand.SetCharacterAttribute).Bytes(5, 10, 20, 30, 40, 1, 44));

        Assert.All(state.Player.Attributes, values => Assert.All(values, value => Assert.Equal(0, value)));
    }

    [Fact]
    public void SetCharacterSkill_StoresSkillsUpToFortyNine()
    {
        var state = Process(
            TestCommand.Of(ServerCommand.SetCharacterSkill).Bytes(49, 1, 2, 3, 4, 5, 6),
            TestCommand.Of(ServerCommand.SetCharacterSkill).Bytes(50, 1, 2, 3, 4, 5, 6));

        Assert.Equal(new[] { 1, 2, 3, 4, 5, 6 }, state.Player.Skills[49]);
        Assert.Equal(new int[6], state.Player.Skills[50]);
    }

    [Fact]
    public void SetCharacterActiveValues_ReadUnsignedCurrentValues()
    {
        var state = Process(
            TestCommand.Of(ServerCommand.SetCharacterActiveHitPoints).UInt16(65535),
            TestCommand.Of(ServerCommand.SetCharacterActiveEndurance).UInt16(200),
            TestCommand.Of(ServerCommand.SetCharacterActiveMana).UInt16(300));

        Assert.Equal(65535, state.Player.ActiveHitPoints);
        Assert.Equal(200, state.Player.ActiveEndurance);
        Assert.Equal(300, state.Player.ActiveMana);
    }

    [Fact]
    public void SetCharacterModeAndDirection_ReadOneByteEach()
    {
        var state = Process(
            TestCommand.Of(ServerCommand.SetCharacterMode).Byte(1),
            TestCommand.Of(ServerCommand.SetCharacterDirection).Byte(7));

        Assert.Equal(1, state.Player.Mode);
        Assert.Equal(7, state.Player.Direction);
    }

    [Fact]
    public void SetCharacterPoints_ReadsPointsTotalAndKindred()
    {
        var state = Process(
            TestCommand.Of(ServerCommand.SetCharacterPoints).UInt32(1500).UInt32(90000).UInt32(0x80000001));

        Assert.Equal(1500, state.Player.Points);
        Assert.Equal(90000, state.Player.PointsTotal);
        Assert.Equal(0x80000001u, state.Player.Kindred);
    }

    [Fact]
    public void SetCharacterWaypoints_ReadsWaypointsAndPointBalances()
    {
        var state = Process(TestCommand.Of(ServerCommand.SetCharacterWaypoints).UInt32(0x3FF).UInt32(12).UInt32(34));

        Assert.Equal(0x3FF, state.Player.Waypoints);
        Assert.Equal(12, state.Player.BlackStrongholdPoints);
        Assert.Equal(34, state.Player.OsirisPoints);
    }

    [Fact]
    public void SetCharacterTokens_ReadsTokensAndBothTrees()
    {
        var state = Process(TestCommand.Of(ServerCommand.SetCharacterTokens).UInt32(77).UInt16(0x1234).UInt16(0x5678));

        Assert.Equal(77, state.Player.Tokens);
        Assert.Equal(0x1234, state.Player.TreePoints);
        Assert.Equal(0x5678, state.Player.OsirisTree);
    }

    [Fact]
    public void SetCharacterTree_SetsOneNodeAndIgnoresNodesPastTheTwelfth()
    {
        var state = Process(
            TestCommand.Of(ServerCommand.SetCharacterTree).Bytes(11, 3),
            TestCommand.Of(ServerCommand.SetCharacterTree).Bytes(12, 9));

        Assert.Equal(3, state.Player.TreeNodes[11]);
        Assert.Equal(PlayerState.TreeNodeCount, state.Player.TreeNodes.Length);
    }

    [Fact]
    public void SetCharacterGold_ReadsGoldArmorAndWeapon()
    {
        var state = Process(TestCommand.Of(ServerCommand.SetCharacterGold).UInt32(123456).UInt32(80).UInt32(95));

        Assert.Equal(123456, state.Player.Gold);
        Assert.Equal(80, state.Player.Armor);
        Assert.Equal(95, state.Player.Weapon);
    }

    [Fact]
    public void SetCharacterLocation_JoinsTheTwoParts()
    {
        var state = Process(
            TestCommand.Of(ServerCommand.SetCharacterLocation1).Text("Aston Temp", 10),
            TestCommand.Of(ServerCommand.SetCharacterLocation2).Text("le", 10));

        Assert.Equal("Aston Temple", state.Player.Location);
    }

    [Fact]
    public void SetTarget_SetsWhatThePlayerIsDoing()
    {
        var state = Process(
            TestCommand.Of(ServerCommand.SetTarget)
                .UInt16(812)
                .UInt16(130)
                .UInt16(245)
                .UInt16(4)
                .UInt16(131)
                .UInt16(246));

        Assert.Equal(new PlayerTarget(812, 130, 245, 4, 131, 246), state.Player.Target);
    }

    private static TestCommand Powers(ServerCommand commandId) =>
        TestCommand.Of(commandId).UInt16(1).UInt16(2).UInt16(3).UInt16(4).UInt16(5).UInt16(65535);

    private static GameState Process(params TestCommand[] commands)
    {
        var state = new GameState();
        new TickProcessor(state).Process(TestCommand.Payload(commands));
        return state;
    }
}
