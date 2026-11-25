using TheLastGate.Net;
using TheLastGate.State;

namespace TheLastGate.Tests;

public class ShopCommandsTests
{
    private const int ShopNumber = 1201;
    private const uint SoulstoneBit = 1u << 30;
    private const uint EnchantedBit = 1u << 31;

    [Fact]
    public void Shop_FillsTwoSlotsPerCommandAndOpensAfterTheLast()
    {
        var state = new GameState();
        var opened = new List<ShopKind>();
        state.ShopOpened += opened.Add;

        Process(state, [.. ShopLookCommands(), .. ShopItemCommands(0)]);

        Assert.Equal([ShopKind.Merchant], opened);
        Assert.Equal(ShopKind.Merchant, state.OpenShop);
        Assert.Equal(new ShopItem(1010, 100, false, false, 0), state.Shop.ShopItems[10]);
        Assert.Equal(new ShopItem(1011, 110, false, false, 1), state.Shop.ShopItems[11]);
        Assert.Equal(ShopNumber, state.Shop.Number);
        Assert.Equal(250, state.Shop.HeldItemPrice);
        Assert.Null(state.Look);
    }

    [Fact]
    public void Shop_OpensNothingBeforeTheLastPair()
    {
        var state = new GameState();

        Process(state, ShopItemCommands(0)[..30]);

        Assert.Null(state.OpenShop);
    }

    [Theory]
    [InlineData(0, ShopKind.Merchant)]
    [InlineData(101, ShopKind.BlackStronghold)]
    [InlineData(102, ShopKind.Casino)]
    [InlineData(103, ShopKind.Contract)]
    [InlineData(104, ShopKind.Experience)]
    public void Shop_TakesItsKindFromTheLastPair(byte kindByte, ShopKind kind)
    {
        var state = new GameState();

        Process(state, ShopItemCommands(kindByte));

        Assert.Equal(kind, state.OpenShop);
    }

    [Fact]
    public void Shop_SplitsTheSoulstoneAndEnchantedBitsOffThePrice()
    {
        var state = new GameState();
        var firstPair = TestCommand.Of(ServerCommand.Look6)
            .Byte(0)
            .UInt16(1000)
            .UInt32(500 | SoulstoneBit)
            .UInt16(1001)
            .UInt32(600 | EnchantedBit)
            .Bytes(0, 0);

        Process(state, [firstPair, .. ShopItemCommands(0)[1..]]);

        Assert.Equal(new ShopItem(1000, 500, true, false, 0), state.Shop.ShopItems[0]);
        Assert.Equal(new ShopItem(1001, 600, false, true, 0), state.Shop.ShopItems[1]);
    }

    [Fact]
    public void Shop_KeepsTheOpenShopWhenTheNextShopStarts()
    {
        var state = new GameState();
        Process(state, ShopItemCommands(0));

        Process(state, TestCommand.Of(ServerCommand.Look6).Byte(0).UInt16(9999));

        Assert.Equal(1000, state.Shop.ShopItems[0].Sprite);
    }

    [Theory]
    [InlineData(0x4000 | 12, ShopKind.Trash)]
    [InlineData(0x8000 | 12, ShopKind.Bank)]
    [InlineData(0xC000 | 12, ShopKind.Bank)]
    [InlineData(12, ShopKind.Merchant)]
    public void Shop_IsMyV2sTrashOrDepotWindowByItsNumber(int shopNumber, ShopKind kind)
    {
        var state = new GameState();

        Process(state, [.. ShopLookCommands((ushort)shopNumber), .. ShopItemCommands(0)]);

        Assert.Equal(kind, state.OpenShop);
    }

    private static TestCommand[] ShopLookCommands(ushort shopNumber = ShopNumber) =>
    [
        TestCommand.Of(ServerCommand.Look1),
        TestCommand.Of(ServerCommand.Look2),
        TestCommand.Of(ServerCommand.Look3).UInt16(0).UInt16(0).UInt16(0).UInt16(shopNumber).UInt16(7),
        TestCommand.Of(ServerCommand.Look4).UInt16(0).UInt16(0).Byte((byte)LookFlags.Shop).UInt32(250),
        TestCommand.Of(ServerCommand.Look5).Text("Merchant", 15),
    ];

    private static TestCommand[] ShopItemCommands(byte kindByte) =>
        Enumerable.Range(0, CharacterLook.ShopSlots / 2)
            .Select(pair => pair * 2)
            .Select(first => TestCommand.Of(ServerCommand.Look6)
                .Byte((byte)first)
                .UInt16((ushort)(1000 + first))
                .UInt32((uint)first * 10)
                .UInt16((ushort)(1001 + first))
                .UInt32((uint)(first + 1) * 10)
                .Bytes(first == 60 ? kindByte : (byte)(first % 5), (byte)((first + 1) % 5)))
            .ToArray();

    private static void Process(GameState state, params TestCommand[] commands) =>
        new TickProcessor(state).Process(TestCommand.Payload(commands));
}
