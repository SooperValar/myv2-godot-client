using System;
using TheLastGate.State;

namespace TheLastGate.World;

public sealed class NameRequests
{
    public const int TicksBetweenRequests = 24;

    public void NoteUnknown(int characterNumber, int characterId) => throw new NotImplementedException();

    public byte[]? Tick(CharacterNames names) => throw new NotImplementedException();
}
