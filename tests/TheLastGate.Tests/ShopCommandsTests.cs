using TheLastGate.Net;
using TheLastGate.State;

namespace TheLastGate.Tests;

public class ShopCommandsTests
{
    private const int ShopNumber = 1201;
    private const int SmithNumber = 1302;
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

    [Fact]
    public void Depot_FillsASlotAndOpensAfterTheLastOne()
    {
        var state = new GameState();
        var opened = new List<ShopKind>();
        state.ShopOpened += opened.Add;

        Process(
            state,
            TestCommand.Of(ServerCommand.Look7).Bytes(2, 5).UInt16(777).Bytes(3, 1, 4),
            TestCommand.Of(ServerCommand.Look7).Bytes(7, 63).UInt16(888).Bytes(1, 0, 0));

        Assert.Equal([ShopKind.Depot], opened);
        Assert.Equal(ShopKind.Depot, state.OpenShop);
        Assert.Equal(new DepotItem(777, 3, 1, 4), state.Shop.Depot[2][5]);
        Assert.Equal(new DepotItem(888, 1, 0, 0), state.Shop.Depot[7][63]);
    }

    [Fact]
    public void Depot_IgnoresASlotOutsideTheDepot()
    {
        var state = new GameState();

        Process(
            state,
            TestCommand.Of(ServerCommand.Look7).Bytes(8, 0).UInt16(777).Bytes(3, 1, 4),
            TestCommand.Of(ServerCommand.Look7).Bytes(0, 64).UInt16(777).Bytes(3, 1, 4),
            TestCommand.Of(ServerCommand.Look7).Bytes(7, 63).UInt16(888).Bytes(1, 0, 0));

        Assert.Equal(ShopKind.Depot, state.OpenShop);
        Assert.Single(state.Shop.Depot.SelectMany(page => page), item => item.Sprite != 0);
    }

    [Theory]
    [InlineData(4, ShopKind.WeaponSmith)]
    [InlineData(5, ShopKind.ArmourSmith)]
    public void Smith_OpensTheWeaponOrArmourSmith(byte slot, ShopKind kind)
    {
        var state = new GameState();
        var opened = new List<ShopKind>();
        state.ShopOpened += opened.Add;

        Process(state, SmithCommand(slot, 0, 0, 0));

        Assert.Equal([kind], opened);
        Assert.Equal(kind, state.OpenShop);
        Assert.Equal(SmithNumber, state.Shop.Number);
    }

    [Fact]
    public void Smith_SetsAnItemSlotWithoutOpening()
    {
        var state = new GameState();

        Process(state, SmithCommand(2, 4321, 3, 1));

        Assert.Equal(new ItemSlot(4321, 0, 3, 1), state.Player.SmithItems[2]);
        Assert.Null(state.OpenShop);
    }

    [Fact]
    public void Smith_IgnoresASlotPastTheArmourSmith()
    {
        var state = new GameState();

        Process(state, SmithCommand(6, 4321, 3, 1));

        Assert.Null(state.OpenShop);
        Assert.All(state.Player.SmithItems, item => Assert.Equal(default, item));
    }

    [Fact]
    public void CloseShop_ClosesTheOpenShop()
    {
        var state = new GameState();
        var closes = new List<bool>();
        state.ShopClosed += closes.Add;
        Process(state, SmithCommand(4, 0, 0, 0));

        Process(state, TestCommand.Of(ServerCommand.CloseShop));

        Assert.Null(state.OpenShop);
        Assert.Equal([true], closes);
    }

    private static TestCommand[] ShopLookCommands() =>
    [
        TestCommand.Of(ServerCommand.Look1),
        TestCommand.Of(ServerCommand.Look2),
        TestCommand.Of(ServerCommand.Look3).UInt16(0).UInt16(0).UInt16(0).UInt16(ShopNumber).UInt16(7),
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

    private static TestCommand SmithCommand(byte slot, ushort sprite, byte stack, byte flags) =>
        TestCommand.Of(ServerCommand.Look8).Byte(slot).UInt16(SmithNumber).UInt16(sprite).Bytes(stack, flags);

    private static void Process(GameState state, params TestCommand[] commands) =>
        new TickProcessor(state).Process(TestCommand.Payload(commands));
}
