using System;

namespace TheLastGate.State;

// The server's own map flags, which it sends only to builders.
[Flags]
public enum TileRules : uint
{
    None = 0,
    MoveBlock = 1u << 0,
    SightBlock = 1u << 1,
    Indoors = 1u << 2,
    Underwater = 1u << 3,
    NoLag = 1u << 4,
    NoMonsters = 1u << 5,
    Bank = 1u << 6,
    Tavern = 1u << 7,
    NoMagic = 1u << 8,
    DeathTrap = 1u << 9,
    NoPlayers = 1u << 10,
    Arena = 1u << 11,
    NoExpire = 1u << 13,
    NoFight = 1u << 14,
    UnnamedTopBit = 1u << 31,
}
