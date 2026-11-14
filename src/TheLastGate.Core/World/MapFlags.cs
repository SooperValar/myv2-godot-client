using System;

namespace TheLastGate.World;

[Flags]
public enum MapFlags : uint
{
    None = 0,
    IsItem = 1u << 8,
    Invisible = 1u << 10,
}
