using GdUnit4;
using TheLastGate.Hud;
using TheLastGate.State;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Hud;

[TestSuite]
public class BuffIconsTests
{
    private const int Buff = 1000;
    private const int Debuff = 89;
    private const int StillFresh = 20;
    private const int DecimalPlaces = 6;

    [TestCase(89)]
    [TestCase(1015)]
    [TestCase(3494)]
    [TestCase(6721)]
    [TestCase(6745)]
    [TestCase(6761)]
    [TestCase(6780)]
    [TestCase(16860)]
    [TestCase(16863)]
    public void IsDebuff_KnowsTheCurrentClientsDebuffSprites(int sprite)
    {
        AssertThat(BuffIcons.IsDebuff(sprite)).IsTrue();
    }

    [TestCase(90)]
    [TestCase(6720)]
    [TestCase(6746)]
    [TestCase(6760)]
    [TestCase(6781)]
    [TestCase(16864)]
    public void IsDebuff_TreatsEverythingElseAsABuff(int sprite)
    {
        AssertThat(BuffIcons.IsDebuff(sprite)).IsFalse();
    }

    [TestCase]
    public void For_SortsSlotsIntoBuffsAndDebuffsInSlotOrder()
    {
        BuffSlot[] slots =
        [
            new(Buff, StillFresh), new(Debuff, StillFresh), new(Buff + 1, StillFresh), new(Debuff + 2, StillFresh),
        ];

        var icons = BuffIcons.For(slots);

        AssertThat(icons.Buffs.Select(icon => icon.Sprite)).ContainsExactly(Buff, Buff + 1);
        AssertThat(icons.Debuffs.Select(icon => icon.Sprite)).ContainsExactly(Debuff, Debuff + 2);
    }

    [TestCase]
    public void For_SkipsEmptySlots()
    {
        BuffSlot[] slots = [new(0, 0), new(Buff, StillFresh), new(-1, 0)];

        var icons = BuffIcons.For(slots);

        AssertThat(icons.Buffs.Select(icon => icon.Sprite)).ContainsExactly(Buff);
        AssertThat(icons.Debuffs).IsEmpty();
    }

    [TestCase(15, 1f)]
    [TestCase(100, 1f)]
    [TestCase(14, 120f / 121f)]
    [TestCase(5, 120f / 220f)]
    [TestCase(0, 120f / 345f)]
    public void For_DarkensIconsAsTheyRunOut(int active, float brightness)
    {
        AssertThat(MathF.Round(BuffIcons.For([new BuffSlot(Buff, active)]).Buffs[0].Brightness, DecimalPlaces))
            .IsEqual(MathF.Round(brightness, DecimalPlaces));
    }

    [TestCase(3725, "1h", "1:02:05 left")]
    [TestCase(272, "4m", "4:32 left")]
    [TestCase(52, "52s", "0:52 left")]
    [TestCase(0, "0s", "0:00 left")]
    public void Times_ShowHoursMinutesOrSeconds(int seconds, string shortTime, string longTime)
    {
        var icon = new BuffIcon(Buff, 1f, "Bless", seconds);

        AssertThat(icon.ShortTime).IsEqual(shortTime);
        AssertThat(icon.LongTime).IsEqual(longTime);
    }

    [TestCase(9, true)]
    [TestCase(10, false)]
    public void RunningOut_UnderTenSeconds(int seconds, bool runningOut)
    {
        AssertThat(new BuffIcon(Buff, 1f, "Bless", seconds).RunningOut).IsEqual(runningOut);
    }

    [TestCase]
    public void Times_AreEmptyWithoutATimer()
    {
        var icon = new BuffIcon(Buff, 1f);

        AssertThat(icon.ShortTime).IsEmpty();
        AssertThat(icon.RunningOut).IsFalse();
    }
}
