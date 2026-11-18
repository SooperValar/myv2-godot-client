using TheLastGate.Map;

namespace TheLastGate.Tests;

public class LabelLayoutTests
{
    private const float Anchor = 100;
    private const float Clearance = 8;
    private const float Width = 40;
    private const float Height = 10;

    [Fact]
    public void PlaceBeside_PutsALabelInFreeSpaceToTheRight()
    {
        var layout = new LabelLayout();

        var placed = layout.PlaceBeside(Anchor, Anchor, Clearance, Width, Height);

        Assert.Equal(new LabelRect(108, 95, 148, 105), placed);
        Assert.Equal(1, layout.Count);
    }

    [Fact]
    public void PlaceBeside_FallsBackToTheLeftWhenTheRightIsTaken()
    {
        var layout = new LabelLayout();
        layout.Reserve(new LabelRect(110, 90, 200, 110));

        var placed = layout.PlaceBeside(Anchor, Anchor, Clearance, Width, Height);

        Assert.Equal(92, placed?.Right);
        Assert.Equal(95, placed?.Top);
    }

    [Fact]
    public void PlaceBeside_KeepsASecondLabelAtTheSameAnchorOffTheFirst()
    {
        var layout = new LabelLayout();

        var first = layout.PlaceBeside(Anchor, Anchor, Clearance, Width, Height)!.Value;
        var second = layout.PlaceBeside(Anchor, Anchor, Clearance, Width, Height)!.Value;

        Assert.False(first.Overlaps(second));
    }

    [Fact]
    public void PlaceBeside_GivesUpWhenEverySpotIsTaken()
    {
        var layout = new LabelLayout();
        layout.Reserve(new LabelRect(0, 0, 300, 300));

        Assert.Null(layout.PlaceBeside(150, 150, Clearance, Width, Height));
        Assert.Equal(1, layout.Count);
    }

    [Fact]
    public void PlaceBeside_TreatsTouchingEdgesAsFree()
    {
        var layout = new LabelLayout();
        layout.Reserve(new LabelRect(148, 0, 200, 200));

        Assert.Equal(108, layout.PlaceBeside(Anchor, Anchor, Clearance, Width, Height)?.Left);
    }

    [Fact]
    public void PlaceBeside_FindsSixSpotsThatAllClearAPinMarker()
    {
        var layout = new LabelLayout();
        layout.Reserve(new LabelRect(96, 96, 105, 105));

        for (var attempt = 0; attempt < 6; attempt++)
        {
            Assert.NotNull(layout.PlaceBeside(Anchor, Anchor, Clearance, Width, Height));
        }
    }

    [Fact]
    public void AFullLayout_RefusesMoreWithoutStoringThem()
    {
        var layout = new LabelLayout(1);

        Assert.True(layout.Reserve(new LabelRect(0, 0, 1, 1)));
        Assert.False(layout.Reserve(new LabelRect(5, 5, 6, 6)));
        Assert.Null(layout.PlaceBeside(Anchor, Anchor, Clearance, Width, Height));
        Assert.Equal(1, layout.Count);
    }
}
