using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using TheLastGate.Characters;

namespace TheLastGate.Net;

public sealed class LoginHandshake
{
    private const int PasswordFieldLength = 15;
    private const int ModPartLength = 15;
    private const int ModPartCount = ServerCommand.Mod8 - ServerCommand.Mod1 + 1;

    private static readonly string[] ExitReasons =
    [
        "unknown",
        "Client failed challenge.",
        "Client was idle too long.",
        "No room to drop character.",
        "Invalid parameters.",
        "Character already active or no player character.",
        "Invalid password.",
        "Client too slow.",
        "Receive failure.",
        "Server is being shutdown.",
        "You entered a Tavern.",
        "Client version too old. Update needed.",
        "Aborting on user request.",
        "this should never show up",
        "You have been banned for an hour. Enhance your social behavior before you come back.",
    ];

    private readonly LoginKey? key;
    private readonly byte[] modBytes = new byte[ModPartCount * ModPartLength];
    private readonly string password;
    private readonly int race;
    private readonly uint uniqueId1;
    private readonly uint uniqueId2;
    private int queueTries;

    public LoginHandshake(LoginKey? key, int newCharacterRace, string password, uint uniqueId1, uint uniqueId2)
    {
        this.key = key is { UserNumber: not 0 } ? key : null;
        race = this.key?.Race ?? newCharacterRace;
        this.password = password;
        this.uniqueId1 = uniqueId1;
        this.uniqueId2 = uniqueId2;
    }

    public LoginStage Stage { get; private set; } = LoginStage.WaitingForChallenge;

    public string Status { get; private set; } = "Waiting for server response...";

    public uint ServerVersion { get; private set; }

    public LoginKey? CreatedKey { get; private set; }

    public string ModText => NulTerminatedText.Read(modBytes);

    public IReadOnlyList<byte[]> CreateOpeningPackets()
    {
        var packets = new List<byte[]>();
        if (password.Length > 0)
        {
            var passwordPacket = NewPacket(ClientCommand.Password);
            NulTerminatedText.Write(passwordPacket.AsSpan(1, PasswordFieldLength), password);
            packets.Add(passwordPacket);
        }

        packets.Add(
            key == null
                ? NewPacket(ClientCommand.NewLogin)
                : NewPacket(ClientCommand.Login, key.UserNumber, key.Pass1, key.Pass2));
        return packets;
    }

    public IReadOnlyList<byte[]> Handle(ReadOnlySpan<byte> packet)
    {
        if (packet.Length != Protocol.PacketSize)
        {
            throw new ArgumentException(
                $"Login packets are {Protocol.PacketSize} bytes, not {packet.Length}.",
                nameof(packet));
        }

        if (Stage is LoginStage.LoggedIn or LoginStage.Failed)
        {
            throw new InvalidOperationException($"The login has already ended ({Stage}).");
        }

        var reader = new PacketReader(packet);
        var command = (ServerCommand)reader.ReadByte();
        switch (command)
        {
            case ServerCommand.Challenge:
                Stage = LoginStage.WaitingForResult;
                Status = "Login challenge...";
                return
                [
                    NewPacket(
                        ClientCommand.Challenge,
                        ChallengeCipher.Encrypt(reader.ReadUInt32()),
                        Protocol.NetworkingVersion,
                        (uint)race),
                    NewPacket(ClientCommand.Unique, uniqueId1, uniqueId2),
                ];

            case ServerCommand.NewPlayer:
                CreatedKey = new LoginKey(
                    reader.ReadUInt32(),
                    reader.ReadUInt32(),
                    reader.ReadUInt32(),
                    CharacterRecord.NewAccountName,
                    race);
                ServerVersion = reader.ReadUInt16() | ((uint)reader.ReadByte() << 16);
                Stage = LoginStage.LoggedIn;
                Status = "Login as new player OK";
                return [];

            case ServerCommand.LoginOk:
                ServerVersion = reader.ReadUInt32();
                Stage = LoginStage.LoggedIn;
                Status = "Login OK";
                return [];

            case ServerCommand.Exit:
                var reasonCode = reader.ReadUInt32();
                var reason = reasonCode >= 1 && reasonCode < ExitReasons.Length
                    ? ExitReasons[reasonCode]
                    : "unknown reason";
                Stage = LoginStage.Failed;
                Status = $"Server demands exit: {reason}";
                return [];

            case ServerCommand.Cap:
                var queuePosition = reader.ReadUInt32();
                var priority = reader.ReadUInt32();
                queueTries++;
                Status =
                    $"Player limit reached. Queue position: {queuePosition}, Priority: {priority}, Try: {queueTries}";
                return [];

            case >= ServerCommand.Mod1 and <= ServerCommand.Mod8:
                var partIndex = command - ServerCommand.Mod1;
                reader.ReadBytes(ModPartLength).CopyTo(modBytes.AsSpan(partIndex * ModPartLength));
                return [];

            default:
                return [];
        }
    }

    private static byte[] NewPacket(ClientCommand command, params uint[] values)
    {
        var packet = new byte[Protocol.PacketSize];
        packet[0] = (byte)command;
        for (var valueIndex = 0; valueIndex < values.Length; valueIndex++)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(packet.AsSpan(1 + valueIndex * sizeof(uint)), values[valueIndex]);
        }

        return packet;
    }
}
