namespace TheLastGate.State;

public readonly record struct ShopItem(int Sprite, int Price, bool IsSoulstone, bool IsEnchanted, byte Catalyst);
