using TheLastGate.Characters;
using TheLastGate.Hotkeys;
using TheLastGate.Windows;

namespace TheLastGate.Tests;

public class KeybindsTabTests
{
    private readonly CharacterSettings settings = new();

    [Fact]
    public void Sections_FollowTheCurrentClientsCategoryOrder()
    {
        var headings = KeybindsTab.Sections(settings).Select(section => section.Heading);

        Assert.Equal(
            ["Spell Hotkeys", "Speed Hotkeys", "World Hotkeys", "Window Hotkeys", "General Hotkeys", "GC Commands"],
            headings);
    }

    [Fact]
    public void Sections_ListEveryOtherBindingByName()
    {
        var speed = KeybindsTab.Sections(settings).Single(section => section.Heading == "Speed Hotkeys");

        Assert.Equal(
            [
                new KeybindRow("speed_fast_hotkey", "Fast Mode"),
                new KeybindRow("speed_normal_hotkey", "Normal Mode"),
                new KeybindRow("speed_slow_hotkey", "Slow Mode"),
            ],
            speed.Rows);
    }

    [Fact]
    public void Sections_ShowOnlyTheSpellsOnTheHotbarWithTheirButtonNames()
    {
        settings.HotbarActions[0] = 37;
        settings.HotbarNames[0] = "Bless";
        settings.HotbarActions[5] = 40;
        settings.HotbarNames[5] = "Enhance";

        var spells = KeybindsTab.Sections(settings)[0];

        Assert.Equal(
            [new KeybindRow("spell_1", "Spell 1 (Bless)"), new KeybindRow("spell_6", "Spell 6 (Enhance)")],
            spells.Rows);
    }

    [Fact]
    public void Sections_KeepTheSpellHeadingWhenTheHotbarIsEmpty()
    {
        var spells = KeybindsTab.Sections(settings)[0];

        Assert.Equal("Spell Hotkeys", spells.Heading);
        Assert.Empty(spells.Rows);
    }

    [Fact]
    public void Sections_ListEveryBindingThatIsNotASpell()
    {
        var rows = KeybindsTab.Sections(settings).SelectMany(section => section.Rows).Count();

        Assert.Equal(Bindings.All.Count - Bindings.SpellCount, rows);
    }
}
