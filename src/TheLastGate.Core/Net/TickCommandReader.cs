using System;

namespace TheLastGate.Net;

public ref struct TickCommandReader
{
    public TickCommandReader(ReadOnlySpan<byte> payload) => throw new NotImplementedException();

    public bool TryReadNext(out ReadOnlySpan<byte> command) => throw new NotImplementedException();
}
