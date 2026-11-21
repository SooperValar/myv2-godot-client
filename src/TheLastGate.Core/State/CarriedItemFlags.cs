using System;

namespace TheLastGate.State;

[Flags]
public enum CarriedItemFlags
{
    None = 0,
    Locked = 1,
    Soulstoned = 2,
    Enchanted = 4,
    Corrupted = 8,
}
