using System;
using System.Diagnostics;
using TheLastGate.Net.Handlers;
using TheLastGate.Profiling;
using TheLastGate.State;

namespace TheLastGate.Net;

public sealed class TickProcessor(GameState state)
{
    private const byte SetMapFlag = 0x80;

    private readonly Action<string> reportUnusualData = state.ReportUnusualData;

    public void Process(ReadOnlySpan<byte> payload)
    {
        using var timing = Timings.Measure("tick.process");
        state.ServerTick = (state.ServerTick + 1) % GameState.ServerTickCycle;
        state.Map.LastUpdatedTile = -1;
        var reader = new TickCommandReader(payload);
        while (reader.TryReadNext(out var command))
        {
            if ((command[0] & SetMapFlag) != 0)
            {
                MapCommands.Apply(state.Map, command);
                continue;
            }

            switch ((ServerCommand)command[0])
            {
                case ServerCommand.SetCharacterName1
                    or ServerCommand.SetCharacterName2
                    or ServerCommand.SetCharacterName3
                    or ServerCommand.SetCharacterMode
                    or ServerCommand.SetCharacterAttribute
                    or ServerCommand.SetCharacterSkill
                    or ServerCommand.SetCharacterHitPoints
                    or ServerCommand.SetCharacterEndurance
                    or ServerCommand.SetCharacterMana
                    or ServerCommand.SetCharacterActiveHitPoints
                    or ServerCommand.SetCharacterActiveEndurance
                    or ServerCommand.SetCharacterActiveMana
                    or ServerCommand.SetCharacterDirection
                    or ServerCommand.SetCharacterPoints
                    or ServerCommand.SetCharacterGold:
                    PlayerCommands.Apply(state.Player, command);
                    break;
                case ServerCommand.SetTarget:
                    var previousTarget = state.Player.Target;
                    PlayerCommands.Apply(state.Player, command);
                    state.ReportTargetChanged(previousTarget, state.Player.Target);
                    break;
                case ServerCommand.SetCharacterItem
                    or ServerCommand.SetCharacterWorn
                    or ServerCommand.SetCharacterSpell:
                    ItemCommands.Apply(state.Player, state.ItemLooks, state.BuffWatch, command);
                    break;
                case ServerCommand.SetCharacterHeldItem:
                    ItemCommands.Apply(state.Player, state.ItemLooks, state.BuffWatch, command);
                    state.ReportHeldItemChanged();
                    break;
                case ServerCommand.SetMap3
                    or ServerCommand.SetMap4
                    or ServerCommand.SetMap5
                    or ServerCommand.SetMap6
                    or ServerCommand.SetOrigin
                    or ServerCommand.TileGlow
                    or ServerCommand.ScrollRight
                    or ServerCommand.ScrollLeft
                    or ServerCommand.ScrollDown
                    or ServerCommand.ScrollUp
                    or ServerCommand.ScrollLeftUp
                    or ServerCommand.ScrollLeftDown
                    or ServerCommand.ScrollRightUp
                    or ServerCommand.ScrollRightDown:
                    MapCommands.Apply(state.Map, command);
                    break;
                case >= ServerCommand.Log0 and <= ServerCommand.Log3:
                    LogCommands.Apply(
                        state.Chat,
                        command,
                        command[0] - (byte)ServerCommand.Log0,
                        line => state.BuffWatch.Capture(line) || state.ItemLooks.Offer(line));
                    break;
                case ServerCommand.Look1
                    or ServerCommand.Look2
                    or ServerCommand.Look3
                    or ServerCommand.Look4
                    or ServerCommand.Look5
                    or ServerCommand.Look6:
                    LookCommands.Apply(state, command);
                    break;
                case ServerCommand.PlaySound
                    or ServerCommand.Exit
                    or ServerCommand.Load
                    or ServerCommand.Unique
                    or ServerCommand.Ignore:
                    RequestCommands.Apply(state, command);
                    break;
                case ServerCommand.SpellTime
                    or ServerCommand.ItemInfo
                    or ServerCommand.Tiers
                    or ServerCommand.ItemMarks
                    or ServerCommand.WaypointRow:
                    ServerInfoCommands.Apply(state, command);
                    break;
                case ServerCommand.Tick:
                    var tickReader = new PacketReader(command);
                    tickReader.Skip(1);
                    state.ServerTick = tickReader.ReadByte();
                    // The server only sends Tick when a login starts the tick stream.
                    state.ItemLooks.Reset();
                    state.BuffWatch.Reset();
                    break;
                default:
                    throw new UnreachableException();
            }
        }

        state.ItemLooks.EndOfTick();
        state.BuffWatch.EndOfTick();
        state.WatchViewCentre();
        state.NameRequests.EndOfTick(state.Map, state.Names);
        // Before the shop refresh: the end of a paused tick can close the shop, and a closed shop isn't refreshed.
        state.ReportTickEnded();
        state.ShopRefresh.EndOfTick(state.OpenShop, state.Shop.Number);
        var tiles = state.Map.Tiles;
        for (var tileIndex = 0; tileIndex < tiles.Length; tileIndex++)
        {
            CharacterStatuses.Advance(ref tiles[tileIndex], state.ServerTick, reportUnusualData);
        }

        state.TickReports.TickPlayed();
    }
}
