using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Godot;
using TheLastGate.Assets;
using TheLastGate.Characters;
using TheLastGate.Config;
using TheLastGate.Cursors;
using TheLastGate.Game;
using TheLastGate.Hotbar;
using TheLastGate.Items;
using TheLastGate.Net;
using TheLastGate.State;
using TheLastGate.Stats;
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
    private const int HitPointsRow = 5;
    private const int ManaRow = 6;
    private static readonly Vector2 HeldItemOffset = new(16, 16);
    private readonly ExitSequence exit = new();
    private readonly LookPanel lookPanel = new();
    private readonly SkillShortcut skillShortcut = new();
    private readonly StatPlan statPlan = new();

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

        foreach (var node in GetNode("%StatRows").GetChildren().Concat(GetNode("%SkillRows").GetChildren()))
        {
            var row = (StatRow)node;
            row.GetNode<ClickArea>("NameArea").Released += button => ClickStatName(row, button);
            row.GetNode<ClickArea>("RaiseArea").Released += button => ClickStat(row, true, button);
            row.GetNode<ClickArea>("LowerArea").Released += button => ClickStat(row, false, button);
        }

        GetNode<ClickArea>("%UpdateButton").Released += ClickUpdate;
        foreach (var node in GetNode("%Hotbar").GetChildren())
        {
            var hotbarButton = (HotbarButtonView)node;
            hotbarButton.Released += button => ClickHotbar(hotbarButton.Slot, button);
        }

        var inventory = GetNode<InventoryGrid>("%Inventory");
        for (var cell = 0; cell < InventoryScroll.VisibleSlots; cell++)
        {
            var clickedCell = cell;
            inventory.GetChild<ItemSlotView>(cell).Released +=
                button => ClickInventory(inventory.SlotShownIn(clickedCell), button);
        }

        state.LookShown += ShowLook;
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
        if (lookPanel.Shown is { } look)
        {
            ShowPortrait(look);
        }
        else
        {
            ShowPortrait(player, state.Map.Tiles[WorldMap.PlayerTile].AnimatedCharacterSprite);
        }

        foreach (var node in GetNode("%OptionButtons").GetChildren())
        {
            var optionButton = (OptionButtonView)node;
            optionButton.ShowMarked(OptionButtons.IsMarked(optionButton.Kind, Settings, player.Mode));
        }

        foreach (var node in GetNode("%Hotbar").GetChildren())
        {
            var hotbarButton = (HotbarButtonView)node;
            var slot = hotbarButton.Slot;
            hotbarButton.ShowName(HotbarButtons.Name(Settings.HotbarActions[slot], Settings.HotbarNames[slot]));
        }

        GetNode<InventoryGrid>("%Inventory").Show(player);
        ShowStats(player);
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

    public override void _UnhandledKeyInput(InputEvent inputEvent)
    {
        if (inputEvent is not InputEventKey { Pressed: true, Echo: false, CtrlPressed: true } key
            || (long)key.Keycode > char.MaxValue
            || HotbarButtons.SlotForKey((char)key.Keycode) is not { } slot)
        {
            return;
        }

        GetViewport().SetInputAsHandled();
        UseHotbar(slot);
    }

    public override void _Process(double delta)
    {
        if (state == null)
        {
            return;
        }

        exit.Advance(TimeSpan.FromSeconds(delta));
        var lookWasShown = lookPanel.Shown != null;
        lookPanel.Advance(TimeSpan.FromSeconds(delta));
        if (lookWasShown && lookPanel.Shown == null)
        {
            Refresh(selectedCharacter);
        }

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

    private void ShowStats(PlayerState player)
    {
        var rows = GetNode("%StatRows");
        var showBase = Settings.ShowStatBase;
        for (var attribute = 0; attribute < PlayerState.AttributeCount; attribute++)
        {
            var line = StatLines.Attribute(attribute, player.Attributes[attribute], statPlan, player.Points);
            rows.GetChild<StatRow>(attribute).Show(line, attribute, showBase);
        }

        var hitPoints = StatLines.HitPoints(player.HitPoints, statPlan, player.Points);
        rows.GetChild<StatRow>(HitPointsRow).Show(hitPoints, StatPlan.HitPointsSlot, showBase);
        var mana = StatLines.Mana(player.Mana, statPlan, player.Points);
        rows.GetChild<StatRow>(ManaRow).Show(mana, StatPlan.ManaSlot, showBase);
        GetNode<SkillList>("%SkillRows").Show(player, state!.Terminology, statPlan, showBase);
        SetText("UpdatePoints", Number(player.Points - statPlan.PointsUsed));
    }

    private void ClickHotbar(int slot, MapButton button)
    {
        if (button == MapButton.Left)
        {
            UseHotbar(slot);
        }
    }

    private void UseHotbar(int slot)
    {
        if (HotbarButtons.UsePacket(Settings.HotbarActions[slot], selectedCharacter) is { } packet)
        {
            sendCommand(packet);
        }
    }

    private void ClickStatName(StatRow row, MapButton button)
    {
        if (state == null)
        {
            return;
        }

        if (row.Slot < StatPlan.FirstSkillSlot)
        {
            if (button == MapButton.Right)
            {
                state.Chat.Add(SkillClicks.StatHelp(row.Slot));
            }

            return;
        }

        var player = state.Player;
        var number = row.Slot - StatPlan.FirstSkillSlot;
        var text = state.Terminology.Skills[number];
        var learnt = player.Skills[number][0] != 0;
        if (button == MapButton.Left)
        {
            sendCommand(SkillClicks.Use(number, selectedCharacter, text.Attributes[0]));
        }
        else if (button == MapButton.Right && SkillClicks.IsKnown(number, learnt, player.Kindred))
        {
            foreach (var line in SkillClicks.Help(text.Name, text.Description, text.Attributes))
            {
                state.Chat.Add(line);
            }

            state.Chat.Add(skillShortcut.Toggle(number, text.Name, player.Kindred));
        }
    }

    private void ClickStat(StatRow row, bool raise, MapButton button)
    {
        if (state == null)
        {
            return;
        }

        if (button == MapButton.Right)
        {
            var help = raise ? StatPlan.RaiseHelp(row.Line.Name) : StatPlan.LowerHelp(row.Line.Name);
            state.Chat.Add(new LogLine(help, HintFont));
            return;
        }

        if (button != MapButton.Left || (raise && !row.Line.CanRaise))
        {
            return;
        }

        var ctrlOrAlt = Input.IsKeyPressed(Key.Ctrl) || Input.IsKeyPressed(Key.Alt);
        var times = StatPlan.ClickTimes(Input.IsKeyPressed(Key.Shift), ctrlOrAlt);
        var values = StatPlan.Values(state.Player, row.Slot);
        if (raise)
        {
            statPlan.Raise(row.Slot, values, state.Player.Points, times);
        }
        else
        {
            statPlan.Lower(row.Slot, values, times);
        }

        ShowStats(state.Player);
    }

    private void ClickUpdate(MapButton button)
    {
        if (button == MapButton.Right)
        {
            state?.Chat.Add(new LogLine(StatPlan.UpdateHelp, HintFont));
            return;
        }

        if (button != MapButton.Left || state == null)
        {
            return;
        }

        foreach (var packet in statPlan.Commit())
        {
            sendCommand(packet);
        }

        ShowStats(state.Player);
    }

    private void ShowLook(CharacterLook look)
    {
        lookPanel.Show(look);
        Refresh(selectedCharacter);
    }

    private void ShowPortrait(PlayerState player, int sprite)
    {
        ShowBar("HitPointBar", player.ActiveHitPoints, player.HitPoints);
        ShowBar("EnduranceBar", player.ActiveEndurance, player.Endurance);
        ShowBar("ManaBar", player.ActiveMana, player.Mana);
        GetNode<CanvasItem>("%OwnName").Visible = true;
        SetText("OwnName", player.Name);
        ShowCharacter(player.Name, player.PointsTotal, sprite, SpriteTint.None);
        ShowEquipment(player);
    }

    private void ShowPortrait(CharacterLook look)
    {
        ShowLookBar("HitPointBar", look.ActiveHitPoints, look.HitPoints);
        ShowLookBar("EnduranceBar", look.ActiveEndurance, look.Endurance);
        ShowLookBar("ManaBar", look.ActiveMana, look.Mana);
        GetNode<CanvasItem>("%OwnName").Visible = false;
        ShowCharacter(look.Name, look.Points, look.Sprite, LookPanel.PortraitTint(look.Flags));
        GetNode<CanvasItem>("%SecondCharmFrame").Visible = look.Worn[(int)WornSlot.SecondCharm] != 0;
        for (var slot = WornSlot.Head; slot <= WornSlot.SecondCharm; slot++)
        {
            GetNode<ItemSlotView>("%" + slot).Show(new ItemSlot(look.Worn[(int)slot], 0, 0, 0), [], false);
        }
    }

    private void ShowCharacter(string name, int points, int sprite, SpriteTint tint)
    {
        var rank = Ranks.For(points);
        SetText("PortraitName", name);
        SetText("RankName", rank.Name);
        SetSprite("RankBadge", rank.BadgeSprite);
        SetSprite("Portrait", sprite);
        var material = (ShaderMaterial)GetNode<CanvasItem>("%Portrait").Material;
        material.SetShaderParameter("grey", tint.HasFlag(SpriteTint.Grey));
        material.SetShaderParameter("infrared", tint.HasFlag(SpriteTint.Infrared));
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
        GetNode<PortraitBar>("%" + name)
            .Show(PortraitBars.For(current, values[MaximumSlot], values[ReservedSlot]), false);

    private void ShowLookBar(string name, int current, int maximum) =>
        GetNode<PortraitBar>("%" + name).Show(PortraitBars.For(current, maximum, 0), true);

    private void ShowPoints(string name, string shownName, int points)
    {
        SetText(name + "Name", Currencies.RowName(shownName, points));
        SetText(name, Number(points));
    }

    private void SetText(string name, string text) => GetNode<Label>("%" + name).Text = text;

    private void SetSprite(string name, int spriteNumber) =>
        GetNode<ArchiveSprite>("%" + name).SpriteNumber = spriteNumber;
}
