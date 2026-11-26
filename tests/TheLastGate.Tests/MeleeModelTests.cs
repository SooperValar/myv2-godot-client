using TheLastGate.Rules;

namespace TheLastGate.Tests;

public class MeleeModelTests
{
    private static readonly MeleeModelInput Input = new()
    {
        ServerHitDamage = 1402,
        ServerWeapon = 8,
        ShownWeapon = 8,
        ServerStrength = 10,
        ShownStrength = 10,
        ServerAgility = 30,
        ShownAgility = 30,
        ServerSpeedStep = 6,
    };

    [Fact]
    public void ActionSpeed_IsTheServersSpeedtabRowOverEightFrames()
    {
        Assert.Equal(157, MeleeModel.ActionSpeed(6));
        Assert.Equal(225, MeleeModel.ActionSpeed(0));
    }

    [Fact]
    public void PlannedHitDamage_AddsWeaponValueAndHalfTheStrengthBonus()
    {
        var planned = Input with { ShownWeapon = 10, ShownStrength = 14 };

        Assert.Equal(1402 + 200 + 2 * 50, MeleeModel.PlannedHitDamage(planned));
    }

    [Fact]
    public void PlannedHitDamage_ScalesTheChangeByTheCriticalChance()
    {
        var planned = Input with { ServerCritical = 1000, ShownWeapon = 18 };

        Assert.Equal(1402 + 1100, MeleeModel.PlannedHitDamage(planned));
    }

    [Fact]
    public void PlannedSpeedStep_FallsOneStepForEveryFiftyAgilityAndStrength()
    {
        Assert.Equal(6, MeleeModel.PlannedSpeedStep(Input with { ShownAgility = 39 }));
        Assert.Equal(5, MeleeModel.PlannedSpeedStep(Input with { ShownAgility = 40 }));
    }

    [Theory]
    [InlineData(3, 0)]
    [InlineData(4, 1)]
    [InlineData(11, 4)]
    public void StrengthBonus_IsHalfStrengthLessOne(int strength, int bonus)
    {
        Assert.Equal(bonus, MeleeModel.StrengthBonus(strength));
    }
}
