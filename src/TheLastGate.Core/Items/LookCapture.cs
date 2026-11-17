using System;
using System.Collections.Generic;
using TheLastGate.State;

namespace TheLastGate.Items;

// Shared is the look reply both requests got; Released is everything else they caught, which belongs in the chat log.
public sealed record LookReply(IReadOnlyList<LogLine> Shared, IReadOnlyList<LogLine> Released);

public sealed class LookCapture
{
    public const int MaxLines = 32;

    private readonly Action<LookReply> resolved;

    public LookCapture(Action<LookReply> resolved) => this.resolved = resolved;

    public bool HasPending => throw new NotImplementedException();

    public void ExpectFetch(int tick) => throw new NotImplementedException();

    public void ExpectManual(int tick) => throw new NotImplementedException();

    public bool Offer(LogLine line) => throw new NotImplementedException();

    public void EndOfTick() => throw new NotImplementedException();

    public IReadOnlyList<LogLine> Abandon() => throw new NotImplementedException();

    public void DropManualsSentBefore(int tick) => throw new NotImplementedException();
}
