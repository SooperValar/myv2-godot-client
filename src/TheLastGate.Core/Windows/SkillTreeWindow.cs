using System;
using TheLastGate.World;

namespace TheLastGate.Windows;

public sealed class SkillTreeWindow
{
    public SkillTree? Open { get; private set; }

    public static bool ButtonShows(int classPoints) => throw new NotImplementedException();

    public static int GlowLight(long milliseconds) => throw new NotImplementedException();

    public static string Counter(int treeValue) => throw new NotImplementedException();

    public static string ShownText(string text) => throw new NotImplementedException();

    public bool ButtonGlows(int classPoints, int contractPoints) => throw new NotImplementedException();

    public void Toggle(MapKeys keys, int classPoints, int contractPoints) => throw new NotImplementedException();

    public bool Close() => throw new NotImplementedException();

    public byte[]? Click(int node, MapButton button, MapKeys keys) => throw new NotImplementedException();
}
