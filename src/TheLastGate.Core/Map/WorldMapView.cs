using System;

namespace TheLastGate.Map;

public sealed class WorldMapView
{
    public const double MaxPixelsPerTile = 16;

    public double CentreTileX { get; set; }

    public double CentreTileY { get; set; }

    public double PixelsPerTile { get; set; }

    public static double MinPixelsPerTile(double panelWidth, double panelHeight) =>
        throw new NotImplementedException();

    public void Clamp(double panelWidth, double panelHeight) => throw new NotImplementedException();

    public (double X, double Y) TileToPanel(double panelWidth, double panelHeight, double tileX, double tileY) =>
        throw new NotImplementedException();

    public (double X, double Y) PanelToTile(double panelWidth, double panelHeight, double panelX, double panelY) =>
        throw new NotImplementedException();

    public (int X, int Y)? TileUnder(double panelWidth, double panelHeight, double panelX, double panelY) =>
        throw new NotImplementedException();

    public void Pan(double panelWidth, double panelHeight, double deltaX, double deltaY) =>
        throw new NotImplementedException();

    public void ZoomAt(double panelWidth, double panelHeight, double panelX, double panelY, double factor) =>
        throw new NotImplementedException();

    public void CentreOn(int tileX, int tileY) => throw new NotImplementedException();

    public void MoveTo(int tileX, int tileY) => throw new NotImplementedException();
}
