using TheLastGate.State;

namespace TheLastGate.Tests;

public class SkillTreesTests
{
    private const int FirstNodeLearned = 1 << 15;
    private const int LastNodeLearned = 1 << 4;
    private const int OneUnspent = 1;
    private const int TwoPointsOneSpent = FirstNodeLearned | OneUnspent;
    private const int AllSpent = FirstNodeLearned;
    private const int NoPoints = 0;

    [Theory]
    [InlineData(FirstNodeLearned, 0, true)]
    [InlineData(FirstNodeLearned, 1, false)]
    [InlineData(LastNodeLearned, 11, true)]
    [InlineData(LastNodeLearned, 10, false)]
    [InlineData(0b1111, 11, false)]
    public void IsLearned_ReadsTheNodesBitFromTheTop(int treeValue, int node, bool learned)
    {
        Assert.Equal(learned, SkillTrees.IsLearned(treeValue, node));
    }

    [Theory]
    [InlineData(TwoPointsOneSpent, 1, 2)]
    [InlineData(AllSpent, 0, 1)]
    [InlineData(NoPoints, 0, 0)]
    [InlineData(0xFFFF, 15, 27)]
    public void UnspentAndTotal_CountTheLowBitsAndTheLearnedNodes(int treeValue, int unspent, int total)
    {
        Assert.Equal(unspent, SkillTrees.Unspent(treeValue));
        Assert.Equal(total, SkillTrees.Total(treeValue));
    }

    [Fact]
    public void Terminology_StartsWithPlaceholdersUntilTheServerSendsTheTrees()
    {
        var terminology = new Terminology();

        Assert.Equal(6601, terminology.Trees[0][0].Icon);
        Assert.Equal("p01", terminology.Trees[0][0].Name);
        Assert.Equal(6720, terminology.Trees[1][11].Icon);
        Assert.Equal("c12", terminology.Trees[1][11].Name);
        Assert.Equal("", terminology.Trees[1][11].Description1);
    }
}
