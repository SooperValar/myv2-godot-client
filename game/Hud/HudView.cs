using System;
using System.Globalization;
using System.IO;
using Godot;
using TheLastGate.Assets;
using TheLastGate.Config;
using TheLastGate.Game;
using TheLastGate.Items;
using TheLastGate.State;
using TheLastGate.World;

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
    private const string SwapSetHint = "Swap equipment with a stored alternate set.";
    private const int HintFont = 1;

    private bool darkMode;
    private SpriteLoader? editorSprites;
    private IFontSet? fonts;
    private int selectedCharacter;
    private Action<byte[]> sendCommand = _ => { };
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

    public void Show(GameState shownState, IFontSet fontSet, Action<byte[]> sendToServer)
    {
        state = shownState;
        sendCommand = sendToServer;
        for (var slot = WornSlot.Head; slot <= WornSlot.SecondCharm; slot++)
        {
            var clickedSlot = slot;
            GetNode<ItemSlotView>("%" + slot).Released += button => ClickWorn(clickedSlot, button);
        }

        GetNode<ClickArea>("%SwapSetButton").Released += ClickSwapSet;
        Fonts = fontSet;
        GetNode<ChatLog>("%ChatLog").Show(state.Chat, fontSet);
        Refresh(0);
    }

    public void Refresh(int selected)
    {
        selectedCharacter = selected;
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
        ShowEquipment(player);
        var buffIcons = BuffIcons.For(player.Buffs);
        GetNode<BuffIconsView>("%Buffs").Show(buffIcons.Buffs);
        GetNode<BuffIconsView>("%Debuffs").Show(buffIcons.Debuffs);
    }

    public override void _Process(double delta)
    {
        if (state == null || GetViewport().GuiGetHoveredControl() is not ItemSlotView hovered
            || !Enum.TryParse<WornSlot>(hovered.Name, out var slot))
        {
            return;
        }

        var cursor = EquipmentClicks.Cursor(
            state.Player.Worn[(int)slot].Sprite != 0,
            state.Player.HeldItem.Sprite != 0,
            HeldKeys.Current());
        hovered.MouseDefaultCursorShape = (CursorShape)CursorShapes.For(cursor);
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

    private void ClickWorn(WornSlot slot, MapButton button)
    {
        if (EquipmentClicks.Packet(slot, button, HeldKeys.Current(), selectedCharacter) is { } packet)
        {
            sendCommand(packet);
        }
    }

    private void ClickSwapSet(MapButton button)
    {
        if (EquipmentClicks.SwapSetPacket(button, selectedCharacter) is { } packet)
        {
            sendCommand(packet);
        }
        else if (button == MapButton.Right)
        {
            state?.Chat.Add(new LogLine(SwapSetHint, HintFont));
        }
    }

    private void ShowEquipment(PlayerState player)
    {
        var blocked = Equipment.BlockedSlots(player.HeldItem, player.Worn);
        var secondCharmWorn = player.Worn[(int)WornSlot.SecondCharm].Sprite != 0;
        GetNode<CanvasItem>("%SecondCharmFrame").Visible = secondCharmWorn;
        for (var slot = WornSlot.Head; slot <= WornSlot.SecondCharm; slot++)
        {
            var item = player.Worn[(int)slot];
            var showBlocked = blocked.Contains(slot) && (slot != WornSlot.SecondCharm || secondCharmWorn);
            GetNode<ItemSlotView>("%" + slot).Show(item, ItemOverlays.Worn(item), showBlocked);
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
