using TheLastGate.Buffs;
using TheLastGate.Planner;

namespace TheLastGate.Tests;

public class AdvancedSheetTests
{
    private const uint SeyanDu = 1u << 1;
    private readonly PlannerChoices choices = new();

    private readonly PlannerGame game = PlannerSheetTests.Game();

    [Fact]
    public void Spells_ListOnlyTheSpellsYouKnow()
    {
        var spell = Assert.Single(Sheet().Spells);

        Assert.Equal(new AdvancedSpell(BuffKind.Bless, "Bless", true, null), spell);
    }

    [Fact]
    public void ASpellOnYouWaitsForItsReading()
    {
        game.BuffActive[(int)BuffKind.Bless] = true;

        Assert.Equal(
            new AdvancedSpell(BuffKind.Bless, "Bless (reading buffs...)", false, null),
            Assert.Single(Sheet().Spells));
    }

    [Fact]
    public void WithoutTheSpellStats_NoSpellCanBeToggled()
    {
        game.MetaStats.Clear();

        Assert.False(Assert.Single(Sheet().Spells).Ready);
    }

    [Fact]
    public void AWeakerCastOfYourOwn_WarnsThatItWontReplaceTheActiveBuff()
    {
        game.BuffActive[(int)BuffKind.Bless] = true;
        game.BuffReadings[(int)BuffKind.Bless] = new BuffReading { Known = true, Power = 90 };

        Assert.Equal(
            "Your Bless (56) won't replace the active one (90)",
            Assert.Single(Sheet().Spells).Warning);
    }

    [Fact]
    public void AStrongerCastOrASpellToggledOff_HasNoWarning()
    {
        game.BuffActive[(int)BuffKind.Bless] = true;
        game.BuffReadings[(int)BuffKind.Bless] = new BuffReading { Known = true, Power = 40 };
        Assert.Null(Assert.Single(Sheet().Spells).Warning);

        game.BuffReadings[(int)BuffKind.Bless] = new BuffReading { Known = true, Power = 90 };
        game.Choices.SpellToggles = 0;
        Assert.Null(Assert.Single(Sheet().Spells).Warning);
    }

    [Fact]
    public void TreeRows_AreTheLearnedNodesInTheMathAndTheCorruptors()
    {
        game.Kindred = SeyanDu;
        game.TreePoints = TreeBit(5) | TreeBit(6) | TreeBit(7) | TreeBit(8) | TreeBit(9);
        game.TreeNodeNames[4] = "Node Five";
        game.TreeNodes[5] = 20;
        game.TreeNodes[6] = 66;
        game.TreeNodes[7] = 250;
        game.TreeNodes[8] = 20;

        var sheet = Sheet();

        Assert.Equal(
            ["Node Five", "False Strength", "False Tactician (not modelled)", "False Strength"],
            sheet.TreeRows);
        Assert.Equal(2, sheet.TreeCopies[(int)Corruptor.FalseStrength]);
        Assert.Equal(1, sheet.TreeCopies[(int)Corruptor.FalseTactician]);
        Assert.Equal(0, sheet.TreeCopies[(int)Corruptor.FalseAgility]);
    }

    [Theory]
    [InlineData(TarotCard.Magician, "I The Magician")]
    [InlineData(TarotCard.FoolReversed, "0 The Fool, Reversed")]
    [InlineData(TarotCard.MoonReversed, "XVIII The Moon, Reversed")]
    public void CardLabels_StartWithTheCardsNumeral(TarotCard card, string label)
    {
        Assert.Equal(label, AdvancedSheet.CardLabel(card));
    }

    [Fact]
    public void EffectTexts_SayWhenTheModelLeavesSomethingOut()
    {
        var unmodelledCorruptor = TreeEffects.Corruptors.ToList().FindIndex(info => !info.Modelled);
        var unmodelledGear = ItemEffects.Gears.ToList().FindIndex(info => !info.Modelled);

        Assert.Equal("+3% Strength", AdvancedSheet.CorruptorEffect((int)Corruptor.FalseStrength));
        Assert.Equal("(not modelled)", AdvancedSheet.CorruptorEffect(unmodelledCorruptor));
        Assert.Equal("+4% Wil & Int", AdvancedSheet.GearEffect(0));
        Assert.Equal(
            $"{ItemEffects.Gears[unmodelledGear].Effect} (not modelled)",
            AdvancedSheet.GearEffect(unmodelledGear));
        Assert.Equal("+2 tree", AdvancedSheet.TreeCopiesText(2));
        Assert.Equal("", AdvancedSheet.TreeCopiesText(0));
    }

    [Fact]
    public void ToggleSpellAndCard_FlipTheirBits()
    {
        choices.ToggleSpell(BuffKind.Protect);
        choices.ToggleCard(TarotCard.Hermit);
        Assert.Equal(1u << (int)BuffKind.Protect, choices.SpellToggles);
        Assert.Equal(1ul << (int)TarotCard.Hermit, choices.TarotCards);

        choices.ToggleSpell(BuffKind.Protect);
        choices.ToggleCard(TarotCard.Hermit);
        Assert.Equal((0u, 0ul), (choices.SpellToggles, choices.TarotCards));
    }

    [Fact]
    public void TickingACorruptor_GivesItACopyAndUntickingKeepsTheCount()
    {
        choices.ToggleCorruptor(3);
        Assert.Equal((1u << 3, 1), (choices.CorruptorTicks, choices.CorruptorCounts[3]));

        choices.SetCorruptorCount(3, 4);
        choices.ToggleCorruptor(3);
        Assert.Equal((0u, 4), (choices.CorruptorTicks, choices.CorruptorCounts[3]));

        choices.ToggleCorruptor(3);
        Assert.Equal(4, choices.CorruptorCounts[3]);
    }

    [Fact]
    public void TickingAnEnchant_GivesACountedOneACopy()
    {
        var uncounted = ItemEffects.Enchants.ToList().FindIndex(info => !info.Counted);

        choices.ToggleEnchant((int)Enchant.MoreBraveness);
        choices.ToggleEnchant(uncounted);

        Assert.Equal(1, choices.EnchantCounts[(int)Enchant.MoreBraveness]);
        Assert.Equal(0, choices.EnchantCounts[uncounted]);
        Assert.Equal((1u << (int)Enchant.MoreBraveness) | (1u << uncounted), choices.EnchantTicks);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(7, 7)]
    [InlineData(25, 20)]
    public void Counts_StayFromOneToTwenty(int typed, int kept)
    {
        choices.SetCorruptorCount(0, typed);
        choices.SetEnchantCount(0, typed);

        Assert.Equal((kept, kept), (choices.CorruptorCounts[0], choices.EnchantCounts[0]));
    }

    [Fact]
    public void ToggleGear_FlipsItsBit()
    {
        choices.ToggleGear(20);
        Assert.Equal(1u << 20, choices.GearTicks);

        choices.ToggleGear(20);
        Assert.Equal(0u, choices.GearTicks);
    }

    private static int TreeBit(int node) => 1 << (12 - node + 4);

    private AdvancedSheet Sheet() => AdvancedSheet.For(PlannerSheet.For(game));
}
