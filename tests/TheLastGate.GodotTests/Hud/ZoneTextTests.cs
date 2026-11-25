using GdUnit4;
using TheLastGate.Hud;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Hud;

[TestSuite]
public class ZoneTextTests
{
    [TestCase]
    public void Fitted_KeepsAShortNameWhole()
    {
        AssertThat(ZoneText.Fitted("Aston, Market")).IsEqual("Aston, Market");
    }

    [TestCase]
    public void Fitted_ShowsTheInnermostZoneWhenAllOfThemDontFit()
    {
        AssertThat(ZoneText.Fitted("Stonehold, Golem Foundry, Hall")).IsEqual("Hall");
    }

    [TestCase]
    public void Fitted_CutsAnInnermostZoneThatIsStillTooLong()
    {
        AssertThat(ZoneText.Fitted("Monastery of the Still Hand")).IsEqual("Monastery of the Sti");
    }
}
