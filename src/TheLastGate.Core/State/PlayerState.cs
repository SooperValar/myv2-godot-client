using System;
using System.Linq;
using TheLastGate.Characters;

namespace TheLastGate.State;

public sealed class PlayerState
{
    public const int AttributeCount = 5;
    public const int SkillSlots = 100;
    public const int StatValueCount = 6;
    public const int TreeNodeCount = 12;

    private const int NameLength = 40;
    private const int LocationLength = 20;

    public string Name => NulTerminatedText.Read(NameField);
    public int Race { get; internal set; }
    public string Location => NulTerminatedText.Read(LocationField);
    public int Mode { get; internal set; }
    public int Direction { get; internal set; }

    public int[][] Attributes { get; } = CreateStats(AttributeCount);
    public int[][] Skills { get; } = CreateStats(SkillSlots);
    public int[] HitPoints { get; } = new int[StatValueCount];
    public int[] Endurance { get; } = new int[StatValueCount];
    public int[] Mana { get; } = new int[StatValueCount];

    public int ActiveHitPoints { get; internal set; }
    public int ActiveEndurance { get; internal set; }
    public int ActiveMana { get; internal set; }

    public int Points { get; internal set; }
    public int PointsTotal { get; internal set; }
    public uint Kindred { get; internal set; }

    public int Gold { get; internal set; }
    public int Armor { get; internal set; }
    public int Weapon { get; internal set; }
    public int Tokens { get; internal set; }
    public int Waypoints { get; internal set; }
    public int BlackStrongholdPoints { get; internal set; }
    public int OsirisPoints { get; internal set; }

    public int TreePoints { get; internal set; }
    public int OsirisTree { get; internal set; }
    public byte[] TreeNodes { get; } = new byte[TreeNodeCount];

    internal byte[] NameField { get; } = new byte[NameLength];
    internal byte[] LocationField { get; } = new byte[LocationLength];

    public event Action? NameReceived;

    internal void ReportNameReceived() => NameReceived?.Invoke();

    private static int[][] CreateStats(int count) =>
        Enumerable.Range(0, count).Select(_ => new int[StatValueCount]).ToArray();
}
