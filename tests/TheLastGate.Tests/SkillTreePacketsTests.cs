using TheLastGate.Net;
using TheLastGate.State;

namespace TheLastGate.Tests;

public class SkillTreePacketsTests
{
    [Theory]
    [InlineData(SkillTree.Class, 3)]
    [InlineData(SkillTree.Contract, 39)]
    public void Learn_SendsTheNodeAfterTheTreesOffset(SkillTree tree, byte value)
    {
        byte[] expected = [39, value, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

        Assert.Equal(expected, SkillTreePackets.Learn(tree, 3));
    }

    [Theory]
    [InlineData(SkillTree.Class, 27)]
    [InlineData(SkillTree.Contract, 63)]
    public void Unlearn_SendsTheNodePast24(SkillTree tree, byte value)
    {
        byte[] expected = [39, value, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

        Assert.Equal(expected, SkillTreePackets.Unlearn(tree, 3));
    }

    [Theory]
    [InlineData(SkillTree.Class, 15)]
    [InlineData(SkillTree.Contract, 51)]
    public void Describe_SendsTheNodePast12(SkillTree tree, byte value)
    {
        byte[] expected = [39, value, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

        Assert.Equal(expected, SkillTreePackets.Describe(tree, 3));
    }
}
