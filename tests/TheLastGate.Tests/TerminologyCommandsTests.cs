using TheLastGate.Net;
using TheLastGate.State;

namespace TheLastGate.Tests;

public class TerminologyCommandsTests
{
    private const int ChunkLength = 10;

    [Fact]
    public void TermTree_SetsANodesIconInEitherTree()
    {
        var terminology = Process(
            TestCommand.Of(ServerCommand.TermSkillTree).Bytes(0, 3).UInt16(1234),
            TestCommand.Of(ServerCommand.TermContractTree).Bytes(0, 3).UInt16(5678));

        Assert.Equal(1234, terminology.Trees[0][3].Icon);
        Assert.Equal(5678, terminology.Trees[1][3].Icon);
    }

    [Fact]
    public void TermTree_JoinsTheNameAndBothDescriptionsFromTheirChunks()
    {
        const string name = "Mastery of the Blade, Rank IV";
        const string first = "Increases the damage of every attack by 10 percent";
        const string second = "Each rank also grants one point of extra stamina.";

        var terminology = Process(
            Chunks(ServerCommand.TermSkillTree, 1, 7, name)
                .Concat(Chunks(ServerCommand.TermSkillTree, 4, 7, first))
                .Concat(Chunks(ServerCommand.TermSkillTree, 9, 7, second))
                .ToArray());

        var node = terminology.Trees[0][7];
        Assert.Equal(name, node.Name);
        Assert.Equal(first, node.Description1);
        Assert.Equal(second, node.Description2);
    }

    [Fact]
    public void TermTree_IgnoresANodePastTheTwelfth()
    {
        var terminology = Process(TestCommand.Of(ServerCommand.TermSkillTree).Bytes(0, 12).UInt16(1234));

        Assert.All(terminology.Trees[0], node => Assert.Equal(0, node.Icon));
    }

    [Fact]
    public void TermSkills_SetsASkillsSortKeyAttributesAndShowFlag()
    {
        var terminology = Process(TestCommand.Of(ServerCommand.TermSkills).Bytes(14, 7, 200, 1, 2, 3, 1));

        var skill = terminology.Skills[7];
        Assert.Equal(-56, skill.SortKey);
        Assert.Equal(new[] { 1, 2, 3 }, skill.Attributes);
        Assert.Equal(1, skill.Show);
    }

    [Fact]
    public void TermSkills_JoinsANameAndATwoHundredCharacterDescription()
    {
        var description = string.Concat(Enumerable.Repeat("0123456789", 20));

        var terminology = Process(
            Chunks(ServerCommand.TermSkills, 15, 54, "Surround Hit")
                .Concat(Chunks(ServerCommand.TermSkills, 18, 54, description))
                .ToArray());

        Assert.Equal("Surround Hit", terminology.Skills[54].Name);
        Assert.Equal(description, terminology.Skills[54].Description);
    }

    [Fact]
    public void TermSkills_IgnoresASkillPastTheLast()
    {
        var terminology = Process(TestCommand.Of(ServerCommand.TermSkills).Bytes(14, 55, 5, 1, 2, 3, 1));

        Assert.All(terminology.Skills, skill => Assert.Equal(0, skill.SortKey));
    }

    [Fact]
    public void TermSkills_IgnoresAKindThatBelongsToAnotherTable()
    {
        var terminology = Process(TestCommand.Of(ServerCommand.TermSkills).Bytes(0, 3).UInt16(1234));

        Assert.Equal(0, terminology.Trees[0][3].Icon);
        Assert.All(terminology.Skills, skill => Assert.Equal(0, skill.SortKey));
    }

    [Fact]
    public void TermMeta_JoinsTheNameAndDescriptionAndSetsTheValues()
    {
        var terminology = Process(
            Chunks(ServerCommand.TermMeta, 38, 88, "Hit Score")
                .Concat(Chunks(ServerCommand.TermMeta, 41, 88, "Your chance to land a blow."))
                .Append(
                    TestCommand.Of(ServerCommand.TermMeta).Bytes(61, 88).UInt16(65535).Byte(3).Text("%", 8).Bytes(2, 1))
                .ToArray());

        var stat = terminology.MetaStats[88];
        Assert.Equal("Hit Score", stat.Name);
        Assert.Equal("Your chance to land a blow.", stat.Description);
        Assert.Equal(65535, stat.Value);
        Assert.Equal(3, stat.Flag);
        Assert.Equal("%", stat.Affix);
        Assert.Equal(2, stat.Font);
        Assert.Equal(1, stat.Show);
    }

    [Fact]
    public void TermMeta_IgnoresAStatPastTheLast()
    {
        var terminology = Process(TestCommand.Of(ServerCommand.TermMeta).Bytes(61, 89).UInt16(5).Byte(3));

        Assert.All(terminology.MetaStats, stat => Assert.Equal(0, stat.Value));
    }

    private static IEnumerable<TestCommand> Chunks(ServerCommand commandId, byte firstKind, byte index, string text)
    {
        for (var chunkIndex = 0; chunkIndex * ChunkLength < text.Length; chunkIndex++)
        {
            var chunk = text.Substring(
                chunkIndex * ChunkLength,
                Math.Min(ChunkLength, text.Length - chunkIndex * ChunkLength));
            yield return TestCommand.Of(commandId)
                .Bytes((byte)(firstKind + chunkIndex), index)
                .Text(chunk, ChunkLength);
        }
    }

    private static Terminology Process(params TestCommand[] commands)
    {
        var state = new GameState();
        new TickProcessor(state).Process(TestCommand.Payload(commands));
        return state.Terminology;
    }
}
