using System;
using TheLastGate.Characters;
using TheLastGate.State;

namespace TheLastGate.Net;

public sealed class UserDataUpload(string name, string description, TextLog chat)
{
    public bool Done => throw new NotImplementedException();

    public static UserDataUpload? For(CharacterRecord record, TextLog chat) =>
        throw new NotImplementedException();

    public byte[]? Tick()
    {
        _ = (name, description, chat);
        throw new NotImplementedException();
    }
}
