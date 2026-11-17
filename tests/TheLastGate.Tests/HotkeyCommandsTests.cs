using TheLastGate.Hotkeys;

namespace TheLastGate.Tests;

public class HotkeyCommandsTests
{
    [Theory]
    [InlineData("fightback_hotkey", "/fightback")]
    [InlineData("swap_position_hotkey", "/swap")]
    [InlineData("who_hotkey", "/who")]
    [InlineData("my_buff_hotkey", "/buffs")]
    [InlineData("gc_offense_hotkey", "Offense")]
    [InlineData("gc_defense_hotkey", "Defense")]
    [InlineData("gc_passive_hotkey", "Passive")]
    [InlineData("gc_wait_hotkey", "Wait")]
    [InlineData("gc_follow_hotkey", "Follow")]
    [InlineData("gc_move_hotkey", "Move")]
    [InlineData("gc_buffs_hotkey", "/gcb")]
    [InlineData("gc_transfer_hotkey", "transfer")]
    public void Says_WhatTheCurrentClientSaysForTheHotkey(string id, string line)
    {
        Assert.Equal(line, HotkeyCommands.Says(id));
    }

    [Theory]
    [InlineData("spell_1")]
    [InlineData("swap_gear_hotkey")]
    [InlineData("exit_hotkey")]
    [InlineData("unknown_hotkey")]
    public void Says_NothingForHotkeysThatDoSomethingElse(string id)
    {
        Assert.Null(HotkeyCommands.Says(id));
    }

    [Fact]
    public void Says_SomethingForEveryGuildCommandBinding()
    {
        var guildCommands = Bindings.All.Where(binding => binding.Category == BindingCategory.GuildCommands);

        Assert.All(guildCommands, binding => Assert.NotNull(HotkeyCommands.Says(binding.Id)));
    }
}
