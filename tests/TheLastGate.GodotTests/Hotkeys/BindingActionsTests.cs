using GdUnit4;
using Godot;
using TheLastGate.Hotkeys;
using TheLastGate.World;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Hotkeys;

[TestSuite]
[RequireGodotRuntime]
public class BindingActionsTests
{
    private const string SpellKeys = "12345QWERTASDFGZXCVB";

    private static readonly Dictionary<string, string?> ShippedBindings = new()
    {
        ["speed_fast_hotkey"] = "F1",
        ["speed_normal_hotkey"] = "F2",
        ["speed_slow_hotkey"] = "F3",
        ["toggle_percent_hotkey"] = "F4",
        ["toggle_stat_base_hotkey"] = "F5",
        ["toggle_hide_sprite_hotkey"] = "F6",
        ["toggle_names_hotkey"] = "F7",
        ["toggle_health_bars_hotkey"] = "F8",
        ["toggle_options_hotkey"] = "F9",
        ["toggle_gamma_hotkey"] = "F10",
        ["reset_window_size_hotkey"] = "Shift+F10",
        ["toggle_planner_hotkey"] = "Ctrl+P",
        ["toggle_worldmap_hotkey"] = null,
        ["toggle_session_stats_hotkey"] = null,
        ["item_tooltip_hotkey"] = "Alt",
        ["fightback_hotkey"] = null,
        ["swap_gear_hotkey"] = "Insert",
        ["swap_position_hotkey"] = null,
        ["who_hotkey"] = null,
        ["area_use_hotkey"] = null,
        ["display_debug_information_hotkey"] = "F11",
        ["exit_hotkey"] = "F12",
        ["my_buff_hotkey"] = null,
        ["gc_offense_hotkey"] = null,
        ["gc_defense_hotkey"] = null,
        ["gc_passive_hotkey"] = null,
        ["gc_wait_hotkey"] = null,
        ["gc_follow_hotkey"] = null,
        ["gc_move_hotkey"] = null,
        ["gc_buffs_hotkey"] = null,
        ["gc_transfer_hotkey"] = null,
    };

    [AfterTest]
    public void RestoreTheDefaults() => BindingActions.ResetAll();

    [TestCase]
    public void Defaults_MatchTheShippedBindings()
    {
        var expected = SpellKeys
            .Select((key, slot) => (Id: Bindings.SpellId(slot), Keys: $"Ctrl+{key}"))
            .Concat(ShippedBindings.Select(entry => (Id: entry.Key, Keys: entry.Value ?? "")))
            .ToDictionary(entry => entry.Id, entry => entry.Keys);

        var defaults = Named(BindingActions.Defaults());

        AssertThat(defaults).IsEqual(expected);
        AssertThat(defaults.Keys.Order().ToArray())
            .IsEqual(Bindings.All.Select(binding => binding.Id).Order().ToArray());
    }

    [TestCase]
    public void Current_StartsAtTheDefaults() =>
        AssertThat(Named(BindingActions.Current())).IsEqual(Named(BindingActions.Defaults()));

    [TestCase("Ctrl+1")]
    [TestCase("Shift+F10")]
    [TestCase("Shift+Ctrl+Alt+1")]
    [TestCase("Keypad 5")]
    [TestCase("Keypad +")]
    [TestCase("PgUp")]
    [TestCase("`")]
    [TestCase("Space")]
    [TestCase("Alt")]
    public void ToEventAndToChord_RoundTrip(string keys)
    {
        var chord = KeyChord.Parse(keys)!.Value;

        AssertThat(BindingActions.ToChord(BindingActions.ToEvent(chord))).IsEqual(chord);
    }

    [TestCase]
    public void ToChord_LeavesAModifierPressedAloneOutOfItsOwnChord()
    {
        var altDown = new InputEventKey { Keycode = Key.Alt, AltPressed = true, Pressed = true };

        AssertThat(BindingActions.ToChord(altDown)).IsEqual(new KeyChord(KeyNames.Alt, MapKeys.None));
    }

    [TestCase]
    public void ToChord_IsNullForAKeyThatCantBeBound()
    {
        var enter = new InputEventKey { Keycode = Key.Enter, Pressed = true };

        AssertThat(BindingActions.ToChord(enter)).IsNull();
    }

    [TestCase]
    public void AHotkeyMatchesOnlyItsExactModifiers()
    {
        var ctrlOne = new InputEventKey { Keycode = Key.Key1, CtrlPressed = true, Pressed = true };
        var ctrlShiftOne = new InputEventKey
        {
            Keycode = Key.Key1, CtrlPressed = true, ShiftPressed = true, Pressed = true,
        };

        AssertThat(InputMap.EventIsAction(ctrlOne, "spell_1", true)).IsTrue();
        AssertThat(InputMap.EventIsAction(ctrlShiftOne, "spell_1", true)).IsFalse();
    }

    [TestCase]
    public void TheItemTooltipKeyIsHeldWhileAltIsDown()
    {
        Input.ParseInputEvent(new InputEventKey { Keycode = Key.Alt, AltPressed = true, Pressed = true });
        Input.FlushBufferedEvents();
        var heldWhileDown = Input.IsActionPressed("item_tooltip_hotkey");

        Input.ParseInputEvent(new InputEventKey { Keycode = Key.Alt, Pressed = false });
        Input.FlushBufferedEvents();

        AssertThat(heldWhileDown).IsTrue();
        AssertThat(Input.IsActionPressed("item_tooltip_hotkey")).IsFalse();
    }

    [TestCase]
    public void Apply_ChangesAndClearsBindings()
    {
        var altQ = new KeyChord("Q", MapKeys.Alt);

        BindingActions.Apply([new BindingChange("spell_1", altQ), new BindingChange("spell_2", null)]);

        AssertThat(BindingActions.Current()["spell_1"]).IsEqual(altQ);
        AssertThat(BindingActions.Current()["spell_2"]).IsNull();
        AssertThat(InputMap.ActionGetEvents("spell_2")).IsEmpty();
    }

    [TestCase]
    public void ResetAll_RestoresEveryDefault()
    {
        BindingActions.Apply([new BindingChange("spell_1", null)]);

        BindingActions.ResetAll();

        AssertThat(Named(BindingActions.Current())).IsEqual(Named(BindingActions.Defaults()));
    }

    private static Dictionary<string, string> Named(IReadOnlyDictionary<string, KeyChord?> bindings) =>
        bindings.ToDictionary(binding => binding.Key, binding => binding.Value?.LongName ?? "");
}
