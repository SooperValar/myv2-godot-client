using TheLastGate.Buffs;
using TheLastGate.Planner;
using TheLastGate.State;
using TheLastGate.Stats;

namespace TheLastGate.Tests;

public class PlannerSheetTests
{
    private const int BlessSkill = 21;
    private const int StaffSkill = 5;
    private const int SwordSkill = 3;
    private const int Strength = (int)AttributeSlot.Strength;
    private const int Agility = (int)AttributeSlot.Agility;
    private const int Braveness = (int)AttributeSlot.Braveness;

    private readonly PlannerGame game = Game();

    [Fact]
    public void WithoutTheMetaStats_TheRowsShowTheServersValues()
    {
        game.MetaStats.Clear();

        var sheet = PlannerSheet.For(game);

        Assert.False(sheet.BuffsAvailable);
        Assert.All(sheet.Attributes, row => Assert.Equal((50, 0), (row.Preview, row.BuffShift)));
    }

    [Fact]
    public void AShownBless_LiftsTheAttributeRows()
    {
        var sheet = PlannerSheet.For(game);

        Assert.True(sheet.BuffsAvailable);
        Assert.Equal(66, sheet.Planned!.CastPower[(int)BuffKind.Bless]);
        Assert.All(sheet.Attributes, row => Assert.Equal((61, 11), (row.Preview, row.BuffShift)));
    }

    [Fact]
    public void ABlessNotToggledOn_LeavesThemAlone()
    {
        game.Choices.SpellToggles = 0;

        Assert.All(PlannerSheet.For(game).Attributes, row => Assert.Equal(50, row.Preview));
    }

    [Fact]
    public void APlannedAttribute_ShowsWhatWasBoughtAndTheNextCost()
    {
        game.Choices.SpellToggles = 0;
        game.Plan.Raise(Strength, game.ValuesFor(Strength), game.Points, 1);

        var strength = PlannerSheet.For(game).Attributes[Strength];

        Assert.Equal(
            ("Strength", 40, 50, 1, 51),
            (strength.Name, strength.Base, strength.Current, strength.DirectRaise, strength.Preview));
        Assert.Equal(41 * 41 * 41 * 3 / 20, strength.Cost);
        Assert.True(strength.CanRaise);
        Assert.True(strength.CanLower);
    }

    [Fact]
    public void SkillRows_FollowTheListAndOnlyShowShownSkills()
    {
        game.SkillOrder = [BlessSkill, StaffSkill, SwordSkill];
        game.SkillTexts[StaffSkill] = new PlannerSkillText("Staff", [3, 3, 4], 1);
        game.SkillTexts[SwordSkill] = new PlannerSkillText("Sword", [3, 3, 4], 0);

        var skills = PlannerSheet.For(game).Skills;

        Assert.Equal(["Bless", "Staff"], skills.Select(row => row.Name));
        Assert.Equal(StatPlan.SkillSlot(BlessSkill), skills[0].Slot);
    }

    [Fact]
    public void ASkillRow_GainsFromItsRaisedAttributes()
    {
        game.MetaStats.Clear();
        game.SkillOrder = [StaffSkill];
        game.SkillTexts[StaffSkill] = new PlannerSkillText("Staff", [Agility, Agility, Strength], 1);
        game.Skills[StaffSkill][0] = 20;
        game.Plan.Raise(Agility, game.ValuesFor(Agility), game.Points, 3);

        var staff = Assert.Single(PlannerSheet.For(game).Skills);

        // (53 + 53 + 50) / 5 = 31 against 150 / 5 = 30.
        Assert.Equal(1, staff.AttributeGain);
        Assert.True(staff.UsesRaisedAttribute);
    }

    [Fact]
    public void InHardMode_TheAttributesDivideBySix()
    {
        game.MetaStats.Clear();
        game.HardMode = true;
        game.SkillOrder = [StaffSkill];
        game.SkillTexts[StaffSkill] = new PlannerSkillText("Staff", [Agility, Agility, Strength], 1);
        game.Skills[StaffSkill][0] = 20;
        game.Plan.Raise(Agility, game.ValuesFor(Agility), game.Points, 3);

        Assert.Equal(1, Assert.Single(PlannerSheet.For(game).Skills).AttributeGain);
        game.Plan.Lower(Agility, game.ValuesFor(Agility), 1);
        Assert.Equal(0, Assert.Single(PlannerSheet.For(game).Skills).AttributeGain);
    }

    [Fact]
    public void ACappedStat_HasNoCostAndCantBeRaised()
    {
        game.Attributes[Strength][2] = 40;

        var strength = PlannerSheet.For(game).Attributes[Strength];

        Assert.Null(strength.Cost);
        Assert.False(strength.CanRaise);
    }

    [Fact]
    public void TooFewPoints_CantRaiseAStat()
    {
        game.Points = 100;

        Assert.False(PlannerSheet.For(game).Attributes[Strength].CanRaise);
    }

    [Fact]
    public void TheVitalRows_AreHitpointsAndMana()
    {
        game.HitPoints[0] = 120;
        game.HitPoints[5] = 300;
        game.Mana[0] = 80;
        game.Mana[5] = 150;

        var vitals = PlannerSheet.For(game).Vitals;

        Assert.Equal(
            [("Hitpoints", StatPlan.HitPointsSlot, 120, 300), ("Mana", StatPlan.ManaSlot, 80, 150)],
            vitals.Select(row => (row.Name, row.Slot, row.Base, row.Current)));
    }

    [Theory]
    [InlineData(1u << 4, 75)]
    [InlineData(1u << 0, 100)]
    [InlineData(1u << 6, 105)]
    [InlineData(1u << 1, 95)]
    [InlineData(1u << 5, 80)]
    [InlineData(1u << 12, 80)]
    [InlineData(1u << 10, 105)]
    [InlineData(1u << 11, 105)]
    [InlineData(1u << 13, 110)]
    [InlineData(1u << 9, 110)]
    [InlineData(1u << 14, 115)]
    [InlineData(1u << 16, 100)]
    [InlineData(0u, 100)]
    public void SpellRaceModifier_IsTheClasssPercent(uint kindred, int percent)
    {
        Assert.Equal(percent, PlannerSheet.SpellRaceModifier(kindred));
    }

    [Fact]
    public void EmptyGearSlots_LeaveOutTheCharmsAndTheOffhand()
    {
        var worn = new int[PlayerState.WornSlots];
        Assert.Equal(10, PlannerSheet.EmptyGearSlots(worn));

        worn[0] = 100;
        worn[5] = 100;
        worn[7] = 100;
        Assert.Equal(9, PlannerSheet.EmptyGearSlots(worn));
    }

    [Fact]
    public void AFirstLook_PicksTheWeaponAndOffhandFromTheBestSkills()
    {
        game.Skills[SwordSkill][0] = 40;
        game.Skills[SwordSkill][5] = 80;

        var sheet = PlannerSheet.For(game);

        Assert.True(sheet.ChoicesChanged);
        Assert.Equal(CombatWeapon.Sword, game.Choices.Weapon);
        Assert.Equal(CombatOffhand.None, game.Choices.Offhand);
        Assert.Equal(CombatWeapon.Sword, sheet.Combat?.Weapon);
        Assert.False(PlannerSheet.For(game).ChoicesChanged);
    }

    [Fact]
    public void TheCombatModel_WaitsForHitAndParry()
    {
        game.MetaStats.Remove("Parry Score");

        Assert.Null(PlannerSheet.For(game).Combat);
    }

    [Fact]
    public void TheCombatModel_ReadsTheClassAndGear()
    {
        game.Kindred = (1u << 5) | (1u << 16);
        game.TreePoints = 1 << (12 - 10 + 4);
        game.WornSprites[0] = 100;

        var combat = PlannerSheet.For(game).Combat!;

        Assert.Equal(CombatGearClass.ArchTemplar, combat.GearClass);
        Assert.True(combat.Rampart);
        Assert.True(combat.Lycanth);
        Assert.Equal(9, combat.EmptySlots);
        Assert.Equal((100, 90), (combat.ServerHit, combat.ServerParry));
    }

    [Fact]
    public void WithNothingPlannedOrShown_TheCombatBlockShowsTheServersValues()
    {
        game.Choices.SpellToggles = 0;

        var sheet = PlannerSheet.For(game);

        Assert.Equal(new CombatValues(100, 90, 30, 30), sheet.CombatShown);
        Assert.False(sheet.CombatChanged);
    }

    [Fact]
    public void RaisingTheWeaponSkill_RaisesHit()
    {
        game.Choices.SpellToggles = 0;
        game.SkillTexts[SwordSkill] = new PlannerSkillText("Sword", [Braveness, Agility, Strength], 1);
        int[] sword = [40, 0, 200, 3, 0, 80];
        sword.CopyTo(game.Skills[SwordSkill], 0);
        var slot = StatPlan.SkillSlot(SwordSkill);
        game.Plan.Raise(slot, game.ValuesFor(slot), game.Points, 10);

        var sheet = PlannerSheet.For(game);

        Assert.Equal(CombatWeapon.Sword, sheet.Combat?.Weapon);
        Assert.True(sheet.CombatShown?.Hit > 100);
        Assert.True(sheet.CombatChanged);
    }

    [Fact]
    public void WithoutHitAndParry_ThereAreNoCombatValues()
    {
        game.MetaStats.Remove("Hit Score");

        Assert.Null(PlannerSheet.For(game).CombatShown);
    }

    [Theory]
    [InlineData(125, true, "%", "1.25%")]
    [InlineData(7, true, "", "0.07")]
    [InlineData(90, false, "", "90")]
    [InlineData(250, false, " pts", "250 pts")]
    public void FormatMeta_WritesHundredthsForADecimalStat(int value, bool isDecimal, string affix, string text)
    {
        Assert.Equal(text, PlannerSheet.FormatMeta(value, new MetaFormat(isDecimal, affix)));
    }

    [Fact]
    public void MetaText_IsADashUntilTheStatArrives()
    {
        game.MetaFormats[PlannerSheet.SpellMultiplier] = new MetaFormat(true, "x");

        var sheet = PlannerSheet.For(game);

        Assert.Equal("-", sheet.MetaText("Nothing Like It"));
        Assert.Equal("1.00x", sheet.MetaText(PlannerSheet.SpellMultiplier));
        Assert.Equal("90", sheet.MetaText(PlannerSheet.ParryScore));
    }

    [Fact]
    public void MathLines_ListWhatFeedsTheNumbers()
    {
        const uint seyan = 1u << 1;
        game.HardMode = true;
        game.Kindred = seyan;
        game.TreePoints = TreeBit(5) | TreeBit(6) | TreeBit(7) | TreeBit(8);
        game.TreeNodeNames[4] = "Node Five";
        game.TreeNodes[5] = 20;
        game.TreeNodes[6] = 66;
        game.TreeNodes[7] = 250;
        game.Choices.TarotCards = 1ul << (int)TarotCard.Magician;
        game.Choices.CorruptorTicks = 1u << (int)Corruptor.FalseAgility;
        game.Choices.CorruptorCounts[(int)Corruptor.FalseAgility] = 2;
        game.Choices.CorruptorTicks |= 1u << (int)Corruptor.FalseStrength;
        game.Choices.EnchantTicks = 1u << (int)Enchant.MoreBraveness;
        game.Choices.EnchantCounts[(int)Enchant.MoreBraveness] = 3;
        var uncounted = FirstIndex(ItemEffects.Enchants, enchant => !enchant.Counted);
        game.Choices.EnchantTicks |= 1u << uncounted;
        game.Choices.EnchantCounts[uncounted] = 5;
        var unmodelled = FirstIndex(ItemEffects.Gears, gear => !gear.Modelled);
        game.Choices.GearTicks = 1u << unmodelled;

        Assert.Equal(
            [
                "Hard Mode", "Bless", "The Magician", "Node Five (skill tree)", "False Strength (skill tree)",
                "False Tactician (not modelled) (skill tree)", "False Agility x2", "More Braveness x3",
                ItemEffects.Enchants[uncounted].Name, $"{ItemEffects.Gears[unmodelled].Name} (not modelled)",
            ],
            PlannerSheet.For(game).MathLines);
    }

    [Fact]
    public void MathLines_LeaveOutUnlearnedNodesAndSpellsYouDontKnow()
    {
        game.Kindred = 1u << 1;
        game.TreePoints = TreeBit(1);
        game.Skills[BlessSkill][0] = 0;

        Assert.Empty(PlannerSheet.For(game).MathLines);
    }

    [Fact]
    public void CardNames_AreTheUprightCardsThenTheReversed()
    {
        Assert.Equal(TarotMath.CardCount, TarotMath.CardNames.Count);
        Assert.Equal("The Fool", TarotMath.CardNames[0]);
        Assert.Equal("The World", TarotMath.CardNames[21]);
        Assert.Equal("The Fool, Reversed", TarotMath.CardNames[22]);
        Assert.Equal("The World, Reversed", TarotMath.CardNames[43]);
    }

    [Fact]
    public void Score_JoinsTheHighAndLowParts()
    {
        Assert.Equal(300, PlannerGame.Score([0, 0, 0, 0, 1, 44]));
    }

    [Fact]
    public void From_CopiesThePlayersStats()
    {
        var player = new PlayerState();
        player.Attributes[Strength][0] = 40;
        player.Skills[SwordSkill][0] = 12;

        var copied = PlannerGame.From(
            player,
            new Terminology(),
            new StatPlan(),
            new PlannerChoices(),
            new BuffWatch(player),
            false);

        Assert.Equal(40, copied.Attributes[Strength][0]);
        Assert.Equal(12, copied.Skills[SwordSkill][0]);
        Assert.Equal(Terminology.SkillCount, copied.SkillOrder.Count);
        Assert.Empty(copied.MetaStats);
        Assert.Empty(copied.MetaFormats);
        Assert.Equal("p01", copied.TreeNodeNames[0]);
    }

    private static int TreeBit(int node) => 1 << (12 - node + 4);

    private static int FirstIndex<TEntry>(IReadOnlyList<TEntry> entries, Func<TEntry, bool> matches) =>
        entries.Select((entry, index) => (entry, index)).First(pair => matches(pair.entry)).index;

    internal static PlannerGame Game()
    {
        var game = new PlannerGame { Points = 1_000_000, Kindred = 1 };
        foreach (var values in game.Attributes)
        {
            int[] attribute = [40, 0, 200, 3, 0, 50];
            attribute.CopyTo(values, 0);
        }

        foreach (var name in (string[])["Hit Score", "Parry Score"])
        {
            game.MetaStats[name] = name == "Hit Score" ? 100 : 90;
        }

        game.MetaStats["Spell Multiplier"] = 100;
        game.MetaStats["Spell Aptitude"] = 200;
        game.MetaStats["Spell Bonus"] = 0;
        game.Weapon = 30;
        game.Armor = 30;
        game.SkillTexts = Enumerable.Range(0, Terminology.SkillCount)
            .Select(number => new PlannerSkillText($"Skill {number}", [-1, -1, -1], 0))
            .ToArray();
        game.SkillTexts[BlessSkill] = new PlannerSkillText("Bless", [0, 1, 1], 1);
        int[] bless = [30, 0, 200, 3, 0, 60];
        bless.CopyTo(game.Skills[BlessSkill], 0);
        game.SkillOrder = [BlessSkill];
        game.Choices.SpellToggles = 1u << (int)BuffKind.Bless;
        return game;
    }
}
