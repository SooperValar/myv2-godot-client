using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace TheLastGate.Net;

public sealed class ServerConnection : IDisposable
{
    private static readonly TimeSpan ConnectTimeout = TimeSpan.FromSeconds(10);

    private readonly CancellationTokenSource cancellation = new();
    private readonly ConcurrentQueue<string> messages = new();
    private readonly ConcurrentQueue<byte[]> outgoingCommands = new();
    private readonly ConcurrentQueue<ReceivedTick> receivedTicks = new();
    private volatile TcpClient? client;
    private Task? runTask;
    private volatile ConnectionState state = ConnectionState.Idle;

    public ConnectionState State => state;

    public void Dispose() => Disconnect();

    public void Start(ServerAddress address)
    {
        if (runTask != null)
        {
            throw new InvalidOperationException("A connection can only be started once.");
        }

        state = ConnectionState.Connecting;
        runTask = Task.Run(() => RunAsync(address, cancellation.Token));
    }

    // Closing the socket is how the server expects a client to leave: it logs the character out
    // without the penalty the in-game exit command gives.
    public void Disconnect()
    {
        cancellation.Cancel();
        client?.Dispose();
    }

    public void SendCommand(byte[] command)
    {
        if (command.Length != Protocol.PacketSize)
        {
            throw new ArgumentException($"Client commands are {Protocol.PacketSize} bytes.", nameof(command));
        }

        outgoingCommands.Enqueue(command);
    }

    public bool TryDequeueTick(out ReceivedTick tick) => receivedTicks.TryDequeue(out tick);

    public bool TryDequeueMessage(out string message) => messages.TryDequeue(out message!);

    private async Task RunAsync(ServerAddress address, CancellationToken cancellationToken)
    {
        try
        {
            var stream = (await ConnectAsync(address, cancellationToken)).GetStream();
            state = ConnectionState.Connected;
            await ReadTicksAsync(stream, cancellationToken);
            End(ConnectionState.Failed, "The server closed the connection.");
        }
        catch (Exception exception) when (cancellationToken.IsCancellationRequested
            && exception is OperationCanceledException or IOException or SocketException or ObjectDisposedException)
        {
            End(ConnectionState.Closed, "Disconnected.");
        }
        catch (EndOfStreamException)
        {
            End(ConnectionState.Failed, "The server closed the connection.");
        }
        catch (Exception exception) when (exception is IOException or SocketException or InvalidDataException
            or ProtocolDesyncException)
        {
            End(ConnectionState.Failed, $"Connection failed: {exception.Message}");
        }
        finally
        {
            client?.Dispose();
        }
    }

    private void End(ConnectionState finalState, string message)
    {
        messages.Enqueue(message);
        state = finalState;
    }

    private async Task<TcpClient> ConnectAsync(ServerAddress address, CancellationToken cancellationToken)
    {
        foreach (var host in new[] { address.Host, address.Proxy })
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                continue;
            }

            messages.Enqueue($"Connecting to {host}:{address.Port}...");
            var candidate = new TcpClient { NoDelay = true };
            client = candidate;
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(ConnectTimeout);
            try
            {
                await candidate.ConnectAsync(host, address.Port, timeout.Token);
                messages.Enqueue($"Connected to {host}.");
                return candidate;
            }
            catch (Exception exception) when (!cancellationToken.IsCancellationRequested
                && exception is SocketException or OperationCanceledException)
            {
                candidate.Dispose();
                messages.Enqueue(
                    exception is OperationCanceledException
                        ? $"Timed out connecting to {host}."
                        : $"Couldn't connect to {host}: {exception.Message}");
            }
        }

        throw new IOException("Couldn't connect to the host or the proxy.");
    }

    private async Task ReadTicksAsync(NetworkStream stream, CancellationToken cancellationToken)
    {
        var frameReader = new TickFrameReader(stream);
        using var decompressor = new TickDecompressor();
        var decompressed = new byte[TickDecompressor.MaxPayloadSize];
        while (await frameReader.ReadAsync(cancellationToken) is { } frame)
        {
            var receivedAt = Stopwatch.GetTimestamp();
            var payload = frame.IsCompressed
                ? decompressed[..decompressor.Decompress(frame.Body, decompressed)]
                : frame.Body;
            receivedTicks.Enqueue(new ReceivedTick(payload, receivedAt));

            while (outgoingCommands.TryDequeue(out var command))
            {
                await stream.WriteAsync(command, cancellationToken);
            }
        }
    }
}
