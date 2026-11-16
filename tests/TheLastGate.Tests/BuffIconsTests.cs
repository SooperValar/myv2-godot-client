using TheLastGate.Hud;
using TheLastGate.State;

namespace TheLastGate.Tests;

public class BuffIconsTests
{
    private const int Buff = 1000;
    private const int Debuff = 89;
    private const int StillFresh = 20;

    [Theory]
    [InlineData(89)]
    [InlineData(1015)]
    [InlineData(3494)]
    [InlineData(6721)]
    [InlineData(6745)]
    [InlineData(6761)]
    [InlineData(6780)]
    [InlineData(16860)]
    [InlineData(16863)]
    public void IsDebuff_KnowsTheCurrentClientsDebuffSprites(int sprite)
    {
        Assert.True(BuffIcons.IsDebuff(sprite));
    }

    [Theory]
    [InlineData(90)]
    [InlineData(6720)]
    [InlineData(6746)]
    [InlineData(6760)]
    [InlineData(6781)]
    [InlineData(16864)]
    public void IsDebuff_TreatsEverythingElseAsABuff(int sprite)
    {
        Assert.False(BuffIcons.IsDebuff(sprite));
    }

    [Fact]
    public void For_SortsSlotsIntoBuffsAndDebuffsInSlotOrder()
    {
        BuffSlot[] slots =
        [
            new(Buff, StillFresh), new(Debuff, StillFresh), new(Buff + 1, StillFresh), new(Debuff + 2, StillFresh),
        ];

        var icons = BuffIcons.For(slots);

        Assert.Equal([Buff, Buff + 1], icons.Buffs.Select(icon => icon.Sprite));
        Assert.Equal([Debuff, Debuff + 2], icons.Debuffs.Select(icon => icon.Sprite));
    }

    [Fact]
    public void For_SkipsEmptySlots()
    {
        BuffSlot[] slots = [new(0, 0), new(Buff, StillFresh), new(-1, 0)];

        var icons = BuffIcons.For(slots);

        Assert.Equal([Buff], icons.Buffs.Select(icon => icon.Sprite));
        Assert.Empty(icons.Debuffs);
    }

    [Theory]
    [InlineData(15, 1f)]
    [InlineData(100, 1f)]
    [InlineData(14, 120f / 121f)]
    [InlineData(5, 120f / 220f)]
    [InlineData(0, 120f / 345f)]
    public void For_DarkensIconsAsTheyRunOut(int active, float brightness)
    {
        Assert.Equal(brightness, BuffIcons.For([new BuffSlot(Buff, active)]).Buffs[0].Brightness, 6);
    }
}
