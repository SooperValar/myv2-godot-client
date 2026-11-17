using System;
using Godot;
using TheLastGate.Windows;

namespace TheLastGate.Map;

// The world map's picture area: the explored map, panned and zoomed, with the player on it.
public partial class WorldMapCanvas : Control
{
    private const int BytesPerPixel = 4;
    private const ulong RefreshMilliseconds = 500;
    private const int MarkerOutline = 2;
    private const int MarkerCentre = 1;
    private const float FollowRingRadius = 5;
    private const int FollowRingPoints = 17;

    private static readonly Color Gold = Color.Color8(243, 219, 106);

    private readonly byte[] pixels = new byte[WorldMapPicture.Width * WorldMapPicture.Height * BytesPerPixel];
    private readonly WorldMapView view = new();
    private bool panning;
    private Image? picture;
    private ulong pictureMadeAt;
    private int pictureRevision = -1;
    private ImageTexture? pictureTexture;

    public ExploredMap? Map { get; set; }

    public WorldMapWindow? Window { get; set; }

    public (int X, int Y) PlayerTile { get; set; }

    public (int X, int Y)? HoveredTile { get; private set; }

    public override void _Process(double delta)
    {
        if (Window == null || !IsVisibleInTree())
        {
            return;
        }

        if (Window.CentreOnPlayer)
        {
            view.CentreOn(PlayerTile.X, PlayerTile.Y);
            Window.CentreOnPlayer = false;
        }

        if (Window.Follow)
        {
            view.MoveTo(PlayerTile.X, PlayerTile.Y);
        }

        view.Clamp(Size.X, Size.Y);
        RefreshPicture();
        QueueRedraw();
    }

    public override void _GuiInput(InputEvent inputEvent)
    {
        switch (inputEvent)
        {
            case InputEventMouseButton
            {
                ButtonIndex: MouseButton.WheelUp or MouseButton.WheelDown, Pressed: true,
            } wheel:
                var notches = wheel.Factor > 0 ? wheel.Factor : 1;
                var steps = wheel.ButtonIndex == MouseButton.WheelUp ? notches : -notches;
                view.ZoomAt(Size.X, Size.Y, wheel.Position.X, wheel.Position.Y, Math.Pow(WorldMapView.ZoomStep, steps));
                AcceptEvent();
                break;
            case InputEventMouseButton click when StartsPanning(click.ButtonIndex):
                panning = click.Pressed;
                AcceptEvent();
                break;
            case InputEventMouseMotion motion:
                if (panning)
                {
                    view.Pan(Size.X, Size.Y, motion.Relative.X, motion.Relative.Y);
                    Window?.Panned();
                }

                HoveredTile = view.TileUnder(Size.X, Size.Y, motion.Position.X, motion.Position.Y);
                break;
        }
    }

    public override void _Notification(int what)
    {
        if (what == NotificationMouseExit)
        {
            HoveredTile = null;
        }
    }

    public override void _Draw()
    {
        DrawRect(new Rect2(Vector2.Zero, Size), Colors.Black);
        DrawWorld();
        DrawPlayer();
    }

    // Right-drag always pans; with the Pan tool so does the left button.
    private bool StartsPanning(MouseButton button) =>
        button == MouseButton.Right || (button == MouseButton.Left && Window?.Tool == WorldMapTool.Pan);

    private Vector2 ToPanel(double tileX, double tileY)
    {
        var (panelX, panelY) = view.TileToPanel(Size.X, Size.Y, tileX, tileY);
        return new Vector2((float)panelX, (float)panelY);
    }

    // Only the part of the world inside the panel is drawn, from the matching part of the picture.
    private void DrawWorld()
    {
        if (pictureTexture == null)
        {
            return;
        }

        var visibleStart = ToPanel(0, 0).Max(Vector2.Zero);
        var visibleEnd = ToPanel(WorldMapPicture.Height, WorldMapPicture.Width).Min(Size);
        if (visibleEnd.X <= visibleStart.X || visibleEnd.Y <= visibleStart.Y)
        {
            return;
        }

        var (startTileX, startTileY) = view.PanelToTile(Size.X, Size.Y, visibleStart.X, visibleStart.Y);
        var (endTileX, endTileY) = view.PanelToTile(Size.X, Size.Y, visibleEnd.X, visibleEnd.Y);
        var source = new Rect2(
            (float)startTileY,
            (float)startTileX,
            (float)(endTileY - startTileY),
            (float)(endTileX - startTileX));
        DrawTextureRectRegion(pictureTexture, new Rect2(visibleStart, visibleEnd - visibleStart), source);
    }

    private void DrawPlayer()
    {
        var centre = ToPanel(PlayerTile.X + 0.5, PlayerTile.Y + 0.5).Floor();
        if (Window?.Follow == true)
        {
            DrawArc(centre + new Vector2(0.5f, 0.5f), FollowRingRadius, 0, Mathf.Tau, FollowRingPoints, Gold, 1);
        }

        DrawSquare(centre, MarkerOutline, Colors.Black);
        DrawSquare(centre, MarkerCentre, Colors.White);
    }

    private void DrawSquare(Vector2 centre, int radius, Color colour) =>
        DrawRect(new Rect2(centre - new Vector2(radius, radius), new Vector2(radius * 2 + 1, radius * 2 + 1)), colour);

    // Remade at most twice a second while the map changes, as the current client does.
    private void RefreshPicture()
    {
        if (Map == null || Map.Revision == pictureRevision
            || (pictureTexture != null && Time.GetTicksMsec() - pictureMadeAt < RefreshMilliseconds))
        {
            return;
        }

        WorldMapPicture.Render(Map, pixels);
        pictureRevision = Map.Revision;
        pictureMadeAt = Time.GetTicksMsec();
        if (picture == null || pictureTexture == null)
        {
            picture = Image.CreateFromData(
                WorldMapPicture.Width,
                WorldMapPicture.Height,
                false,
                Image.Format.Rgba8,
                pixels);
            pictureTexture = ImageTexture.CreateFromImage(picture);
            return;
        }

        picture.SetData(WorldMapPicture.Width, WorldMapPicture.Height, false, Image.Format.Rgba8, pixels);
        pictureTexture.Update(picture);
    }
}
