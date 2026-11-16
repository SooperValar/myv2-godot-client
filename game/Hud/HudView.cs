using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Godot;
using TheLastGate.Assets;
using TheLastGate.Characters;
using TheLastGate.Config;
using TheLastGate.Cursors;
using TheLastGate.Game;
using TheLastGate.Items;
using TheLastGate.Net;
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
    private const string TrashCanName = "TrashCan";
    private const int HintFont = 1;
    private const int WarningFont = 0;
    private static readonly Vector2 HeldItemOffset = new(16, 16);
    private readonly ExitSequence exit = new();

    private bool darkMode;
    private SpriteLoader? editorSprites;
    private IFontSet? fonts;
    private Vector2 mousePosition;
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

    public CharacterSettings Settings { get; set; } = new();

    public IFontSet? Fonts
    {
        get => fonts;
        set
        {
            fonts = value;
            Reload(this);
        }
    }

    public event Action? SettingsChanged;

    public event Action? LeaveRequested;

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
        GetNode<ClickArea>("%" + TrashCanName).Released += ClickTrashCan;
        foreach (var node in GetNode("%OptionButtons").GetChildren())
        {
            var optionButton = (OptionButtonView)node;
            optionButton.Released += button => ClickOption(optionButton.Kind, button);
        }

        foreach (var node in GetNode("%MoneyButtons").GetChildren())
        {
            var moneyButton = (MoneyButton)node;
            moneyButton.Released += button => ClickMoney(moneyButton.Silver, button);
        }

        var inventory = GetNode<InventoryGrid>("%Inventory");
        for (var cell = 0; cell < InventoryScroll.VisibleSlots; cell++)
        {
            var clickedCell = cell;
            inventory.GetChild<ItemSlotView>(cell).Released +=
                button => ClickInventory(inventory.SlotShownIn(clickedCell), button);
        }

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
        SetText("OwnName", player.Name);
        SetText("PlayerName", player.Name);
        SetText("RankName", rank.Name);
        SetSprite("RankBadge", rank.BadgeSprite);
        SetSprite("Portrait", state.Map.Tiles[WorldMap.PlayerTile].AnimatedCharacterSprite);
        ShowEquipment(player);
        foreach (var node in GetNode("%OptionButtons").GetChildren())
        {
            var optionButton = (OptionButtonView)node;
            optionButton.ShowMarked(OptionButtons.IsMarked(optionButton.Kind, Settings, player.Mode));
        }

        GetNode<InventoryGrid>("%Inventory").Show(player);
        var buffIcons = BuffIcons.For(player.Buffs);
        GetNode<BuffIconsView>("%Buffs").Show(buffIcons.Buffs);
        GetNode<BuffIconsView>("%Debuffs").Show(buffIcons.Debuffs);
    }

    public override void _Input(InputEvent inputEvent)
    {
        if (inputEvent is InputEventMouse mouse)
        {
            mousePosition = mouse.Position;
        }
    }

    public override void _Process(double delta)
    {
        if (state == null)
        {
            return;
        }

        exit.Advance(TimeSpan.FromSeconds(delta));
        ShowHeldItem(state.Player.HeldItem);
        if (GetViewport().GuiGetHoveredControl() is not ClickArea hovered)
        {
            return;
        }

        var player = state.Player;
        var holdingItem = player.HeldItem.Sprite != 0;
        MouseCursor cursor;
        if (hovered.Name == TrashCanName)
        {
            cursor = TrashCan.Cursor(holdingItem);
        }
        else if (hovered.GetParent() is InventoryGrid inventory)
        {
            var item = player.Inventory[inventory.SlotShownIn(hovered.GetIndex())];
            cursor = InventoryClicks.Cursor(item, holdingItem, HeldKeys.Current());
        }
        else if (Enum.TryParse<WornSlot>(hovered.Name, out var slot))
        {
            cursor = EquipmentClicks.Cursor(player.Worn[(int)slot].Sprite != 0, holdingItem, HeldKeys.Current());
        }
        else
        {
            return;
        }

        var shape = (CursorShape)CursorShapes.For(cursor);
        if (hovered.MouseDefaultCursorShape != shape)
        {
            hovered.MouseDefaultCursorShape = shape;
            GetViewport().UpdateMouseCursorState();
        }
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

    private void ClickInventory(int slot, MapButton button)
    {
        if (state == null)
        {
            return;
        }

        var player = state.Player;
        var packets = InventoryClicks.Packets(
            slot,
            player.Inventory[slot],
            player.HeldItem.Sprite != 0,
            button,
            HeldKeys.Current(),
            SaleToOpenShop(),
            selectedCharacter);
        foreach (var packet in packets)
        {
            sendCommand(packet);
        }
    }

    private ShopSale? SaleToOpenShop() =>
        state?.OpenShop is { } shop and not ShopKind.WeaponSmith and not ShopKind.ArmourSmith
            ? new ShopSale(state.Shop.Number, 0)
            : null;

    private void ClickOption(OptionButton option, MapButton button)
    {
        if (button == MapButton.Right)
        {
            if (OptionButtons.Help(option) is { } help)
            {
                state?.Chat.Add(new LogLine(help, HintFont));
            }

            return;
        }

        if (button != MapButton.Left)
        {
            return;
        }

        if (option == OptionButton.Exit)
        {
            ClickExit();
            return;
        }

        if (OptionButtons.Packet(option) is { } packet)
        {
            sendCommand(packet);
        }

        if (OptionButtons.Toggle(option, Settings))
        {
            SettingsChanged?.Invoke();
        }
    }

    public void ServerEnded(int reasonCode)
    {
        if (exit.ServerEnded(reasonCode))
        {
            LeaveRequested?.Invoke();
        }
    }

    private void ClickExit()
    {
        switch (exit.Click())
        {
            case ExitStep.Warn:
                foreach (var packet in ChatPackets.Say(ExitSequence.TavernCommand))
                {
                    sendCommand(packet);
                }

                AddWarnings(ExitSequence.WarningLines);
                break;
            case ExitStep.RequestExit:
                sendCommand(ExitSequence.ExitPacket);
                AddWarnings(ExitSequence.RequestLines);
                break;
            case ExitStep.Leave:
                LeaveRequested?.Invoke();
                break;
            case ExitStep.Wait:
                break;
            default:
                throw new UnreachableException();
        }
    }

    private void AddWarnings(IEnumerable<string> lines)
    {
        foreach (var line in lines)
        {
            state?.Chat.Add(new LogLine(line, WarningFont));
        }
    }

    private void ClickMoney(int silver, MapButton button)
    {
        if (MoneyButtons.Packet(silver, button, selectedCharacter) is { } packet)
        {
            sendCommand(packet);
        }
        else if (button == MapButton.Right)
        {
            state?.Chat.Add(new LogLine(MoneyButtons.Hint(silver), HintFont));
        }
    }

    private void ClickTrashCan(MapButton button)
    {
        if (TrashCan.Packet(button, selectedCharacter) is { } packet)
        {
            sendCommand(packet);
        }
        else if (button == MapButton.Right)
        {
            state?.Chat.Add(new LogLine(TrashCan.Hint, HintFont));
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

    private void ShowHeldItem(ItemSlot item)
    {
        var held = GetNode<ItemSlotView>("%HeldItem");
        held.Show(item, ItemOverlays.Worn(item), false);
        held.Position = mousePosition - HeldItemOffset;
        held.SetHighlighted(HeldItem.IsHighlighted(CursorShapes.CursorFor(Input.GetCurrentCursorShape())));
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
