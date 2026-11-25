using TheLastGate.Net;
using TheLastGate.State;

namespace TheLastGate.Windows;

// However the trash window goes away (the close button, Escape, the pause, another window), the server has to hear it
// closed, or the items in it wait there until the next login.
public sealed class TrashWatch
{
    private int? openTrash;

    public byte[]? Follow(ShopKind? openShop, int shopNumber)
    {
        int? nowOpen = openShop == ShopKind.Trash ? shopNumber : null;
        var closed = openTrash is { } was && nowOpen != was ? ShopPackets.CloseTrash(was) : null;
        openTrash = nowOpen;
        return closed;
    }
}
