using GdUnit4;
using Godot;
using TheLastGate.Game;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Game;

[TestSuite]
[RequireGodotRuntime]
public class NavigationKeysTests
{
    private readonly NavigationKeys navigationKeys = new();

    [AfterTest]
    public void PutTheKeysBack() => navigationKeys.TurnOn();

    [TestCase]
    public void TurnOff_TakesEveryNavigationKeyAway()
    {
        navigationKeys.TurnOff();

        AssertThat(InputMap.ActionGetEvents("ui_focus_next")).IsEmpty();
        AssertThat(InputMap.ActionGetEvents("ui_up")).IsEmpty();
        AssertThat(InputMap.ActionGetEvents("ui_accept")).IsEmpty();
    }

    [TestCase]
    public void TurnOn_PutsTheKeysBackAsTheyWere()
    {
        var before = InputMap.ActionGetEvents("ui_accept").Count;

        navigationKeys.TurnOff();
        navigationKeys.TurnOff();
        navigationKeys.TurnOn();

        AssertThat(InputMap.ActionGetEvents("ui_accept").Count).IsEqual(before);
        AssertThat(before).IsGreater(0);
    }

    [TestCase]
    public void TurnOff_LeavesTheTextEditingKeys()
    {
        navigationKeys.TurnOff();

        AssertThat(InputMap.ActionGetEvents("ui_text_submit")).IsNotEmpty();
        AssertThat(InputMap.ActionGetEvents("ui_cancel")).IsNotEmpty();
    }
}
