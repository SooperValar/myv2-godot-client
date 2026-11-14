using System.Buffers.Binary;
using System.Diagnostics;
using TheLastGate.Characters;
using TheLastGate.Net;

namespace TheLastGate.Tests;

public class ServerConnectionTests
{
    private const string LoopbackHost = "127.0.0.1";
    private const string UnreachableHost = "host.invalid";
    private static readonly TimeSpan WaitLimit = TimeSpan.FromSeconds(10);
    private static readonly LoginKey Key = new(1001, 0xAAAA0001, 0xBBBB0002, "Ishtar", 76);

    [Fact]
    public async Task Start_LogsInWithTheHandshakeBeforeReadingTicks()
    {
        using var server = new LoopbackServer();
        using var connection = new ServerConnection();
        var handshake = Handshake();
        connection.Start(new ServerAddress(LoopbackHost, "", server.Port), handshake);
        await server.AcceptAsync();

        var login = await server.ReadPacketAsync();
        await server.SendPacketAsync(ServerCommand.Challenge, 0x12345678);
        var challengeAnswer = await server.ReadPacketAsync();
        var uniqueId = await server.ReadPacketAsync();
        await WaitUntilAsync(() => connection.State == ConnectionState.LoggingIn);
        await server.SendPacketAsync(ServerCommand.LoginOk, Protocol.NetworkingVersion);

        await WaitUntilAsync(() => connection.State == ConnectionState.Connected);
        Assert.Equal(handshake.CreateOpeningPackets()[0], login);
        Assert.Equal((byte)ClientCommand.Challenge, challengeAnswer[0]);
        Assert.Equal(
            ChallengeCipher.Encrypt(0x12345678),
            BinaryPrimitives.ReadUInt32LittleEndian(challengeAnswer.AsSpan(1)));
        Assert.Equal((byte)ClientCommand.Unique, uniqueId[0]);
        Assert.Equal(LoginStage.LoggedIn, handshake.Stage);
        Assert.Contains("Login OK", Messages(connection));
    }

    [Fact]
    public async Task Start_FailsWithTheServersReasonWhenTheLoginIsRefused()
    {
        using var server = new LoopbackServer();
        using var connection = new ServerConnection();
        connection.Start(new ServerAddress(LoopbackHost, "", server.Port), Handshake());
        await server.AcceptAsync();

        await server.ReadPacketAsync();
        await server.SendPacketAsync(ServerCommand.Exit, 6);

        await WaitUntilAsync(() => connection.State == ConnectionState.Failed);
        Assert.Contains("Server demands exit: Invalid password.", Messages(connection));
    }

    [Fact]
    public async Task Start_ReportsEachChangeOfLoginStatus()
    {
        using var server = new LoopbackServer();
        using var connection = new ServerConnection();
        connection.Start(new ServerAddress(LoopbackHost, "", server.Port), Handshake());
        await server.AcceptAsync();

        await server.ReadPacketAsync();
        await server.SendPacketAsync(ServerCommand.Challenge, 1);
        await server.ReadPacketAsync();
        await server.ReadPacketAsync();
        await server.SendPacketAsync(ServerCommand.Cap, 3, 2);
        await server.SendPacketAsync(ServerCommand.LoginOk, Protocol.NetworkingVersion);

        await WaitUntilAsync(() => connection.State == ConnectionState.Connected);
        var messages = Messages(connection);
        Assert.Contains("Login challenge...", messages);
        Assert.Contains("Player limit reached. Queue position: 3, Priority: 2, Try: 1", messages);
        Assert.Contains("Login OK", messages);
    }

    [Fact]
    public async Task Start_FailsWhenTheServerClosesTheConnectionDuringLogin()
    {
        using var server = new LoopbackServer();
        using var connection = new ServerConnection();
        connection.Start(new ServerAddress(LoopbackHost, "", server.Port), Handshake());
        await server.AcceptAsync();
        await server.ReadPacketAsync();

        server.CloseConnection();

        await WaitUntilAsync(() => connection.State == ConnectionState.Failed);
        Assert.Contains("The server closed the connection.", Messages(connection));
    }

    [Fact]
    public async Task Start_QueuesEachTickDecompressedInArrivalOrder()
    {
        using var server = new LoopbackServer();
        using var connection = new ServerConnection();
        connection.Start(new ServerAddress(LoopbackHost, "", server.Port), Handshake());
        await server.AcceptAsync();
        await server.CompleteLoginAsync();

        await server.SendTickAsync([1, 2, 3], true);
        await server.SendTickAsync([4, 5], false);
        await server.SendTickAsync([1, 2, 3], true);

        var ticks = await DequeueTicksAsync(connection, 3);
        Assert.Equal(new byte[] { 1, 2, 3 }, ticks[0].Payload);
        Assert.Equal(new byte[] { 4, 5 }, ticks[1].Payload);
        Assert.Equal(new byte[] { 1, 2, 3 }, ticks[2].Payload);
        Assert.True(ticks[0].ReceivedAtTimestamp <= ticks[2].ReceivedAtTimestamp);
        Assert.Equal(ConnectionState.Connected, connection.State);
    }

    [Fact]
    public async Task Start_FallsBackToTheProxyWhenTheHostCannotBeReached()
    {
        using var server = new LoopbackServer();
        using var connection = new ServerConnection();

        connection.Start(new ServerAddress(UnreachableHost, LoopbackHost, server.Port), Handshake());
        await server.AcceptAsync();
        await server.CompleteLoginAsync();
        await server.SendTickAsync([9], false);

        Assert.Equal(new byte[] { 9 }, (await DequeueTicksAsync(connection, 1))[0].Payload);
    }

    [Fact]
    public async Task Start_FailsWhenNeitherTheHostNorTheProxyCanBeReached()
    {
        using var server = new LoopbackServer();
        using var connection = new ServerConnection();

        connection.Start(new ServerAddress(UnreachableHost, UnreachableHost, server.Port), Handshake());

        await WaitUntilAsync(() => connection.State == ConnectionState.Failed);
        Assert.Contains(Messages(connection), message => message.Contains("Couldn't connect"));
    }

    [Fact]
    public async Task SendCommand_SendsQueuedCommandsAfterTheNextTick()
    {
        using var server = new LoopbackServer();
        using var connection = new ServerConnection();
        connection.Start(new ServerAddress(LoopbackHost, "", server.Port), Handshake());
        await server.AcceptAsync();
        await server.CompleteLoginAsync();
        var command = new byte[Protocol.PacketSize];
        command[0] = (byte)ClientCommand.Move;
        command[1] = 42;

        connection.SendCommand(command);
        await server.SendTickAsync([1], true);

        Assert.Equal(command, await server.ReadPacketAsync());
    }

    [Fact]
    public void SendCommand_RejectsACommandThatIsNotOnePacketLong()
    {
        using var connection = new ServerConnection();

        Assert.Throws<ArgumentException>(() => connection.SendCommand(new byte[Protocol.PacketSize - 1]));
    }

    [Fact]
    public async Task Start_FailsWhenTheServerClosesTheConnection()
    {
        using var server = new LoopbackServer();
        using var connection = new ServerConnection();
        connection.Start(new ServerAddress(LoopbackHost, "", server.Port), Handshake());
        await server.AcceptAsync();
        await server.CompleteLoginAsync();
        await WaitUntilAsync(() => connection.State == ConnectionState.Connected);

        server.CloseConnection();

        await WaitUntilAsync(() => connection.State == ConnectionState.Failed);
        Assert.Contains("The server closed the connection.", Messages(connection));
    }

    [Fact]
    public async Task Start_FailsWithTheReasonWhenTheStreamCannotBeTrusted()
    {
        using var server = new LoopbackServer();
        using var connection = new ServerConnection();
        connection.Start(new ServerAddress(LoopbackHost, "", server.Port), Handshake());
        await server.AcceptAsync();
        await server.CompleteLoginAsync();

        await server.SendRawAsync([1, 0]);

        await WaitUntilAsync(() => connection.State == ConnectionState.Failed);
        Assert.Contains(Messages(connection), message => message.Contains("shorter than its 2-byte header"));
    }

    [Fact]
    public async Task Disconnect_ClosesTheConnection()
    {
        using var server = new LoopbackServer();
        using var connection = new ServerConnection();
        connection.Start(new ServerAddress(LoopbackHost, "", server.Port), Handshake());
        await server.AcceptAsync();
        await server.CompleteLoginAsync();
        await WaitUntilAsync(() => connection.State == ConnectionState.Connected);

        connection.Disconnect();

        await WaitUntilAsync(() => connection.State == ConnectionState.Closed);
        Assert.Contains("Disconnected.", Messages(connection));
    }

    private static LoginHandshake Handshake() => new(Key, 0, "", 0x11111111, 0x22222222);

    private static async Task<List<ReceivedTick>> DequeueTicksAsync(ServerConnection connection, int count)
    {
        var ticks = new List<ReceivedTick>();
        await WaitUntilAsync(() =>
        {
            while (ticks.Count < count && connection.TryDequeueTick(out var tick))
            {
                ticks.Add(tick);
            }

            return ticks.Count == count;
        });
        return ticks;
    }

    private static List<string> Messages(ServerConnection connection)
    {
        var messages = new List<string>();
        while (connection.TryDequeueMessage(out var message))
        {
            messages.Add(message);
        }

        return messages;
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        var stopwatch = Stopwatch.StartNew();
        while (!condition())
        {
            if (stopwatch.Elapsed > WaitLimit)
            {
                throw new TimeoutException("The connection didn't reach the expected state in time.");
            }

            await Task.Delay(10);
        }
    }
}
