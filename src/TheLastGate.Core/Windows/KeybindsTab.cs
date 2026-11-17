using System;
using System.Collections.Generic;
using System.Linq;
using TheLastGate.Characters;
using TheLastGate.Hotkeys;

namespace TheLastGate.Windows;

public sealed record KeybindRow(string Id, string Label);

public sealed record KeybindSection(string Heading, IReadOnlyList<KeybindRow> Rows);

public static class KeybindsTab
{
    public static IReadOnlyList<KeybindSection> Sections(CharacterSettings settings) =>
        Enum.GetValues<BindingCategory>()
            .Select(category => new KeybindSection(
                Bindings.CategoryName(category),
                Bindings.All
                    .Where(binding => binding.Category == category)
                    .Select(binding => Row(binding, settings))
                    .OfType<KeybindRow>()
                    .ToList()))
            .ToList();

    private static KeybindRow? Row(Binding binding, CharacterSettings settings)
    {
        if (Bindings.SpellSlot(binding.Id) is not { } slot)
        {
            return new KeybindRow(binding.Id, binding.Name);
        }

        return settings.HotbarActions[slot] == CharacterSettings.Unassigned
            ? null
            : new KeybindRow(binding.Id, $"{binding.Name} ({settings.HotbarNames[slot]})");
    }
}
