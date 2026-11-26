using GdUnit4;
using TheLastGate.Buffs;
using TheLastGate.Planner;
using TheLastGate.Rules;
using TheLastGate.State;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Planner;

[TestSuite]
public class BuffFocusTests
{
    private const int FocusedSlot = 8;
    private const int BlessSlot = 9;

    // Raise slots the model never reads, so points there change nothing.
    private const int FirstInertSlot = 20;

    private const int Bless = (int)BuffKind.Bless;

    [TestCase]
    public void BlessPoints_WinWhenCheapest()
    {
        var result = BuffFocus.Search(Fixture());

        AssertThat(result.CurrentValue).IsEqual(109);
        AssertSingle(result.Best, BlessSlot, 1, 20);
        AssertSingle(result.Direct, FocusedSlot, 1, 1000);
        AssertSingle(result.ThroughAttributes, 1, 1, 300);
        AssertSingle(result.ThroughBless, BlessSlot, 1, 20);
    }

    [TestCase]
    public void Direct_WinsWhenCheap()
    {
        var input = Fixture();
        input.Candidates[0].StepCosts[0] = 10;

        var result = BuffFocus.Search(input);

        AssertSingle(result.Best, FocusedSlot, 1, 10);
        AssertSingle(result.Direct, FocusedSlot, 1, 10);
        AssertSingle(result.ThroughAttributes, 1, 1, 300);
        AssertSingle(result.ThroughBless, BlessSlot, 1, 20);
    }

    [TestCase]
    public void ACappedBless_IsGoneAround()
    {
        var input = Fixture();
        Array.Fill(input.Candidates[6].StepCosts, SkillFocus.NoCost);

        var result = BuffFocus.Search(input);

        AssertSingle(result.Best, 1, 1, 300);
        AssertSingle(result.ThroughAttributes, 1, 1, 300);
        AssertThat(result.ThroughBless).IsNull();
    }

    [TestCase]
    public void EverythingCapped_LeavesNoRoute()
    {
        var input = Fixture();
        foreach (var candidate in input.Candidates)
        {
            Array.Fill(candidate.StepCosts, SkillFocus.NoCost);
        }

        var result = BuffFocus.Search(input);

        AssertThat(result.Best).IsNull();
        AssertThat(result.Direct).IsNull();
        AssertThat(result.ThroughAttributes).IsNull();
        AssertThat(result.ThroughBless).IsNull();
    }

    [TestCase]
    public void FocusingBlessItself_MakesABlessPointTheDirectRaise()
    {
        var input = new BuffFocusInput
        {
            Model = Model(),
            Focused = Skill(60, AttributeSlot.Braveness, AttributeSlot.Willpower, AttributeSlot.Willpower),
            FocusedRaiseIndex = BlessSlot,
        };
        input.SpellRaiseIndex[Bless] = BlessSlot;
        AddCandidate(input, BlessSlot, 16, 20);
        AddCandidate(input, 0, 6, 300);

        var result = BuffFocus.Search(input);

        AssertThat(result.CurrentValue).IsEqual(69);
        AssertSingle(result.Best, BlessSlot, 1, 20);
        AssertSingle(result.Direct, BlessSlot, 1, 20);
        AssertThat(result.ThroughBless).IsNull();
    }

    [TestCase]
    public void ManyCandidates_StillFindACheapRoute()
    {
        var input = Fixture();
        for (var inert = 0; input.Candidates.Count < FocusSearch.MostCandidates; inert++)
        {
            AddCandidate(input, FirstInertSlot + inert, FocusSearch.MostPoints, 1000);
        }

        var result = BuffFocus.Search(input);

        AssertThat(input.Candidates.Count).IsEqual(FocusSearch.MostCandidates);
        AssertSingle(result.Best, BlessSlot, 1, 20);
        AssertSingle(result.Direct, FocusedSlot, 1, 1000);
        AssertSingle(result.ThroughAttributes, 1, 1, 300);
        AssertSingle(result.ThroughBless, BlessSlot, 1, 20);
    }

    [TestCase]
    public void ASearchWithoutARoute_StopsAtTheStateLimit()
    {
        var input = Fixture();
        input.Candidates.Clear();
        while (input.Candidates.Count < FocusSearch.MostCandidates)
        {
            AddCandidate(input, FirstInertSlot + input.Candidates.Count, FocusSearch.MostPoints, 1);
        }

        var result = BuffFocus.Search(input);

        AssertThat(result.CurrentValue).IsEqual(109);
        AssertThat(result.Best).IsNull();
        AssertThat(result.Direct).IsNull();
        AssertThat(result.ThroughAttributes).IsNull();
        AssertThat(result.ThroughBless).IsNull();
    }

    private static BuffModelInput Model()
    {
        var model = new BuffModelInput
        {
            Divisor = 5,
            SpellMultiplier = 100,
            SpellAptitude = 200,
            ServerWeapon = 30,
            ServerArmor = 30,
        };
        model.ServerAttributes.SetAll(50);
        model.BaseAttributes.SetAll(40);
        model.SpellKnown[Bless] = true;
        model.SpellSkills[Bless] = Skill(60, AttributeSlot.Braveness, AttributeSlot.Willpower, AttributeSlot.Willpower);
        model.ShowBuffed[Bless] = true;
        return model;
    }

    // The focused skill uses Agility, Strength and Strength; Bless is the lever.
    private static BuffFocusInput Fixture()
    {
        var input = new BuffFocusInput
        {
            Model = Model(),
            Focused = Skill(100, AttributeSlot.Agility, AttributeSlot.Strength, AttributeSlot.Strength),
            FocusedRaiseIndex = FocusedSlot,
        };
        input.SpellRaiseIndex[Bless] = BlessSlot;
        AddCandidate(input, FocusedSlot, 1, 1000);
        AddCandidate(input, 0, 6, 300);
        AddCandidate(input, 1, 6, 300);
        AddCandidate(input, 2, 6, 300);
        AddCandidate(input, 3, 6, 500);
        AddCandidate(input, 4, 6, 500);
        AddCandidate(input, BlessSlot, 16, 20);
        return input;
    }

    private static BuffModelSkill Skill(int serverTotal, params AttributeSlot[] attributes) =>
        new() { ServerTotal = serverTotal, Attributes = attributes };

    private static void AddCandidate(BuffFocusInput input, int raiseIndex, int mostPoints, int cost)
    {
        var candidate = new FocusCandidate { RaiseIndex = raiseIndex, MostPoints = mostPoints };
        Array.Fill(candidate.StepCosts, cost);
        input.Candidates.Add(candidate);
    }

    private static void AssertSingle(FocusRoute? route, int raiseIndex, int points, int cost)
    {
        AssertThat(route).IsNotNull();
        AssertThat(route!.TotalCost).IsEqual(cost);
        AssertThat(route.Raises).ContainsExactly(new FocusRaise(raiseIndex, points, cost));
    }
}
