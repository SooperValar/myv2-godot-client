using System;
using System.Diagnostics;
using TheLastGate.State;

namespace TheLastGate.Net.Handlers;

internal static class MapCommands
{
    private const int SetMapFlag = 0x80;
    private const int SetMapTileOffsetMask = 0x7F;
    private const int LowLightMask = 0x0F;
    private const int HighLightShift = 4;
    private const int LightTileMask = 4095;
    private const int TileLightShift = 12;

    public static void Apply(WorldMap map, ReadOnlySpan<byte> command)
    {
        if ((command[0] & SetMapFlag) != 0)
        {
            SetMap(map, command);
            return;
        }

        switch ((ServerCommand)command[0])
        {
            case ServerCommand.SetMap3:
                SetLight(map, command, LightPairs.SetMap3);
                break;
            case ServerCommand.SetMap4:
                SetLight(map, command, LightPairs.SetMap4);
                break;
            case ServerCommand.SetMap5:
                SetLight(map, command, LightPairs.SetMap5);
                break;
            case ServerCommand.SetMap6:
                SetLight(map, command, LightPairs.SetMap6);
                break;
            case ServerCommand.TileGlow:
                SetGlow(map, command);
                break;
            case ServerCommand.SetOrigin:
                SetOrigin(map, command);
                break;
            case ServerCommand.ScrollRight:
                ShiftTiles(map, 1);
                break;
            case ServerCommand.ScrollLeft:
                ShiftTiles(map, -1);
                break;
            case ServerCommand.ScrollDown:
                ShiftTiles(map, WorldMap.Size);
                break;
            case ServerCommand.ScrollUp:
                ShiftTiles(map, -WorldMap.Size);
                break;
            case ServerCommand.ScrollLeftUp:
                ShiftTiles(map, -(WorldMap.Size + 1));
                break;
            case ServerCommand.ScrollLeftDown:
                ShiftTiles(map, WorldMap.Size - 1);
                break;
            case ServerCommand.ScrollRightUp:
                ShiftTiles(map, -(WorldMap.Size - 1));
                break;
            case ServerCommand.ScrollRightDown:
                ShiftTiles(map, WorldMap.Size + 1);
                break;
            default:
                throw new UnreachableException();
        }
    }

    private static void SetMap(WorldMap map, ReadOnlySpan<byte> command)
    {
        var reader = new PacketReader(command);
        var tileOffset = reader.ReadByte() & SetMapTileOffsetMask;
        var fields = (SetMapFields)reader.ReadByte();
        var tileIndex = tileOffset != 0 ? map.LastUpdatedTile + tileOffset : reader.ReadUInt16();
        RequireTile(tileIndex, "SetMap");
        map.LastUpdatedTile = tileIndex;

        ref var tile = ref map.Tiles[tileIndex];
        if (fields.HasFlag(SetMapFields.Background))
        {
            tile.BackgroundSprite = reader.ReadUInt16();
        }

        if (fields.HasFlag(SetMapFields.Flags))
        {
            tile.Flags = reader.ReadUInt32();
        }

        if (fields.HasFlag(SetMapFields.Flags2))
        {
            tile.Flags2 = reader.ReadUInt32();
        }

        if (fields.HasFlag(SetMapFields.ItemSprite))
        {
            tile.ItemSprite = reader.ReadUInt16();
        }

        if (fields.HasFlag(SetMapFields.ItemStatus))
        {
            tile.ItemStatus = reader.ReadByte();
        }

        if (fields.HasFlag(SetMapFields.CharacterLook))
        {
            tile.CharacterSprite = reader.ReadUInt16();
            tile.CharacterStatus = reader.ReadByte();
            tile.CharacterStatusOffset = reader.ReadByte();
        }

        if (fields.HasFlag(SetMapFields.CharacterIdentity))
        {
            tile.CharacterNumber = reader.ReadUInt16();
            tile.CharacterId = reader.ReadUInt16();
            tile.CharacterSpeed = reader.ReadByte();
        }

        if (fields.HasFlag(SetMapFields.CharacterHealth))
        {
            tile.CharacterHealthPercent = reader.ReadByte();
        }
    }

    private static void SetLight(WorldMap map, ReadOnlySpan<byte> command, int pairedTiles)
    {
        var reader = new PacketReader(command);
        reader.Skip(1);
        var packedTile = reader.ReadUInt16();
        var tileIndex = packedTile & LightTileMask;
        RequireTile(tileIndex, "SetMap light");
        map.Tiles[tileIndex].Light = packedTile >> TileLightShift;

        for (var pairEnd = tileIndex + 2; pairEnd < tileIndex + pairedTiles + 2; pairEnd += 2)
        {
            var packed = reader.ReadByte();
            if (pairEnd < WorldMap.TileCount)
            {
                map.Tiles[pairEnd].Light = packed & LowLightMask;
                map.Tiles[pairEnd - 1].Light = packed >> HighLightShift;
            }
        }
    }

    private static void SetGlow(WorldMap map, ReadOnlySpan<byte> command)
    {
        var reader = new PacketReader(command);
        reader.Skip(1);
        var tileIndex = reader.ReadUInt16();
        var glow = reader.ReadByte();
        if (tileIndex < WorldMap.TileCount)
        {
            map.Tiles[tileIndex].Glow = glow;
        }
    }

    private static void SetOrigin(WorldMap map, ReadOnlySpan<byte> command)
    {
        var reader = new PacketReader(command);
        reader.Skip(1);
        var originX = reader.ReadInt16();
        var originY = reader.ReadInt16();
        for (var row = 0; row < WorldMap.Size; row++)
        {
            for (var column = 0; column < WorldMap.Size; column++)
            {
                ref var tile = ref map.Tiles[row * WorldMap.Size + column];
                tile.X = (ushort)(column + originX);
                tile.Y = (ushort)(row + originY);
            }
        }
    }

    private static void ShiftTiles(WorldMap map, int sourceOffset)
    {
        var count = WorldMap.TileCount - Math.Abs(sourceOffset);
        if (sourceOffset > 0)
        {
            Array.Copy(map.Tiles, sourceOffset, map.Tiles, 0, count);
        }
        else
        {
            Array.Copy(map.Tiles, 0, map.Tiles, -sourceOffset, count);
        }
    }

    private static void RequireTile(long tileIndex, string commandName)
    {
        if (tileIndex is < 0 or >= WorldMap.TileCount)
        {
            throw new ProtocolDesyncException(
                $"{commandName} names tile {tileIndex}, outside the {WorldMap.Size}x{WorldMap.Size} map.");
        }
    }

    [Flags]
    private enum SetMapFields : byte
    {
        Background = 1 << 0,
        Flags = 1 << 1,
        Flags2 = 1 << 2,
        ItemSprite = 1 << 3,
        ItemStatus = 1 << 4,
        CharacterLook = 1 << 5,
        CharacterIdentity = 1 << 6,
        CharacterHealth = 1 << 7,
    }
}
