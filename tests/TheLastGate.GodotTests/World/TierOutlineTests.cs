using GdUnit4;
using TheLastGate.World;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.World;

[TestSuite]
public class TierOutlineTests
{
    [TestCase(3, 3)]
    [TestCase(5, 5)]
    [TestCase(8 + 2, 2)]
    [TestCase(8, 0)]
    public void OfObject_IsTheLowThreeBits(int tileGlow, int outline)
    {
        AssertThat(TierOutline.OfObject(tileGlow)).IsEqual(outline);
    }

    [TestCase(8, TierOutline.HoldsItems)]
    [TestCase(8 + 4, TierOutline.HoldsItems)]
    [TestCase(4, TierOutline.None)]
    public void OfCharacter_IsABodyHoldingItems(int tileGlow, int outline)
    {
        AssertThat(TierOutline.OfCharacter(tileGlow)).IsEqual(outline);
    }

    [TestCase(0)]
    [TestCase(6)]
    public void For_HasNoColourOutsideTheTiers(int glow)
    {
        AssertThat(TierOutline.For(glow).A).IsEqual(0f);
    }
}
