using System;
using TheLastGate.Net;
using TheLastGate.World;

namespace TheLastGate.Windows;

public sealed class SkillTreeWindow
{
    private const int UnlearnOffset = 24;
    private const int DescribeOffset = 12;
    private const int ContractOffset = 36;
    private const int GlowPeriodMilliseconds = 16;
    private const int GlowBrightestAt = 8;

    public SkillTree? Open { get; private set; }

    public static bool ButtonShows(int classPoints) => SkillTrees.Total(classPoints) > 0;

    // The current client's glow changes nearly every frame, so it reads as a flicker rather than a pulse.
    public static int GlowLight(long milliseconds)
    {
        var fromBrightest = Math.Abs(GlowBrightestAt - (int)(milliseconds % GlowPeriodMilliseconds));
        return Math.Min(SpriteLighting.DarkestLight, fromBrightest * 2);
    }

    public static string Counter(int treeValue) => $"{SkillTrees.Unspent(treeValue)}/{SkillTrees.Total(treeValue)}";

    // The server writes its descriptions as format strings, so a percent sign arrives doubled.
    public static string ShownText(string text) => text.Replace("%%", "%");

    public bool ButtonGlows(int classPoints, int contractPoints) =>
        (SkillTrees.Unspent(classPoints) > 0 && Open != SkillTree.Class)
        || (SkillTrees.Unspent(contractPoints) > 0 && Open != SkillTree.Contract);

    // Any modifier asks for the contract tree. Asking for the open tree, or one without points, closes the window.
    public void Toggle(MapKeys keys, int classPoints, int contractPoints)
    {
        var (tree, points) = keys == MapKeys.None
            ? (SkillTree.Class, classPoints)
            : (SkillTree.Contract, contractPoints);
        Open = SkillTrees.Total(points) > 0 && Open != tree ? tree : null;
    }

    public bool Close()
    {
        var wasOpen = Open != null;
        Open = null;
        return wasOpen;
    }

    public byte[]? Click(int node, MapButton button, MapKeys keys)
    {
        if (Open is not { } tree)
        {
            return null;
        }

        var value = button switch
        {
            MapButton.Left => keys == MapKeys.None ? node : node + UnlearnOffset,
            MapButton.Right => node + DescribeOffset,
            _ => (int?)null,
        };
        var treeOffset = tree == SkillTree.Contract ? ContractOffset : 0;
        return value is { } nodeValue ? ClientPackets.OneValue(ClientCommand.Tree, treeOffset + nodeValue) : null;
    }
}
