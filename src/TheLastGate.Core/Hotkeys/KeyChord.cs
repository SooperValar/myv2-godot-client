using System;
using TheLastGate.World;

namespace TheLastGate.Hotkeys;

public readonly record struct KeyChord(string Key, MapKeys Modifiers)
{
    public string LongName => throw new NotImplementedException();

    public string ShortName => throw new NotImplementedException();

    public static KeyChord? Parse(string text) => throw new NotImplementedException();
}
