using Godot;
using TheLastGate.State;
using TheLastGate.Windows;

namespace TheLastGate.Hud;

// The class or contract tree: twelve nodes, a ring around each learned one, and the hovered node's text at the bottom.
public partial class SkillTreeWindowView : Control
{
    private const int ClassCentre = 6600;
    private const int ContractCentre = 7066;
    private const float HighlightBrightness = 2f;

    private int? hoveredNode;
    private GameState? shownState;

    public SkillTreeNodeArea Node(int node) => GetNode<SkillTreeNodeArea>($"Node{node}");

    public override void _Ready()
    {
        for (var node = 0; node < SkillTrees.Nodes; node++)
        {
            var shownNode = node;
            Node(node).MouseEntered += () => Hover(shownNode, true);
            Node(node).MouseExited += () => Hover(shownNode, false);
        }
    }

    public void ShowTree(GameState state)
    {
        shownState = state;
        var tree = state.SkillTreeWindow.Open;
        Visible = tree != null;
        if (tree is not { } openTree)
        {
            return;
        }

        var player = state.Player;
        var treeValue = openTree == SkillTree.Class ? player.TreePoints : player.OsirisTree;
        var nodes = state.Terminology.Trees[(int)openTree];
        for (var node = 0; node < SkillTrees.Nodes; node++)
        {
            var area = Node(node);
            area.GetNode<CanvasItem>("Ring").Visible = SkillTrees.IsLearned(treeValue, node);
            area.GetNode<ArchiveSprite>("Icon").SpriteNumber = nodes[node].Icon;
            area.GetNode<CanvasItem>("Corruption").Visible = openTree == SkillTree.Class && player.TreeNodes[node] != 0;
        }

        GetNode<ArchiveSprite>("Centre").SpriteNumber = openTree == SkillTree.Class ? ClassCentre : ContractCentre;
        GetNode<Label>("Counter").Text = SkillTreeWindow.Counter(treeValue);
        var hovered = hoveredNode is { } index ? nodes[index] : null;
        GetNode<Label>("Name").Text = SkillTreeWindow.ShownText(hovered?.Name ?? "");
        GetNode<Label>("Description1").Text = SkillTreeWindow.ShownText(hovered?.Description1 ?? "");
        GetNode<Label>("Description2").Text = SkillTreeWindow.ShownText(hovered?.Description2 ?? "");
    }

    private void Hover(int node, bool entered)
    {
        if (entered)
        {
            hoveredNode = node;
        }
        else if (hoveredNode == node)
        {
            hoveredNode = null;
        }

        var brightness = entered ? HighlightBrightness : 1f;
        var highlight = new Color(brightness, brightness, brightness);
        Node(node).GetNode<CanvasItem>("Icon").Modulate = highlight;
        Node(node).GetNode<CanvasItem>("Corruption").Modulate = highlight;
        if (shownState != null)
        {
            ShowTree(shownState);
        }
    }
}
