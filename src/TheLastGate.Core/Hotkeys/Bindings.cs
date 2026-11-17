using System;
using System.Collections.Generic;

namespace TheLastGate.Hotkeys;

public enum BindingCategory
{
    Spell,
    Speed,
    World,
    Window,
    General,
    GuildCommands,
}

public sealed record Binding(string Id, string Name, BindingCategory Category, KeyChord? Default, bool Hold = false);

public static class Bindings
{
    public static IReadOnlyList<Binding> All => throw new NotImplementedException();

    public static string CategoryName(BindingCategory category) => throw new NotImplementedException();

    public static string SpellId(int slot) => throw new NotImplementedException();

    public static int? SpellSlot(string id) => throw new NotImplementedException();
}
