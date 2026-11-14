using System;
using System.Diagnostics;
using TheLastGate.State;

namespace TheLastGate.Net.Handlers;

internal static class TerminologyCommands
{
    private const int ChunkLength = 10;

    private const byte TreeIcon = 0;
    private const byte TreeName = 1;
    private const byte TreeDescription1 = 4;
    private const byte TreeDescription2 = 9;
    private const byte LastTreeText = 13;

    private const byte SkillSort = 14;
    private const byte SkillName = 15;
    private const byte SkillDescription = 18;
    private const byte LastSkillText = 37;

    private const byte MetaName = 38;
    private const byte MetaDescription = 41;
    private const byte LastMetaText = 60;
    private const byte MetaValues = 61;

    public static void Apply(Terminology terminology, ReadOnlySpan<byte> command)
    {
        var reader = new PacketReader(command);
        var commandId = (ServerCommand)reader.ReadByte();
        var kind = reader.ReadByte();
        var index = reader.ReadByte();
        switch (commandId)
        {
            case ServerCommand.TermSkillTree:
                ApplyTree(terminology.Trees[0], kind, index, ref reader);
                break;
            case ServerCommand.TermContractTree:
                ApplyTree(terminology.Trees[1], kind, index, ref reader);
                break;
            case ServerCommand.TermSkills:
                if (index < Terminology.SkillCount)
                {
                    ApplySkill(terminology.Skills[index], kind, ref reader);
                }

                break;
            case ServerCommand.TermMeta:
                if (index < Terminology.MetaStatCount)
                {
                    ApplyMetaStat(terminology.MetaStats[index], kind, ref reader);
                }

                break;
            default:
                throw new UnreachableException();
        }
    }

    private static void ApplyTree(SkillTreeNode[] tree, byte kind, byte index, ref PacketReader reader)
    {
        if (index >= tree.Length)
        {
            return;
        }

        var node = tree[index];
        switch (kind)
        {
            case TreeIcon:
                node.Icon = reader.ReadUInt16();
                break;
            case < TreeDescription1:
                CopyChunk(ref reader, node.NameField, kind - TreeName);
                break;
            case < TreeDescription2:
                CopyChunk(ref reader, node.Description1Field, kind - TreeDescription1);
                break;
            case <= LastTreeText:
                CopyChunk(ref reader, node.Description2Field, kind - TreeDescription2);
                break;
        }
    }

    private static void ApplySkill(SkillText skill, byte kind, ref PacketReader reader)
    {
        switch (kind)
        {
            case SkillSort:
                skill.SortKey = (sbyte)reader.ReadByte();
                for (var attribute = 0; attribute < SkillText.AttributeCount; attribute++)
                {
                    skill.Attributes[attribute] = (sbyte)reader.ReadByte();
                }

                skill.Show = (sbyte)reader.ReadByte();
                break;
            case >= SkillName and < SkillDescription:
                CopyChunk(ref reader, skill.NameField, kind - SkillName);
                break;
            case >= SkillDescription and <= LastSkillText:
                CopyChunk(ref reader, skill.DescriptionField, kind - SkillDescription);
                break;
        }
    }

    private static void ApplyMetaStat(MetaStat stat, byte kind, ref PacketReader reader)
    {
        switch (kind)
        {
            case >= MetaName and < MetaDescription:
                CopyChunk(ref reader, stat.NameField, kind - MetaName);
                break;
            case >= MetaDescription and <= LastMetaText:
                CopyChunk(ref reader, stat.DescriptionField, kind - MetaDescription);
                break;
            case MetaValues:
                stat.Value = reader.ReadUInt16();
                stat.Flag = (sbyte)reader.ReadByte();
                reader.ReadBytes(MetaStat.AffixLength).CopyTo(stat.AffixField);
                stat.Font = (sbyte)reader.ReadByte();
                stat.Show = (sbyte)reader.ReadByte();
                break;
        }
    }

    private static void CopyChunk(ref PacketReader reader, byte[] field, int chunkIndex) =>
        reader.ReadBytes(ChunkLength).CopyTo(field.AsSpan(chunkIndex * ChunkLength));
}
