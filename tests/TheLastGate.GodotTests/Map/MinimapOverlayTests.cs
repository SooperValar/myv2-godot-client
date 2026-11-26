using GdUnit4;
using TheLastGate.Map;
using TheLastGate.Notes;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Map;

[TestSuite]
public class MinimapOverlayTests
{
    private const int Size = MinimapView.Pixels;
    private const int BytesPerPixel = 4;

    private static readonly PathSegment[] Segments =
    [
        new(1000, 5, 3, 950), new(950, 850, 20, 30), new(0, 0, 300, 300), new(700, 1023, 300, 0),
        new(350, 410, 360, 600), new(420, 900, 330, 905), new(400, 400, 401, 450), new(380, 390, 381, 395),
        new(200, 1023, 600, 0), new(400, 420, 400, 420), new(0, 1023, 1023, 0), new(750, 470, 0, 431),
    ];

    private NoteSet notes = new();
    private byte[] rgba = [];

    public static IEnumerable<object[]> ClippedSegments
    {
        get
        {
            int[][] origins = [[0, 0], [330, 400], [390, 410], [950, 950]];
            int[] magnifications = [1, 2, 3, 5];
            for (var segmentIndex = 0; segmentIndex < Segments.Length; segmentIndex++)
            {
                foreach (var origin in origins)
                {
                    foreach (var magnification in magnifications)
                    {
                        yield return [segmentIndex, origin[0], origin[1], magnification];
                    }
                }
            }
        }
    }

    [BeforeTest]
    public void StartWithAnEmptyMinimap()
    {
        notes = new NoteSet { ShowOnMinimap = true };
        rgba = new byte[Size * Size * BytesPerPixel];
    }

    [TestCase]
    public void Paint_DrawsNothingUnlessNotesAreShownOnTheMinimap()
    {
        notes.AddPoint(notes.AddCategory("Pins", 0), NoteKind.Pin, 10, 20, "pin");
        notes.ShowOnMinimap = false;

        MinimapOverlay.Paint(notes, rgba, 0, 0, 1);

        AssertThat(CountPainted()).IsEqual(0);
    }

    [TestCase]
    public void Paint_DrawsAPinAsAFiveByFiveDiamond()
    {
        notes.AddPoint(notes.AddCategory("Pins", 0), NoteKind.Pin, 10, 20, "pin");

        MinimapOverlay.Paint(notes, rgba, 0, 0, 1);

        for (var offsetX = -2; offsetX <= 2; offsetX++)
        {
            for (var offsetY = -2; offsetY <= 2; offsetY++)
            {
                AssertThat(IsPainted(10 + offsetX, 20 + offsetY)).IsEqual(Math.Abs(offsetX) + Math.Abs(offsetY) <= 2);
            }
        }

        AssertThat(CountPainted()).IsEqual(13);
    }

    [TestCase]
    public void Paint_DrawsAMagnifiedSegmentOneDottedPixelWide()
    {
        notes.AddPath(notes.AddCategory("Paths", 0), [new PathSegment(0, 0, 0, 3)], "path");

        MinimapOverlay.Paint(notes, rgba, 0, 0, 2);

        for (var column = 1; column <= 7; column++)
        {
            AssertThat(IsPainted(1, column)).IsEqual(column % 2 == 1);
        }

        AssertThat(CountPainted()).IsEqual(4);
    }

    [TestCase]
    public void Paint_SkipsHiddenCategoriesAndText()
    {
        var shown = notes.AddCategory("Shown", 0);
        var hidden = notes.AddCategory("Hidden", 1);
        notes.AddPoint(shown, NoteKind.Text, 10, 10, "text");
        notes.AddPoint(hidden, NoteKind.Pin, 30, 30, "pin");
        notes.FindCategory(hidden)!.Visible = false;

        MinimapOverlay.Paint(notes, rgba, 0, 0, 1);

        AssertThat(CountPainted()).IsEqual(0);
    }

    [TestCase]
    public void Paint_SkipsTheChildOfAHiddenCategory()
    {
        var parent = notes.AddCategory("Parent", 0);
        var child = notes.AddCategory("Child", 0, parent);
        notes.AddPoint(child, NoteKind.Pin, 10, 20, "pin");
        notes.FindCategory(parent)!.Visible = false;

        MinimapOverlay.Paint(notes, rgba, 0, 0, 1);

        AssertThat(CountPainted()).IsEqual(0);
        AssertThat(MinimapOverlay.PinAt(notes, 0, 0, 1, 10, 20)).IsNull();
    }

    [TestCase]
    public void Paint_ClipsToTheMinimap()
    {
        var category = notes.AddCategory("Pins", 0);
        notes.AddPoint(category, NoteKind.Pin, 500, 500, "far");
        notes.AddPoint(category, NoteKind.Pin, 0, 0, "corner");
        notes.AddPath(category, [new PathSegment(0, 0, 300, 300)], "long");

        MinimapOverlay.Paint(notes, rgba, 0, 0, 1);

        AssertThat(IsPainted(126, 126)).IsTrue();
        AssertThat(IsPainted(127, 127)).IsFalse();
    }

    [TestCase]
    public void Paint_DrawsOnlyTheVisiblePartOfALongRow()
    {
        AssertThat(PaintMatchesReference(new PathSegment(10, 0, 10, 1000), 0, 0, 1)).IsTrue();
        for (var column = 0; column < Size; column++)
        {
            AssertThat(IsPainted(10, column)).IsEqual(column % 2 == 0);
        }

        AssertThat(CountPainted()).IsEqual(Size / 2);
    }

    [TestCase]
    public void Paint_DrawsOnlyTheVisiblePartOfALongColumn()
    {
        AssertThat(PaintMatchesReference(new PathSegment(0, 10, 1000, 10), 0, 0, 1)).IsTrue();
        for (var row = 0; row < Size; row++)
        {
            AssertThat(IsPainted(row, 10)).IsEqual(row % 2 == 0);
        }

        AssertThat(CountPainted()).IsEqual(Size / 2);
    }

    [TestCase]
    public void Paint_DrawsNothingForSegmentsOutsideTheMinimap()
    {
        AssertThat(PaintMatchesReference(new PathSegment(500, 0, 500, 2000), 0, 0, 1)).IsTrue();
        AssertThat(CountPainted()).IsEqual(0);
        AssertThat(PaintMatchesReference(new PathSegment(0, 0, 40, 40), 300, 300, 1)).IsTrue();
        AssertThat(CountPainted()).IsEqual(0);
    }

    // Clipping must not move a single dot: each clipped segment matches a full walk from its start.
    [TestCase]
    [DataPoint(nameof(ClippedSegments))]
    public void Paint_ClipsSegmentsWithoutMovingTheirDots(int segmentIndex, int originX, int originY, int magnification)
    {
        AssertThat(PaintMatchesReference(Segments[segmentIndex], originX, originY, magnification)).IsTrue();
    }

    [TestCase]
    public void Paint_DrawsNothingForAFloodOfSegmentsOffTheMinimap()
    {
        var segments = Enumerable.Range(0, NoteLimits.SegmentsPerPath)
            .Select(index => new PathSegment(200 + index, 0, 200 + index, 1023))
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

        AssertThat(CountPainted()).IsEqual(0);
    }

    [TestCase]
    public void Paint_UsesANotesOwnColour()
    {
        var pin = notes.AddPoint(notes.AddCategory("Pins", 0), NoteKind.Pin, 10, 20, "pin");
        notes.SetNoteColour(pin, 2);

        MinimapOverlay.Paint(notes, rgba, 0, 0, 1);

        var colour = NotePalette.Rgb(2);
        AssertThat(rgba.AsSpan((10 * Size + 20) * BytesPerPixel, BytesPerPixel).ToArray())
            .ContainsExactly((byte)(colour >> 16), (byte)(colour >> 8), (byte)colour, (byte)255);
    }

    [TestCase]
    public void PinAt_FindsAPinWithinThreePixelsOfTheCursor()
    {
        notes.AddPoint(notes.AddCategory("Pins", 0), NoteKind.Pin, 10, 20, "key");

        AssertThat(MinimapOverlay.PinAt(notes, 0, 0, 1, 11, 22)?.Label).IsEqual("key");
        AssertThat(MinimapOverlay.PinAt(notes, 0, 0, 1, 14, 20)).IsNull();
        AssertThat(MinimapOverlay.PinAt(notes, 5, 10, 2, 11, 21)?.Label).IsEqual("key");
        AssertThat(MinimapOverlay.PinAt(notes, 5, 10, 2, 10, 20)?.Label).IsEqual("key");
        AssertThat(MinimapOverlay.PinAt(notes, 5, 10, 2, 16, 21)).IsNull();
    }

    [TestCase]
    public void PinAt_PicksTheNearestShownPin()
    {
        var shown = notes.AddCategory("Shown", 0);
        var hidden = notes.AddCategory("Hidden", 1);
        notes.AddPoint(shown, NoteKind.Pin, 10, 20, "far");
        notes.AddPoint(shown, NoteKind.Pin, 10, 23, "near");
        notes.AddPoint(shown, NoteKind.Text, 10, 22, "text");
        notes.AddPoint(hidden, NoteKind.Pin, 10, 22, "hidden");
        notes.FindCategory(hidden)!.Visible = false;

        AssertThat(MinimapOverlay.PinAt(notes, 0, 0, 1, 10, 22)?.Label).IsEqual("near");

        notes.ShowOnMinimap = false;
        AssertThat(MinimapOverlay.PinAt(notes, 0, 0, 1, 10, 22)).IsNull();
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
