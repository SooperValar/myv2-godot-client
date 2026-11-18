using System;
using System.Collections.Generic;
using System.Linq;
using TheLastGate.Items;
using TheLastGate.Stats;

namespace TheLastGate.Planner;

// One row of the planner: an attribute, Hitpoints, Mana or a skill, by its stat plan slot.
public sealed record PlannerRow(
    string Name,
    int Slot,
    int Base,
    int Current,
    int DirectRaise,
    int AttributeGain,
    int BuffShift,
    int? Cost,
    bool CanRaise,
    bool CanLower,
    bool UsesRaisedAttribute)
{
    // What the row shows with the plan: the server's value, the buffs toggled, the points bought and what the raised
    // attributes add.
    public int Preview => Current + BuffShift + DirectRaise + AttributeGain;
}

// The planner's numbers for one look at the game: the buff model with and without the plan, the combat model's input
// and every row. Without the spell meta stats the rows fall back to the plain skill preview.
public sealed class PlannerSheet
{
    private const string HitScore = "Hit Score";
    private const string ParryScore = "Parry Score";
    private const string SpellMultiplier = "Spell Multiplier";
    private const string SpellAptitude = "Spell Aptitude";
    private const string SpellBonus = "Spell Bonus";

    private const int UncappedValue = 4;
    private const int TotalValue = 5;
    private const int ShownSkill = 1;
    private const int RampartNode = 10;
    private const int DefaultSpellRacePercent = 100;

    private const int TacticsSkill = 12;
    private const int ResistSkill = 23;
    private const int ImmunitySkill = 32;

    private const uint Mercenary = 1u << 0;
    private const uint SeyanDu = 1u << 1;
    private const uint Templar = 1u << 4;
    private const uint ArchTemplar = 1u << 5;
    private const uint Harakim = 1u << 6;
    private const uint ArchHarakim = 1u << 9;
    private const uint Warrior = 1u << 10;
    private const uint Sorcerer = 1u << 11;
    private const uint Skald = 1u << 12;
    private const uint Summoner = 1u << 13;
    private const uint Braver = 1u << 14;
    private const uint Lycanth = 1u << 16;

    // In BuffKind order.
    private static readonly int[] SpellSkillNumbers = [21, 18, 17];

    // In CombatSkill order.
    private static readonly int[] CombatSkillNumbers = [0, 2, 3, 4, 5, 6, 36, 16, TacticsSkill, 38, 53];

    // The first class that matches gives the percent.
    private static readonly (uint Class, int Percent)[] SpellRacePercents =
    [
        (Templar, 75), (Mercenary, 100), (Harakim, 105), (SeyanDu, 95), (ArchTemplar, 80), (Skald, 80),
        (Warrior, 105), (Sorcerer, 105), (Summoner, 110), (ArchHarakim, 110), (Braver, 115),
        (Lycanth, DefaultSpellRacePercent),
    ];

    private readonly PlannerGame game;
    private int plannedArmor;
    private BuffModelResult? unplanned;
    private int unplannedArmor;
    private BuffModelInput? unplannedInput;

    private PlannerSheet(PlannerGame game)
    {
        this.game = game;
        plannedArmor = game.Armor;
        unplannedArmor = game.Armor;
    }

    public bool BuffsAvailable => Planned != null;

    public BuffModelInput? PlannedInput { get; private set; }

    public BuffModelResult? Planned { get; private set; }

    public CombatModelInput? Combat { get; private set; }

    // A first look picked the weapon and off-hand, so the choices want saving.
    public bool ChoicesChanged { get; private set; }

    public IReadOnlyList<PlannerRow> Attributes { get; private set; } = [];

    public IReadOnlyList<PlannerRow> Vitals { get; private set; } = [];

    public IReadOnlyList<PlannerRow> Skills { get; private set; } = [];

    public static PlannerSheet For(PlannerGame game)
    {
        var sheet = new PlannerSheet(game);
        sheet.ComputeBuffs();
        sheet.Attributes = Enumerable.Range(0, AttributeAmounts.Count).Select(sheet.AttributeRow).ToArray();
        sheet.Vitals =
        [
            sheet.VitalRow("Hitpoints", game.HitPoints, StatPlan.HitPointsSlot),
            sheet.VitalRow("Mana", game.Mana, StatPlan.ManaSlot),
        ];
        sheet.Skills = game.SkillOrder
            .Where(number => number < game.SkillTexts.Length && game.SkillTexts[number].Show == ShownSkill)
            .Select(sheet.SkillRow)
            .ToArray();
        return sheet;
    }

    public static int SpellRaceModifier(uint kindred)
    {
        foreach (var (characterClass, percent) in SpellRacePercents)
        {
            if ((kindred & characterClass) != 0)
            {
                return percent;
            }
        }

        return DefaultSpellRacePercent;
    }

    // Ferocity grows with the gear slots left empty; the charms and the off-hand don't count.
    public static int EmptyGearSlots(IReadOnlyList<int> wornSprites)
    {
        var emptySlots = 0;
        for (var slot = WornSlot.Head; slot <= WornSlot.SecondCharm; slot++)
        {
            if (slot is not (WornSlot.Charm or WornSlot.SecondCharm or WornSlot.OffHand) && wornSprites[(int)slot] == 0)
            {
                emptySlots++;
            }
        }

        return emptySlots;
    }

    private bool Is(uint characterClass) => (game.Kindred & characterClass) != 0;

    private int? Meta(string name) => game.MetaStats.TryGetValue(name, out var value) ? value : null;

    private void ComputeBuffs()
    {
        PlannedInput = BuffInput();
        if (PlannedInput == null)
        {
            return;
        }

        unplannedInput = PlannedInput.Copy();
        unplannedInput.PlannedAttributes.SetAll(0);
        for (var kind = 0; kind < BuffModelInput.BuffCount; kind++)
        {
            unplannedInput.SpellSkills[kind] = unplannedInput.SpellSkills[kind] with { Planned = 0 };
        }

        unplannedInput.PlannedHitpoints = 0;
        unplannedInput.PlannedMana = 0;
        Planned = BuffModel.Compute(PlannedInput);
        unplanned = BuffModel.Compute(unplannedInput);
        Combat = CombatInput(PlannedInput, Planned);
        if (Combat != null)
        {
            plannedArmor = BuffModel.CombatShown(PlannedInput, Planned, Combat, CombatSkills(true)).Armor;
            unplannedArmor = BuffModel.CombatShown(unplannedInput, unplanned, Combat, CombatSkills(false)).Armor;
        }
    }

    private BuffModelInput? BuffInput()
    {
        if (Meta(SpellMultiplier) is not { } multiplier || Meta(SpellAptitude) is not { } aptitude)
        {
            return null;
        }

        var choices = game.Choices;
        var input = new BuffModelInput
        {
            Divisor = SkillPreview.AttributeDivisor(game.HardMode),
            SpellMultiplier = multiplier,
            SpellBonus = Meta(SpellBonus) ?? 0,
            SpellAptitude = aptitude,
            RaceModifier = SpellRaceModifier(game.Kindred),
            ReducedItemBuffs = Is(SeyanDu) || Is(Braver),
            ServerWeapon = game.Weapon,
            ServerArmor = game.Armor,
            TarotCards = choices.TarotCards,
            Effects = ItemEffects.Compute(TreeInput(), ItemInput()),
            UncappedHitpoints = game.HitPoints[UncappedValue],
            PlannedHitpoints = game.Plan.Raised(StatPlan.HitPointsSlot),
            UncappedMana = game.Mana[UncappedValue],
            PlannedMana = game.Plan.Raised(StatPlan.ManaSlot),
        };
        foreach (var attribute in Enum.GetValues<AttributeSlot>())
        {
            var values = game.Attributes[(int)attribute];
            input.ServerAttributes[attribute] = PlannerGame.Score(values);
            input.BaseAttributes[attribute] = values[0];
            input.PlannedAttributes[attribute] = game.Plan.Raised((int)attribute);
        }

        for (var kind = 0; kind < BuffModelInput.BuffCount; kind++)
        {
            var number = SpellSkillNumbers[kind];
            var known = InTable(number) && game.Skills[number][0] > 0;
            input.SpellKnown[kind] = known;
            input.SpellSkills[kind] = InTable(number) ? ModelSkill(number, true) : new BuffModelSkill();
            if (game.BuffReadings[kind] is { } reading)
            {
                input.Active[kind] = reading;
            }

            // A buff from a spell you don't know stays in the numbers; it has no toggle.
            var toggledOn = ((choices.SpellToggles >> kind) & 1) != 0;
            var readingReady = !game.BuffActive[kind] || game.BuffReadings[kind] != null;
            input.ShowBuffed[kind] = known ? toggledOn && readingReady : game.BuffActive[kind];
        }

        return input;
    }

    private TreeEffectsInput TreeInput()
    {
        var input = new TreeEffectsInput
        {
            TreeClass = TreeEffects.ClassFromKindred(game.Kindred),
            TreePoints = game.TreePoints,
            CorruptorTicks = game.Choices.CorruptorTicks,
        };
        game.TreeNodes.CopyTo(input.TreeNodes, 0);
        game.Choices.CorruptorCounts.CopyTo(input.CorruptorCounts, 0);
        return input;
    }

    private ItemEffectsInput ItemInput()
    {
        var input = new ItemEffectsInput
            { EnchantTicks = game.Choices.EnchantTicks, GearTicks = game.Choices.GearTicks };
        game.Choices.EnchantCounts.CopyTo(input.EnchantCounts, 0);
        return input;
    }

    private CombatModelInput? CombatInput(BuffModelInput plannedInput, BuffModelResult planned)
    {
        if (Meta(HitScore) is not { } hit || Meta(ParryScore) is not { } parry)
        {
            return null;
        }

        var effects = plannedInput.Effects;
        var input = new CombatModelInput
        {
            ServerHit = hit,
            ServerParry = parry,
            GearClass = GearClass(),
            Rampart = Is(ArchTemplar) && TreeEffects.NodeLearned(game.TreePoints, RampartNode),
            Lycanth = Is(Lycanth),
            EmptySlots = EmptyGearSlots(game.WornSprites),
            BaseWeapon = planned.Weapon,
            BaseArmor = planned.Armor,
            BestWeaponSkill = effects.FistsOfHeavens != 0,
            HitFeedsParry = effects.HitToParryPercent != 0,
            ParryFeedsHit = effects.ParryToHitPercent != 0,
        };
        for (var skill = 0; skill < CombatModel.SkillCount; skill++)
        {
            var number = CombatSkillNumbers[skill];
            input.Known[skill] = Known(number);
            input.Current[skill] = PlannerGame.Score(game.Skills[number]);
            input.Shown[skill] = input.Known[skill]
                ? BuffModel.SkillValue(plannedInput, ModelSkill(number, true), planned.Attributes)
                : input.Current[skill];
        }

        var choices = game.Choices;
        if (choices.Weapon == null || choices.Offhand == null)
        {
            choices.Weapon ??= CombatModel.DefaultWeapon(input.Known, input.Current);
            choices.Offhand ??= CombatModel.DefaultOffhand(input.Known, input.Current);
            ChoicesChanged = true;
        }

        input.Weapon = choices.Weapon.Value;
        input.Offhand = choices.Offhand.Value;
        return input;
    }

    private CombatGearClass GearClass()
    {
        if (Is(Skald))
        {
            return CombatGearClass.Skald;
        }

        if (Is(ArchTemplar))
        {
            return CombatGearClass.ArchTemplar;
        }

        return Is(Templar) || Is(Braver) ? CombatGearClass.TemplarOrBraver : CombatGearClass.Other;
    }

    private BuffModelSkill[] CombatSkills(bool withPlan) =>
        CombatSkillNumbers
            .Select(number => Known(number) ? ModelSkill(number, withPlan) : new BuffModelSkill())
            .ToArray();

    private bool InTable(int number) => number < game.SkillTexts.Length;

    private bool Known(int number) => InTable(number) && game.Skills[number][0] > 0;

    private BuffModelSkill ModelSkill(int number, bool withPlan) => new()
    {
        ServerTotal = PlannerGame.Score(game.Skills[number]),
        Planned = withPlan ? game.Plan.Raised(StatPlan.SkillSlot(number)) : 0,
        Attributes = SkillAttributes(number).Select(attribute => (AttributeSlot)attribute).ToArray(),
        Scale = number switch
        {
            ResistSkill => SkillScale.Resist,
            ImmunitySkill => SkillScale.Immunity,
            TacticsSkill => SkillScale.Tactics,
            _ => SkillScale.None,
        },
        Groups = SkillGroupTable.Of(number),
    };

    private IEnumerable<int> SkillAttributes(int number) =>
        game.SkillTexts[number].Attributes.Where(attribute => attribute is >= 0 and < AttributeAmounts.Count);

    private PlannerRow RaisableRow(string name, int slot, int baseValue, int current)
    {
        var plan = game.Plan;
        var cost = plan.NextCost(slot, game.ValuesFor(slot));
        return new PlannerRow(
            name,
            slot,
            baseValue,
            current,
            plan.Raised(slot),
            0,
            0,
            cost,
            cost <= game.Points - plan.PointsUsed,
            plan.Raised(slot) > 0,
            false);
    }

    private PlannerRow AttributeRow(int attribute)
    {
        var values = game.Attributes[attribute];
        var row = RaisableRow(StatLines.AttributeNames[attribute], attribute, values[0], PlannerGame.Score(values));
        if (Planned == null || unplanned == null)
        {
            return row;
        }

        var slot = (AttributeSlot)attribute;
        return row with
        {
            BuffShift = unplanned.Attributes[slot] - row.Current,
            AttributeGain = Planned.Attributes[slot] - unplanned.Attributes[slot] - row.DirectRaise,
        };
    }

    private PlannerRow VitalRow(string name, int[] values, int slot) =>
        RaisableRow(name, slot, values[0], values[TotalValue]);

    private PlannerRow SkillRow(int number)
    {
        var skillValues = game.Skills[number];
        var row = RaisableRow(
            game.SkillTexts[number].Name,
            StatPlan.SkillSlot(number),
            skillValues[0],
            PlannerGame.Score(skillValues));
        var attributes = SkillAttributes(number).ToArray();
        var attributeValues = attributes.Select(attribute => PlannerGame.Score(game.Attributes[attribute])).ToArray();
        var attributeRaises = attributes.Select(attribute => game.Plan.Raised(attribute)).ToArray();
        row = row with
        {
            AttributeGain = SkillPreview.AttributeGain(
                attributeValues,
                attributeRaises,
                PlannedInput?.Divisor
                ?? SkillPreview.AttributeDivisor(game.HardMode)),
            UsesRaisedAttribute = SkillPreview.UsesRaisedAttribute(attributeRaises),
        };
        if (PlannedInput == null || Planned == null || unplannedInput == null || unplanned == null)
        {
            return row;
        }

        var unplannedValue = BuffModel.SkillValueWithArmor(
            unplannedInput,
            ModelSkill(number, false),
            unplanned.Attributes,
            unplannedArmor);
        var plannedValue = BuffModel.SkillValueWithArmor(
            PlannedInput,
            ModelSkill(number, true),
            Planned.Attributes,
            plannedArmor);
        return row with
        {
            BuffShift = unplannedValue - row.Current,
            AttributeGain = plannedValue - unplannedValue - row.DirectRaise,
        };
    }
}
