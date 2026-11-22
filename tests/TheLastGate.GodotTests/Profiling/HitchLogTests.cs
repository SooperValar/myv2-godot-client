using GdUnit4;
using TheLastGate.Profiling;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Profiling;

[TestSuite]
public class HitchLogTests
{
    [TestCase]
    public void AFrameUnderTheThreshold_IsNotLogged() =>
        AssertThat(HitchLog.Describe(new FrameSummary(49.9, [("world.refresh", 40)]))).IsNull();

    [TestCase]
    public void AHitch_ListsItsSectionsHighestFirst()
    {
        var frame = new FrameSummary(63, [("tick.process", 41.2), ("world.refresh", 9.8)]);

        AssertThat(HitchLog.Describe(frame)).IsEqual("Hitch 63.0 ms: tick.process 41.2, world.refresh 9.8");
    }

    [TestCase]
    public void AHitch_ListsOnlyItsFiveSlowestSections()
    {
        var frame = new FrameSummary(
            80,
            [("a.one", 6), ("b.two", 5), ("c.three", 4), ("d.four", 3), ("e.five", 2), ("f.six", 1)]);

        AssertThat(HitchLog.Describe(frame))
            .IsEqual("Hitch 80.0 ms: a.one 6.0, b.two 5.0, c.three 4.0, d.four 3.0, e.five 2.0");
    }
}
