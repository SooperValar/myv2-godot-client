using System;
using System.Diagnostics;
using TheLastGate.Net.Handlers;
using TheLastGate.State;

namespace TheLastGate.Net;

public sealed class TickProcessor(GameState state)
{
    private const byte SetMapFlag = 0x80;

    public void Process(ReadOnlySpan<byte> payload)
    {
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
                    or ServerCommand.SetCharacterWaypoints
                    or ServerCommand.SetCharacterTokens
                    or ServerCommand.SetCharacterTree
                    or ServerCommand.SetCharacterGold
                    or ServerCommand.SetCharacterLocation1
                    or ServerCommand.SetCharacterLocation2
                    or ServerCommand.SetTarget:
                    PlayerCommands.Apply(state.Player, command);
                    break;
                case ServerCommand.SetCharacterItem
                    or ServerCommand.SetCharacterWorn
                    or ServerCommand.SetCharacterSpell
                    or ServerCommand.SetCharacterHeldItem:
                    ItemCommands.Apply(state.Player, command);
                    break;
                case ServerCommand.SetMap3
                    or ServerCommand.SetMap4
                    or ServerCommand.SetMap5
                    or ServerCommand.SetMap6
                    or ServerCommand.SetOrigin
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
                case >= ServerCommand.Log0 and <= ServerCommand.Log9:
                    LogCommands.Apply(state.Chat, command, command[0] - (byte)ServerCommand.Log0);
                    break;
                case >= ServerCommand.MessageOfTheDay0 and <= ServerCommand.MessageOfTheDay3:
                    LogCommands.Apply(
                        state.MessageOfTheDay,
                        command,
                        command[0] - (byte)ServerCommand.MessageOfTheDay0);
                    break;
                case ServerCommand.TermSkillTree
                    or ServerCommand.TermContractTree
                    or ServerCommand.TermSkills
                    or ServerCommand.TermMeta:
                    TerminologyCommands.Apply(state.Terminology, command);
                    break;
                case ServerCommand.Look1
                    or ServerCommand.Look2
                    or ServerCommand.Look3
                    or ServerCommand.Look4
                    or ServerCommand.Look5
                    or ServerCommand.Look6
                    or ServerCommand.Look7
                    or ServerCommand.Look8
                    or ServerCommand.CloseShop:
                    LookCommands.Apply(state, command);
                    break;
                case ServerCommand.PlaySound
                    or ServerCommand.Exit
                    or ServerCommand.Load
                    or ServerCommand.Unique
                    or ServerCommand.Ignore
                    or ServerCommand.Waypoints
                    or ServerCommand.ShowMessageOfTheDay
                    or ServerCommand.ClearBox:
                    RequestCommands.Apply(state, command);
                    break;
                case ServerCommand.Tick:
                    var tickReader = new PacketReader(command);
                    tickReader.Skip(1);
                    state.ServerTick = tickReader.ReadByte();
                    break;
                default:
                    throw new UnreachableException();
            }
        }
    }
}
