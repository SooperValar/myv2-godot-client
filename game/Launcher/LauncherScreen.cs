using System;
using System.Collections.Generic;
using System.IO;
using Godot;
using TheLastGate.Characters;
using TheLastGate.Config;

namespace TheLastGate.Launcher;

public partial class LauncherScreen : Control
{
    private const string DiscordInvite = "https://discord.gg/jJbPv2R";
    private const int MaxDescriptionLength = 159;
    private const int WindowedOptionId = 1;
    private const int DialogButtonGap = 10;

    private static readonly Color LockedRowColour = new(1, 1, 1, 0.6f);
    private static readonly Vector2 PreviousCharacterButtonSize = new(240, 24);

    private Label accountName = null!;
    private OptionButton bodyOption = null!;
    private OptionButton classOption = null!;
    private ConfirmationDialog confirmDialog = null!;
    private Label confirmExplanation = null!;
    private Label confirmQuestion = null!;
    private Label confirmTitle = null!;
    private Label confirmWarning = null!;
    private CheckBox darkGuiCheck = null!;
    private TextEdit descriptionField = null!;
    private OptionButton gameModeOption = null!;
    private FileDialog loadDialog = null!;
    private AcceptDialog messageDialog = null!;
    private Label messageText = null!;
    private Label messageTitle = null!;
    private ColorRect modalDim = null!;
    private LineEdit nameField = null!;
    private LineEdit passwordField = null!;
    private Action? pendingConfirmation;
    private VBoxContainer previousCharacterList = null!;
    private (Label Label, OptionButton Option)[] raceRows = [];
    private CharacterRecord record = null!;
    private FileDialog saveDialog = null!;
    private OptionButton seasonOption = null!;
    private int selectableClassCount;
    private int selectableGameModeCount;
    private ClientSettings settings = null!;
    private CheckBox shadowsCheck = null!;
    private bool showingCharacter;
    private CheckBox soundCheck = null!;
    private HSlider volumeSlider = null!;
    private Label volumeValue = null!;
    private OptionButton windowTypeOption = null!;

    public override void _Ready()
    {
        FindNodes();

        settings = ClientSettings.Load();
        ApplyWindowMode();
        ShowOptions();

        record = CharacterStore.Load() ?? CharacterRecord.CreateNew();
        CharacterStore.Save(record);
        selectableClassCount = classOption.ItemCount;
        selectableGameModeCount = gameModeOption.ItemCount;
        AddDescriptions();
        ShowCharacter();
        ShowPreviousCharacters();

        ConnectSignals();
    }

    private void FindNodes()
    {
        accountName = GetNode<Label>("%AccountName");
        nameField = GetNode<LineEdit>("%NameField");
        passwordField = GetNode<LineEdit>("%PasswordField");
        descriptionField = GetNode<TextEdit>("%DescriptionField");
        classOption = GetNode<OptionButton>("%ClassOption");
        bodyOption = GetNode<OptionButton>("%BodyOption");
        gameModeOption = GetNode<OptionButton>("%GameModeOption");
        seasonOption = GetNode<OptionButton>("%SeasonOption");
        raceRows =
        [
            (GetNode<Label>("%ClassLabel"), classOption),
            (GetNode<Label>("%BodyLabel"), bodyOption),
            (GetNode<Label>("%GameModeLabel"), gameModeOption),
            (GetNode<Label>("%SeasonLabel"), seasonOption),
        ];
        previousCharacterList = GetNode<VBoxContainer>("%PreviousCharacterList");
        windowTypeOption = GetNode<OptionButton>("%WindowTypeOption");
        shadowsCheck = GetNode<CheckBox>("%ShadowsCheck");
        soundCheck = GetNode<CheckBox>("%SoundCheck");
        volumeSlider = GetNode<HSlider>("%VolumeSlider");
        volumeValue = GetNode<Label>("%VolumeValue");
        darkGuiCheck = GetNode<CheckBox>("%DarkGuiCheck");
        modalDim = GetNode<ColorRect>("%ModalDim");
        messageDialog = GetNode<AcceptDialog>("%MessageDialog");
        messageTitle = GetNode<Label>("%MessageTitle");
        messageText = GetNode<Label>("%MessageText");
        confirmDialog = GetNode<ConfirmationDialog>("%ConfirmDialog");
        confirmTitle = GetNode<Label>("%ConfirmTitle");
        confirmQuestion = GetNode<Label>("%ConfirmQuestion");
        confirmExplanation = GetNode<Label>("%ConfirmExplanation");
        confirmWarning = GetNode<Label>("%ConfirmWarning");
        loadDialog = GetNode<FileDialog>("%LoadDialog");
        saveDialog = GetNode<FileDialog>("%SaveDialog");
    }

    private void ConnectSignals()
    {
        GetNode<Button>("%CreateAccountButton").Pressed += OnCreateAccountPressed;
        GetNode<Button>("%LoadButton").Pressed += () => loadDialog.PopupCentered();
        GetNode<Button>("%SaveButton").Pressed += () => saveDialog.PopupCentered();
        GetNode<Button>("%DiscordButton").Pressed += () => OS.ShellOpen(DiscordInvite);
        GetNode<Button>("%ExitButton").Pressed += () => GetTree().Quit();

        nameField.TextChanged += OnNameChanged;
        descriptionField.TextChanged += OnDescriptionChanged;
        classOption.ItemSelected += _ => ShowSelectedDescription(classOption);
        gameModeOption.ItemSelected += _ => ShowSelectedDescription(gameModeOption);
        seasonOption.ItemSelected += _ => ShowSelectedDescription(seasonOption);

        windowTypeOption.ItemSelected += _ => ChangeSetting(
            () => settings.Windowed = windowTypeOption.GetSelectedId() == WindowedOptionId,
            ApplyWindowMode);
        shadowsCheck.Toggled += pressed => ChangeSetting(() => settings.Shadows = pressed);
        soundCheck.Toggled += pressed => ChangeSetting(() => settings.SoundEnabled = pressed);
        darkGuiCheck.Toggled += pressed => ChangeSetting(() => settings.DarkMode = pressed);
        volumeSlider.ValueChanged += value => ChangeSetting(
            () => settings.SoundVolume = (int)value,
            () => volumeValue.Text = settings.SoundVolume.ToString());

        loadDialog.FileSelected += OnLoadFileSelected;
        saveDialog.FileSelected += OnSaveFileSelected;
        confirmDialog.Confirmed += OnConfirmed;
        confirmDialog.Canceled += () => pendingConfirmation = null;
        confirmDialog.VisibilityChanged += OnConfirmDialogVisibilityChanged;
        messageDialog.VisibilityChanged += ShowModalDim;
        confirmDialog.VisibilityChanged += ShowModalDim;
    }

    private void ShowOptions()
    {
        windowTypeOption.Select(windowTypeOption.GetItemIndex(settings.Windowed ? WindowedOptionId : 0));
        shadowsCheck.ButtonPressed = settings.Shadows;
        soundCheck.ButtonPressed = settings.SoundEnabled;
        darkGuiCheck.ButtonPressed = settings.DarkMode;
        volumeSlider.Value = settings.SoundVolume;
        volumeValue.Text = settings.SoundVolume.ToString();
    }

    private void ChangeSetting(Action change, Action? afterChange = null)
    {
        change();
        afterChange?.Invoke();
        settings.Save();
    }

    private void ApplyWindowMode() =>
        DisplayServer.WindowSetMode(
            settings.Windowed ? DisplayServer.WindowMode.Windowed : DisplayServer.WindowMode.Fullscreen);

    private void AddDescriptions()
    {
        AddDescriptions(classOption, CharacterChoiceText.ClassDescriptions);
        AddDescriptions(gameModeOption, CharacterChoiceText.GameModeDescriptions);
        AddDescriptions(seasonOption, CharacterChoiceText.SeasonDescriptions);
    }

    private static void AddDescriptions<TChoice>(OptionButton option, IReadOnlyDictionary<TChoice, string> descriptions)
        where TChoice : struct, Enum
    {
        foreach (var (choice, description) in descriptions)
        {
            var index = option.GetItemIndex(Convert.ToInt32(choice));
            if (index >= 0)
            {
                option.SetItemTooltip(index, description);
            }
        }
    }

    private static void ShowSelectedDescription(OptionButton option) =>
        option.TooltipText = option.Selected >= 0 ? option.GetItemTooltip(option.Selected) : "";

    private void ShowCharacter()
    {
        showingCharacter = true;
        accountName.Text = record.Key.Name;
        nameField.Text = record.CharacterName;
        descriptionField.Text = record.Description;

        var existingCharacter = record.Key.UserNumber != 0;
        if (existingCharacter)
        {
            var race = CharacterRace.FromNumber(record.Key.Race);
            SelectOrAdd(classOption, (int)race.Class, CharacterChoiceText.ClassNames[race.Class]);
            SelectOrAdd(bodyOption, (int)race.Body, race.Body.ToString());
            SelectOrAdd(
                gameModeOption,
                (int)race.GameMode,
                CharacterChoiceText.GameModeNames.GetValueOrDefault(race.GameMode, race.GameMode.ToString()));
            SelectOrAdd(seasonOption, (int)race.Season, race.Season.ToString());
        }
        else
        {
            RemoveLockedItems(classOption, selectableClassCount);
            RemoveLockedItems(gameModeOption, selectableGameModeCount);
        }

        foreach (var (label, option) in raceRows)
        {
            option.Disabled = existingCharacter;
            label.Modulate = existingCharacter ? LockedRowColour : Colors.White;
            ShowSelectedDescription(option);
        }

        showingCharacter = false;
    }

    private static void SelectOrAdd(OptionButton option, int id, string name)
    {
        if (option.GetItemIndex(id) < 0)
        {
            option.AddItem(name, id);
        }

        option.Select(option.GetItemIndex(id));
    }

    private static void RemoveLockedItems(OptionButton option, int selectableCount)
    {
        while (option.ItemCount > selectableCount)
        {
            option.RemoveItem(option.ItemCount - 1);
        }

        if (option.Selected < 0)
        {
            option.Select(0);
        }
    }

    private void ResetNewCharacterChoices()
    {
        classOption.Select(classOption.GetItemIndex((int)CharacterClass.Templar));
        bodyOption.Select(bodyOption.GetItemIndex((int)Body.Masculine));
        gameModeOption.Select(gameModeOption.GetItemIndex((int)GameMode.Normal));
        seasonOption.Select(seasonOption.GetItemIndex((int)Season.Current));
    }

    private void OnNameChanged(string name)
    {
        record.CharacterName = name;
        SaveEditedCharacter();
    }

    private void OnDescriptionChanged()
    {
        if (descriptionField.Text.Length > MaxDescriptionLength)
        {
            descriptionField.Text = descriptionField.Text[..MaxDescriptionLength];
            var lastLine = descriptionField.GetLineCount() - 1;
            descriptionField.SetCaretLine(lastLine);
            descriptionField.SetCaretColumn(descriptionField.GetLine(lastLine).Length);
        }

        record.Description = descriptionField.Text;
        SaveEditedCharacter();
    }

    private void SaveEditedCharacter()
    {
        if (showingCharacter)
        {
            return;
        }

        record.Changed = true;
        CharacterStore.Save(record);
    }

    private void OnCreateAccountPressed() =>
        ConfirmTwice(
            "Do you really want to create a new account?",
            () =>
            {
                record.StartNewAccount();
                CharacterStore.Save(record);
                passwordField.Text = "";
                ResetNewCharacterChoices();
                ShowCharacter();
            });

    private void OnLoadFileSelected(string filePath) =>
        ConfirmTwice("Do you really want to switch accounts?", () => Import(filePath));

    private void Import(string filePath)
    {
        try
        {
            record = CharacterRecord.Read(filePath);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException
            or InvalidDataException)
        {
            GD.PushError($"Couldn't open the character file {filePath}: {exception.Message}");
            ShowMessage("FYI", $"Could not open file {filePath}");
            return;
        }

        record.Changed = true;
        CharacterStore.Save(record);
        RememberCharacterFile(filePath);
        passwordField.Text = "";
        ShowCharacter();
    }

    private void OnSaveFileSelected(string filePath)
    {
        try
        {
            record.Write(filePath);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            GD.PushError($"Couldn't save the character file {filePath}: {exception.Message}");
            ShowMessage("FYI", $"Could not save file {filePath}");
            return;
        }

        RememberCharacterFile(filePath);
        ShowMessage("FYI", $"Saved file as {filePath}");
    }

    private void RememberCharacterFile(string filePath)
    {
        settings.CharacterHistory.Use(record.Key.Name, filePath);
        settings.Save();
        ShowPreviousCharacters();
    }

    private void ShowPreviousCharacters()
    {
        foreach (var child in previousCharacterList.GetChildren())
        {
            child.QueueFree();
        }

        foreach (var entry in settings.CharacterHistory.Entries)
        {
            var button = new Button
            {
                Text = entry.Name.Length > 0 ? entry.Name : Path.GetFileNameWithoutExtension(entry.FilePath),
                TooltipText = entry.FilePath,
                CustomMinimumSize = PreviousCharacterButtonSize,
                SizeFlagsHorizontal = SizeFlags.ShrinkCenter,
            };
            var filePath = entry.FilePath;
            button.Pressed += () => Import(filePath);
            previousCharacterList.AddChild(button);
        }
    }

    private void ShowMessage(string title, string text)
    {
        messageTitle.Text = title;
        messageText.Text = text;
        messageDialog.PopupCentered();
    }

    private void ConfirmTwice(string question, Action onYes) =>
        Confirm(
            "Are you sure?",
            question,
            "Your old account will no longer be accessible, unless you remembered to save it.",
            "You did save it, didn't you?",
            () => Confirm(
                "Are you really sure?",
                "No Really!",
                "This will replace your current character.",
                "This action cannot be undone.",
                onYes));

    private void Confirm(string title, string question, string explanation, string warning, Action onYes)
    {
        pendingConfirmation = onYes;
        confirmTitle.Text = title;
        confirmQuestion.Text = question;
        confirmExplanation.Text = explanation;
        confirmWarning.Text = warning;
        confirmDialog.CallDeferred(Window.MethodName.PopupCentered);
    }

    private void ShowModalDim() => modalDim.Visible = messageDialog.Visible || confirmDialog.Visible;

    private void OnConfirmDialogVisibilityChanged()
    {
        if (confirmDialog.Visible)
        {
            Callable.From(() => KeepButtonsTogether(confirmDialog)).CallDeferred();
        }
    }

    private static void KeepButtonsTogether(ConfirmationDialog dialog)
    {
        var buttonRow = (HBoxContainer)dialog.GetOkButton().GetParent();
        buttonRow.AddThemeConstantOverride("separation", DialogButtonGap);
        var okIndex = dialog.GetOkButton().GetIndex();
        var cancelIndex = dialog.GetCancelButton().GetIndex();
        for (var childIndex = 0; childIndex < buttonRow.GetChildCount(); childIndex++)
        {
            if (buttonRow.GetChild(childIndex) is Control spacer and not Button)
            {
                spacer.Visible = childIndex < Math.Min(okIndex, cancelIndex)
                    || childIndex > Math.Max(okIndex, cancelIndex);
            }
        }
    }

    private void OnConfirmed()
    {
        var confirmed = pendingConfirmation;
        pendingConfirmation = null;
        confirmed?.Invoke();
    }
}
