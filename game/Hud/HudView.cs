using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Godot;
using TheLastGate.Assets;
using TheLastGate.Characters;
using TheLastGate.Chat;
using TheLastGate.Config;
using TheLastGate.Cursors;
using TheLastGate.Game;
using TheLastGate.Hotbar;
using TheLastGate.Hotkeys;
using TheLastGate.Items;
using TheLastGate.Net;
using TheLastGate.State;
using TheLastGate.Stats;
using TheLastGate.Windows;
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
    private const string ItemTooltipBinding = "item_tooltip_hotkey";
    private const int HintFont = 1;
    private const int WarningFont = 0;
    private const int HitPointsRow = 5;
    private const int ManaRow = 6;
    private const int HitScoreMeta = 5;
    private const int ParryScoreMeta = 6;

    private const int ExpandedQuickStatsPicture = 2;
    private const int CompactQuickStatsPicture = 3;
    private const int DarkPictureOffset = 10000;
    private const float ExperienceBarTop = 71;
    private const float HiddenQuickStatsRows = 28;
    private static readonly Vector2 HeldItemOffset = new(16, 16);

    private static readonly (string Button, Func<TextWindows, TextWindowAction> Click)[] TextWindowButtons =
    [
        ("CloseButton", windows => windows.Close()),
        ("OkButton", windows => windows.Ok()),
        ("LeftButton", windows => windows.Left()),
        ("RightButton", windows => windows.Right()),
    ];

    private static readonly Vector2 OffScreen = new(-1, -1);

    // Labels shown in the editor, where no settings are loaded.
    private static readonly KeyBindings DefaultBindings = new();

    private readonly DepotPage depotPage = new();
    private readonly ExitSequence exit = new();
    private readonly LookPanel lookPanel = new();
    private readonly SkillShortcut skillShortcut = new();
    private readonly StatPlan statPlan = new();

    private bool darkMode;
    private SpriteLoader? editorSprites;
    private IFontSet? fonts;
    private int? hoveredShopCell;
    private bool itemTooltipsShown = true;
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
    public KeyBindings KeyBindings { get; set; } = new();
    public ClientSettings? ClientSettings { get; set; }

    public IFontSet? Fonts
    {
        get => fonts;
        set
        {
            fonts = value;
            Reload(this);
        }
    }

    public Action PlayClick { get; set; } = () => { };

    public Func<DebugFacts>? ReadDebugFacts { get; set; }

    public event Action? SettingsChanged;

    public event Action? LeaveRequested;

    public event Action? ClientSettingsChanged;

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
            var wornView = GetNode<ItemSlotView>("%" + slot);
            wornView.Released += button => ClickWorn(clickedSlot, button);
            wornView.TooltipRequest = () => WornTooltip(clickedSlot);
            wornView.MouseEntered += () => state?.Tooltips.SetHovered(TooltipSlotKind.Worn, (int)clickedSlot);
            wornView.MouseExited += () => state?.Tooltips.ClearHovered();
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

        GetNode<HotbarMenu>("%HotbarMenu").SkillChosen += ChooseHotbarSkill;
        var inventory = GetNode<InventoryGrid>("%Inventory");
        for (var cell = 0; cell < InventoryScroll.VisibleSlots; cell++)
        {
            var clickedCell = cell;
            var cellView = inventory.GetChild<ItemSlotView>(cell);
            cellView.TooltipRequest = () => InventoryTooltip(inventory.SlotShownIn(clickedCell));
            cellView.MouseEntered += () =>
                state?.Tooltips.SetHovered(TooltipSlotKind.Inventory, inventory.SlotShownIn(clickedCell));
            cellView.MouseExited += () => state?.Tooltips.ClearHovered();
            cellView.Released +=
                button => ClickInventory(inventory.SlotShownIn(clickedCell), button);
        }

        state.LookShown += ShowLook;
        Fonts = fontSet;
        GetNode<ChatLog>("%ChatLog").Show(state.Chat, fontSet);
        var chatLine = GetNode<ChatLine>("%ChatLine");
        chatLine.LineSent += Say;
        chatLine.EscapePressed += PressEscape;
        var shopWindow = GetNode<ShopWindowView>("%ShopWindow");
        shopWindow.GetNode<ClickArea>("CloseButton").Released += ClickCloseShop;
        foreach (var node in shopWindow.GetNode("Grid").GetChildren())
        {
            var shopCell = (ItemSlotView)node;
            var cell = shopCell.GetIndex();
            shopCell.Released += button => ClickShop(cell, button);
            shopCell.MouseEntered += () => hoveredShopCell = cell;
            shopCell.MouseExited += () => hoveredShopCell = null;
        }

        var depotWindow = GetNode<DepotWindowView>("%DepotWindow");
        depotWindow.GetNode<ClickArea>("CloseButton").Released += ClickCloseShop;
        depotWindow.GetNode<Control>("Blocker").GuiInput += ScrollDepot;
        foreach (var node in depotWindow.GetNode("Blocker/Grid").GetChildren())
        {
            var depotCell = (ItemSlotView)node;
            var cell = depotCell.GetIndex();
            depotCell.Released += button => ClickDepot(cell, button);
        }

        for (var page = 0; page < DepotPage.Pages; page++)
        {
            var shownPage = page;
            var pageButton = depotWindow.GetNode<ClickArea>($"Blocker/PageButton{page}");
            pageButton.Released += button => ClickDepotPage(shownPage, button);
        }

        var smithWindow = GetNode<SmithWindowView>("%SmithWindow");
        smithWindow.GetNode<ClickArea>("CloseButton").Released += ClickCloseShop;
        foreach (var (node, slot) in SmithWindowView.ItemSlots)
        {
            smithWindow.GetNode<ItemSlotView>(node).Released += button => ClickSmithItem(slot, button);
        }

        for (var number = SmithClicks.FirstButton; number <= SmithClicks.LastButton; number++)
        {
            var buttonNumber = number;
            var smithButton = smithWindow.GetNode<ClickArea>($"Button{number}");
            smithButton.Released += button => ClickSmithButton(buttonNumber, button);
        }

        var waypointWindow = GetNode<WaypointWindowView>("%WaypointWindow");
        waypointWindow.GetNode<ClickArea>("CloseButton").Released += ClickCloseWaypoints;
        waypointWindow.GetNode<ClickArea>("ScrollUp").Released += button => ClickScrollWaypoints(-1, button);
        waypointWindow.GetNode<ClickArea>("ScrollDown").Released += button => ClickScrollWaypoints(1, button);
        waypointWindow.GetNode<Control>("Blocker").GuiInput += ScrollWaypoints;
        for (var row = 0; row < WaypointWindow.VisibleRows; row++)
        {
            var shownRow = row;
            ((ClickArea)waypointWindow.Row(row)).Released += button => ClickWaypoint(shownRow, button);
        }

        var skillTreeWindow = GetNode<SkillTreeWindowView>("%SkillTreeWindow");
        skillTreeWindow.GetNode<ClickArea>("CloseButton").Released += ClickCloseSkillTree;
        for (var node = 0; node < SkillTrees.Nodes; node++)
        {
            var shownNode = node;
            skillTreeWindow.Node(node).Released += button => ClickSkillTreeNode(shownNode, button);
        }

        GetNode<ClickArea>("%TreeButton").Released += ClickTreeButton;
        var optionsWindow = GetNode<OptionsWindowView>("%OptionsWindow");
        optionsWindow.TabChosen += ChooseOptionsTab;
        optionsWindow.CloseChosen += CloseOptions;
        optionsWindow.SoundEnabledChanged +=
            enabled => ChangeClientSettings(settings => settings.SoundEnabled = enabled);
        optionsWindow.SoundVolumeChanged += volume => ChangeClientSettings(settings => settings.SoundVolume = volume);
        optionsWindow.OptionChanged += (option, isOn) => ChangeClientSettings(settings => settings[option] = isOn);
        optionsWindow.CursorScaleChanged += scale => ChangeClientSettings(settings => settings.CursorScale = scale);
        optionsWindow.CaptureChosen += id => ChangeOptionsWindow(window => window.StartCapture(id));
        optionsWindow.CaptureCancelled += () => ChangeOptionsWindow(window => window.CancelCapture());
        optionsWindow.CaptureUnbound += () => ChangeKeyBindings((window, bindings) =>
        {
            window.UnbindCaptured(bindings);
            return null;
        });
        optionsWindow.KeyCaptured += chord => ChangeKeyBindings((window, bindings) => window.Capture(chord, bindings));
        optionsWindow.BindingResetChosen += id => ChangeKeyBindings((_, bindings) =>
        {
            bindings.Reset(id);
            return null;
        });
        optionsWindow.ResetAllChosen += () => ChangeKeyBindings((window, bindings) => window.ChooseResetAll(bindings));

        var textWindows = GetNode<TextWindowView>("%TextWindows");
        foreach (var (buttonName, click) in TextWindowButtons)
        {
            textWindows.GetNode<ClickArea>(buttonName).Released += button => ClickTextWindow(button, click);
        }

        chatLine.GrabFocus();
        chatLine.EnterToTalk = ClientSettings?.EnterToTalk ?? false;
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
        if (state.TextWindows.AnyOpen || state.WaypointWindow.IsOpen || state.SkillTreeWindow.Open != null
            || state.OpenShop != null)
        {
            lookPanel.Hide();
        }

        ShowTextWindows();
        GetNode<ShopWindowView>("%ShopWindow").ShowItems(state);
        GetNode<DepotWindowView>("%DepotWindow").ShowItems(state, depotPage);
        GetNode<SmithWindowView>("%SmithWindow").ShowItems(state);
        ShowWaypoints();
        GetNode<SkillTreeWindowView>("%SkillTreeWindow").ShowTree(state);
        ShowOptions();
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
            hotbarButton.ShowKey();
        }

        GetNode<InventoryGrid>("%Inventory").Show(player, Settings.HotbarActions);
        ShowStats(player);
        ShowQuickStats(player);
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

    // Held keys repeat their hotkey, as in the current client.
    // A closed enter-to-talk chat line holds no focus, so the keys that open it, and Escape, arrive here; a key can
    // run its hotkey and open the line too, as in the current client.
    public override void _UnhandledKeyInput(InputEvent inputEvent)
    {
        if (inputEvent is not InputEventKey { Pressed: true } key)
        {
            return;
        }

        var chatLine = GetNode<ChatLine>("%ChatLine");
        var handled = chatLine.RunsHotkeys
            && HotkeyInput.Chord(key) is { } chord
            && KeyBindings.Find(chord) is { } binding
            && RunHotkey(binding.Id);
        if (!chatLine.TakesTyping && ChatKeyFor(key.Keycode) is { } chatKey)
        {
            if (chatKey == ChatKey.Escape)
            {
                PressEscape();
                handled = true;
            }
            else
            {
                handled |= chatLine.Open(chatKey);
            }
        }

        if (handled)
        {
            GetViewport().SetInputAsHandled();
        }
    }

    private static ChatKey? ChatKeyFor(Key key) => key switch
    {
        Key.Enter or Key.KpEnter => ChatKey.Enter,
        Key.Escape => ChatKey.Escape,
        Key.Slash => ChatKey.Slash,
        Key.Up => ChatKey.Older,
        Key.Down => ChatKey.Newer,
        _ => null,
    };

    public static string HotbarKeyName(Node node, int slot) =>
        (Containing(node)?.KeyBindings ?? DefaultBindings).ShortName(Bindings.SpellId(slot));

    public override void _Process(double delta)
    {
        if (state == null)
        {
            return;
        }

        RecheckTooltipWhenShowingChanges();
        exit.Advance(TimeSpan.FromSeconds(delta));
        var lookWasShown = lookPanel.Shown != null;
        lookPanel.Advance(TimeSpan.FromSeconds(delta));
        if (lookWasShown && lookPanel.Shown == null)
        {
            Refresh(selectedCharacter);
        }

        ShowHeldItem(state.Player.HeldItem);
        ShowTreeButton();
        var buyingTen = Input.IsKeyPressed(Key.Ctrl) || Input.IsKeyPressed(Key.Alt);
        GetNode<ShopWindowView>("%ShopWindow").ShowText(state, hoveredShopCell, buyingTen);
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
        else if (hovered.Owner is DepotWindowView)
        {
            var item = state.Shop.Depot[depotPage.Current][hovered.GetIndex()];
            cursor = ShopClicks.Cursor(item.Sprite != 0, holdingItem);
        }
        else if (hovered.Owner is ShopWindowView)
        {
            var slot = ShopClicks.SlotFor(hovered.GetIndex());
            var items = state.Shop.ShopItems;
            cursor = ShopClicks.Cursor(slot < items.Length && items[slot].Sprite != 0, holdingItem);
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
                case ChatLine chatLine:
                    chatLine.ApplyFont();
                    break;
                case HudLabel label:
                    label.ApplyFont();
                    break;
                case UiPicture picture:
                    picture.LoadPicture();
                    break;
                case DarkModeShift shift:
                    shift.ApplyDarkMode();
                    break;
                case HotbarMenu menu:
                    menu.ApplyFont();
                    break;
                case OptionsWindowView options:
                    options.ApplyButtonSprite();
                    break;
            }

            Reload(child);
        }
    }

    private void ClickWorn(WornSlot slot, MapButton button)
    {
        if (SkillShortcut.SelectsItem(button, HeldKeys.Current()))
        {
            state?.Chat.Add(skillShortcut.ToggleEquipment(slot));
            return;
        }

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

        var keys = HeldKeys.Current();
        if (SkillShortcut.SelectsItem(button, keys))
        {
            state.Chat.Add(skillShortcut.ToggleInventory(slot));
            return;
        }

        var player = state.Player;
        var packets = InventoryClicks.Packets(
            slot,
            player.Inventory[slot],
            player.HeldItem.Sprite != 0,
            button,
            keys,
            SaleToOpenShop(),
            selectedCharacter);
        foreach (var packet in packets)
        {
            sendCommand(packet);
        }
    }

    private ShopSale? SaleToOpenShop() =>
        state?.OpenShop is { } shop and not ShopKind.WeaponSmith and not ShopKind.ArmourSmith
            ? new ShopSale(state.Shop.Number, depotPage.Current)
            : null;

    private bool RunHotkey(string id)
    {
        if (Bindings.SpellSlot(id) is { } slot)
        {
            UseHotbar(slot);
            return true;
        }

        if (id == "toggle_options_hotkey")
        {
            ToggleOptions();
            return true;
        }

        if (id == "swap_gear_hotkey")
        {
            ClickSwapSet(MapButton.Left);
            return true;
        }

        if (id == "reset_window_size_hotkey")
        {
            ResetWindowSize();
            return true;
        }

        if (id == "display_debug_information_hotkey")
        {
            ShowDebugInformation();
            return true;
        }

        if (HotkeyCommands.Says(id) is { } line)
        {
            Say(line);
            return true;
        }

        if (OptionForHotkey(id) is not { } option)
        {
            return false;
        }

        ClickOption(option, MapButton.Left);
        return true;
    }

    // A maximised window is brought back to a plain window first, or the new size would not show.
    private void ResetWindowSize()
    {
        if (WindowReset.Line(ClientSettings?.Windowed ?? true) is not { } line)
        {
            return;
        }

        DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
        DisplayServer.WindowSetSize(new Vector2I(WindowReset.Size.Width, WindowReset.Size.Height));
        state?.Chat.Add(line);
    }

    private void ShowDebugInformation()
    {
        if (ReadDebugFacts?.Invoke() is not { } facts)
        {
            return;
        }

        foreach (var line in DebugInformation.Lines(facts))
        {
            state?.Chat.Add(line);
        }
    }

    private static OptionButton? OptionForHotkey(string id) => id switch
    {
        "speed_fast_hotkey" => OptionButton.Fast,
        "speed_normal_hotkey" => OptionButton.Normal,
        "speed_slow_hotkey" => OptionButton.Slow,
        "toggle_percent_hotkey" => OptionButton.HealthPercent,
        "toggle_stat_base_hotkey" => OptionButton.StatBase,
        "toggle_hide_sprite_hotkey" => OptionButton.HideWalls,
        "toggle_names_hotkey" => OptionButton.Names,
        "toggle_health_bars_hotkey" => OptionButton.HealthBars,
        "exit_hotkey" => OptionButton.Exit,
        _ => null,
    };

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

        if (option == OptionButton.Options)
        {
            ToggleOptions();
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

    private void ClickShop(int cell, MapButton button)
    {
        if (state == null)
        {
            return;
        }

        var slot = ShopClicks.SlotFor(cell);
        if (ShopClicks.Packet(button, HeldKeys.Current(), state.Shop.Number, slot) is { } packet)
        {
            sendCommand(packet);
        }
    }

    private void ClickDepot(int cell, MapButton button)
    {
        if (state != null && DepotClicks.Packet(button, state.Shop.Number, depotPage.Current, cell) is { } packet)
        {
            sendCommand(packet);
        }
    }

    private void ClickDepotPage(int page, MapButton button)
    {
        if (button != MapButton.Left || state == null)
        {
            return;
        }

        depotPage.Show(page);
        PlayClick();
        GetNode<DepotWindowView>("%DepotWindow").ShowItems(state, depotPage);
    }

    private void ScrollDepot(InputEvent inputEvent)
    {
        if (inputEvent is not InputEventMouseButton { Pressed: true } wheel || state == null
            || wheel.ButtonIndex is not (MouseButton.WheelUp or MouseButton.WheelDown)
            || !DepotWindowView.InWheelArea(wheel.Position))
        {
            return;
        }

        depotPage.Turn(wheel.ButtonIndex == MouseButton.WheelDown ? 1 : -1);
        GetNode<DepotWindowView>("%DepotWindow").ShowItems(state, depotPage);
    }

    private void ClickSmithItem(int slot, MapButton button)
    {
        if (state != null && SmithClicks.ItemPacket(button, HeldKeys.Current(), state.Shop.Number, slot) is { } packet)
        {
            sendCommand(packet);
        }
    }

    private void ClickSmithButton(int buttonNumber, MapButton button)
    {
        if (state != null && SmithClicks.ButtonPacket(button, state.Shop.Number, buttonNumber) is { } packet)
        {
            sendCommand(packet);
        }
    }

    private void ShowWaypoints()
    {
        if (state != null)
        {
            GetNode<WaypointWindowView>("%WaypointWindow").ShowWaypoints(state);
        }
    }

    private void ClickWaypoint(int row, MapButton button)
    {
        if (state?.WaypointWindow.Choose(row, button, state.Player.Waypoints) is { } packet)
        {
            sendCommand(packet);
            ShowWaypoints();
        }
    }

    private void ClickScrollWaypoints(int direction, MapButton button)
    {
        if (button == MapButton.Left && state != null)
        {
            state.WaypointWindow.ScrollBy(direction * WaypointWindow.ArrowStep(HeldKeys.Current()));
            ShowWaypoints();
        }
    }

    private void ScrollWaypoints(InputEvent inputEvent)
    {
        if (inputEvent is not InputEventMouseButton { Pressed: true } wheel || state == null
            || wheel.ButtonIndex is not (MouseButton.WheelUp or MouseButton.WheelDown)
            || !WaypointWindowView.InWheelArea(wheel.Position))
        {
            return;
        }

        state.WaypointWindow.ScrollBy(wheel.ButtonIndex == MouseButton.WheelDown ? 1 : -1);
        ShowWaypoints();
    }

    private void ClickCloseWaypoints(MapButton button)
    {
        if (button == MapButton.Left && state?.WaypointWindow.Close() == true)
        {
            Refresh(selectedCharacter);
        }
    }

    // The button answers clicks even while it isn't drawn, so a character with only contract points can open that tree.
    private void ShowTreeButton()
    {
        if (state == null)
        {
            return;
        }

        var button = GetNode<Control>("%TreeButton");
        var player = state.Player;
        button.GetNode<CanvasItem>("Picture").Visible = SkillTreeWindow.ButtonShows(player.TreePoints);
        var glow = button.GetNode<CanvasItem>("Glow");
        glow.Visible = SkillTreeWindow.ButtonShows(player.TreePoints)
            && state.SkillTreeWindow.ButtonGlows(player.TreePoints, player.OsirisTree);
        var brightness = SpriteLighting.Brightness(SkillTreeWindow.GlowLight((long)Time.GetTicksMsec()));
        glow.Modulate = new Color(brightness, brightness, brightness);
    }

    private void ClickTreeButton(MapButton button)
    {
        if (button == MapButton.Left && state != null)
        {
            state.ToggleSkillTree(HeldKeys.Current());
            Refresh(selectedCharacter);
        }
    }

    private void ClickSkillTreeNode(int node, MapButton button)
    {
        if (state?.SkillTreeWindow.Click(node, button, HeldKeys.Current()) is { } packet)
        {
            sendCommand(packet);
        }
    }

    private void ClickCloseSkillTree(MapButton button)
    {
        if (button == MapButton.Left && state?.SkillTreeWindow.Close() == true)
        {
            Refresh(selectedCharacter);
        }
    }

    private void ShowOptions()
    {
        if (state != null)
        {
            GetNode<OptionsWindowView>("%OptionsWindow").ShowOptions(state.OptionsWindow, ClientSettings, Settings);
        }
    }

    private void ChangeOptionsWindow(Action<OptionsWindow> change)
    {
        if (state != null)
        {
            change(state.OptionsWindow);
            ShowOptions();
        }
    }

    // The hotbar's key labels are refreshed with the window, since they show the bindings too.
    private void ChangeKeyBindings(Func<OptionsWindow, KeyBindings, LogLine?> change)
    {
        if (state == null)
        {
            return;
        }

        LogLine? line = null;
        ChangeClientSettings(settings => line = change(state.OptionsWindow, settings.KeyBindings));
        if (line is { } shown)
        {
            state.Chat.Add(shown);
        }

        Refresh(selectedCharacter);
    }

    private void ToggleOptions()
    {
        state?.OptionsWindow.Toggle();
        ShowOptions();
    }

    private void ChooseOptionsTab(OptionsTab tab)
    {
        state?.OptionsWindow.Show(tab);
        ShowOptions();
    }

    private void CloseOptions()
    {
        state?.OptionsWindow.Close();
        ShowOptions();
    }

    // Options are saved the moment they change, so nothing is lost if the game closes unexpectedly.
    private void ChangeClientSettings(Action<ClientSettings> change)
    {
        if (ClientSettings == null)
        {
            return;
        }

        change(ClientSettings);
        ClientSettings.Save();
        ShowQuickStatsLayout();
        GetNode<ChatLine>("%ChatLine").EnterToTalk = ClientSettings.EnterToTalk;
        ClientSettingsChanged?.Invoke();
    }

    // Without Hit and Parry the current client draws a shorter box, and the experience bar moves up with the rows.
    private void ShowQuickStatsLayout()
    {
        var expanded = ClientSettings?.ExpandedQuickStats ?? true;
        var quickStats = GetNode<Control>("QuickStats");
        quickStats.GetNode<CanvasItem>("RowsShift/Rows/HitRow").Visible = expanded;
        quickStats.GetNode<CanvasItem>("RowsShift/Rows/ParryRow").Visible = expanded;
        var background = quickStats.GetNode<UiPicture>("Background");
        background.PictureNumber = expanded ? ExpandedQuickStatsPicture : CompactQuickStatsPicture;
        background.DarkPictureNumber = background.PictureNumber + DarkPictureOffset;
        var bar = GetNode<Control>("%ExperienceBar");
        bar.Position = bar.Position with { Y = expanded ? ExperienceBarTop : ExperienceBarTop - HiddenQuickStatsRows };
    }

    private void ClickCloseShop(MapButton button)
    {
        if (button == MapButton.Left && state?.CloseShop() == true)
        {
            Refresh(selectedCharacter);
        }
    }

    // With a window open, Escape only closes it; otherwise it stops what the character is doing.
    private void PressEscape()
    {
        var closedAWindow = state?.CloseWindowsByEscape() == true;
        if (closedAWindow)
        {
            Refresh(selectedCharacter);
        }

        if (GeneralOptions.EscapeSendsReset(ClientSettings?.EscapeClosesWindowsFirst ?? true, closedAWindow))
        {
            sendCommand(ClientPackets.ShortAndInt(ClientCommand.Reset, 0, 0));
        }
    }

    private void ClickTextWindow(MapButton button, Func<TextWindows, TextWindowAction> click)
    {
        if (button != MapButton.Left || state == null)
        {
            return;
        }

        var action = click(state.TextWindows);
        foreach (var packet in action.Packets)
        {
            sendCommand(packet);
        }

        if (action.PlaysClick)
        {
            PlayClick();
        }

        ShowTextWindows();
    }

    private void ShowTextWindows()
    {
        if (state != null)
        {
            GetNode<TextWindowView>("%TextWindows").Show(state.TextWindows, state.MessageOfTheDayLines);
        }
    }

    private void Say(string line)
    {
        foreach (var packet in ChatPackets.Say(line))
        {
            sendCommand(packet);
        }
    }

    private bool ItemTooltipsShown() =>
        ClientSettings is not { } settings
        || GeneralOptions.ShowsItemTooltips(
            settings.ItemTooltips,
            settings.ItemTooltipsAlways,
            KeyBindings.For(ItemTooltipBinding) is { } chord && HotkeyInput.IsHeld(chord));

    // Godot looks for a tooltip again only when the mouse enters a control or moves 5 pixels, so when the tooltip
    // key goes down or up the mouse is moved off everything and back, entering the slot afresh.
    private void RecheckTooltipWhenShowingChanges()
    {
        var shown = ItemTooltipsShown();
        if (shown == itemTooltipsShown)
        {
            return;
        }

        itemTooltipsShown = shown;
        var away = new InputEventMouseMotion { Position = OffScreen, GlobalPosition = OffScreen };
        var back = new InputEventMouseMotion { Position = mousePosition, GlobalPosition = mousePosition };
        GetViewport().PushInput(away, true);
        GetViewport().PushInput(back, true);
    }

    private ItemTooltipRequest? InventoryTooltip(int slot)
    {
        if (!itemTooltipsShown || state is not { Tooltips.Enabled: true } shown || shown.Player.HeldItem.Sprite != 0
            || shown.Player.Inventory[slot].Sprite == 0)
        {
            return null;
        }

        return new ItemTooltipRequest(ItemTooltipMode.Plain, shown.Tooltips.For(TooltipSlotKind.Inventory, slot), null);
    }

    // Holding an item over gear it fits compares the two; gear it doesn't fit shows its usual tooltip.
    private ItemTooltipRequest? WornTooltip(WornSlot slot)
    {
        if (!itemTooltipsShown || state is not { Tooltips.Enabled: true } shown)
        {
            return null;
        }

        var player = shown.Player;
        var filled = player.Worn[(int)slot].Sprite != 0;
        var worn = shown.Tooltips.For(TooltipSlotKind.Worn, (int)slot);
        if (player.HeldItem.Sprite != 0 && !Equipment.BlockedSlots(player.HeldItem, player.Worn).Contains(slot))
        {
            var held = shown.Tooltips.Held;
            if (filled)
            {
                return new ItemTooltipRequest(ItemTooltipMode.Comparing, worn, held);
            }

            return held != null ? new ItemTooltipRequest(ItemTooltipMode.EmptySlot, null, held) : null;
        }

        return filled ? new ItemTooltipRequest(ItemTooltipMode.Plain, worn, null) : null;
    }

    private void ShowQuickStats(PlayerState player)
    {
        ShowQuickStatsLayout();
        var metaStats = state!.Terminology.MetaStats;
        SetText("WeaponValue", Number(player.Weapon));
        SetText("ArmorValue", Number(player.Armor));
        SetText("HitScore", Number(metaStats[HitScoreMeta].Value));
        SetText("ParryScore", Number(metaStats[ParryScoreMeta].Value));
        SetText("Experience", Number(player.PointsTotal));
        GetNode<ProgressBar>("%ExperienceBar").Value = Ranks.Progress(player.PointsTotal);
    }

    private void ClickHotbar(int slot, MapButton button)
    {
        if (button == MapButton.Left)
        {
            UseHotbar(slot);
            return;
        }

        if (button != MapButton.Right || state == null)
        {
            return;
        }

        if (skillShortcut.Selected is { } selectedAction)
        {
            AssignHotbar(slot, selectedAction);
            return;
        }

        var skillNames = state.Terminology.Skills;
        var skills = HotbarAssignments.MenuSkills(state.Player.Skills, state.Player.Kindred)
            .Select(number => (number, skillNames[number].Name));
        GetNode<HotbarMenu>("%HotbarMenu").Open(slot, skills, mousePosition);
    }

    private void ChooseHotbarSkill(int slot, int? skill)
    {
        if (skill is { } number)
        {
            AssignHotbar(slot, number);
            return;
        }

        HotbarAssignments.Clear(Settings, slot);
        HotbarChanged();
    }

    private void AssignHotbar(int slot, int action)
    {
        var name = action < HotbarButtons.FirstInventoryAction
            ? state!.Terminology.Skills[action].Name
            : HotbarButtons.ItemName(action);
        var keyName = KeyBindings.ShortName(Bindings.SpellId(slot));
        state!.Chat.Add(HotbarAssignments.Assign(Settings, slot, action, name, keyName));
        HotbarChanged();
    }

    private void HotbarChanged()
    {
        SettingsChanged?.Invoke();
        Refresh(selectedCharacter);
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
        if (state is { OpenShop: not (null or ShopKind.WeaponSmith or ShopKind.ArmourSmith) })
        {
            var shop = state.Shop;
            ShowCharacter(shop.Name, shop.Points, shop.Sprite, SpriteTint.None);
        }

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
            var slotView = GetNode<ItemSlotView>("%" + slot);
            slotView.Show(new ItemSlot(look.Worn[(int)slot], 0, 0, 0), [], false);
            slotView.ShowHotbarKey(null);
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
            var slotView = GetNode<ItemSlotView>("%" + slot);
            slotView.Show(item, ItemOverlays.Worn(item), showBlocked);
            var hotbarKey = item.Sprite != 0 ? HotbarButtons.GearKeyButton(Settings.HotbarActions, slot) : null;
            slotView.ShowHotbarKey(hotbarKey);
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
