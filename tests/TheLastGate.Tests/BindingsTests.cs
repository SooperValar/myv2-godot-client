using TheLastGate.Hotkeys;
using TheLastGate.World;

namespace TheLastGate.Tests;

public class BindingsTests
{
    [Fact]
    public void All_HasEveryBindingOfTheCurrentClientOnce()
    {
        Assert.Equal(51, Bindings.All.Count);
        Assert.Equal(Bindings.All.Count, Bindings.All.Select(binding => binding.Id).Distinct().Count());
    }

    [Fact]
    public void All_PutsTheGammaKeyAfterTheOptionsWindow()
    {
        var ids = Bindings.All.Select(binding => binding.Id).ToList();

        Assert.Equal(ids.IndexOf("toggle_options_hotkey") + 1, ids.IndexOf("toggle_gamma_hotkey"));
    }

    [Theory]
    [InlineData("spell_1", "Spell 1", BindingCategory.Spell)]
    [InlineData("spell_6", "Spell 6", BindingCategory.Spell)]
    [InlineData("spell_20", "Spell 20", BindingCategory.Spell)]
    [InlineData("speed_fast_hotkey", "Fast Mode", BindingCategory.Speed)]
    [InlineData("speed_slow_hotkey", "Slow Mode", BindingCategory.Speed)]
    [InlineData("toggle_percent_hotkey", "Toggle Health %", BindingCategory.World)]
    [InlineData("toggle_health_bars_hotkey", "Toggle Health Bars", BindingCategory.World)]
    [InlineData("toggle_options_hotkey", "Options Window", BindingCategory.Window)]
    [InlineData("toggle_gamma_hotkey", "Toggle World Gamma", BindingCategory.Window)]
    [InlineData("reset_window_size_hotkey", "Reset Window Size", BindingCategory.Window)]
    [InlineData("toggle_planner_hotkey", "Skill Planner", BindingCategory.Window)]
    [InlineData("toggle_worldmap_hotkey", "World Map", BindingCategory.Window)]
    [InlineData("fightback_hotkey", "Toggle Fightback", BindingCategory.General)]
    [InlineData("swap_gear_hotkey", "Swap Gearset", BindingCategory.General)]
    [InlineData("display_debug_information_hotkey", "Display Debug Info", BindingCategory.General)]
    [InlineData("exit_hotkey", "Exit", BindingCategory.General)]
    [InlineData("gc_transfer_hotkey", "Transfer", BindingCategory.GuildCommands)]
    public void All_MatchesTheCurrentClientsNames(string id, string name, BindingCategory category)
    {
        var binding = Bindings.All.Single(binding => binding.Id == id);

        Assert.Equal(name, binding.Name);
        Assert.Equal(category, binding.Category);
        Assert.False(binding.Hold);
    }

    [Fact]
    public void All_HoldsTheItemTooltipKey()
    {
        var binding = Bindings.All.Single(binding => binding.Id == "item_tooltip_hotkey");

        Assert.Equal("Item Tooltip (hold)", binding.Name);
        Assert.True(binding.Hold);
    }

    [Theory]
    [InlineData(BindingCategory.Spell, "Spell Hotkeys")]
    [InlineData(BindingCategory.World, "World Hotkeys")]
    [InlineData(BindingCategory.GuildCommands, "GC Commands")]
    public void CategoryName_UsesTheCurrentClientsHeadings(BindingCategory category, string name)
    {
        Assert.Equal(name, Bindings.CategoryName(category));
    }

    [Theory]
    [InlineData("spell_1", 0)]
    [InlineData("spell_20", 19)]
    public void SpellSlot_FindsTheHotbarButtonOfASpellBinding(string id, int slot)
    {
        Assert.Equal(slot, Bindings.SpellSlot(id));
        Assert.Equal(id, Bindings.SpellId(slot));
    }

    [Theory]
    [InlineData("speed_fast_hotkey")]
    [InlineData("spell_")]
    [InlineData("spell_21")]
    public void SpellSlot_IgnoresOtherBindings(string id)
    {
        Assert.Null(Bindings.SpellSlot(id));
    }

    [Theory]
    [InlineData("A", MapKeys.Ctrl, "Ctrl+A", "C-A")]
    [InlineData("F10", MapKeys.Shift, "Shift+F10", "S-F10")]
    [InlineData("1", MapKeys.Shift | MapKeys.Ctrl | MapKeys.Alt, "Shift+Ctrl+Alt+1", "S-C-A-1")]
    [InlineData("Insert", MapKeys.None, "Insert", "Insert")]
    [InlineData("Keypad 7", MapKeys.Alt, "Alt+Keypad 7", "A-Keypad 7")]
    public void KeyChord_NamesModifiersInTheCurrentClientsOrder(
        string key,
        MapKeys modifiers,
        string longName,
        string shortName)
    {
        var chord = new KeyChord(key, modifiers);

        Assert.Equal(longName, chord.LongName);
        Assert.Equal(shortName, chord.ShortName);
        Assert.Equal(chord, KeyChord.Parse(longName));
    }

    [Theory]
    [InlineData("")]
    [InlineData("Ctrl+")]
    [InlineData("Meta+A")]
    [InlineData("Ctrl+Ctrl+A")]
    public void KeyChord_ParseRejectsTextItDidNotWrite(string text)
    {
        Assert.Null(KeyChord.Parse(text));
    }
}
