using System;
using System.Collections.Generic;

namespace TheLastGate.Hotkeys;

public sealed class KeyBindings
{
    public KeyChord? For(string id) => throw new NotImplementedException();

    public string LongName(string id) => throw new NotImplementedException();

    public string ShortName(string id) => throw new NotImplementedException();

    public Binding? Find(KeyChord pressed) => throw new NotImplementedException();

    public string? Assign(string id, KeyChord chord) => throw new NotImplementedException();

    public void Clear(string id) => throw new NotImplementedException();

    public void Reset(string id) => throw new NotImplementedException();

    public void ResetAll() => throw new NotImplementedException();

    public void Load(IReadOnlyDictionary<string, string> saved) => throw new NotImplementedException();

    public IReadOnlyDictionary<string, string> Saved() => throw new NotImplementedException();
}
