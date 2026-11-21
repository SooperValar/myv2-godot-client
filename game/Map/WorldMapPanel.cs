namespace TheLastGate.Map;

// The world map as one panel shows it, for turning cursor positions on the panel into tiles and notes.
public readonly record struct WorldMapPanel(WorldMapView View, double Width, double Height);
