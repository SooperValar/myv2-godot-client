using System;
using TheLastGate.Net;

namespace TheLastGate.Windows;

// The first click only arms the button; a second one while it is armed destroys what is in the trash window.
public sealed class TrashButton
{
    public static readonly TimeSpan ArmedFor = TimeSpan.FromSeconds(10);

    private TimeSpan? armedAt;

    public bool IsArmed(TimeSpan now) => armedAt is { } at && now - at < ArmedFor;

    public byte[]? Click(TimeSpan now, int trashNumber, int itemCount)
    {
        if (itemCount == 0)
        {
            armedAt = null;
            return null;
        }

        if (IsArmed(now))
        {
            armedAt = null;
            return ShopPackets.EmptyTrash(trashNumber);
        }

        armedAt = now;
        return null;
    }

    public void Disarm() => armedAt = null;
}
