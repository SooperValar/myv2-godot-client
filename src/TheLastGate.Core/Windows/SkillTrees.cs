using System;

namespace TheLastGate.Windows;

public enum SkillTree
{
    Class,
    Contract,
}

public static class SkillTrees
{
    public const int Nodes = 12;

    public static bool IsLearned(int treeValue, int node) => throw new NotImplementedException();

    public static int Unspent(int treeValue) => throw new NotImplementedException();

    public static int Total(int treeValue) => throw new NotImplementedException();
}
