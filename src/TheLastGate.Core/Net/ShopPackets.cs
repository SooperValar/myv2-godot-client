namespace TheLastGate.Net;

public static class ShopPackets
{
    private const int DescribeOffset = 62;
    private const int BuyTenOffset = 124;
    private const int PutInTrashOffset = 1000;
    private const int EmptyTrashValue = 2000;
    private const int CloseTrashValue = 3000;

    public static byte[] Buy(int shopNumber, int slot) => Shop(shopNumber, slot);

    public static byte[] BuyTen(int shopNumber, int slot) => Shop(shopNumber, slot + BuyTenOffset);

    public static byte[] Describe(int shopNumber, int slot) => Shop(shopNumber, slot + DescribeOffset);

    public static byte[] PutInTrash(int trashNumber, int inventorySlot) =>
        Shop(trashNumber, inventorySlot + PutInTrashOffset);

    public static byte[] EmptyTrash(int trashNumber) => Shop(trashNumber, EmptyTrashValue);

    // The server gives back whatever is still in the trash window when it hears the window closed.
    public static byte[] CloseTrash(int trashNumber) => Shop(trashNumber, CloseTrashValue);

    private static byte[] Shop(int shopNumber, int value) =>
        ClientPackets.ShortAndInt(ClientCommand.Shop, shopNumber, value);
}
