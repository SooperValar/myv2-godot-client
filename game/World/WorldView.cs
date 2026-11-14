using System.Diagnostics;
using Godot;
using TheLastGate.Assets;
using TheLastGate.State;

namespace TheLastGate.World;

public partial class WorldView : Node2D
{
    private const int TileCount = WorldMap.TileCount;

    private readonly Sprite2D[] characterNodes = new Sprite2D[TileCount];
    private readonly Sprite2D[] floorNodes = new Sprite2D[TileCount];
    private readonly Sprite2D[] objectNodes = new Sprite2D[TileCount];
    private readonly int[] shownCharacters = new int[TileCount];
    private readonly int[] shownFloors = new int[TileCount];
    private readonly int[] shownObjects = new int[TileCount];

    public GameState State { get; set; } = null!;
    public SpriteLoader Sprites { get; set; } = null!;
    public bool HideWalls { get; set; } = true;
    public double LastRefreshMilliseconds { get; private set; }

    public override void _Ready()
    {
        var floorLayer = AddLayer();
        var objectLayer = AddLayer();
        for (var row = WorldMap.Size - 1; row >= 0; row--)
        {
            for (var column = 0; column < WorldMap.Size; column++)
            {
                var tileIndex = column + row * WorldMap.Size;
                floorNodes[tileIndex] = AddSpriteNode(floorLayer, column, row);
                objectNodes[tileIndex] = AddSpriteNode(objectLayer, column, row);
                characterNodes[tileIndex] = AddSpriteNode(objectLayer, column, row);
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
                Show(floorNodes, shownFloors, tileIndex, MapSpriteRules.FloorSprite(tile));
                Show(
                    objectNodes,
                    shownObjects,
                    tileIndex,
                    MapSpriteRules.ObjectSprite(tile, column, row, State.Player, HideWalls));
                Show(characterNodes, shownCharacters, tileIndex, MapSpriteRules.CharacterSprite(tile, column, row));
            }
        }

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
        var layer = new Node2D { Position = new Vector2(IsometricProjection.CameraOffsetX, 0) };
        AddChild(layer);
        return layer;
    }

    private void Show(Sprite2D[] nodes, int[] shownSprites, int tileIndex, int spriteNumber)
    {
        if (shownSprites[tileIndex] == spriteNumber)
        {
            return;
        }

        shownSprites[tileIndex] = spriteNumber;
        var node = nodes[tileIndex];
        var texture = spriteNumber == 0 ? null : Sprites.GetTexture(spriteNumber);
        node.Texture = texture;
        node.Visible = texture != null;
        if (texture != null)
        {
            var (offsetX, offsetY) = IsometricProjection.SpriteOffset(texture.GetWidth(), texture.GetHeight());
            node.Offset = new Vector2(offsetX, offsetY);
        }
    }
}
