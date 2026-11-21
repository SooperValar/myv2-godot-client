using GdUnit4;
using TheLastGate.Hotkeys;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Hotkeys;

[TestSuite]
public class HotkeyCommandsTests
{
    [TestCase("fightback_hotkey", "/fightback")]
    [TestCase("swap_position_hotkey", "/swap")]
    [TestCase("who_hotkey", "/who")]
    [TestCase("my_buff_hotkey", "/buffs")]
    [TestCase("gc_offense_hotkey", "Offense")]
    [TestCase("gc_defense_hotkey", "Defense")]
    [TestCase("gc_passive_hotkey", "Passive")]
    [TestCase("gc_wait_hotkey", "Wait")]
    [TestCase("gc_follow_hotkey", "Follow")]
    [TestCase("gc_move_hotkey", "Move")]
    [TestCase("gc_buffs_hotkey", "/gcb")]
    [TestCase("gc_transfer_hotkey", "transfer")]
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

    [TestCase]
    public void Says_SomethingForEveryGuildCommandBinding()
    {
        var guildCommands = Bindings.All.Where(binding => binding.Category == BindingCategory.GuildCommands);

        foreach (var binding in guildCommands)
        {
            AssertThat(HotkeyCommands.Says(binding.Id)).IsNotNull();
        }
    }
}
