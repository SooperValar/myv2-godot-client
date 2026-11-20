using GdUnit4;
using TheLastGate.Map;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Map;

[TestSuite]
public class LabelLayoutTests
{
    private const float Anchor = 100;
    private const float Clearance = 8;
    private const float Width = 40;
    private const float Height = 10;

    [TestCase]
    public void PlaceBeside_PutsALabelInFreeSpaceToTheRight()
    {
        var layout = new LabelLayout();

        var placed = layout.PlaceBeside(Anchor, Anchor, Clearance, Width, Height);

        AssertThat(placed).IsEqual(new LabelRect(108, 95, 148, 105));
        AssertThat(layout.Count).IsEqual(1);
    }

    [TestCase]
    public void PlaceBeside_FallsBackToTheLeftWhenTheRightIsTaken()
    {
        var layout = new LabelLayout();
        layout.Reserve(new LabelRect(110, 90, 200, 110));

        var placed = layout.PlaceBeside(Anchor, Anchor, Clearance, Width, Height);

        AssertThat(placed?.Right).IsEqual(92);
        AssertThat(placed?.Top).IsEqual(95);
    }

    [TestCase]
    public void PlaceBeside_KeepsASecondLabelAtTheSameAnchorOffTheFirst()
    {
        var layout = new LabelLayout();

        var first = layout.PlaceBeside(Anchor, Anchor, Clearance, Width, Height)!.Value;
        var second = layout.PlaceBeside(Anchor, Anchor, Clearance, Width, Height)!.Value;

        AssertThat(first.Overlaps(second)).IsFalse();
    }

    [TestCase]
    public void PlaceBeside_GivesUpWhenEverySpotIsTaken()
    {
        var layout = new LabelLayout();
        layout.Reserve(new LabelRect(0, 0, 300, 300));

        AssertThat(layout.PlaceBeside(150, 150, Clearance, Width, Height)).IsNull();
        AssertThat(layout.Count).IsEqual(1);
    }

    [TestCase]
    public void PlaceBeside_TreatsTouchingEdgesAsFree()
    {
        var layout = new LabelLayout();
        layout.Reserve(new LabelRect(148, 0, 200, 200));

        AssertThat(layout.PlaceBeside(Anchor, Anchor, Clearance, Width, Height)?.Left).IsEqual(108);
    }

    [TestCase]
    public void PlaceBeside_FindsSixSpotsThatAllClearAPinMarker()
    {
        var layout = new LabelLayout();
        layout.Reserve(new LabelRect(96, 96, 105, 105));

        for (var attempt = 0; attempt < 6; attempt++)
        {
            AssertThat(layout.PlaceBeside(Anchor, Anchor, Clearance, Width, Height)).IsNotNull();
        }
    }

    [TestCase]
    public void AFullLayout_RefusesMoreWithoutStoringThem()
    {
        var layout = new LabelLayout(1);

        AssertThat(layout.Reserve(new LabelRect(0, 0, 1, 1))).IsTrue();
        AssertThat(layout.Reserve(new LabelRect(5, 5, 6, 6))).IsFalse();
        AssertThat(layout.PlaceBeside(Anchor, Anchor, Clearance, Width, Height)).IsNull();
        AssertThat(layout.Count).IsEqual(1);
    }
}
