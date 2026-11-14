using System.Buffers.Binary;
using System.Text;
using TheLastGate.Characters;
using TheLastGate.Net;

namespace TheLastGate.Tests;

public class LoginHandshakeTests
{
    private const int NewCharacterRace = 3;
    private const uint UniqueId1 = 0x11111111;
    private const uint UniqueId2 = 0x22222222;
    private static readonly LoginKey Key = new(1001, 0xAAAA0001, 0xBBBB0002, "Ishtar", 76);

    [Fact]
    public void CreateOpeningPackets_LogsInWithTheCharactersKey()
    {
        var packets = Handshake(Key).CreateOpeningPackets();

        Assert.Equal([Packet(ClientCommand.Login, 1001, 0xAAAA0001, 0xBBBB0002)], packets);
    }

    [Fact]
    public void CreateOpeningPackets_AsksForANewCharacterWithoutAKey()
    {
        Assert.Equal([Packet(ClientCommand.NewLogin)], Handshake(null).CreateOpeningPackets());
    }

    [Fact]
    public void CreateOpeningPackets_TreatsAKeyNumberedZeroAsANewCharacter()
    {
        var unsavedKey = Key with { UserNumber = 0 };

        Assert.Equal([Packet(ClientCommand.NewLogin)], Handshake(unsavedKey).CreateOpeningPackets());
    }

    [Fact]
    public void CreateOpeningPackets_SendsThePasswordFirst()
    {
        var packets = Handshake(Key, "open sesame").CreateOpeningPackets();

        Assert.Equal(2, packets.Count);
        Assert.Equal(PasswordPacket("open sesame"), packets[0]);
        Assert.Equal((byte)ClientCommand.Login, packets[1][0]);
    }

    [Fact]
    public void CreateOpeningPackets_KeepsTheFirstFourteenCharactersOfALongPassword()
    {
        var packets = Handshake(Key, "abcdefghijklmnopqrstuvwxyz").CreateOpeningPackets();

        Assert.Equal(PasswordPacket("abcdefghijklmn"), packets[0]);
    }

    [Fact]
    public void Handle_AnswersTheChallengeWithTheVersionRaceAndUniqueId()
    {
        var handshake = Handshake(Key);

        var replies = handshake.Handle(Packet(ServerCommand.Challenge, 0x12345678));

        Assert.Equal(
            [
                Packet(ClientCommand.Challenge, ChallengeCipher.Encrypt(0x12345678), Protocol.NetworkingVersion, 76),
                Packet(ClientCommand.Unique, UniqueId1, UniqueId2),
            ],
            replies);
        Assert.Equal(LoginStage.WaitingForResult, handshake.Stage);
    }

    [Fact]
    public void Handle_AnswersTheChallengeWithTheNewCharactersRace()
    {
        var replies = Handshake(null).Handle(Packet(ServerCommand.Challenge, 7));

        Assert.Equal((uint)NewCharacterRace, BinaryPrimitives.ReadUInt32LittleEndian(replies[0].AsSpan(9)));
    }

    [Fact]
    public void Handle_LogsInWhenTheServerAcceptsTheKey()
    {
        var handshake = AnsweredHandshake(Key);

        var replies = handshake.Handle(Packet(ServerCommand.LoginOk, 0x000F02));

        Assert.Empty(replies);
        Assert.Equal(LoginStage.LoggedIn, handshake.Stage);
        Assert.Equal(0x000F02u, handshake.ServerVersion);
        Assert.Null(handshake.CreatedKey);
    }

    [Fact]
    public void Handle_KeepsTheKeyOfANewCharacter()
    {
        var handshake = AnsweredHandshake(null);
        var packet = Packet(ServerCommand.NewPlayer, 2002, 0xCCCC0003, 0xDDDD0004);
        packet[13] = 0x02;
        packet[14] = 0x0F;
        packet[15] = 0x00;

        handshake.Handle(packet);

        Assert.Equal(LoginStage.LoggedIn, handshake.Stage);
        Assert.Equal(
            new LoginKey(2002, 0xCCCC0003, 0xDDDD0004, CharacterRecord.NewAccountName, NewCharacterRace),
            handshake.CreatedKey);
        Assert.Equal(0x000F02u, handshake.ServerVersion);
    }

    [Theory]
    [InlineData(6u, "Server demands exit: Invalid password.")]
    [InlineData(11u, "Server demands exit: Client version too old. Update needed.")]
    [InlineData(0u, "Server demands exit: unknown reason")]
    [InlineData(15u, "Server demands exit: unknown reason")]
    public void Handle_FailsWithTheServersReasonForSendingTheClientAway(uint reasonCode, string expected)
    {
        var handshake = AnsweredHandshake(Key);

        handshake.Handle(Packet(ServerCommand.Exit, reasonCode));

        Assert.Equal(LoginStage.Failed, handshake.Stage);
        Assert.Equal(expected, handshake.Status);
    }

    [Fact]
    public void Handle_ShowsThePlaceInTheQueueWhileTheServerIsFull()
    {
        var handshake = AnsweredHandshake(Key);

        handshake.Handle(Packet(ServerCommand.Cap, 5, 1));
        handshake.Handle(Packet(ServerCommand.Cap, 3, 2));

        Assert.Equal(LoginStage.WaitingForResult, handshake.Stage);
        Assert.Equal("Player limit reached. Queue position: 3, Priority: 2, Try: 2", handshake.Status);
    }

    [Fact]
    public void Handle_JoinsTheServersMessageFromItsEightParts()
    {
        var handshake = AnsweredHandshake(Key);
        var message = "Welcome to The Last Gate! Season 4 starts on Friday.";
        var messageBytes = Encoding.Latin1.GetBytes(message).Concat(new byte[120]).Take(120).ToArray();

        for (var partIndex = 0; partIndex < 8; partIndex++)
        {
            var packet = Packet((ServerCommand)((byte)ServerCommand.Mod1 + partIndex));
            messageBytes.AsSpan(partIndex * 15, 15).CopyTo(packet.AsSpan(1));
            handshake.Handle(packet);
        }

        Assert.Equal(message, handshake.ModText);
        Assert.Equal(LoginStage.WaitingForResult, handshake.Stage);
    }

    [Fact]
    public void Handle_RejectsAPacketThatIsNotSixteenBytes()
    {
        Assert.Throws<ArgumentException>(() => Handshake(Key).Handle(new byte[15]));
    }

    [Fact]
    public void Handle_RejectsPacketsAfterTheLoginHasEnded()
    {
        var handshake = AnsweredHandshake(Key);
        handshake.Handle(Packet(ServerCommand.LoginOk, 1));

        Assert.Throws<InvalidOperationException>(() => handshake.Handle(Packet(ServerCommand.LoginOk, 1)));
    }

    private static LoginHandshake Handshake(LoginKey? key, string password = "") =>
        new(key, NewCharacterRace, password, UniqueId1, UniqueId2);

    private static LoginHandshake AnsweredHandshake(LoginKey? key)
    {
        var handshake = Handshake(key);
        handshake.Handle(Packet(ServerCommand.Challenge, 1));
        return handshake;
    }

    private static byte[] Packet(ClientCommand command, params uint[] values) => Packet((byte)command, values);

    private static byte[] Packet(ServerCommand command, params uint[] values) => Packet((byte)command, values);

    private static byte[] Packet(byte command, uint[] values)
    {
        var packet = new byte[Protocol.PacketSize];
        packet[0] = command;
        for (var valueIndex = 0; valueIndex < values.Length; valueIndex++)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(packet.AsSpan(1 + valueIndex * 4), values[valueIndex]);
        }

        return packet;
    }

    private static byte[] PasswordPacket(string password)
    {
        var packet = Packet(ClientCommand.Password);
        Encoding.Latin1.GetBytes(password).CopyTo(packet, 1);
        return packet;
    }
}
