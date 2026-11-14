using System;
using TheLastGate.Net.Handlers;
using TheLastGate.State;

namespace TheLastGate.Net;

public sealed class TickProcessor(GameState state)
{
    private const byte SetMapFlag = 0x80;

    public void Process(ReadOnlySpan<byte> payload)
    {
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
                    or ServerCommand.SetCharacterLocation2:
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
            }
        }
    }
}
