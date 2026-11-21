using GdUnit4;
using TheLastGate.Hotkeys;
using TheLastGate.World;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Hotkeys;

[TestSuite]
public class BindingsTests
{
    [TestCase]
    public void All_HasEveryBindingOfTheCurrentClientOnce()
    {
        AssertThat(Bindings.All.Count).IsEqual(51);
        AssertThat(Bindings.All.Select(binding => binding.Id).Distinct().Count()).IsEqual(Bindings.All.Count);
    }

    [TestCase]
    public void All_PutsTheGammaKeyAfterTheOptionsWindow()
    {
        var ids = Bindings.All.Select(binding => binding.Id).ToList();

        AssertThat(ids.IndexOf("toggle_gamma_hotkey")).IsEqual(ids.IndexOf("toggle_options_hotkey") + 1);
    }

    [TestCase("spell_1", "Spell 1", BindingCategory.Spell)]
    [TestCase("spell_6", "Spell 6", BindingCategory.Spell)]
    [TestCase("spell_20", "Spell 20", BindingCategory.Spell)]
    [TestCase("speed_fast_hotkey", "Fast Mode", BindingCategory.Speed)]
    [TestCase("speed_slow_hotkey", "Slow Mode", BindingCategory.Speed)]
    [TestCase("toggle_percent_hotkey", "Toggle Health %", BindingCategory.World)]
    [TestCase("toggle_health_bars_hotkey", "Toggle Health Bars", BindingCategory.World)]
    [TestCase("toggle_options_hotkey", "Options Window", BindingCategory.Window)]
    [TestCase("toggle_gamma_hotkey", "Toggle World Gamma", BindingCategory.Window)]
    [TestCase("reset_window_size_hotkey", "Reset Window Size", BindingCategory.Window)]
    [TestCase("toggle_planner_hotkey", "Skill Planner", BindingCategory.Window)]
    [TestCase("toggle_worldmap_hotkey", "World Map", BindingCategory.Window)]
    [TestCase("fightback_hotkey", "Toggle Fightback", BindingCategory.General)]
    [TestCase("swap_gear_hotkey", "Swap Gearset", BindingCategory.General)]
    [TestCase("display_debug_information_hotkey", "Display Debug Info", BindingCategory.General)]
    [TestCase("exit_hotkey", "Exit", BindingCategory.General)]
    [TestCase("gc_transfer_hotkey", "Transfer", BindingCategory.GuildCommands)]
    public void All_MatchesTheCurrentClientsNames(string id, string name, BindingCategory category)
    {
        var binding = Bindings.All.Single(binding => binding.Id == id);

        AssertThat(binding.Name).IsEqual(name);
        AssertThat(binding.Category).IsEqual(category);
        AssertThat(binding.Hold).IsFalse();
    }

    [TestCase]
    public void All_HoldsTheItemTooltipKey()
    {
        var binding = Bindings.All.Single(binding => binding.Id == "item_tooltip_hotkey");

        AssertThat(binding.Name).IsEqual("Item Tooltip (hold)");
        AssertThat(binding.Hold).IsTrue();
    }

    [TestCase(BindingCategory.Spell, "Spell Hotkeys")]
    [TestCase(BindingCategory.World, "World Hotkeys")]
    [TestCase(BindingCategory.GuildCommands, "GC Commands")]
    public void CategoryName_UsesTheCurrentClientsHeadings(BindingCategory category, string name)
    {
        AssertThat(Bindings.CategoryName(category)).IsEqual(name);
    }

    [TestCase("spell_1", 0)]
    [TestCase("spell_20", 19)]
    public void SpellSlot_FindsTheHotbarButtonOfASpellBinding(string id, int slot)
    {
        AssertThat(Bindings.SpellSlot(id)).IsEqual(slot);
        AssertThat(Bindings.SpellId(slot)).IsEqual(id);
    }

    [TestCase("speed_fast_hotkey")]
    [TestCase("spell_")]
    [TestCase("spell_21")]
    public void SpellSlot_IgnoresOtherBindings(string id)
    {
        AssertThat(Bindings.SpellSlot(id)).IsNull();
    }

    [TestCase("A", MapKeys.Ctrl, "Ctrl+A", "C-A")]
    [TestCase("F10", MapKeys.Shift, "Shift+F10", "S-F10")]
    [TestCase("1", MapKeys.Shift | MapKeys.Ctrl | MapKeys.Alt, "Shift+Ctrl+Alt+1", "S-C-A-1")]
    [TestCase("Insert", MapKeys.None, "Insert", "Insert")]
    [TestCase("Keypad 7", MapKeys.Alt, "Alt+Keypad 7", "A-Keypad 7")]
    public void KeyChord_NamesModifiersInTheCurrentClientsOrder(
        string key,
        MapKeys modifiers,
        string longName,
        string shortName)
    {
        var chord = new KeyChord(key, modifiers);

        AssertThat(chord.LongName).IsEqual(longName);
        AssertThat(chord.ShortName).IsEqual(shortName);
        AssertThat(KeyChord.Parse(longName)).IsEqual(chord);
    }

    [TestCase("")]
    [TestCase("Ctrl+")]
    [TestCase("Meta+A")]
    [TestCase("Ctrl+Ctrl+A")]
    public void KeyChord_ParseRejectsTextItDidNotWrite(string text)
    {
        AssertThat(KeyChord.Parse(text)).IsNull();
    }
}
