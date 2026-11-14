using System;

namespace TheLastGate.World;

[Flags]
public enum MapFlags : uint
{
    None = 0,
    Stoned = 1u << 3,
    Infrared = 1u << 4,
    Underwater = 1u << 5,
    Bloody = 1u << 6,
    IsItem = 1u << 8,
    Invisible = 1u << 10,
    Stunned = 1u << 11,
}
