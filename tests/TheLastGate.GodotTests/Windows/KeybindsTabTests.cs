using GdUnit4;
using TheLastGate.Characters;
using TheLastGate.Hotkeys;
using TheLastGate.Windows;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Windows;

[TestSuite]
public class KeybindsTabTests
{
    private CharacterSettings settings = new();

    [BeforeTest]
    public void StartWithTheDefaults() => settings = new CharacterSettings();

    [TestCase]
    public void Sections_FollowTheCurrentClientsCategoryOrder()
    {
        var headings = KeybindsTab.Sections(settings).Select(section => section.Heading);

        AssertThat(headings)
            .ContainsExactly(
                "Spell Hotkeys",
                "Speed Hotkeys",
                "World Hotkeys",
                "Window Hotkeys",
                "General Hotkeys",
                "GC Commands");
    }

    [TestCase]
    public void Sections_ListEveryOtherBindingByName()
    {
        var speed = KeybindsTab.Sections(settings).Single(section => section.Heading == "Speed Hotkeys");

        AssertThat(speed.Rows)
            .ContainsExactly(
                new KeybindRow("speed_fast_hotkey", "Fast Mode"),
                new KeybindRow("speed_normal_hotkey", "Normal Mode"),
                new KeybindRow("speed_slow_hotkey", "Slow Mode"));
    }

    [TestCase]
    public void Sections_ShowOnlyTheSpellsOnTheHotbarWithTheirButtonNames()
    {
        settings.HotbarActions[0] = 37;
        settings.HotbarNames[0] = "Bless";
        settings.HotbarActions[5] = 40;
        settings.HotbarNames[5] = "Enhance";

        var spells = KeybindsTab.Sections(settings)[0];

        AssertThat(spells.Rows)
            .ContainsExactly(
                new KeybindRow("spell_1", "Spell 1 (Bless)"),
                new KeybindRow("spell_6", "Spell 6 (Enhance)"));
    }

    [TestCase]
    public void Sections_KeepTheSpellHeadingWhenTheHotbarIsEmpty()
    {
        var spells = KeybindsTab.Sections(settings)[0];

        AssertThat(spells.Heading).IsEqual("Spell Hotkeys");
        AssertThat(spells.Rows).IsEmpty();
    }

    [TestCase]
    public void Sections_ListEveryBindingThatIsNotASpell()
    {
        var rows = KeybindsTab.Sections(settings).SelectMany(section => section.Rows).Count();

        AssertThat(rows).IsEqual(Bindings.All.Count - Bindings.SpellCount);
    }
}
