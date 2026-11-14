using System;

namespace TheLastGate.Net;

public sealed class ServerConnection : IDisposable
{
    public ConnectionState State => throw new NotImplementedException();

    public void Dispose()
    {
    }

    public void Start(ServerAddress address) => throw new NotImplementedException();

    public void Disconnect() => throw new NotImplementedException();

    public void SendCommand(byte[] command) => throw new NotImplementedException();

    public bool TryDequeueTick(out ReceivedTick tick) => throw new NotImplementedException();

    public bool TryDequeueMessage(out string message) => throw new NotImplementedException();
}
