using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using TheLastGate.Assets;
using TheLastGate.Net;
using TheLastGate.State;

namespace TheLastGate.World;

public partial class WorldView : Node2D
{
    private const int TileCount = WorldMap.TileCount;
    private const int TintShift = 4;
    private const int HighlightBit = 1 << 7;
    private const float HighlightBrightness = 2f;
    private const int NoTile = -1;
    private const string TintShaderPath = "res://World/sprite_tint.gdshader";
    private const string DefaultCursorPath = "res://images/cursors/cursor1.png";
    private const string WalkCursorPath = "res://images/cursors/cursor6.png";

    private readonly Vector2I[] characterOffsets = new Vector2I[TileCount];
    private readonly SpriteLayer characters = new();
    private readonly SpriteLayer floors = new();
    private readonly SpriteLayer objects = new();
    private readonly Dictionary<(SpriteTint Tint, bool Highlighted), ShaderMaterial> tintMaterials = [];
    private Node2D floorLayer = null!;
    private int hoveredTile = NoTile;
    private Vector2I mapShift = new(IsometricProjection.BaseMapShiftX, 0);
    private Node2D objectLayer = null!;
    private Shader? tintShader;

    public GameState State { get; set; } = null!;
    public SpriteLoader Sprites { get; set; } = null!;
    public bool HideWalls { get; set; } = true;
    public double LastRefreshMilliseconds { get; private set; }
    public bool PickTileDrawnUnderMouse { get; set; }
    public Action<byte[]> SendCommand { get; set; } = _ => { };

    public override void _Ready()
    {
        floorLayer = AddLayer();
        objectLayer = AddLayer();
        for (var row = WorldMap.Size - 1; row >= 0; row--)
        {
            for (var column = 0; column < WorldMap.Size; column++)
            {
                var tileIndex = column + row * WorldMap.Size;
                floors.Nodes[tileIndex] = AddSpriteNode(floorLayer, column, row);
                objects.Nodes[tileIndex] = AddSpriteNode(objectLayer, column, row);
                characters.Nodes[tileIndex] = AddSpriteNode(objectLayer, column, row);
            }
        }

        Input.SetCustomMouseCursor(GD.Load<Texture2D>(DefaultCursorPath), Input.CursorShape.Arrow, Vector2.Zero);
        Input.SetCustomMouseCursor(GD.Load<Texture2D>(WalkCursorPath), Input.CursorShape.Move, Vector2.Zero);
    }

    public override void _ExitTree()
    {
        Input.SetCustomMouseCursor(null);
        Input.SetCustomMouseCursor(null, Input.CursorShape.Move);
    }

    public override void _Process(double delta)
    {
        var tileIndex = IsModifierHeld() || GetViewport().GuiGetHoveredControl() != null
            ? NoTile
            : TileUnder(GetViewport().GetMousePosition());
        if (tileIndex == hoveredTile)
        {
            return;
        }

        var previousTile = hoveredTile;
        hoveredTile = tileIndex;
        Input.SetDefaultCursorShape(tileIndex == NoTile ? Input.CursorShape.Arrow : Input.CursorShape.Move);
        if (previousTile != NoTile)
        {
            ShowFloor(previousTile);
        }

        if (tileIndex != NoTile)
        {
            ShowFloor(tileIndex);
        }
    }

    public override void _UnhandledInput(InputEvent inputEvent)
    {
        if (inputEvent is not InputEventMouseButton { Pressed: false } click
            || click.ShiftPressed
            || click.CtrlPressed
            || click.AltPressed)
        {
            return;
        }

        var tileIndex = TileUnder(click.Position);
        if (tileIndex == NoTile)
        {
            return;
        }

        ref readonly var tile = ref State.Map.Tiles[tileIndex];
        var command = click.ButtonIndex switch
        {
            MouseButton.Left => MapCommandPackets.Move(tile.X, tile.Y),
            MouseButton.Right => MapCommandPackets.Turn(tile.X, tile.Y),
            _ => null,
        };
        if (command == null)
        {
            return;
        }

        SendCommand(command);
        GetViewport().SetInputAsHandled();
    }

    public void Refresh()
    {
        var clock = Stopwatch.StartNew();
        var tiles = State.Map.Tiles;
        for (var row = 0; row < WorldMap.Size; row++)
        {
            for (var column = 0; column < WorldMap.Size; column++)
            {
                var tileIndex = column + row * WorldMap.Size;
                ref readonly var tile = ref tiles[tileIndex];
                var environmentTint = MapSpriteRules.EnvironmentTint(tile);
                ShowFloor(tileIndex);
                Show(
                    objects,
                    tileIndex,
                    MapSpriteRules.ObjectSprite(tile, column, row, State.Player, HideWalls),
                    tile.Light,
                    environmentTint);
                Show(
                    characters,
                    tileIndex,
                    MapSpriteRules.CharacterSprite(tile, column, row),
                    tile.Light,
                    MapSpriteRules.CharacterTint(tile));
                PlaceCharacter(tileIndex, column, row, new Vector2I(tile.CharacterOffsetX, tile.CharacterOffsetY));
            }
        }

        ShiftMapUnderPlayer();
        LastRefreshMilliseconds = clock.Elapsed.TotalMilliseconds;
    }

    private static bool IsModifierHeld() =>
        Input.IsKeyPressed(Key.Shift) || Input.IsKeyPressed(Key.Ctrl) || Input.IsKeyPressed(Key.Alt);

    private static Sprite2D AddSpriteNode(Node2D layer, int column, int row)
    {
        var (x, y) = IsometricProjection.TilePosition(column, row);
        var node = new Sprite2D { Centered = false, Position = new Vector2(x, y), Visible = false };
        layer.AddChild(node);
        return node;
    }

    private Node2D AddLayer()
    {
        var layer = new Node2D { Position = mapShift };
        AddChild(layer);
        return layer;
    }

    private int TileUnder(Vector2 screenPosition)
    {
        var (x, y) = ((int)screenPosition.X, (int)screenPosition.Y);
        var picked = PickTileDrawnUnderMouse
            ? MapPicking.TileAt(x, y, IsometricProjection.BaseMapShiftX - mapShift.X, -mapShift.Y)
            : MapPicking.TileAt(x, y);
        return picked is var (column, row) ? column + row * WorldMap.Size : NoTile;
    }

    private void ShowFloor(int tileIndex)
    {
        ref readonly var tile = ref State.Map.Tiles[tileIndex];
        Show(
            floors,
            tileIndex,
            MapSpriteRules.FloorSprite(tile, State.ClientTick),
            tile.Light,
            MapSpriteRules.EnvironmentTint(tile),
            tileIndex == hoveredTile);
    }

    private void PlaceCharacter(int tileIndex, int column, int row, Vector2I offset)
    {
        if (characterOffsets[tileIndex] == offset)
        {
            return;
        }

        characterOffsets[tileIndex] = offset;
        var (x, y) = IsometricProjection.TilePosition(column, row);
        characters.Nodes[tileIndex].Position = new Vector2(x + offset.X, y + offset.Y);
    }

    private void ShiftMapUnderPlayer()
    {
        var (x, y) = IsometricProjection.MapShift(State.Map);
        var offset = new Vector2I(x, y);
        if (offset == mapShift)
        {
            return;
        }

        mapShift = offset;
        floorLayer.Position = offset;
        objectLayer.Position = offset;
    }

    private void Show(
        SpriteLayer layer,
        int tileIndex,
        int spriteNumber,
        int light,
        SpriteTint tint,
        bool highlighted = false)
    {
        var node = layer.Nodes[tileIndex];
        if (layer.ShownSprites[tileIndex] != spriteNumber)
        {
            layer.ShownSprites[tileIndex] = spriteNumber;
            var texture = spriteNumber == 0 ? null : Sprites.GetTexture(spriteNumber);
            node.Texture = texture;
            node.Visible = texture != null;
            if (texture != null)
            {
                var (offsetX, offsetY) = IsometricProjection.SpriteOffset(texture.GetWidth(), texture.GetHeight());
                node.Offset = new Vector2(offsetX, offsetY);
            }
        }

        var effect = light | ((int)tint << TintShift) | (highlighted ? HighlightBit : 0);
        if (layer.ShownEffects[tileIndex] == effect)
        {
            return;
        }

        layer.ShownEffects[tileIndex] = effect;
        var brightness = SpriteLighting.Brightness(light);
        if (highlighted && tint == SpriteTint.None)
        {
            brightness *= HighlightBrightness;
        }

        node.Modulate = new Color(brightness, brightness, brightness);
        node.Material = tint == SpriteTint.None ? null : TintMaterial(tint, highlighted);
    }

    private ShaderMaterial TintMaterial(SpriteTint tint, bool highlighted)
    {
        if (tintMaterials.TryGetValue((tint, highlighted), out var material))
        {
            return material;
        }

        tintShader ??= GD.Load<Shader>(TintShaderPath);
        material = new ShaderMaterial { Shader = tintShader };
        material.SetShaderParameter("grey", tint.HasFlag(SpriteTint.Grey));
        material.SetShaderParameter("infrared", tint.HasFlag(SpriteTint.Infrared));
        material.SetShaderParameter("underwater", tint.HasFlag(SpriteTint.Underwater));
        material.SetShaderParameter("highlight", highlighted);
        tintMaterials[(tint, highlighted)] = material;
        return material;
    }

    private sealed class SpriteLayer
    {
        public Sprite2D[] Nodes { get; } = new Sprite2D[TileCount];
        public int[] ShownSprites { get; } = new int[TileCount];
        public int[] ShownEffects { get; } = new int[TileCount];
    }
}
