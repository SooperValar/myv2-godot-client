using TheLastGate.Buffs;
using TheLastGate.Rules;
using TheLastGate.State;

namespace TheLastGate.Tests;

public class BuffModelTests
{
    private const int Bless = (int)BuffKind.Bless;
    private const int Enhance = (int)BuffKind.Enhance;
    private const int Protect = (int)BuffKind.Protect;
    private const int MagicShield = (int)BuffKind.MagicShield;

    private static readonly AttributeSlot Braveness = AttributeSlot.Braveness;
    private static readonly AttributeSlot Willpower = AttributeSlot.Willpower;
    private static readonly AttributeSlot Intuition = AttributeSlot.Intuition;
    private static readonly AttributeSlot Strength = AttributeSlot.Strength;

    [Fact]
    public void Bonuses_FollowTheServersFormulas()
    {
        Assert.Equal(15, BuffModel.BlessBonus(60));
        Assert.Equal(0, BuffModel.BlessBonus(0));
        Assert.Equal(16, BuffModel.ItemBonus(50));
        Assert.Equal(127, BuffModel.ItemBonus(1000));
        Assert.Equal(11, BuffModel.ShieldBonus(40));
        Assert.Equal(0, BuffModel.ShieldBonus(0));
    }

    [Fact]
    public void Compute_RecastsBlessFromItsOwnRaisedSkillUntilItSettles()
    {
        var result = BuffModel.Compute(Fixture());

        Assert.Equal(69, result.CastPower[Bless]);
        Assert.Equal(66, result.Attributes[Strength]);
    }

    [Fact]
    public void Compute_HoldsTheCastToCompatibility()
    {
        var input = Fixture();
        input.SpellAptitude = 50;

        Assert.Equal(50, BuffModel.Compute(input).CastPower[Bless]);
    }

    [Fact]
    public void Compute_MultipliesTheCastBySpellBonus()
    {
        var input = UnbuffedFixture();
        input.SpellMultiplier = 110;
        input.SpellKnown[Enhance] = true;
        input.SpellSkills[Enhance] = Skill(50, Braveness, Willpower, Intuition);

        Assert.Equal(55, BuffModel.Compute(input).CastPower[Enhance]);
    }

    [Fact]
    public void Compute_RaisesCompatibilityByEveryPlannedAttribute()
    {
        var input = Fixture();
        input.PlannedAttributes[Strength] = 3;
        input.PlannedAttributes[Willpower] = 2;

        Assert.Equal(205, BuffModel.Compute(input).SpellAptitude);
    }

    [Fact]
    public void Compute_TakesAnActiveBlessOutWhenItIsntShown()
    {
        var input = UnbuffedFixture();
        input.Active[Bless] = new BuffReading { Known = true, Power = 60 };

        Assert.Equal(35, BuffModel.Compute(input).Attributes[Strength]);
    }

    [Fact]
    public void Compute_ShownProtectAddsToArmor()
    {
        var input = UnbuffedFixture();
        input.SpellKnown[Protect] = true;
        input.SpellSkills[Protect] = Skill(50, Braveness, Willpower, Intuition);
        input.ShowBuffed[Protect] = true;

        Assert.Equal(46, BuffModel.Compute(input).Armor);
    }

    [Fact]
    public void Compute_ShownEnhanceAddsToWeaponValue()
    {
        var input = UnbuffedFixture();
        input.SpellKnown[Enhance] = true;
        input.SpellSkills[Enhance] = Skill(50, Braveness, Willpower, Intuition);
        input.ShowBuffed[Enhance] = true;

        Assert.Equal(46, BuffModel.Compute(input).Weapon);
    }

    [Fact]
    public void Compute_MagicShieldIgnoresCompatibility()
    {
        var input = UnbuffedFixture();
        input.SpellAptitude = 10;
        input.SpellKnown[MagicShield] = true;
        input.SpellSkills[MagicShield] = Skill(40, Braveness, Intuition, Willpower);
        input.ShowBuffed[MagicShield] = true;

        var result = BuffModel.Compute(input);

        Assert.Equal(40, result.CastPower[MagicShield]);
        Assert.Equal(41, result.Armor);
    }

    [Fact]
    public void Compute_ReplacesAnActiveShieldWithThePlannedOne()
    {
        var input = UnbuffedFixture();
        input.Active[MagicShield] = new BuffReading { Known = true, Power = 20 };

        Assert.Equal(24, BuffModel.Compute(input).Armor);
    }

    [Fact]
    public void CombatShown_CountsAPlannedWeaponSkill()
    {
        var model = UnbuffedFixture();
        var combat = new CombatModelInput
        {
            Weapon = CombatWeapon.Sword,
            HeldWeapon = CombatWeapon.Sword,
            ServerHit = 100,
            ServerParry = 100,
        };
        combat.Known[(int)CombatSkill.Sword] = true;
        combat.Current[(int)CombatSkill.Sword] = 80;
        var skills = Enumerable.Range(0, CombatModel.SkillCount).Select(_ => Skill(0)).ToArray();
        skills[(int)CombatSkill.Sword] = new BuffModelSkill { ServerTotal = 80, Planned = 10 };

        Assert.Equal(
            new CombatValues(110, 110, 30, 30),
            BuffModel.CombatShown(model, BuffModel.Compute(model), combat, skills));
    }

    private static BuffModelInput Fixture()
    {
        var input = new BuffModelInput
        {
            Divisor = 5,
            SpellMultiplier = 100,
            SpellAptitude = 200,
            ServerWeapon = 30,
            ServerArmor = 30,
        };
        input.ServerAttributes.SetAll(50);
        input.BaseAttributes.SetAll(40);
        input.SpellKnown[Bless] = true;
        input.SpellSkills[Bless] = Skill(60, Braveness, Willpower, Willpower);
        input.ShowBuffed[Bless] = true;
        return input;
    }

    private static BuffModelInput UnbuffedFixture()
    {
        var input = Fixture();
        input.SpellKnown[Bless] = false;
        input.ShowBuffed[Bless] = false;
        return input;
    }

    private static BuffModelSkill Skill(int serverTotal, params AttributeSlot[] attributes) =>
        new() { ServerTotal = serverTotal, Attributes = attributes };
}
