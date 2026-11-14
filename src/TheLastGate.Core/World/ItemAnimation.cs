using System;
using TheLastGate.State;

namespace TheLastGate.World;

public static class ItemAnimation
{
    private const int LastStill = 1;
    private const int TickCounted = 21;
    private const int TickCountedFrameMask = 63;

    private static readonly ItemLoop[] Loops =
    [
        new(2, 5, 2),
        new(6, 7, 1),
        new(8, 15, 1),
        new(16, 20, 1),
    ];

    internal static void Advance(ref MapTile tile, int serverTick, int clientTick, Action<string> reportUnusualData)
    {
        if (tile.ItemSprite == 0)
        {
            tile.AnimatedItemSprite = 0;
            return;
        }

        var status = tile.ItemStatus;
        if (status <= LastStill)
        {
            tile.AnimatedItemSprite = tile.ItemSprite;
            return;
        }

        if (status == TickCounted)
        {
            tile.AnimatedItemSprite = tile.ItemSprite + (clientTick & TickCountedFrameMask);
            return;
        }

        foreach (var loop in Loops)
        {
            if (status < loop.FirstStatus || status > loop.LastStatus)
            {
                continue;
            }

            tile.AnimatedItemSprite = tile.ItemSprite + (status - loop.FirstStatus) * loop.SpritesPerFrame;
            if ((serverTick - 1) % 2 != 0)
            {
                tile.ItemStatus = status == loop.LastStatus ? loop.FirstStatus : status + 1;
            }

            return;
        }

        reportUnusualData($"An item has status {status}, which the current client doesn't animate.");
        tile.AnimatedItemSprite = tile.ItemSprite;
    }

    private readonly record struct ItemLoop(int FirstStatus, int LastStatus, int SpritesPerFrame);
}
