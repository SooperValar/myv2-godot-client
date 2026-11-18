using TheLastGate.Map;
using TheLastGate.Notes;

namespace TheLastGate.Tests;

public class MinimapOverlayTests
{
    private const int Size = MinimapView.Pixels;
    private const int BytesPerPixel = 4;

    private static readonly PathSegment[] Segments =
    [
        new(2000, 5, 3, 1900), new(1900, 1700, 20, 30), new(0, 0, 300, 300), new(700, 2047, 300, 0),
        new(350, 410, 360, 1200), new(420, 900, 330, 905), new(400, 400, 401, 450), new(380, 390, 381, 395),
        new(200, 2047, 600, 0), new(400, 420, 400, 420), new(0, 2047, 2047, 0), new(1500, 470, 0, 431),
    ];

    private readonly NoteSet notes = new() { ShowOnMinimap = true };
    private readonly byte[] rgba = new byte[Size * Size * BytesPerPixel];

    [Fact]
    public void Paint_DrawsNothingUnlessNotesAreShownOnTheMinimap()
    {
        notes.AddPoint(notes.AddCategory("Pins", 0), NoteKind.Pin, 10, 20, "pin");
        notes.ShowOnMinimap = false;

        MinimapOverlay.Paint(notes, rgba, 0, 0, 1);

        Assert.Equal(0, CountPainted());
    }

    [Fact]
    public void Paint_DrawsAPinAsAFiveByFiveDiamond()
    {
        notes.AddPoint(notes.AddCategory("Pins", 0), NoteKind.Pin, 10, 20, "pin");

        MinimapOverlay.Paint(notes, rgba, 0, 0, 1);

        for (var offsetX = -2; offsetX <= 2; offsetX++)
        {
            for (var offsetY = -2; offsetY <= 2; offsetY++)
            {
                Assert.Equal(Math.Abs(offsetX) + Math.Abs(offsetY) <= 2, IsPainted(10 + offsetX, 20 + offsetY));
            }
        }

        Assert.Equal(13, CountPainted());
    }

    [Fact]
    public void Paint_DrawsAMagnifiedSegmentOneDottedPixelWide()
    {
        notes.AddPath(notes.AddCategory("Paths", 0), [new PathSegment(0, 0, 0, 3)], "path");

        MinimapOverlay.Paint(notes, rgba, 0, 0, 2);

        for (var column = 1; column <= 7; column++)
        {
            Assert.Equal(column % 2 == 1, IsPainted(1, column));
        }

        Assert.Equal(4, CountPainted());
    }

    [Fact]
    public void Paint_SkipsHiddenCategoriesAndText()
    {
        var shown = notes.AddCategory("Shown", 0);
        var hidden = notes.AddCategory("Hidden", 1);
        notes.AddPoint(shown, NoteKind.Text, 10, 10, "text");
        notes.AddPoint(hidden, NoteKind.Pin, 30, 30, "pin");
        notes.FindCategory(hidden)!.Visible = false;

        MinimapOverlay.Paint(notes, rgba, 0, 0, 1);

        Assert.Equal(0, CountPainted());
    }

    [Fact]
    public void Paint_SkipsTheChildOfAHiddenCategory()
    {
        var parent = notes.AddCategory("Parent", 0);
        var child = notes.AddCategory("Child", 0, parent);
        notes.AddPoint(child, NoteKind.Pin, 10, 20, "pin");
        notes.FindCategory(parent)!.Visible = false;

        MinimapOverlay.Paint(notes, rgba, 0, 0, 1);

        Assert.Equal(0, CountPainted());
        Assert.Null(MinimapOverlay.PinAt(notes, 0, 0, 1, 10, 20));
    }

    [Fact]
    public void Paint_ClipsToTheMinimap()
    {
        var category = notes.AddCategory("Pins", 0);
        notes.AddPoint(category, NoteKind.Pin, 500, 500, "far");
        notes.AddPoint(category, NoteKind.Pin, 0, 0, "corner");
        notes.AddPath(category, [new PathSegment(0, 0, 300, 300)], "long");

        MinimapOverlay.Paint(notes, rgba, 0, 0, 1);

        Assert.True(IsPainted(126, 126));
        Assert.False(IsPainted(127, 127));
    }

    [Fact]
    public void Paint_DrawsOnlyTheVisiblePartOfALongRow()
    {
        Assert.True(PaintMatchesReference(new PathSegment(10, 0, 10, 2000), 0, 0, 1));
        for (var column = 0; column < Size; column++)
        {
            Assert.Equal(column % 2 == 0, IsPainted(10, column));
        }

        Assert.Equal(Size / 2, CountPainted());
    }

    [Fact]
    public void Paint_DrawsOnlyTheVisiblePartOfALongColumn()
    {
        Assert.True(PaintMatchesReference(new PathSegment(0, 10, 2000, 10), 0, 0, 1));
        for (var row = 0; row < Size; row++)
        {
            Assert.Equal(row % 2 == 0, IsPainted(row, 10));
        }

        Assert.Equal(Size / 2, CountPainted());
    }

    [Fact]
    public void Paint_DrawsNothingForSegmentsOutsideTheMinimap()
    {
        Assert.True(PaintMatchesReference(new PathSegment(500, 0, 500, 2000), 0, 0, 1));
        Assert.Equal(0, CountPainted());
        Assert.True(PaintMatchesReference(new PathSegment(0, 0, 40, 40), 300, 300, 1));
        Assert.Equal(0, CountPainted());
    }

    // Clipping must not move a single dot: each clipped segment matches a full walk from its start.
    [Theory]
    [MemberData(nameof(ClippedSegments))]
    public void Paint_ClipsSegmentsWithoutMovingTheirDots(int segmentIndex, int originX, int originY, int magnification)
    {
        Assert.True(PaintMatchesReference(Segments[segmentIndex], originX, originY, magnification));
    }

    [Fact]
    public void Paint_DrawsNothingForAFloodOfSegmentsOffTheMinimap()
    {
        var segments = Enumerable.Range(0, NoteLimits.SegmentsPerPath)
            .Select(index => new PathSegment(200 + index, 0, 200 + index, 2047))
            .ToArray();
        for (var categoryIndex = 0; categoryIndex < 16; categoryIndex++)
        {
            var category = notes.AddCategory("Flood", 0);
            for (var pathIndex = 0; pathIndex < 8; pathIndex++)
            {
                notes.AddPath(category, segments, "flood");
            }
        }

        MinimapOverlay.Paint(notes, rgba, 0, 0, 1);

        Assert.Equal(0, CountPainted());
    }

    [Fact]
    public void Paint_UsesANotesOwnColour()
    {
        var pin = notes.AddPoint(notes.AddCategory("Pins", 0), NoteKind.Pin, 10, 20, "pin");
        notes.SetNoteColour(pin, 2);

        MinimapOverlay.Paint(notes, rgba, 0, 0, 1);

        var colour = NotePalette.Rgb(2);
        Assert.Equal(
            new byte[] { (byte)(colour >> 16), (byte)(colour >> 8), (byte)colour, 255 },
            rgba.AsSpan((10 * Size + 20) * BytesPerPixel, BytesPerPixel).ToArray());
    }

    [Fact]
    public void PinAt_FindsAPinWithinThreePixelsOfTheCursor()
    {
        notes.AddPoint(notes.AddCategory("Pins", 0), NoteKind.Pin, 10, 20, "key");

        Assert.Equal("key", MinimapOverlay.PinAt(notes, 0, 0, 1, 11, 22)?.Label);
        Assert.Null(MinimapOverlay.PinAt(notes, 0, 0, 1, 14, 20));
        Assert.Equal("key", MinimapOverlay.PinAt(notes, 5, 10, 2, 11, 21)?.Label);
        Assert.Equal("key", MinimapOverlay.PinAt(notes, 5, 10, 2, 10, 20)?.Label);
        Assert.Null(MinimapOverlay.PinAt(notes, 5, 10, 2, 16, 21));
    }

    [Fact]
    public void PinAt_PicksTheNearestShownPin()
    {
        var shown = notes.AddCategory("Shown", 0);
        var hidden = notes.AddCategory("Hidden", 1);
        notes.AddPoint(shown, NoteKind.Pin, 10, 20, "far");
        notes.AddPoint(shown, NoteKind.Pin, 10, 23, "near");
        notes.AddPoint(shown, NoteKind.Text, 10, 22, "text");
        notes.AddPoint(hidden, NoteKind.Pin, 10, 22, "hidden");
        notes.FindCategory(hidden)!.Visible = false;

        Assert.Equal("near", MinimapOverlay.PinAt(notes, 0, 0, 1, 10, 22)?.Label);

        notes.ShowOnMinimap = false;
        Assert.Null(MinimapOverlay.PinAt(notes, 0, 0, 1, 10, 22));
    }

    public static TheoryData<int, int, int, int> ClippedSegments()
    {
        var cases = new TheoryData<int, int, int, int>();
        int[][] origins = [[0, 0], [330, 400], [390, 410], [1900, 1900]];
        int[] magnifications = [1, 2, 3, 5];
        for (var segmentIndex = 0; segmentIndex < Segments.Length; segmentIndex++)
        {
            foreach (var origin in origins)
            {
                foreach (var magnification in magnifications)
                {
                    cases.Add(segmentIndex, origin[0], origin[1], magnification);
                }
            }
        }

        return cases;
    }

    private bool PaintMatchesReference(PathSegment segment, int originX, int originY, int magnification)
    {
        Array.Clear(rgba);
        var paths = new NoteSet { ShowOnMinimap = true };
        paths.AddPath(paths.AddCategory("Paths", 0), [segment], "path");
        MinimapOverlay.Paint(paths, rgba, originX, originY, magnification);

        var reference = WalkEveryOtherPixel(segment, originX, originY, magnification);
        for (var row = 0; row < Size; row++)
        {
            for (var column = 0; column < Size; column++)
            {
                if (IsPainted(row, column) != reference[row, column])
                {
                    return false;
                }
            }
        }

        return true;
    }

    // A plain Bresenham walk over the whole segment, lighting every second step along its longer axis.
    private static bool[,] WalkEveryOtherPixel(PathSegment segment, int originX, int originY, int magnification)
    {
        var lit = new bool[Size, Size];
        var row = (segment.StartX - originX) * magnification + magnification / 2;
        var column = (segment.StartY - originY) * magnification + magnification / 2;
        var endRow = (segment.EndX - originX) * magnification + magnification / 2;
        var endColumn = (segment.EndY - originY) * magnification + magnification / 2;
        var deltaRow = Math.Abs(endRow - row);
        var deltaColumn = -Math.Abs(endColumn - column);
        var stepRow = row < endRow ? 1 : -1;
        var stepColumn = column < endColumn ? 1 : -1;
        var rowMajor = deltaRow >= -deltaColumn;
        var error = deltaRow + deltaColumn;
        var majorSteps = 0;
        while (true)
        {
            if (majorSteps % 2 == 0 && row is >= 0 and < Size && column is >= 0 and < Size)
            {
                lit[row, column] = true;
            }

            if (row == endRow && column == endColumn)
            {
                return lit;
            }

            var doubledError = 2 * error;
            var movedRow = false;
            var movedColumn = false;
            if (doubledError >= deltaColumn)
            {
                error += deltaColumn;
                row += stepRow;
                movedRow = true;
            }

            if (doubledError <= deltaRow)
            {
                error += deltaRow;
                column += stepColumn;
                movedColumn = true;
            }

            if (rowMajor ? movedRow : movedColumn)
            {
                majorSteps++;
            }
        }
    }

    private bool IsPainted(int row, int column) => rgba[(row * Size + column) * BytesPerPixel + 3] != 0;

    private int CountPainted() => Enumerable.Range(0, Size * Size).Count(pixel => rgba[pixel * BytesPerPixel + 3] != 0);
}
