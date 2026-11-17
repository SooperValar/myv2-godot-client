using System;
using System.Collections.Generic;
using TheLastGate.State;
using TheLastGate.World;

namespace TheLastGate.Windows;

public static class SmithClicks
{
    public const int Slots = 4;
    public const int FirstButton = 4;
    public const int LastButton = 6;

    public static bool UsesTwoMaterials(IReadOnlyList<ItemSlot> items) => throw new NotImplementedException();

    public static bool ShowsOuterButtons(IReadOnlyList<ItemSlot> items) => throw new NotImplementedException();

    public static byte[]? ItemPacket(MapButton button, MapKeys keys, int shopNumber, int slot) =>
        throw new NotImplementedException();

    public static byte[]? ButtonPacket(MapButton button, int shopNumber, int buttonNumber) =>
        throw new NotImplementedException();
}
