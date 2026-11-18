using System;
using TheLastGate.Notes;

namespace TheLastGate.Map;

// Pins and paths painted over the minimap: each pin a small diamond, each path a line of dots. Text labels stay off it.
public static class MinimapOverlay
{
    private const int Size = MinimapView.Pixels;
    private const int BytesPerPixel = 4;
    private const int DotSpacing = 2;
    private const int PinRadius = 2;
    private const int PinHoverPixels = 3;

    public static void Paint(NoteSet? notes, Span<byte> rgba, int originX, int originY, int magnification)
    {
        if (notes is not { ShowOnMinimap: true } || magnification < 1)
        {
            return;
        }

        var canvas = new Canvas(originX, originY, magnification);
        foreach (var category in notes.Categories)
        {
            if (!notes.IsShown(category))
            {
                continue;
            }

            foreach (var note in category.Notes)
            {
                var colour = NoteSet.Rgb(category, note);
                if (note.Kind == NoteKind.Pin)
                {
                    PaintPin(canvas, rgba, colour, note.TileX, note.TileY);
                }
                else if (note.Kind == NoteKind.Path)
                {
                    foreach (var segment in note.Segments)
                    {
                        PaintSegment(canvas, rgba, colour, segment);
                    }
                }
            }
        }
    }

    // Row and column are the hovered pixel: rows run down the minimap with map x, columns across with map y.
    public static Note? PinAt(NoteSet? notes, int originX, int originY, int magnification, int row, int column)
    {
        if (notes is not { ShowOnMinimap: true } || magnification < 1)
        {
            return null;
        }

        var canvas = new Canvas(originX, originY, magnification);
        Note? nearest = null;
        var nearestDistance = PinHoverPixels * PinHoverPixels + 1;
        foreach (var category in notes.Categories)
        {
            if (!notes.IsShown(category))
            {
                continue;
            }

            foreach (var note in category.Notes)
            {
                if (note.Kind != NoteKind.Pin)
                {
                    continue;
                }

                var offsetRow = canvas.Row(note.TileX) - row;
                var offsetColumn = canvas.Column(note.TileY) - column;
                if (Math.Abs(offsetRow) > PinHoverPixels || Math.Abs(offsetColumn) > PinHoverPixels)
                {
                    continue;
                }

                var distance = offsetRow * offsetRow + offsetColumn * offsetColumn;
                if (distance < nearestDistance)
                {
                    nearest = note;
                    nearestDistance = distance;
                }
            }
        }

        return nearest;
    }

    private static void PaintPin(Canvas canvas, Span<byte> rgba, uint colour, int tileX, int tileY)
    {
        var centreRow = canvas.Row(tileX);
        var centreColumn = canvas.Column(tileY);
        for (var offsetRow = -PinRadius; offsetRow <= PinRadius; offsetRow++)
        {
            var halfWidth = PinRadius - Math.Abs(offsetRow);
            for (var offsetColumn = -halfWidth; offsetColumn <= halfWidth; offsetColumn++)
            {
                Put(rgba, colour, centreRow + offsetRow, centreColumn + offsetColumn);
            }
        }
    }

    // Lights every second pixel counted from the segment's start, so the dots stay put as the minimap scrolls. Only
    // the steps whose pixel on the longer axis is on the minimap are walked: after a number of steps, Bresenham's
    // offset on the shorter axis is floor((2 * shorter * steps + longer) / (2 * longer)), so any step can be worked
    // out directly and the pixels lit are exactly those of a full walk from the start.
    private static void PaintSegment(Canvas canvas, Span<byte> rgba, uint colour, PathSegment segment)
    {
        if (canvas.Misses(segment))
        {
            return;
        }

        var startRow = canvas.Row(segment.StartX);
        var startColumn = canvas.Column(segment.StartY);
        var endRow = canvas.Row(segment.EndX);
        var endColumn = canvas.Column(segment.EndY);
        var rowSpan = Math.Abs(endRow - startRow);
        var columnSpan = Math.Abs(endColumn - startColumn);
        var rowStep = startRow < endRow ? 1 : -1;
        var columnStep = startColumn < endColumn ? 1 : -1;
        var rowsLonger = rowSpan >= columnSpan;
        var longer = rowsLonger ? rowSpan : columnSpan;
        var shorter = rowsLonger ? columnSpan : rowSpan;
        var longerStart = rowsLonger ? startRow : startColumn;
        var longerStep = rowsLonger ? rowStep : columnStep;
        if (longer == 0)
        {
            Put(rgba, colour, startRow, startColumn);
            return;
        }

        var firstStep = longerStep > 0 ? -longerStart : longerStart - (Size - 1);
        var lastStep = longerStep > 0 ? Size - 1 - longerStart : longerStart;
        firstStep = Math.Max(firstStep, 0);
        lastStep = Math.Min(lastStep, longer);
        if (firstStep % DotSpacing != 0)
        {
            firstStep += DotSpacing - firstStep % DotSpacing;
        }

        for (var step = firstStep; step <= lastStep; step += DotSpacing)
        {
            var shorterOffset = (int)((2L * shorter * step + longer) / (2L * longer));
            if (rowsLonger)
            {
                Put(rgba, colour, startRow + rowStep * step, startColumn + columnStep * shorterOffset);
            }
            else
            {
                Put(rgba, colour, startRow + rowStep * shorterOffset, startColumn + columnStep * step);
            }
        }
    }

    private static void Put(Span<byte> rgba, uint colour, int row, int column)
    {
        if (row is < 0 or >= Size || column is < 0 or >= Size)
        {
            return;
        }

        var pixel = rgba.Slice((row * Size + column) * BytesPerPixel, BytesPerPixel);
        pixel[0] = (byte)(colour >> 16);
        pixel[1] = (byte)(colour >> 8);
        pixel[2] = (byte)colour;
        pixel[3] = 255;
    }

    private readonly record struct Canvas(int OriginX, int OriginY, int Magnification)
    {
        private int VisibleTiles => (Size + Magnification - 1) / Magnification;

        public int Row(int tileX) => (tileX - OriginX) * Magnification + Magnification / 2;

        public int Column(int tileY) => (tileY - OriginY) * Magnification + Magnification / 2;

        public bool Misses(PathSegment segment) =>
            Math.Max(segment.StartX, segment.EndX) < OriginX
            || Math.Min(segment.StartX, segment.EndX) >= OriginX + VisibleTiles
            || Math.Max(segment.StartY, segment.EndY) < OriginY
            || Math.Min(segment.StartY, segment.EndY) >= OriginY + VisibleTiles;
    }
}
