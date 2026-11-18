using System;
using TheLastGate.State;

namespace TheLastGate.Buffs;

public sealed class BuffWatch
{
    public const string Command = "#buffs";
    public const string FloodReply = "Oops, you're a bit too fast for me!";

    private readonly PlayerState player;

    public BuffWatch(PlayerState player) => this.player = player;

    public Action<string> Say { get; set; } = _ => { };

    public bool Waiting => throw new NotImplementedException();

    public void Reset() => throw new NotImplementedException();

    public void SpellPacket(int slot, BuffSlot old, BuffSlot updated) => throw new NotImplementedException();

    public bool Capture(LogLine line) => throw new NotImplementedException();

    public void EndOfTick() => throw new NotImplementedException();

    public bool IsActive(BuffKind kind) => throw new NotImplementedException();

    public BuffReading? Reading(BuffKind kind) => throw new NotImplementedException();
}
