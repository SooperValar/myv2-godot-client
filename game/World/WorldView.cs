using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using TheLastGate.Assets;
using TheLastGate.State;

namespace TheLastGate.World;

public partial class WorldView : Node2D
{
    private const int TileCount = WorldMap.TileCount;
    private const int TintShift = 4;
    private const string TintShaderPath = "res://World/sprite_tint.gdshader";

    private readonly Vector2I[] characterOffsets = new Vector2I[TileCount];
    private readonly SpriteLayer characters = new();
    private readonly SpriteLayer floors = new();
    private readonly SpriteLayer objects = new();
    private readonly Dictionary<SpriteTint, ShaderMaterial> tintMaterials = [];
    private Node2D floorLayer = null!;
    private Vector2I mapShift = new(IsometricProjection.BaseMapShiftX, 0);
    private Node2D objectLayer = null!;
    private Shader? tintShader;

    public GameState State { get; set; } = null!;
    public SpriteLoader Sprites { get; set; } = null!;
    public bool HideWalls { get; set; } = true;
    public double LastRefreshMilliseconds { get; private set; }

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
                Show(
                    floors,
                    tileIndex,
                    MapSpriteRules.FloorSprite(tile, State.ClientTick),
                    tile.Light,
                    environmentTint);
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

    private void Show(SpriteLayer layer, int tileIndex, int spriteNumber, int light, SpriteTint tint)
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

        var effect = light | ((int)tint << TintShift);
        if (layer.ShownEffects[tileIndex] == effect)
        {
            return;
        }

        layer.ShownEffects[tileIndex] = effect;
        var brightness = SpriteLighting.Brightness(light);
        node.Modulate = new Color(brightness, brightness, brightness);
        node.Material = tint == SpriteTint.None ? null : TintMaterial(tint);
    }

    private ShaderMaterial TintMaterial(SpriteTint tint)
    {
        if (tintMaterials.TryGetValue(tint, out var material))
        {
            return material;
        }

        tintShader ??= GD.Load<Shader>(TintShaderPath);
        material = new ShaderMaterial { Shader = tintShader };
        material.SetShaderParameter("grey", tint.HasFlag(SpriteTint.Grey));
        material.SetShaderParameter("infrared", tint.HasFlag(SpriteTint.Infrared));
        material.SetShaderParameter("underwater", tint.HasFlag(SpriteTint.Underwater));
        tintMaterials[tint] = material;
        return material;
    }

    private sealed class SpriteLayer
    {
        public Sprite2D[] Nodes { get; } = new Sprite2D[TileCount];
        public int[] ShownSprites { get; } = new int[TileCount];
        public int[] ShownEffects { get; } = new int[TileCount];
    }
}
