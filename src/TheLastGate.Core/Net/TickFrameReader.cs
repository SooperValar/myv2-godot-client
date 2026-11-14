using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TheLastGate.Net;

public sealed class TickFrameReader
{
    public TickFrameReader(Stream stream) => throw new NotImplementedException();

    public ValueTask<TickFrame?> ReadAsync(CancellationToken cancellationToken) =>
        throw new NotImplementedException();
}
