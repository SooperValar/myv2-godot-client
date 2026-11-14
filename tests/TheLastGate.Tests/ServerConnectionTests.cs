using System.Diagnostics;
using TheLastGate.Net;

namespace TheLastGate.Tests;

public class ServerConnectionTests
{
    private const string LoopbackHost = "127.0.0.1";
    private const string UnreachableHost = "host.invalid";
    private static readonly TimeSpan WaitLimit = TimeSpan.FromSeconds(10);

    [Fact]
    public async Task Start_QueuesEachTickDecompressedInArrivalOrder()
    {
        using var server = new LoopbackServer();
        using var connection = new ServerConnection();
        connection.Start(new ServerAddress(LoopbackHost, "", server.Port));
        await server.AcceptAsync();

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

        connection.Start(new ServerAddress(UnreachableHost, LoopbackHost, server.Port));
        await server.AcceptAsync();
        await server.SendTickAsync([9], false);

        Assert.Equal(new byte[] { 9 }, (await DequeueTicksAsync(connection, 1))[0].Payload);
    }

    [Fact]
    public async Task Start_FailsWhenNeitherTheHostNorTheProxyCanBeReached()
    {
        using var server = new LoopbackServer();
        using var connection = new ServerConnection();

        connection.Start(new ServerAddress(UnreachableHost, UnreachableHost, server.Port));

        await WaitUntilAsync(() => connection.State == ConnectionState.Failed);
        Assert.Contains(Messages(connection), message => message.Contains("Couldn't connect"));
    }

    [Fact]
    public async Task SendCommand_SendsQueuedCommandsAfterTheNextTick()
    {
        using var server = new LoopbackServer();
        using var connection = new ServerConnection();
        connection.Start(new ServerAddress(LoopbackHost, "", server.Port));
        await server.AcceptAsync();
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
        connection.Start(new ServerAddress(LoopbackHost, "", server.Port));
        await server.AcceptAsync();

        server.CloseConnection();

        await WaitUntilAsync(() => connection.State == ConnectionState.Failed);
        Assert.Contains("The server closed the connection.", Messages(connection));
    }

    [Fact]
    public async Task Start_FailsWithTheReasonWhenTheStreamCannotBeTrusted()
    {
        using var server = new LoopbackServer();
        using var connection = new ServerConnection();
        connection.Start(new ServerAddress(LoopbackHost, "", server.Port));
        await server.AcceptAsync();

        await server.SendRawAsync([1, 0]);

        await WaitUntilAsync(() => connection.State == ConnectionState.Failed);
        Assert.Contains(Messages(connection), message => message.Contains("shorter than its 2-byte header"));
    }

    [Fact]
    public async Task Disconnect_ClosesTheConnection()
    {
        using var server = new LoopbackServer();
        using var connection = new ServerConnection();
        connection.Start(new ServerAddress(LoopbackHost, "", server.Port));
        await server.AcceptAsync();
        await WaitUntilAsync(() => connection.State == ConnectionState.Connected);

        connection.Disconnect();

        await WaitUntilAsync(() => connection.State == ConnectionState.Closed);
        Assert.Contains("Disconnected.", Messages(connection));
    }

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
