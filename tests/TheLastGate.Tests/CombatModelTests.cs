using TheLastGate.Rules;

namespace TheLastGate.Tests;

public class CombatModelTests
{
    private readonly CombatModelInput input;

    public CombatModelTests()
    {
        input = new CombatModelInput
        {
            Weapon = CombatWeapon.Sword,
            HeldWeapon = CombatWeapon.Sword,
            ServerHit = 100,
            ServerParry = 100,
            BaseWeapon = 30,
            BaseArmor = 20,
        };
        Array.Fill(input.Known, true);
    }

    [Theory]
    [InlineData(0, CombatWeapon.Unarmed)]
    [InlineData(1, CombatWeapon.Unarmed)]
    [InlineData(3, CombatWeapon.Sword)]
    [InlineData(36, CombatWeapon.Greataxe)]
    [InlineData(23, CombatWeapon.Unarmed)]
    public void WeaponForSkill_IsTheWeaponTheServersSkillStandsFor(int skillNumber, CombatWeapon weapon)
    {
        Assert.Equal(weapon, CombatModel.WeaponForSkill(skillNumber));
    }

    [Fact]
    public void Compute_WithNoPlan_IsTheServersNumbers()
    {
        Set(CombatSkill.Sword, 95, 95);

        Assert.Equal(new CombatValues(100, 100, 30, 20), CombatModel.Compute(input));
    }

    [Fact]
    public void Compute_RaisesHitAndParryByTheWeaponSkillsGain()
    {
        Set(CombatSkill.Sword, 95, 99);

        var shown = CombatModel.Compute(input);

        Assert.Equal(104, shown.Hit);
        Assert.Equal(104, shown.Parry);
    }

    [Fact]
    public void Compute_ForAnotherWeapon_SwapsTheHeldWeaponsSkillForTheChosenOne()
    {
        Set(CombatSkill.Sword, 95, 95);
        Set(CombatSkill.Greataxe, 80, 82);
        input.Weapon = CombatWeapon.Greataxe;

        Assert.Equal(87, CombatModel.Compute(input).Hit);
    }

    [Fact]
    public void Compute_Unarmed_UsesTheBetterOfHandToHandAndKarate()
    {
        input.Weapon = CombatWeapon.Unarmed;
        input.HeldWeapon = CombatWeapon.Unarmed;
        Set(CombatSkill.Hand, 40, 40);
        Set(CombatSkill.Karate, 38, 45);

        Assert.Equal(105, CombatModel.Compute(input).Hit);
    }

    [Fact]
    public void Compute_ReadsASkillNotLearnt()
    {
        Set(CombatSkill.Sword, 20, 24);
        input.Known[(int)CombatSkill.Sword] = false;

        Assert.Equal(104, CombatModel.Compute(input).Hit);
    }

    [Fact]
    public void SkillsAffecting_HitIsTheChosenWeaponsKnownSkills()
    {
        input.Weapon = CombatWeapon.Unarmed;
        input.Known[(int)CombatSkill.Karate] = false;

        Assert.Equal([CombatSkill.Hand], CombatModel.SkillsAffecting(input, CombatStat.Hit));
    }

    [Fact]
    public void SkillsAffecting_WeaponValue_IsNoSkill()
    {
        Assert.Empty(CombatModel.SkillsAffecting(input, CombatStat.Weapon));
    }

    private void Set(CombatSkill skill, int current, int shown)
    {
        input.Current[(int)skill] = current;
        input.Shown[(int)skill] = shown;
    }
}
