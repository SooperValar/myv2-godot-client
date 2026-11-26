using GdUnit4;
using TheLastGate.Hotkeys;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Hotkeys;

[TestSuite]
public class HotkeyCommandsTests
{
    [TestCase("fightback_hotkey", "/fightback")]
    [TestCase("who_hotkey", "/who")]
    [TestCase("my_buff_hotkey", "/buffs")]
    [TestCase("gc_offense_hotkey", "Offense")]
    [TestCase("gc_defense_hotkey", "Defense")]
    [TestCase("gc_passive_hotkey", "Passive")]
    [TestCase("gc_wait_hotkey", "Wait")]
    [TestCase("gc_follow_hotkey", "Follow")]
    [TestCase("gc_move_hotkey", "Move")]
    [TestCase("gc_buffs_hotkey", "Buffs")]
    [TestCase("gc_transfer_hotkey", "Transfer")]
    public void Says_WhatTheCurrentClientSaysForTheHotkey(string id, string line)
    {
        AssertThat(HotkeyCommands.Says(id)).IsEqual(line);
    }

    [TestCase("spell_1")]
    [TestCase("swap_gear_hotkey")]
    [TestCase("exit_hotkey")]
    [TestCase("unknown_hotkey")]
    public void Says_NothingForHotkeysThatDoSomethingElse(string id)
    {
        AssertThat(HotkeyCommands.Says(id)).IsNull();
    }

    [TestCase(0, "#gca")]
    [TestCase(1234, "#gca 1234")]
    public void GcAttack_NamesTheSelectedCharacter(int selected, string line)
    {
        AssertThat(GcAttack.Line(selected)).IsEqual(line);
    }

    [TestCase("#gca", "#gca 77")]
    [TestCase("/GCA ", "#gca 77")]
    [TestCase("#gcattack", "#gca 77")]
    [TestCase("#gca 5", "#gca 5")]
    [TestCase("#gc", "#gc")]
    [TestCase("hello", "hello")]
    public void GcAttack_CompletesABareTypedCommand(string typed, string sent)
    {
        AssertThat(GcAttack.Complete(typed, 77)).IsEqual(sent);
    }
}
