using System;

namespace TheLastGate.World;

[Flags]
public enum MapFlags : uint
{
    None = 0,
    Injured = 1u << 0,
    Injured1 = 1u << 1,
    Injured2 = 1u << 2,
    Stoned = 1u << 3,
    Infrared = 1u << 4,
    Underwater = 1u << 5,
    Bloody = 1u << 6,
    IsUsable = 1u << 7,
    IsItem = 1u << 8,
    IsCharacter = 1u << 9,
    Invisible = 1u << 10,
    Stunned = 1u << 11,
    Tomb = 0x1Fu << 12,
    Death = 0x1Fu << 17,
    CriticallyHit = 1u << 31,
}
