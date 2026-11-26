using System;

namespace TheLastGate.Rules;

public sealed record MeleeModelInput
{
    // Est. Melee Hit Dmg as the server sends it, in hundredths.
    public int ServerHitDamage { get; init; }

    // The two-handed axe's critical chance the server sends, in hundredths of a percent; 0 without one.
    public int ServerCritical { get; init; }

    public int ServerWeapon { get; init; }

    public int ShownWeapon { get; init; }

    public int ServerStrength { get; init; }

    public int ShownStrength { get; init; }

    public int ServerAgility { get; init; }

    public int ShownAgility { get; init; }

    // ch[].speed: the row of the speed table, 0 fastest.
    public int ServerSpeedStep { get; init; }
}

// Est. Melee Hit Dmg and Action Speed with the plan, by MyV2's do_attack(), do_update_char() and speedtab[] in
// svr_act.c. The server's numbers carry what the client can't see (items' speed, the 2H-axe special bonus), so each is
// the server's number plus what the planned Strength, Agility and Weapon Value change.
public static class MeleeModel
{
    public const int TicksPerSecond = 18;

    private const int FramesPerAction = 8;
    private const int SpeedSteps = 20;
    private const int SpeedAttributeDivisor = 50;
    private const int Hundredths = 100;
    private const int HalfInHundredths = 50;
    private const int PerMille = 1000;
    private const int PerMillePerHundredthPercent = 10;

    public static int PlannedHitDamage(MeleeModelInput input)
    {
        var change = (input.ShownWeapon - input.ServerWeapon) * Hundredths
            + (StrengthBonus(input.ShownStrength) - StrengthBonus(input.ServerStrength)) * HalfInHundredths;
        var criticalPerMille = input.ServerCritical / PerMillePerHundredthPercent;
        return input.ServerHitDamage + change * (PerMille + criticalPerMille) / PerMille;
    }

    // Actions a second, in hundredths. Row n of speedtab[20][20] in svr_act.c moves an action on a frame in 20 - n of
    // every 20 ticks.
    public static int ActionSpeed(int speedStep) =>
        (SpeedSteps - Math.Clamp(speedStep, 0, SpeedSteps - 1)) * TicksPerSecond * Hundredths
        / (SpeedSteps * FramesPerAction);

    public static int PlannedSpeedStep(MeleeModelInput input) =>
        Math.Clamp(
            input.ServerSpeedStep - (SpeedTerm(input.ShownAgility, input.ShownStrength)
                - SpeedTerm(input.ServerAgility, input.ServerStrength)),
            0,
            SpeedSteps - 1);

    // RANDOM(Strength / 2) in do_attack, from Strength 4 up.
    public static int StrengthBonus(int strength) => strength > 3 ? strength / 2 - 1 : 0;

    private static int SpeedTerm(int agility, int strength) => (agility + strength) / SpeedAttributeDivisor;
}
