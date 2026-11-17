using System;
using System.Collections.Generic;
using TheLastGate.Characters;

namespace TheLastGate.Windows;

public sealed record KeybindRow(string Id, string Label);

public sealed record KeybindSection(string Heading, IReadOnlyList<KeybindRow> Rows);

public static class KeybindsTab
{
    public static IReadOnlyList<KeybindSection> Sections(CharacterSettings settings) =>
        throw new NotImplementedException();
}
