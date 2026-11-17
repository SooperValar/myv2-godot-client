using TheLastGate.Hotkeys;
using TheLastGate.World;

namespace TheLastGate.Tests;

public class KeyBindingsTests
{
    private readonly KeyBindings bindings = new();

    [Fact]
    public void All_HasEveryBindingOfTheCurrentClientOnceButGamma()
    {
        Assert.Equal(48, Bindings.All.Count);
        Assert.Equal(Bindings.All.Count, Bindings.All.Select(binding => binding.Id).Distinct().Count());
    }

    [Fact]
    public void All_LeavesGammaToTheDisplaySliders()
    {
        Assert.DoesNotContain(Bindings.All, binding => binding.Id == "toggle_gamma_hotkey");
    }

    [Fact]
    public void Load_DropsAGammaKeySavedByAnEarlierVersion()
    {
        bindings.Load(new Dictionary<string, string> { ["toggle_gamma_hotkey"] = "F10" });

        Assert.False(bindings.Saved().ContainsKey("toggle_gamma_hotkey"));
        Assert.Null(bindings.Find(new KeyChord("F10", MapKeys.None)));
    }

    [Theory]
    [InlineData("spell_1", "Spell 1", BindingCategory.Spell, "Ctrl+1")]
    [InlineData("spell_6", "Spell 6", BindingCategory.Spell, "Ctrl+Q")]
    [InlineData("spell_20", "Spell 20", BindingCategory.Spell, "Ctrl+B")]
    [InlineData("speed_fast_hotkey", "Fast Mode", BindingCategory.Speed, "F1")]
    [InlineData("speed_slow_hotkey", "Slow Mode", BindingCategory.Speed, "F3")]
    [InlineData("toggle_percent_hotkey", "Toggle Health %", BindingCategory.World, "F4")]
    [InlineData("toggle_health_bars_hotkey", "Toggle Health Bars", BindingCategory.World, "F8")]
    [InlineData("toggle_options_hotkey", "Options Window", BindingCategory.Window, "F9")]
    [InlineData("reset_window_size_hotkey", "Reset Window Size", BindingCategory.Window, "Shift+F10")]
    [InlineData("toggle_planner_hotkey", "Skill Planner", BindingCategory.Window, "Ctrl+P")]
    [InlineData("toggle_worldmap_hotkey", "World Map", BindingCategory.Window, null)]
    [InlineData("fightback_hotkey", "Toggle Fightback", BindingCategory.General, null)]
    [InlineData("swap_gear_hotkey", "Swap Gearset", BindingCategory.General, "Insert")]
    [InlineData("display_debug_information_hotkey", "Display Debug Info", BindingCategory.General, "F11")]
    [InlineData("exit_hotkey", "Exit", BindingCategory.General, "F12")]
    [InlineData("gc_transfer_hotkey", "Transfer", BindingCategory.GuildCommands, null)]
    public void All_MatchesTheCurrentClientsNamesAndDefaults(
        string id,
        string name,
        BindingCategory category,
        string? defaultKeys)
    {
        var binding = Bindings.All.Single(binding => binding.Id == id);

        Assert.Equal(name, binding.Name);
        Assert.Equal(category, binding.Category);
        Assert.Equal(defaultKeys, binding.Default?.LongName);
        Assert.False(binding.Hold);
    }

    [Fact]
    public void All_HoldsTheItemTooltipKey()
    {
        var binding = Bindings.All.Single(binding => binding.Id == "item_tooltip_hotkey");

        Assert.Equal("Item Tooltip (hold)", binding.Name);
        Assert.Equal(new KeyChord("Alt", MapKeys.None), binding.Default);
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

    [Fact]
    public void ANewSet_StartsWithTheDefaults()
    {
        Assert.Equal("Ctrl+1", bindings.LongName("spell_1"));
        Assert.Equal("C-1", bindings.ShortName("spell_1"));
        Assert.Equal("Not Set", bindings.LongName("gc_move_hotkey"));
        Assert.Equal("", bindings.ShortName("gc_move_hotkey"));
    }

    [Fact]
    public void Find_NeedsTheExactModifiers()
    {
        Assert.Equal("spell_1", bindings.Find(new KeyChord("1", MapKeys.Ctrl))?.Id);
        Assert.Null(bindings.Find(new KeyChord("1", MapKeys.Ctrl | MapKeys.Shift)));
        Assert.Null(bindings.Find(new KeyChord("1", MapKeys.None)));
        Assert.Equal("reset_window_size_hotkey", bindings.Find(new KeyChord("F10", MapKeys.Shift))?.Id);
        Assert.Null(bindings.Find(new KeyChord("F10", MapKeys.None)));
    }

    [Fact]
    public void Find_LeavesHoldBindingsToBeCheckedWhileHeld()
    {
        Assert.Null(bindings.Find(new KeyChord("Alt", MapKeys.None)));
    }

    [Fact]
    public void Assign_TakesTheKeyFromTheBindingThatHadIt()
    {
        var displaced = bindings.Assign("speed_fast_hotkey", new KeyChord("1", MapKeys.Ctrl));

        Assert.Equal("spell_1", displaced);
        Assert.Equal("Not Set", bindings.LongName("spell_1"));
        Assert.Equal("speed_fast_hotkey", bindings.Find(new KeyChord("1", MapKeys.Ctrl))?.Id);
        Assert.Null(bindings.Find(new KeyChord("F1", MapKeys.None)));
    }

    [Fact]
    public void Assign_DisplacesNothingWhenTheKeyWasFree()
    {
        Assert.Null(bindings.Assign("gc_move_hotkey", new KeyChord("M", MapKeys.Alt)));
        Assert.Null(bindings.Assign("gc_move_hotkey", new KeyChord("M", MapKeys.Alt)));
        Assert.Equal("Alt+M", bindings.LongName("gc_move_hotkey"));
    }

    [Fact]
    public void ClearAndReset_UnbindAndRestoreOneBinding()
    {
        bindings.Clear("spell_1");
        Assert.Null(bindings.For("spell_1"));
        Assert.Null(bindings.Find(new KeyChord("1", MapKeys.Ctrl)));

        bindings.Reset("spell_1");
        Assert.Equal(new KeyChord("1", MapKeys.Ctrl), bindings.For("spell_1"));
    }

    [Fact]
    public void ResetAll_RestoresEveryDefault()
    {
        bindings.Assign("speed_fast_hotkey", new KeyChord("1", MapKeys.Ctrl));
        bindings.Clear("exit_hotkey");

        bindings.ResetAll();

        Assert.Equal("Ctrl+1", bindings.LongName("spell_1"));
        Assert.Equal("F1", bindings.LongName("speed_fast_hotkey"));
        Assert.Equal("F12", bindings.LongName("exit_hotkey"));
    }

    [Fact]
    public void Load_ReadsSavedKeysAndKeepsDefaultsForAnythingElse()
    {
        var saved = new Dictionary<string, string>
        {
            ["spell_1"] = "Alt+Q",
            ["spell_2"] = "",
            ["spell_3"] = "Banana+Q",
            ["no_such_hotkey"] = "F5",
        };

        bindings.Load(saved);

        Assert.Equal("Alt+Q", bindings.LongName("spell_1"));
        Assert.Equal("Not Set", bindings.LongName("spell_2"));
        Assert.Equal("Ctrl+3", bindings.LongName("spell_3"));
        Assert.Equal("Ctrl+4", bindings.LongName("spell_4"));
    }

    [Fact]
    public void Saved_WritesEveryBindingReadably()
    {
        bindings.Clear("spell_2");

        var saved = bindings.Saved();

        Assert.Equal(Bindings.All.Count, saved.Count);
        Assert.Equal("Ctrl+1", saved["spell_1"]);
        Assert.Equal("", saved["spell_2"]);
        Assert.Equal("Shift+F10", saved["reset_window_size_hotkey"]);
        Assert.Equal("Alt", saved["item_tooltip_hotkey"]);
    }

    [Theory]
    [InlineData(MapKeys.Alt, false, true)]
    [InlineData(MapKeys.Alt | MapKeys.Shift, false, true)]
    [InlineData(MapKeys.Shift, true, false)]
    [InlineData(MapKeys.None, false, false)]
    public void KeyChord_AModifierAloneIsHeldWhileThatModifierIsDown(MapKeys heldModifiers, bool keyDown, bool isHeld)
    {
        Assert.Equal(isHeld, new KeyChord(KeyNames.Alt, MapKeys.None).IsHeld(heldModifiers, keyDown));
    }

    [Theory]
    [InlineData(MapKeys.Ctrl, true, true)]
    [InlineData(MapKeys.Ctrl | MapKeys.Shift, true, true)]
    [InlineData(MapKeys.Ctrl, false, false)]
    [InlineData(MapKeys.Shift, true, false)]
    [InlineData(MapKeys.None, true, false)]
    public void KeyChord_IsHeldWithItsKeyAndAtLeastItsModifiers(MapKeys heldModifiers, bool keyDown, bool isHeld)
    {
        Assert.Equal(isHeld, new KeyChord("T", MapKeys.Ctrl).IsHeld(heldModifiers, keyDown));
    }

    [Fact]
    public void SavedAndLoaded_RoundTrip()
    {
        bindings.Assign("spell_5", new KeyChord("Keypad 5", MapKeys.None));
        var reloaded = new KeyBindings();

        reloaded.Load(bindings.Saved());

        Assert.Equal(bindings.Saved(), reloaded.Saved());
    }
}
