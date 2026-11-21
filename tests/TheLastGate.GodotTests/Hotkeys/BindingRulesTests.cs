using GdUnit4;
using TheLastGate.Hotkeys;
using TheLastGate.World;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Hotkeys;

[TestSuite]
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

    [TestCase]
    public void Assign_TakesTheKeyFromTheBindingThatHadIt()
    {
        var reassignment = BindingRules.Assign(Defaults, "speed_fast_hotkey", CtrlOne);

        AssertThat(reassignment.DisplacedId).IsEqual("spell_1");
        AssertThat(reassignment.Changes)
            .ContainsExactly(new BindingChange("spell_1", null), new BindingChange("speed_fast_hotkey", CtrlOne));
    }

    [TestCase]
    public void Assign_DisplacesNothingWhenTheKeyWasFree()
    {
        var altM = new KeyChord("M", MapKeys.Alt);

        var reassignment = BindingRules.Assign(Defaults, "gc_move_hotkey", altM);

        AssertThat(reassignment.DisplacedId).IsNull();
        AssertThat(reassignment.Changes).ContainsExactly(new BindingChange("gc_move_hotkey", altM));
    }

    [TestCase]
    public void Assign_DisplacesNothingWhenTheBindingAlreadyHasTheKey()
    {
        var reassignment = BindingRules.Assign(Defaults, "spell_1", CtrlOne);

        AssertThat(reassignment.DisplacedId).IsNull();
        AssertThat(reassignment.Changes).ContainsExactly(new BindingChange("spell_1", CtrlOne));
    }

    [TestCase]
    public void Assign_NeedsTheExactModifiersToDisplace()
    {
        var reassignment = BindingRules.Assign(
            Defaults,
            "gc_move_hotkey",
            new KeyChord("1", MapKeys.Ctrl | MapKeys.Shift));

        AssertThat(reassignment.DisplacedId).IsNull();
    }

    [TestCase]
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

        AssertThat(current["spell_1"]).IsEqual(new KeyChord("Q", MapKeys.Alt));
        AssertThat(current["spell_2"]).IsNull();
        AssertThat(current["speed_fast_hotkey"]).IsEqual(FunctionOne);
        AssertThat(current["item_tooltip_hotkey"]).IsEqual(AltAlone);
        AssertThat(current.ContainsKey("no_such_hotkey")).IsFalse();
    }

    [TestCase]
    public void Load_DropsAGammaKeySavedByAnEarlierVersion()
    {
        var current = BindingRules.Load(Defaults, new Dictionary<string, string> { ["toggle_gamma_hotkey"] = "F10" });

        AssertThat(current.ContainsKey("toggle_gamma_hotkey")).IsFalse();
    }

    [TestCase]
    public void Load_ReadsAFileThatSavedEveryBinding()
    {
        var saved = new Dictionary<string, string>
        {
            ["spell_1"] = "Ctrl+1",
            ["spell_2"] = "Ctrl+2",
            ["speed_fast_hotkey"] = "F1",
            ["item_tooltip_hotkey"] = "Alt",
        };

        AssertThat(BindingRules.Load(Defaults, saved)).IsEqual(Defaults);
    }

    [TestCase]
    public void Saved_WritesOnlyWhatDiffersFromTheDefaults()
    {
        var current = Snapshot(
            ("spell_1", new KeyChord("Q", MapKeys.Alt)),
            ("spell_2", null),
            ("speed_fast_hotkey", FunctionOne),
            ("item_tooltip_hotkey", AltAlone));

        var saved = BindingRules.Saved(Defaults, current);

        AssertThat(saved).IsEqual(new Dictionary<string, string> { ["spell_1"] = "Alt+Q", ["spell_2"] = "" });
    }

    [TestCase]
    public void Saved_WritesABindingThatHasAKeyWhereTheDefaultHasNone()
    {
        var current = Snapshot(("gc_move_hotkey", new KeyChord("M", MapKeys.Alt)));

        AssertThat(BindingRules.Saved(Snapshot(("gc_move_hotkey", null)), current))
            .IsEqual(new Dictionary<string, string> { ["gc_move_hotkey"] = "Alt+M" });
    }

    [TestCase]
    public void SavedAndLoaded_RoundTrip()
    {
        var current = Snapshot(
            ("spell_1", new KeyChord("Keypad 5", MapKeys.None)),
            ("spell_2", null),
            ("speed_fast_hotkey", FunctionOne),
            ("item_tooltip_hotkey", AltAlone));

        AssertThat(BindingRules.Load(Defaults, BindingRules.Saved(Defaults, current))).IsEqual(current);
    }

    [TestCase("Q", MapKeys.None, true)]
    [TestCase("Q", MapKeys.Shift, true)]
    [TestCase("7", MapKeys.None, true)]
    [TestCase("-", MapKeys.None, true)]
    [TestCase("/", MapKeys.None, true)]
    [TestCase("Space", MapKeys.None, true)]
    [TestCase("Keypad 5", MapKeys.None, true)]
    [TestCase("Keypad +", MapKeys.Shift, true)]
    [TestCase("Q", MapKeys.Ctrl, false)]
    [TestCase("Q", MapKeys.Alt, false)]
    [TestCase("Q", MapKeys.Shift | MapKeys.Ctrl, false)]
    [TestCase("F1", MapKeys.None, false)]
    [TestCase("Insert", MapKeys.None, false)]
    [TestCase("Tab", MapKeys.None, false)]
    [TestCase("PgUp", MapKeys.None, false)]
    [TestCase("Up", MapKeys.None, false)]
    [TestCase(KeyNames.Alt, MapKeys.None, false)]
    public void TypingHotkeys_CountsPlainCharactersWithoutCtrlOrAlt(
        string key,
        MapKeys modifiers,
        bool isPlainCharacter)
    {
        var current = Snapshot(("spell_1", new KeyChord(key, modifiers)));

        AssertThat(BindingRules.TypingHotkeys(current).Count == 1).IsEqual(isPlainCharacter);
    }

    [TestCase]
    public void TypingHotkeys_ListsBindingsOnPlainCharactersInTheOptionsOrder()
    {
        var current = Snapshot(
            ("speed_fast_hotkey", new KeyChord("Q", MapKeys.None)),
            ("spell_1", new KeyChord("E", MapKeys.Shift)),
            ("spell_2", CtrlTwo));

        var typing = BindingRules.TypingHotkeys(current);

        AssertThat(typing.Select(binding => binding.Id)).ContainsExactly("spell_1", "speed_fast_hotkey");
    }

    [TestCase]
    public void TypingHotkeys_LeavesOutHeldBindings()
    {
        var current = Snapshot(("item_tooltip_hotkey", new KeyChord("Q", MapKeys.None)));

        AssertThat(BindingRules.TypingHotkeys(current)).IsEmpty();
    }

    [TestCase]
    public void TypingHotkeysWarning_NamesEachBindingAndItsKeyWhileEnterToTalkIsOff()
    {
        var current = Snapshot(
            ("spell_1", new KeyChord("Q", MapKeys.None)),
            ("speed_fast_hotkey", new KeyChord("E", MapKeys.Shift)));

        AssertThat(BindingRules.TypingHotkeysWarning(current, false))
            .IsEqual(
                "With enter to talk off, these hotkeys type into the chat line instead of running:\n"
                + "Spell 1: Q\n"
                + "Fast Mode: Shift+E");
    }

    [TestCase]
    public void TypingHotkeysWarning_IsNoneWithEnterToTalkOn()
    {
        var current = Snapshot(("spell_1", new KeyChord("Q", MapKeys.None)));

        AssertThat(BindingRules.TypingHotkeysWarning(current, true)).IsNull();
    }

    [TestCase]
    public void TypingHotkeysWarning_IsNoneWhenNoHotkeyTypes()
    {
        AssertThat(BindingRules.TypingHotkeysWarning(Defaults, false)).IsNull();
    }

    [TestCase]
    public void TypingHotkeyWarning_MarksAPlainCharacterBindingWhileEnterToTalkIsOff()
    {
        var current = Snapshot(("spell_1", new KeyChord("Q", MapKeys.None)), ("spell_2", CtrlTwo));

        AssertThat(BindingRules.TypingHotkeyWarning(current, "spell_1", false))
            .IsEqual("With enter to talk off, this hotkey types into the chat line instead of running.");
        AssertThat(BindingRules.TypingHotkeyWarning(current, "spell_2", false)).IsNull();
    }

    [TestCase]
    public void TypingHotkeyWarning_IsNoneWithEnterToTalkOn()
    {
        var current = Snapshot(("spell_1", new KeyChord("Q", MapKeys.None)));

        AssertThat(BindingRules.TypingHotkeyWarning(current, "spell_1", true)).IsNull();
    }

    [TestCase]
    public void Names_SayNotSetForAnUnboundKey()
    {
        AssertThat(BindingRules.LongName(null)).IsEqual("Not Set");
        AssertThat(BindingRules.ShortName(null)).IsEqual("");
    }

    [TestCase]
    public void Names_UseTheChordsNames()
    {
        AssertThat(BindingRules.LongName(CtrlOne)).IsEqual("Ctrl+1");
        AssertThat(BindingRules.ShortName(CtrlOne)).IsEqual("C-1");
    }

    private static IReadOnlyDictionary<string, KeyChord?> Snapshot(params (string Id, KeyChord? Chord)[] bindings) =>
        bindings.ToDictionary(binding => binding.Id, binding => binding.Chord);
}
