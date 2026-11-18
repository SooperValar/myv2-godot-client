using TheLastGate.Planner;

namespace TheLastGate.Tests;

public class FocusSearchTests
{
    private const uint Everyone = uint.MaxValue;

    [Fact]
    public void Cheapest_FindsTheCheapestAcceptedMix()
    {
        FocusCandidate[] candidates = [Candidate(0, 10, 10, 10), Candidate(1, 15, 15)];

        var route = FocusSearch.Cheapest(candidates, Everyone, points => points.Sum() >= 2);

        Assert.Equal(20, route?.TotalCost);
        Assert.Equal([new FocusRaise(0, 2, 20)], route!.Raises);
    }

    [Fact]
    public void ATie_GoesToFewerPoints()
    {
        FocusCandidate[] candidates = [Candidate(0, 10, 10), Candidate(1, 20)];

        var route = FocusSearch.Cheapest(candidates, Everyone, points => points[0] >= 2 || points[1] >= 1);

        Assert.Equal([new FocusRaise(1, 1, 20)], route!.Raises);
    }

    [Fact]
    public void ATieOnPoints_GoesToTheEarlierCandidate()
    {
        FocusCandidate[] candidates = [Candidate(0, 10), Candidate(1, 10)];

        var route = FocusSearch.Cheapest(candidates, Everyone, points => points.Sum() >= 1);

        Assert.Equal([new FocusRaise(0, 1, 10)], route!.Raises);
    }

    [Fact]
    public void ACandidateNotAllowed_GetsNoPoints()
    {
        FocusCandidate[] candidates = [Candidate(0, 10), Candidate(1, 50)];

        var route = FocusSearch.Cheapest(candidates, 1u << 1, points => points.Sum() >= 1);

        Assert.Equal([new FocusRaise(1, 1, 50)], route!.Raises);
    }

    [Fact]
    public void ACappedPoint_CantBeBought()
    {
        FocusCandidate[] candidates = [Candidate(0, 10, SkillFocus.NoCost, 10)];

        Assert.Null(FocusSearch.Cheapest(candidates, Everyone, points => points[0] >= 2));
    }

    [Fact]
    public void ACandidate_TakesNoMoreThanItsMostPoints()
    {
        var candidate = Candidate(0, 10, 10, 10);
        candidate.MostPoints = 2;

        Assert.Null(FocusSearch.Cheapest([candidate], Everyone, points => points[0] >= 3));
    }

    [Fact]
    public void Raises_AreInRaiseSlotOrder()
    {
        FocusCandidate[] candidates = [Candidate(8, 10), Candidate(2, 10)];

        var route = FocusSearch.Cheapest(candidates, Everyone, points => points[0] >= 1 && points[1] >= 1);

        Assert.Equal([new FocusRaise(2, 1, 10), new FocusRaise(8, 1, 10)], route!.Raises);
    }

    [Fact]
    public void NoPoints_IsNeverARoute()
    {
        Assert.Null(FocusSearch.Cheapest([Candidate(0)], Everyone, _ => true));
    }

    [Fact]
    public void ASearchThatFindsNothing_StopsAtTheStateLimit()
    {
        var candidates = Enumerable.Range(0, FocusSearch.MostCandidates)
            .Select(index => Candidate(index, Enumerable.Repeat(1, FocusSearch.MostPoints).ToArray()))
            .ToArray();

        Assert.Null(FocusSearch.Cheapest(candidates, Everyone, _ => false));
    }

    private static FocusCandidate Candidate(int raiseIndex, params int[] stepCosts)
    {
        var candidate = new FocusCandidate { RaiseIndex = raiseIndex, MostPoints = stepCosts.Length };
        stepCosts.CopyTo(candidate.StepCosts, 0);
        return candidate;
    }
}
