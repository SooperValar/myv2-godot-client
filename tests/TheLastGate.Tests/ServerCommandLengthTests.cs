using TheLastGate.Net;

namespace TheLastGate.Tests;

public class ServerCommandLengthTests
{
    [Theory]
    [InlineData(ServerCommand.SetCharacterMode, 2)]
    [InlineData(ServerCommand.SetCharacterAttribute, 8)]
    [InlineData(ServerCommand.SetCharacterSkill, 8)]
    [InlineData(ServerCommand.SetCharacterHitPoints, 13)]
    [InlineData(ServerCommand.SetCharacterEndurance, 13)]
    [InlineData(ServerCommand.SetCharacterMana, 13)]
    [InlineData(ServerCommand.SetCharacterActiveHitPoints, 3)]
    [InlineData(ServerCommand.SetCharacterActiveEndurance, 3)]
    [InlineData(ServerCommand.SetCharacterActiveMana, 3)]
    [InlineData(ServerCommand.SetCharacterDirection, 2)]
    [InlineData(ServerCommand.SetCharacterPoints, 13)]
    [InlineData(ServerCommand.SetCharacterWaypoints, 13)]
    [InlineData(ServerCommand.SetCharacterTokens, 9)]
    [InlineData(ServerCommand.SetCharacterTree, 3)]
    [InlineData(ServerCommand.SetCharacterGold, 13)]
    [InlineData(ServerCommand.SetCharacterItem, 11)]
    [InlineData(ServerCommand.SetCharacterWorn, 10)]
    [InlineData(ServerCommand.SetCharacterSpell, 9)]
    [InlineData(ServerCommand.SetCharacterHeldItem, 6)]
    [InlineData(ServerCommand.SetCharacterLocation1, 11)]
    [InlineData(ServerCommand.SetCharacterLocation2, 11)]
    [InlineData(ServerCommand.SetMap3, 16)]
    [InlineData(ServerCommand.SetMap4, 6)]
    [InlineData(ServerCommand.SetMap5, 7)]
    [InlineData(ServerCommand.SetMap6, 9)]
    [InlineData(ServerCommand.SetOrigin, 5)]
    [InlineData(ServerCommand.Tick, 2)]
    [InlineData(ServerCommand.ScrollRight, 1)]
    [InlineData(ServerCommand.ScrollLeft, 1)]
    [InlineData(ServerCommand.ScrollUp, 1)]
    [InlineData(ServerCommand.ScrollDown, 1)]
    [InlineData(ServerCommand.ScrollRightUp, 1)]
    [InlineData(ServerCommand.ScrollRightDown, 1)]
    [InlineData(ServerCommand.ScrollLeftUp, 1)]
    [InlineData(ServerCommand.ScrollLeftDown, 1)]
    [InlineData(ServerCommand.Look7, 8)]
    [InlineData(ServerCommand.Look8, 8)]
    [InlineData(ServerCommand.CloseShop, 1)]
    [InlineData(ServerCommand.SetTarget, 13)]
    [InlineData(ServerCommand.PlaySound, 13)]
    [InlineData(ServerCommand.Load, 5)]
    [InlineData(ServerCommand.Unique, 9)]
    [InlineData(ServerCommand.Waypoints, 1)]
    [InlineData(ServerCommand.ShowMessageOfTheDay, 2)]
    [InlineData(ServerCommand.ClearBox, 9)]
    public void Of_GivesEachFixedLengthCommandItsLength(ServerCommand commandId, int expectedLength)
    {
        Assert.Equal(expectedLength, ServerCommandLength.Of(Command(commandId)));
    }

    [Theory]
    [InlineData(ServerCommand.SetCharacterName1)]
    [InlineData(ServerCommand.SetCharacterName2)]
    [InlineData(ServerCommand.SetCharacterName3)]
    [InlineData(ServerCommand.Log0)]
    [InlineData(ServerCommand.Log9)]
    [InlineData(ServerCommand.MessageOfTheDay0)]
    [InlineData(ServerCommand.MessageOfTheDay3)]
    [InlineData(ServerCommand.Look1)]
    [InlineData(ServerCommand.Look6)]
    [InlineData(ServerCommand.Exit)]
    public void Of_GivesFullPacketCommandsSixteenBytes(ServerCommand commandId)
    {
        Assert.Equal(16, ServerCommandLength.Of(Command(commandId)));
    }

    [Theory]
    [InlineData(0b0000_0001, 2)]
    [InlineData(0b0000_0010, 4)]
    [InlineData(0b0000_0100, 4)]
    [InlineData(0b0000_1000, 2)]
    [InlineData(0b0001_0000, 1)]
    [InlineData(0b0010_0000, 4)]
    [InlineData(0b0100_0000, 6)]
    [InlineData(0b1000_0000, 8)]
    [InlineData(0b1111_1111, 31)]
    public void Of_AddsTheFieldsASetMapFlagsToItsHeader(byte flags, int fieldBytes)
    {
        const byte relativeSetMap = 0x80 | 3;
        const byte absoluteSetMap = 0x80;

        Assert.Equal(2 + fieldBytes, ServerCommandLength.Of(Command(relativeSetMap, flags)));
        Assert.Equal(4 + fieldBytes, ServerCommandLength.Of(Command(absoluteSetMap, flags)));
    }

    [Fact]
    public void Of_RejectsASetMapWithNoFlags()
    {
        Assert.Throws<ProtocolDesyncException>(() => ServerCommandLength.Of(Command(0x80 | 3, 0)));
    }

    [Theory]
    [InlineData(ServerCommand.TermSkillTree, 0, 5)]
    [InlineData(ServerCommand.TermSkillTree, 1, 13)]
    [InlineData(ServerCommand.TermContractTree, 13, 13)]
    [InlineData(ServerCommand.TermSkillTree, 14, 16)]
    [InlineData(ServerCommand.TermSkills, 14, 8)]
    [InlineData(ServerCommand.TermSkills, 15, 13)]
    [InlineData(ServerCommand.TermSkills, 37, 13)]
    [InlineData(ServerCommand.TermSkills, 0, 16)]
    [InlineData(ServerCommand.TermMeta, 38, 13)]
    [InlineData(ServerCommand.TermMeta, 60, 13)]
    [InlineData(ServerCommand.TermMeta, 61, 16)]
    public void Of_ReadsATerminologyCommandsLengthFromItsKind(ServerCommand commandId, byte kind, int expectedLength)
    {
        Assert.Equal(expectedLength, ServerCommandLength.Of(Command((byte)commandId, kind)));
    }

    [Fact]
    public void Of_ReadsAnIgnoreCommandsLengthFromItsSizeField()
    {
        Assert.Equal(300, ServerCommandLength.Of(Command((byte)ServerCommand.Ignore, 0x2C, 0x01, 0, 0)));
    }

    [Fact]
    public void Of_RejectsAnIgnoreCommandShorterThanItsOwnHeader()
    {
        Assert.Throws<ProtocolDesyncException>(() =>
            ServerCommandLength.Of(Command((byte)ServerCommand.Ignore, 4, 0, 0, 0)));
    }

    [Theory]
    [InlineData(ServerCommand.Empty)]
    [InlineData(ServerCommand.Challenge)]
    [InlineData(ServerCommand.NewPlayer)]
    [InlineData(ServerCommand.LoginOk)]
    [InlineData(ServerCommand.SetMap2)]
    [InlineData(ServerCommand.Message)]
    [InlineData(ServerCommand.Cap)]
    [InlineData(ServerCommand.Mod1)]
    [InlineData((ServerCommand)26)]
    public void Of_RejectsCommandsTheClientDoesNotHandleInATick(ServerCommand commandId)
    {
        Assert.Throws<ProtocolDesyncException>(() => ServerCommandLength.Of(Command(commandId)));
    }

    [Fact]
    public void Of_RejectsAVariableLengthCommandCutOffBeforeItsLengthField()
    {
        byte[] command = [(byte)ServerCommand.Ignore, 10, 0];

        Assert.Throws<ProtocolDesyncException>(() => ServerCommandLength.Of(command));
    }

    private static byte[] Command(ServerCommand commandId) => Command((byte)commandId);

    private static byte[] Command(byte commandId, params byte[] following)
    {
        var command = new byte[64];
        command[0] = commandId;
        following.CopyTo(command, 1);
        return command;
    }
}
