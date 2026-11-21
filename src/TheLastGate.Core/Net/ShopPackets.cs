namespace TheLastGate.Net;

public static class ShopPackets
{
    private const int DescribeOffset = 62;
    private const int BuyTenOffset = 124;

    public static byte[] Buy(int shopNumber, int slot) => Shop(shopNumber, slot);

    public static byte[] BuyTen(int shopNumber, int slot) => Shop(shopNumber, slot + BuyTenOffset);

    public static byte[] Describe(int shopNumber, int slot) => Shop(shopNumber, slot + DescribeOffset);

    private static byte[] Shop(int shopNumber, int value) =>
        ClientPackets.ShortAndInt(ClientCommand.Shop, shopNumber, value);
}
