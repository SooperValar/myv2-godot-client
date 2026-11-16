using System;
using System.Globalization;
using System.IO;
using Godot;
using TheLastGate.Assets;
using TheLastGate.Config;
using TheLastGate.State;

namespace TheLastGate.Hud;

[Tool]
public partial class HudView : Control
{
    private const long EditorCacheBudgetBytes = 64L * 1024 * 1024;
    private const int ReservedSlot = 4;
    private const int MaximumSlot = 5;
    private const string StrongholdName = "Stronghold";
    private const string CasinoName = "Casino";
    private const string ContractName = "Contract";

    private bool darkMode;
    private SpriteLoader? editorSprites;
    private IFontSet? fonts;
    private SpriteLoader? sprites;
    private GameState? state;

    [Export]
    public bool DarkMode
    {
        get => darkMode;
        set
        {
            darkMode = value;
            Reload(this);
        }
    }

    public SpriteLoader? Sprites
    {
        get => sprites;
        set
        {
            sprites = value;
            Reload(this);
        }
    }

    public IFontSet? Fonts
    {
        get => fonts;
        set
        {
            fonts = value;
            Reload(this);
        }
    }

    public static HudView? Containing(Node node)
    {
        for (var parent = node.GetParent(); parent != null; parent = parent.GetParent())
        {
            if (parent is HudView hud)
            {
                return hud;
            }
        }

        return null;
    }

    public void Show(GameState shownState, IFontSet fontSet)
    {
        state = shownState;
        Fonts = fontSet;
        GetNode<ChatLog>("%ChatLog").Show(state.Chat, fontSet);
        Refresh(0);
    }

    public void Refresh(int selectedCharacter)
    {
        if (state == null)
        {
            return;
        }

        var player = state.Player;
        ShowPool("HitPoints", player.ActiveHitPoints, player.HitPoints[MaximumSlot]);
        ShowPool("Endurance", player.ActiveEndurance, player.Endurance[MaximumSlot]);
        ShowPool("Mana", player.ActiveMana, player.Mana[MaximumSlot]);
        SetText("Money", Currencies.MoneyText(player.Gold));
        ShowPoints("Stronghold", StrongholdName, player.BlackStrongholdPoints);
        ShowPoints("Casino", CasinoName, player.Tokens);
        ShowPoints("Contract", ContractName, player.OsirisPoints);
        SetText("Location", player.Location);
        ShowBar("HitPointBar", player.ActiveHitPoints, player.HitPoints);
        ShowBar("EnduranceBar", player.ActiveEndurance, player.Endurance);
        ShowBar("ManaBar", player.ActiveMana, player.Mana);
        var rank = Ranks.For(player.PointsTotal);
        SetText(
            "SelectedName",
            selectedCharacter == 0 ? player.Name : state.Names.NameOf(selectedCharacter, 0));
        SetText("PlayerName", player.Name);
        SetText("RankName", rank.Name);
        SetSprite("RankBadge", rank.BadgeSprite);
        SetSprite("Portrait", state.Map.Tiles[WorldMap.PlayerTile].AnimatedCharacterSprite);
    }

    public override void _EnterTree()
    {
        if (!Engine.IsEditorHint())
        {
            return;
        }

        try
        {
            editorSprites = new SpriteLoader(DataPaths.Root(), EditorCacheBudgetBytes);
            sprites = editorSprites;
            fonts = new ClassicFonts(editorSprites);
        }
        catch (Exception exception) when (exception is IOException or InvalidDataException
            or UnauthorizedAccessException)
        {
            GD.PushWarning($"The interface sprites can't be shown: {exception.Message}");
        }
    }

    public override void _ExitTree()
    {
        if (editorSprites == null)
        {
            return;
        }

        editorSprites.Dispose();
        editorSprites = null;
        sprites = null;
        fonts = null;
    }

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);

    private static void Reload(Node parent)
    {
        foreach (var child in parent.GetChildren())
        {
            switch (child)
            {
                case ArchiveSprite archiveSprite:
                    archiveSprite.LoadSprite();
                    break;
                case HudLabel label:
                    label.ApplyFont();
                    break;
            }

            Reload(child);
        }
    }

    private void ShowPool(string name, int current, int maximum)
    {
        SetText(name, Number(current));
        SetText("Max" + name, Number(maximum));
    }

    private void ShowBar(string name, int current, int[] values) =>
        GetNode<PortraitBar>("%" + name).Show(PortraitBars.For(current, values[MaximumSlot], values[ReservedSlot]));

    private void ShowPoints(string name, string shownName, int points)
    {
        SetText(name + "Name", Currencies.RowName(shownName, points));
        SetText(name, Number(points));
    }

    private void SetText(string name, string text) => GetNode<Label>("%" + name).Text = text;

    private void SetSprite(string name, int spriteNumber) =>
        GetNode<ArchiveSprite>("%" + name).SpriteNumber = spriteNumber;
}
