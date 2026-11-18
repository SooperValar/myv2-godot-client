using System;
using TheLastGate.Notes;

namespace TheLastGate.Map;

public readonly record struct NoteHit(int NoteId, int SegmentIndex);

// Where the world map window looks: map y runs across the panel and map x down it, as on the minimap. Tile (x, y)
// covers tile coordinates x..x+1 and y..y+1, so its middle is at (x + 0.5, y + 0.5).
public sealed class WorldMapView
{
    public const double MaxPixelsPerTile = 16;
    public const double ZoomStep = 1.25;

    private const int TilesDown = 1024;
    private const int TilesAcross = 2048;
    private const double OpeningPixelsPerTile = 8;
    private const double SmallestFit = 1e-3;

    private static readonly NoteKind[] HitOrder = [NoteKind.Pin, NoteKind.Text, NoteKind.Path];

    public double CentreTileX { get; set; }

    public double CentreTileY { get; set; }

    public double PixelsPerTile { get; set; }

    // Zoomed out as far as the whole world fitting in the panel, and never to nothing in a panel with no size.
    public static double MinPixelsPerTile(double panelWidth, double panelHeight) =>
        Math.Max(Math.Min(panelWidth / TilesAcross, panelHeight / TilesDown), SmallestFit);

    public void Clamp(double panelWidth, double panelHeight)
    {
        PixelsPerTile = Math.Clamp(PixelsPerTile, MinPixelsPerTile(panelWidth, panelHeight), MaxPixelsPerTile);
        CentreTileX = Math.Clamp(CentreTileX, 0, TilesDown);
        CentreTileY = Math.Clamp(CentreTileY, 0, TilesAcross);
    }

    public (double X, double Y) TileToPanel(double panelWidth, double panelHeight, double tileX, double tileY) =>
        ((tileY - CentreTileY) * PixelsPerTile + panelWidth / 2,
            (tileX - CentreTileX) * PixelsPerTile + panelHeight / 2);

    public (double X, double Y) PanelToTile(double panelWidth, double panelHeight, double panelX, double panelY) =>
        ((panelY - panelHeight / 2) / PixelsPerTile + CentreTileX,
            (panelX - panelWidth / 2) / PixelsPerTile + CentreTileY);

    public (int X, int Y)? TileUnder(double panelWidth, double panelHeight, double panelX, double panelY)
    {
        var (exactX, exactY) = PanelToTile(panelWidth, panelHeight, panelX, panelY);
        var tileX = (int)Math.Floor(exactX);
        var tileY = (int)Math.Floor(exactY);
        return tileX is >= 0 and < TilesDown && tileY is >= 0 and < TilesAcross ? (tileX, tileY) : null;
    }

    public void Pan(double panelWidth, double panelHeight, double deltaX, double deltaY)
    {
        CentreTileY -= deltaX / PixelsPerTile;
        CentreTileX -= deltaY / PixelsPerTile;
        Clamp(panelWidth, panelHeight);
    }

    // The tile under the cursor stays under it.
    public void ZoomAt(double panelWidth, double panelHeight, double panelX, double panelY, double factor)
    {
        var (anchorX, anchorY) = PanelToTile(panelWidth, panelHeight, panelX, panelY);
        PixelsPerTile = Math.Clamp(
            PixelsPerTile * factor,
            MinPixelsPerTile(panelWidth, panelHeight),
            MaxPixelsPerTile);
        CentreTileY = anchorY - (panelX - panelWidth / 2) / PixelsPerTile;
        CentreTileX = anchorX - (panelY - panelHeight / 2) / PixelsPerTile;
        Clamp(panelWidth, panelHeight);
    }

    // Pins win over text and text over paths; among one kind the nearest within the radius wins.
    public NoteHit? HitTest(
        NoteSet notes,
        double panelWidth,
        double panelHeight,
        double panelX,
        double panelY,
        double radius)
    {
        foreach (var kind in HitOrder)
        {
            NoteHit? best = null;
            var bestDistance = double.PositiveInfinity;
            foreach (var category in notes.Categories)
            {
                if (!notes.IsShown(category))
                {
                    continue;
                }

                foreach (var note in category.Notes)
                {
                    if (note.Kind != kind)
                    {
                        continue;
                    }

                    var segmentIndex = -1;
                    var distance = kind == NoteKind.Path
                        ? NearestSegment(note, panelWidth, panelHeight, panelX, panelY, out segmentIndex)
                        : DistanceToTile(note.TileX, note.TileY, panelWidth, panelHeight, panelX, panelY);
                    if (distance <= radius && distance < bestDistance)
                    {
                        bestDistance = distance;
                        best = new NoteHit(note.Id, segmentIndex);
                    }
                }
            }

            if (best != null)
            {
                return best;
            }
        }

        return null;
    }

    public int HitSegment(
        Note path,
        double panelWidth,
        double panelHeight,
        double panelX,
        double panelY,
        double radius)
    {
        var distance = NearestSegment(path, panelWidth, panelHeight, panelX, panelY, out var segmentIndex);
        return distance <= radius ? segmentIndex : -1;
    }

    public void CentreOn(int tileX, int tileY)
    {
        MoveTo(tileX, tileY);
        PixelsPerTile = OpeningPixelsPerTile;
    }

    public void MoveTo(int tileX, int tileY)
    {
        CentreTileX = tileX + 0.5;
        CentreTileY = tileY + 0.5;
    }

    private static double DistanceToSegment(
        (double X, double Y) point,
        (double X, double Y) start,
        (double X, double Y) end)
    {
        var spanX = end.X - start.X;
        var spanY = end.Y - start.Y;
        var spanLengthSquared = spanX * spanX + spanY * spanY;
        var along = spanLengthSquared > 0
            ? Math.Clamp(((point.X - start.X) * spanX + (point.Y - start.Y) * spanY) / spanLengthSquared, 0, 1)
            : 0;
        return double.Hypot(point.X - (start.X + along * spanX), point.Y - (start.Y + along * spanY));
    }

    private double DistanceToTile(
        int tileX,
        int tileY,
        double panelWidth,
        double panelHeight,
        double panelX,
        double panelY)
    {
        var (centreX, centreY) = TileToPanel(panelWidth, panelHeight, tileX + 0.5, tileY + 0.5);
        return double.Hypot(panelX - centreX, panelY - centreY);
    }

    private double NearestSegment(
        Note path,
        double panelWidth,
        double panelHeight,
        double panelX,
        double panelY,
        out int segmentIndex)
    {
        var bestDistance = double.PositiveInfinity;
        segmentIndex = -1;
        for (var index = 0; index < path.Segments.Count; index++)
        {
            var segment = path.Segments[index];
            var distance = DistanceToSegment(
                (panelX, panelY),
                TileToPanel(panelWidth, panelHeight, segment.StartX + 0.5, segment.StartY + 0.5),
                TileToPanel(panelWidth, panelHeight, segment.EndX + 0.5, segment.EndY + 0.5));
            if (distance < bestDistance)
            {
                bestDistance = distance;
                segmentIndex = index;
            }
        }

        return bestDistance;
    }
}
