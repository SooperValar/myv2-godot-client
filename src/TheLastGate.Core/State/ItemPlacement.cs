using System;

namespace TheLastGate.State;

[Flags]
public enum ItemPlacement
{
    None = 0,
    Head = 1,
    Neck = 2,
    Body = 4,
    Arms = 8,
    Belt = 32,
    Legs = 64,
    Feet = 128,
    Weapon = 256,
    Shield = 512,
    Cloak = 1024,
    TwoHanded = 2048,
    Ring = 4096,
    Corrupted = 8192,
    Soulstoned = 16384,
    Enchanted = 32768,
}
