using TheLastGate.Hotkeys;
using TheLastGate.World;

namespace TheLastGate.Tests;

public class BindingRulesTests
{
    private static readonly KeyChord CtrlOne = new("1", MapKeys.Ctrl);
    private static readonly KeyChord CtrlTwo = new("2", MapKeys.Ctrl);
    private static readonly KeyChord FunctionOne = new("F1", MapKeys.None);
    private static readonly KeyChord AltAlone = new(KeyNames.Alt, MapKeys.None);

    private static readonly IReadOnlyDictionary<string, KeyChord?> Defaults = Snapshot(
        ("spell_1", CtrlOne),
        ("spell_2", CtrlTwo),
        ("speed_fast_hotkey", FunctionOne),
        ("item_tooltip_hotkey", AltAlone));

    [Fact]
    public void Assign_TakesTheKeyFromTheBindingThatHadIt()
    {
        var reassignment = BindingRules.Assign(Defaults, "speed_fast_hotkey", CtrlOne);

        Assert.Equal("spell_1", reassignment.DisplacedId);
        Assert.Equal(
            [new BindingChange("spell_1", null), new BindingChange("speed_fast_hotkey", CtrlOne)],
            reassignment.Changes);
    }

    [Fact]
    public void Assign_DisplacesNothingWhenTheKeyWasFree()
    {
        var altM = new KeyChord("M", MapKeys.Alt);

        var reassignment = BindingRules.Assign(Defaults, "gc_move_hotkey", altM);

        Assert.Null(reassignment.DisplacedId);
        Assert.Equal([new BindingChange("gc_move_hotkey", altM)], reassignment.Changes);
    }

    [Fact]
    public void Assign_DisplacesNothingWhenTheBindingAlreadyHasTheKey()
    {
        var reassignment = BindingRules.Assign(Defaults, "spell_1", CtrlOne);

        Assert.Null(reassignment.DisplacedId);
        Assert.Equal([new BindingChange("spell_1", CtrlOne)], reassignment.Changes);
    }

    [Fact]
    public void Assign_NeedsTheExactModifiersToDisplace()
    {
        var reassignment = BindingRules.Assign(
            Defaults,
            "gc_move_hotkey",
            new KeyChord("1", MapKeys.Ctrl | MapKeys.Shift));

        Assert.Null(reassignment.DisplacedId);
    }

    [Fact]
    public void Load_ReadsSavedKeysAndKeepsDefaultsForAnythingElse()
    {
        var saved = new Dictionary<string, string>
        {
            ["spell_1"] = "Alt+Q",
            ["spell_2"] = "",
            ["speed_fast_hotkey"] = "Banana+Q",
            ["no_such_hotkey"] = "F5",
        };

        var current = BindingRules.Load(Defaults, saved);

        Assert.Equal(new KeyChord("Q", MapKeys.Alt), current["spell_1"]);
        Assert.Null(current["spell_2"]);
        Assert.Equal(FunctionOne, current["speed_fast_hotkey"]);
        Assert.Equal(AltAlone, current["item_tooltip_hotkey"]);
        Assert.False(current.ContainsKey("no_such_hotkey"));
    }

    [Fact]
    public void Load_DropsAGammaKeySavedByAnEarlierVersion()
    {
        var current = BindingRules.Load(Defaults, new Dictionary<string, string> { ["toggle_gamma_hotkey"] = "F10" });

        Assert.False(current.ContainsKey("toggle_gamma_hotkey"));
    }

    [Fact]
    public void Load_ReadsAFileThatSavedEveryBinding()
    {
        var saved = new Dictionary<string, string>
        {
            ["spell_1"] = "Ctrl+1",
            ["spell_2"] = "Ctrl+2",
            ["speed_fast_hotkey"] = "F1",
            ["item_tooltip_hotkey"] = "Alt",
        };

        Assert.Equal(Defaults, BindingRules.Load(Defaults, saved));
    }

    [Fact]
    public void Saved_WritesOnlyWhatDiffersFromTheDefaults()
    {
        var current = Snapshot(
            ("spell_1", new KeyChord("Q", MapKeys.Alt)),
            ("spell_2", null),
            ("speed_fast_hotkey", FunctionOne),
            ("item_tooltip_hotkey", AltAlone));

        var saved = BindingRules.Saved(Defaults, current);

        Assert.Equal(new Dictionary<string, string> { ["spell_1"] = "Alt+Q", ["spell_2"] = "" }, saved);
    }

    [Fact]
    public void Saved_WritesABindingThatHasAKeyWhereTheDefaultHasNone()
    {
        var current = Snapshot(("gc_move_hotkey", new KeyChord("M", MapKeys.Alt)));

        Assert.Equal(
            new Dictionary<string, string> { ["gc_move_hotkey"] = "Alt+M" },
            BindingRules.Saved(Snapshot(("gc_move_hotkey", null)), current));
    }

    [Fact]
    public void SavedAndLoaded_RoundTrip()
    {
        var current = Snapshot(
            ("spell_1", new KeyChord("Keypad 5", MapKeys.None)),
            ("spell_2", null),
            ("speed_fast_hotkey", FunctionOne),
            ("item_tooltip_hotkey", AltAlone));

        Assert.Equal(current, BindingRules.Load(Defaults, BindingRules.Saved(Defaults, current)));
    }

    [Theory]
    [InlineData("Q", MapKeys.None, true)]
    [InlineData("Q", MapKeys.Shift, true)]
    [InlineData("7", MapKeys.None, true)]
    [InlineData("-", MapKeys.None, true)]
    [InlineData("/", MapKeys.None, true)]
    [InlineData("Space", MapKeys.None, true)]
    [InlineData("Keypad 5", MapKeys.None, true)]
    [InlineData("Keypad +", MapKeys.Shift, true)]
    [InlineData("Q", MapKeys.Ctrl, false)]
    [InlineData("Q", MapKeys.Alt, false)]
    [InlineData("Q", MapKeys.Shift | MapKeys.Ctrl, false)]
    [InlineData("F1", MapKeys.None, false)]
    [InlineData("Insert", MapKeys.None, false)]
    [InlineData("Tab", MapKeys.None, false)]
    [InlineData("PgUp", MapKeys.None, false)]
    [InlineData("Up", MapKeys.None, false)]
    [InlineData(KeyNames.Alt, MapKeys.None, false)]
    public void TypingHotkeys_CountsPlainCharactersWithoutCtrlOrAlt(
        string key,
        MapKeys modifiers,
        bool isPlainCharacter)
    {
        var current = Snapshot(("spell_1", new KeyChord(key, modifiers)));

        Assert.Equal(isPlainCharacter, BindingRules.TypingHotkeys(current).Count == 1);
    }

    [Fact]
    public void TypingHotkeys_ListsBindingsOnPlainCharactersInTheOptionsOrder()
    {
        var current = Snapshot(
            ("speed_fast_hotkey", new KeyChord("Q", MapKeys.None)),
            ("spell_1", new KeyChord("E", MapKeys.Shift)),
            ("spell_2", CtrlTwo));

        var typing = BindingRules.TypingHotkeys(current);

        Assert.Equal(["spell_1", "speed_fast_hotkey"], typing.Select(binding => binding.Id));
    }

    [Fact]
    public void TypingHotkeys_LeavesOutHeldBindings()
    {
        var current = Snapshot(("item_tooltip_hotkey", new KeyChord("Q", MapKeys.None)));

        Assert.Empty(BindingRules.TypingHotkeys(current));
    }

    [Fact]
    public void TypingHotkeysWarning_NamesEachBindingAndItsKeyWhileEnterToTalkIsOff()
    {
        var current = Snapshot(
            ("spell_1", new KeyChord("Q", MapKeys.None)),
            ("speed_fast_hotkey", new KeyChord("E", MapKeys.Shift)));

        Assert.Equal(
            "With enter to talk off, these hotkeys type into the chat line instead of running:\n"
            + "Spell 1: Q\n"
            + "Fast Mode: Shift+E",
            BindingRules.TypingHotkeysWarning(current, false));
    }

    [Fact]
    public void TypingHotkeysWarning_IsNoneWithEnterToTalkOn()
    {
        var current = Snapshot(("spell_1", new KeyChord("Q", MapKeys.None)));

        Assert.Null(BindingRules.TypingHotkeysWarning(current, true));
    }

    [Fact]
    public void TypingHotkeysWarning_IsNoneWhenNoHotkeyTypes()
    {
        Assert.Null(BindingRules.TypingHotkeysWarning(Defaults, false));
    }

    [Fact]
    public void TypingHotkeyWarning_MarksAPlainCharacterBindingWhileEnterToTalkIsOff()
    {
        var current = Snapshot(("spell_1", new KeyChord("Q", MapKeys.None)), ("spell_2", CtrlTwo));

        Assert.Equal(
            "With enter to talk off, this hotkey types into the chat line instead of running.",
            BindingRules.TypingHotkeyWarning(current, "spell_1", false));
        Assert.Null(BindingRules.TypingHotkeyWarning(current, "spell_2", false));
    }

    [Fact]
    public void TypingHotkeyWarning_IsNoneWithEnterToTalkOn()
    {
        var current = Snapshot(("spell_1", new KeyChord("Q", MapKeys.None)));

        Assert.Null(BindingRules.TypingHotkeyWarning(current, "spell_1", true));
    }

    [Fact]
    public void Names_SayNotSetForAnUnboundKey()
    {
        Assert.Equal("Not Set", BindingRules.LongName(null));
        Assert.Equal("", BindingRules.ShortName(null));
    }

    [Fact]
    public void Names_UseTheChordsNames()
    {
        Assert.Equal("Ctrl+1", BindingRules.LongName(CtrlOne));
        Assert.Equal("C-1", BindingRules.ShortName(CtrlOne));
    }

    private static IReadOnlyDictionary<string, KeyChord?> Snapshot(params (string Id, KeyChord? Chord)[] bindings) =>
        bindings.ToDictionary(binding => binding.Id, binding => binding.Chord);
}
