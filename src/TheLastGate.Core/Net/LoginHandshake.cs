using System;
using System.Collections.Generic;
using TheLastGate.Characters;

namespace TheLastGate.Net;

public sealed class LoginHandshake
{
    public LoginHandshake(LoginKey? key, int newCharacterRace, string password, uint uniqueId1, uint uniqueId2) =>
        throw new NotImplementedException();

    public LoginStage Stage => throw new NotImplementedException();

    public string Status => throw new NotImplementedException();

    public uint ServerVersion => throw new NotImplementedException();

    public LoginKey? CreatedKey => throw new NotImplementedException();

    public string ModText => throw new NotImplementedException();

    public IReadOnlyList<byte[]> CreateOpeningPackets() => throw new NotImplementedException();

    public IReadOnlyList<byte[]> Handle(ReadOnlySpan<byte> packet) => throw new NotImplementedException();
}
