using System;
using TheLastGate.State;

namespace TheLastGate.World;

public enum UseQueueAdd
{
    Ignored,
    Full,
    Queued,
}

public sealed class UseQueue(TextLog chat)
{
    public const int MostQueued = 5;

    public bool Enabled { get; set; }

    public Action<byte[]> Send { get; set; } = _ => { };

    public int Count => throw new NotImplementedException();

    public int PositionOf(int x, int y) => throw new NotImplementedException();

    public UseQueueAdd Add(int x, int y, PlayerTarget target)
    {
        _ = chat;
        throw new NotImplementedException();
    }

    public void TargetChanged(PlayerTarget previous, PlayerTarget now) => throw new NotImplementedException();

    public void Sent(byte[] packet) => throw new NotImplementedException();
}
