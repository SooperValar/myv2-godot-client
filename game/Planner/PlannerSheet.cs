using System;
using System.Collections.Generic;
using System.Linq;
using TheLastGate.Buffs;
using TheLastGate.Rules;
using TheLastGate.State;

namespace TheLastGate.Planner;

public sealed record MeleeValues(int ServerHitDamage, int HitDamage, int ServerSpeed, int Speed);

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
    public const string HitScore = "Hit Score";
    public const string ParryScore = "Parry Score";
    public const string SpellMultiplier = "Spell Bonus";
    public const string SpellAptitude = "Compatibility";
    public const string WeaponSkillMeta = "Weapon Skill Number";
    public const string SpeedStepMeta = "Speed Step";
    public const string HitDamage = "Est. Melee Hit Dmg";
    public const string ActionSpeed = "Action Speed";
    public const string CriticalChance = "Critical Chance";

    private const int UncappedValue = 4;
    private const int TotalValue = 5;
    private const int ShownSkill = 1;
    private const int MetaHundredths = 100;

    // In BuffKind order: Bless, Enhance Weapon, Protection, Magic Shield.
    internal static readonly int[] SpellSkillNumbers = [21, 18, 17, 11];

    // In CombatSkill order.
    internal static IReadOnlyList<int> CombatSkillNumbers => CombatModel.SkillNumbers;

    private int plannedArmor;
    private BuffModelResult? unplanned;
    private int unplannedArmor;
    private BuffModelInput? unplannedInput;

    private PlannerSheet(PlannerGame game)
    {
        Game = game;
        plannedArmor = game.Armor;
        unplannedArmor = game.Armor;
    }

    public bool BuffsAvailable => Planned != null;

    internal PlannerGame Game { get; }

    public int ServerWeapon => Game.Weapon;

    public int ServerArmor => Game.Armor;

    public BuffModelInput? PlannedInput { get; private set; }

    public BuffModelResult? Planned { get; private set; }

    public CombatModelInput? Combat { get; private set; }

    // Hit, Parry, Weapon and Armor Value with the plan and the buffs shown; missing until Hit and Parry have arrived.
    public CombatValues? CombatShown { get; private set; }

    // Est. Melee Hit Dmg and Action Speed, the server's and with the plan and buffs shown, in hundredths; missing until
    // their meta stats have arrived.
    public MeleeValues? Melee { get; private set; }

    // The plan or the buffs change a combat number or the spell aptitude.
    public bool CombatChanged { get; private set; }

    // What feeds the numbers besides the stats, one line each: the game mode, the spells, cards, tree and items.
    public IReadOnlyList<string> MathLines { get; private set; } = [];

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
            sheet.VitalRow("Hitpoints", game.HitPoints, StatSlots.HitPoints),
            sheet.VitalRow("Endurance", game.Endurance, StatSlots.Endurance),
            sheet.VitalRow("Mana", game.Mana, StatSlots.Mana),
        ];
        sheet.Skills = game.SkillOrder
            .Where(number => number < game.SkillTexts.Length && game.SkillTexts[number].Show == ShownSkill)
            .Select(sheet.SkillRow)
            .ToArray();
        sheet.MathLines = sheet.MathLineTexts().ToArray();
        return sheet;
    }

    // "-" while the meta stat hasn't arrived.
    public string MetaText(string name) =>
        Meta(name) is { } value
            ? FormatMeta(value, Game.MetaFormats.GetValueOrDefault(name, new MetaFormat(false, "")))
            : "-";

    public static string FormatMeta(int value, MetaFormat format) =>
        format.Decimal
            ? $"{value / MetaHundredths}.{value % MetaHundredths:00}{format.Affix}"
            : $"{value}{format.Affix}";

    private int? Meta(string name) => Game.MetaStats.TryGetValue(name, out var value) ? value : null;

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
            var shown = BuffModel.CombatShown(PlannedInput, Planned, Combat, CombatSkills(true));
            CombatShown = shown;
            CombatChanged = shown != new CombatValues(Combat.ServerHit, Combat.ServerParry, Game.Weapon, Game.Armor)
                || Planned.SpellAptitude != PlannedInput.SpellAptitude;
            plannedArmor = shown.Armor;
            unplannedArmor = BuffModel.CombatShown(unplannedInput, unplanned, Combat, CombatSkills(false)).Armor;
            Melee = MeleeShown(shown);
            CombatChanged |= Melee is { } melee
                && (melee.HitDamage != melee.ServerHitDamage || melee.Speed != melee.ServerSpeed);
        }
    }

    private MeleeValues? MeleeShown(CombatValues shown)
    {
        if (Meta(HitDamage) is not { } hitDamage || Meta(SpeedStepMeta) is not { } speedStep || Planned == null)
        {
            return null;
        }

        var input = new MeleeModelInput
        {
            ServerHitDamage = hitDamage,
            ServerCritical = Meta(CriticalChance) ?? 0,
            ServerWeapon = Game.Weapon,
            ShownWeapon = shown.Weapon,
            ServerStrength = PlannerGame.Score(Game.Attributes[(int)AttributeSlot.Strength]),
            ShownStrength = Planned.Attributes[AttributeSlot.Strength],
            ServerAgility = PlannerGame.Score(Game.Attributes[(int)AttributeSlot.Agility]),
            ShownAgility = Planned.Attributes[AttributeSlot.Agility],
            ServerSpeedStep = speedStep,
        };
        return new MeleeValues(
            hitDamage,
            MeleeModel.PlannedHitDamage(input),
            MeleeModel.ActionSpeed(speedStep),
            MeleeModel.ActionSpeed(MeleeModel.PlannedSpeedStep(input)));
    }

    private BuffModelInput? BuffInput()
    {
        if (Meta(SpellMultiplier) is not { } multiplier || Meta(SpellAptitude) is not { } aptitude)
        {
            return null;
        }

        var choices = Game.Choices;
        var input = new BuffModelInput
        {
            Divisor = SkillPreview.AttributeDivisor(Game.HardMode),
            SpellMultiplier = multiplier,
            SpellAptitude = aptitude,
            ServerWeapon = Game.Weapon,
            ServerArmor = Game.Armor,
            UncappedHitpoints = Game.HitPoints[UncappedValue],
            PlannedHitpoints = Game.Plan.Raised(StatSlots.HitPoints),
            UncappedMana = Game.Mana[UncappedValue],
            PlannedMana = Game.Plan.Raised(StatSlots.Mana),
        };
        foreach (var attribute in Enum.GetValues<AttributeSlot>())
        {
            var values = Game.Attributes[(int)attribute];
            input.ServerAttributes[attribute] = PlannerGame.Score(values);
            input.BaseAttributes[attribute] = values[0];
            input.PlannedAttributes[attribute] = Game.Plan.Raised((int)attribute);
        }

        for (var kind = 0; kind < BuffModelInput.BuffCount; kind++)
        {
            var number = SpellSkillNumbers[kind];
            var known = InTable(number) && Game.Skills[number][0] > 0;
            input.SpellKnown[kind] = known;
            input.SpellSkills[kind] = InTable(number) ? ModelSkill(number, true) : new BuffModelSkill();
            if (Game.BuffReadings[kind] is { } reading)
            {
                input.Active[kind] = reading.Copy();
            }

            var toggledOn = ((choices.SpellToggles >> kind) & 1) != 0;
            input.ShowBuffed[kind] = known && toggledOn;
        }

        return input;
    }

    private IEnumerable<string> MathLineTexts()
    {
        var choices = Game.Choices;
        if (Game.HardMode)
        {
            yield return "Hard Mode";
        }

        for (var kind = 0; kind < BuffModelInput.BuffCount; kind++)
        {
            var number = SpellSkillNumbers[kind];
            if (Known(number) && ((choices.SpellToggles >> kind) & 1) != 0)
            {
                yield return SpellLine(kind, Game.SkillTexts[number].Name);
            }
        }
    }

    // Magic Shield is the one spell Compatibility doesn't hold back.
    private string SpellLine(int kind, string name)
    {
        if (PlannedInput == null || Planned == null)
        {
            return name;
        }

        var skill = BuffModel.SkillValue(PlannedInput, PlannedInput.SpellSkills[kind], Planned.Attributes);
        var capped = kind != (int)BuffKind.MagicShield && skill > Planned.SpellAptitude;
        return $"{name} {Planned.EffectivePower[kind]}{(capped ? " (capped)" : "")}";
    }

    // A learned node counts under its own name while it is in the math, or as its corruptor once corrupted.
    internal IEnumerable<string> TreeLines()
    {
        var treeClass = TreeEffects.ClassFromKindred(Game.Kindred);
        for (var node = 1; node <= PlayerState.TreeNodeCount; node++)
        {
            if (!TreeEffects.NodeLearned(Game.TreePoints, node))
            {
                continue;
            }

            var serverNumber = Game.TreeNodes[node - 1];
            if (serverNumber == 0)
            {
                if (TreeEffects.NodeInMath(treeClass, node))
                {
                    yield return Game.TreeNodeNames[node - 1];
                }
            }
            else if (TreeEffects.CorruptorForServerNumber(serverNumber) is { } corruptor)
            {
                var info = TreeEffects.Corruptors[(int)corruptor];
                yield return info.Modelled ? info.Name : $"{info.Name} (not modelled)";
            }
        }
    }

    internal TreeEffectsInput TreeInput()
    {
        var input = new TreeEffectsInput
        {
            TreeClass = TreeEffects.ClassFromKindred(Game.Kindred),
            TreePoints = Game.TreePoints,
            CorruptorTicks = Game.Choices.CorruptorTicks,
        };
        Game.TreeNodes.CopyTo(input.TreeNodes, 0);
        Game.Choices.CorruptorCounts.CopyTo(input.CorruptorCounts, 0);
        return input;
    }

    private ItemEffectsInput ItemInput()
    {
        var input = new ItemEffectsInput
            { EnchantTicks = Game.Choices.EnchantTicks, GearTicks = Game.Choices.GearTicks };
        Game.Choices.EnchantCounts.CopyTo(input.EnchantCounts, 0);
        return input;
    }

    private CombatModelInput? CombatInput(BuffModelInput plannedInput, BuffModelResult planned)
    {
        if (Meta(HitScore) is not { } hit || Meta(ParryScore) is not { } parry)
        {
            return null;
        }

        var input = new CombatModelInput
        {
            ServerHit = hit,
            ServerParry = parry,
            BaseWeapon = planned.Weapon,
            BaseArmor = planned.Armor,
            HeldWeapon = CombatModel.WeaponForSkill(Meta(WeaponSkillMeta) ?? 0),
            Offhand = CombatOffhand.None,
        };
        for (var skill = 0; skill < CombatModel.SkillCount; skill++)
        {
            var number = CombatSkillNumbers[skill];
            input.Known[skill] = Known(number);
            input.Current[skill] = PlannerGame.Score(Game.Skills[number]);
            input.Shown[skill] = InTable(number)
                ? BuffModel.SkillValue(plannedInput, ModelSkill(number, true), planned.Attributes)
                : input.Current[skill];
        }

        var choices = Game.Choices;
        if (choices.Weapon == null)
        {
            choices.Weapon = input.HeldWeapon;
            ChoicesChanged = true;
        }

        input.Weapon = choices.Weapon.Value;
        return input;
    }

    internal BuffModelSkill[] CombatSkills(bool withPlan) =>
        CombatSkillNumbers
            .Select(number => InTable(number) ? ModelSkill(number, withPlan) : new BuffModelSkill())
            .ToArray();

    private bool InTable(int number) => number < Game.SkillTexts.Length;

    internal bool Known(int number) => InTable(number) && Game.Skills[number][0] > 0;

    internal BuffModelSkill ModelSkill(int number, bool withPlan) => new()
    {
        ServerTotal = PlannerGame.Score(Game.Skills[number]),
        Planned = withPlan ? Game.Plan.Raised(StatSlots.Skill(number)) : 0,
        Attributes = SkillAttributes(number).Select(attribute => (AttributeSlot)attribute).ToArray(),
        Scale = SkillScale.None,
        Groups = SkillGroupTable.Of(number),
    };

    private IEnumerable<int> SkillAttributes(int number) =>
        Game.SkillTexts[number].Attributes.Where(attribute => attribute is >= 0 and < AttributeAmounts.Count);

    private PlannerRow RaisableRow(string name, int slot, int baseValue, int current)
    {
        var plan = Game.Plan;
        var cost = plan.NextCost(slot, Game.ValuesFor(slot));
        return new PlannerRow(
            name,
            slot,
            baseValue,
            current,
            plan.Raised(slot),
            0,
            0,
            cost,
            cost <= Game.Points - plan.PointsUsed,
            plan.Raised(slot) > 0,
            false);
    }

    private PlannerRow AttributeRow(int attribute)
    {
        var values = Game.Attributes[attribute];
        var row = RaisableRow(StatSlots.AttributeNames[attribute], attribute, values[0], PlannerGame.Score(values));
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
        var skillValues = Game.Skills[number];
        var row = RaisableRow(
            Game.SkillTexts[number].Name,
            StatSlots.Skill(number),
            skillValues[0],
            PlannerGame.Score(skillValues));
        var attributes = SkillAttributes(number).ToArray();
        var attributeValues = attributes.Select(attribute => PlannerGame.Score(Game.Attributes[attribute])).ToArray();
        var attributeRaises = attributes.Select(attribute => Game.Plan.Raised(attribute)).ToArray();
        row = row with
        {
            AttributeGain = SkillPreview.AttributeGain(
                attributeValues,
                attributeRaises,
                PlannedInput?.Divisor
                ?? SkillPreview.AttributeDivisor(Game.HardMode)),
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
