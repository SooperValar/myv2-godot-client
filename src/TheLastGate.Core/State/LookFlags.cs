using System;

namespace TheLastGate.State;

[Flags]
public enum LookFlags
{
    None = 0,
    Shop = 1,
    Shadow = 2,
    Bloody = 4,
    Grave = 8,
}
