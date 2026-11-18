using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;
using TheLastGate.Notes;
using TheLastGate.Windows;

namespace TheLastGate.Map;

// The world map's category column: the tree of categories with their notes, the category menu, the New, Import and
// Export buttons, and the prompts they open. The list is rebuilt only when the notes change; the current category and
// the selected note are picked out by a darker row.
public partial class WorldMapCategories : Control
{
    private const int SwatchSize = 12;
    private const int PaletteSwatchSize = 20;
    private const int NoParent = 0;

    private static readonly Color Gold = Color.Color8(243, 219, 106);
    private static readonly Color DimmedText = Color.Color8(72, 81, 81);
    private static readonly Color ChosenRow = Color.Color8(58, 16, 16);

    private readonly Dictionary<int, TreeItem> rows = [];
    private NoteSet? builtNotes;
    private int builtRevision = -1;
    private (int Current, int Selected) highlighted;
    private NoteImportDraft? importDraft;
    private int menuCategoryId;
    private int newColour;
    private int newParentId;
    private NoteSet? notes;
    private WorldMapWindow? window;

    public bool PromptOpen => Prompts.Any(prompt => prompt.Visible);

    private Tree List => GetNode<Tree>("List");

    private PopupMenu Menu => GetNode<PopupMenu>("Menu");

    private PopupMenu ColourMenu => GetNode<PopupMenu>("Menu/Colour");

    private PopupMenu SendToMenu => GetNode<PopupMenu>("Menu/SendTo");

    private Control NewPrompt => GetNode<Control>("NewBlocker");

    private Control ImportPrompt => GetNode<Control>("ImportBlocker");

    private Control ExportPrompt => GetNode<Control>("ExportBlocker");

    private Control DeletePrompt => GetNode<Control>("DeleteBlocker");

    private Control DuplicatesPrompt => GetNode<Control>("DuplicatesBlocker");

    private Control[] Prompts => [NewPrompt, ImportPrompt, ExportPrompt, DeletePrompt, DuplicatesPrompt];

    private LineEdit NewName => PromptNode<LineEdit>(NewPrompt, "Name");

    private TextEdit ImportCode => PromptNode<TextEdit>(ImportPrompt, "Code");

    private LineEdit ImportRootName => PromptNode<LineEdit>(ImportPrompt, "Rename/RootName");

    private static double Now => Time.GetTicksMsec() / 1000.0;

    public event Action? NotesChanged;

    public event Action<NoteHit, Vector2>? NoteMenuWanted;

    public override void _Ready()
    {
        List.CreateItem();
        List.ItemMouseSelected += ClickRow;
        List.ItemCollapsed += item => window?.Editor.Expand(IdOf(item), !item.Collapsed);
        List.ItemEdited += FinishRename;
        GetNode<Button>("Footer/Buttons/New").Pressed += () => OpenNewPrompt(NoParent);
        GetNode<Button>("Footer/Buttons/Import").Pressed += () => OpenImportPrompt(NoParent);
        GetNode<Button>("Footer/Buttons/Export").Pressed += () => Export(window?.Editor.CurrentCategoryId ?? 0);
        PrepareMenus();
        ConnectNewPrompt();
        ConnectImportPrompt();
        PromptNode<Button>(ExportPrompt, "Buttons/Confirm").Pressed += () => ExportPrompt.Visible = false;
        PromptNode<Button>(DeletePrompt, "Buttons/Confirm").Pressed += DeleteCategory;
        PromptNode<Button>(DeletePrompt, "Buttons/Cancel").Pressed += () => DeletePrompt.Visible = false;
        PromptNode<Button>(DuplicatesPrompt, "Buttons/Confirm").Pressed += RemoveDuplicates;
        PromptNode<Button>(DuplicatesPrompt, "Buttons/Cancel").Pressed += () => DuplicatesPrompt.Visible = false;
    }

    public override void _Process(double delta)
    {
        if (window == null || notes == null || !IsVisibleInTree())
        {
            return;
        }

        if (notes != builtNotes || notes.Revision != builtRevision)
        {
            Rebuild(notes, window.Editor);
        }
        else if (highlighted != (window.Editor.CurrentCategoryId, window.Editor.SelectedNoteId))
        {
            Highlight(window.Editor);
        }

        GetNode<Button>("Footer/Buttons/Export").Disabled = notes.FindCategory(window.Editor.CurrentCategoryId) == null;
        ClosePromptsThatLostTheirCategory(notes);
    }

    public override void _Input(InputEvent inputEvent)
    {
        if (inputEvent is InputEventKey { Pressed: true, Echo: false, Keycode: Key.Escape } && PromptOpen)
        {
            CloseAll();
            GetViewport().SetInputAsHandled();
        }
    }

    public void Show(WorldMapWindow shownWindow, NoteSet? shownNotes)
    {
        window = shownWindow;
        notes = shownNotes;
        foreach (var name in (string[])["New", "Import"])
        {
            GetNode<Button>($"Footer/Buttons/{name}").Disabled = shownNotes == null;
        }
    }

    public void CloseAll()
    {
        foreach (var prompt in Prompts)
        {
            prompt.Visible = false;
        }

        importDraft = null;
        Menu.Hide();
    }

    private static T PromptNode<T>(Control prompt, string path)
        where T : Node =>
        prompt.GetNode<T>($"Prompt/Margins/Rows/{path}");

    private static int IdOf(TreeItem item) => item.GetMetadata(0).AsInt32();

    private void SaveChanges()
    {
        if (notes?.Changed == true)
        {
            NotesChanged?.Invoke();
        }
    }

    // Categories and notes share one id counter, so a row's id says which it is.
    private void Rebuild(NoteSet shown, WorldMapEditor editor)
    {
        var root = List.GetRoot();
        foreach (var child in root.GetChildren())
        {
            child.Free();
        }

        rows.Clear();
        AddBranch(shown, editor, root, NoParent);
        builtNotes = shown;
        builtRevision = shown.Revision;
        Highlight(editor);
    }

    private void AddBranch(NoteSet shown, WorldMapEditor editor, TreeItem parent, int parentId)
    {
        foreach (var category in shown.Categories.Where(category =>
            category.ParentId == parentId && category.Id != parentId))
        {
            var dimmed = !shown.IsShown(category);
            var row = AddRow(
                parent,
                category.Id,
                category.Name,
                NoteSwatches.Of(NotePalette.Rgb(category.ColourIndex), SwatchSize, dimmed),
                dimmed);
            AddBranch(shown, editor, row, category.Id);
            foreach (var note in category.Notes)
            {
                var swatch = note.ColourIndex == Note.CategoryColour
                    ? NoteSwatches.Blank(SwatchSize)
                    : NoteSwatches.Of(NoteSet.Rgb(category, note), SwatchSize, dimmed);
                AddRow(row, note.Id, WorldMapWording.KindPrefix(note.Kind) + note.Label, swatch, dimmed);
            }

            row.Collapsed = !editor.ExpandedCategories.Contains(category.Id);
        }
    }

    private TreeItem AddRow(TreeItem parent, int id, string text, Texture2D swatch, bool dimmed)
    {
        var row = List.CreateItem(parent);
        row.SetText(0, text);
        row.SetIcon(0, swatch);
        row.SetMetadata(0, id);
        row.SetCustomColor(0, dimmed ? DimmedText : Gold);
        rows[id] = row;
        return row;
    }

    private void Highlight(WorldMapEditor editor)
    {
        foreach (var id in (int[])[highlighted.Current, highlighted.Selected])
        {
            if (rows.TryGetValue(id, out var row))
            {
                row.ClearCustomBgColor(0);
            }
        }

        highlighted = (editor.CurrentCategoryId, editor.SelectedNoteId);
        foreach (var id in (int[])[highlighted.Current, highlighted.Selected])
        {
            if (rows.TryGetValue(id, out var row))
            {
                row.SetCustomBgColor(0, ChosenRow);
            }
        }
    }

    // A click on a category makes it the one new notes go into; a click on a note selects it and moves the map to it.
    // A right click opens the row's menu.
    private void ClickRow(Vector2 position, long buttonIndex)
    {
        if (List.GetSelected() is not { } row || notes == null || window == null)
        {
            return;
        }

        var id = IdOf(row);
        List.DeselectAll();
        var rightClick = buttonIndex == (long)MouseButton.Right;
        if (notes.FindCategory(id) != null)
        {
            if (rightClick)
            {
                OpenMenu(id);
            }
            else
            {
                window.Editor.ChooseCategory(id);
            }
        }
        else if (notes.FindNote(id) is { } note)
        {
            if (rightClick)
            {
                NoteMenuWanted?.Invoke(new NoteHit(id, -1), GetViewport().GetMousePosition());
            }
            else
            {
                window.Editor.Select(id);
                window.ShowNote(note);
            }
        }
    }

    private void StartRename(int categoryId)
    {
        if (!rows.TryGetValue(categoryId, out var row))
        {
            return;
        }

        row.SetEditable(0, true);
        List.SetSelected(row, 0);
        List.EditSelected(true);
    }

    private void FinishRename()
    {
        if (List.GetEdited() is not { } row || notes == null)
        {
            return;
        }

        row.SetEditable(0, false);
        var id = IdOf(row);
        var name = row.GetText(0).Trim();
        if (name.Length > 0 && notes.RenameCategory(id, name))
        {
            SaveChanges();
        }
        else
        {
            row.SetText(0, notes.FindCategory(id)?.Name ?? "");
        }

        List.DeselectAll();
    }

    // The menus are windows of their own, so they are given the window's font. Showing, hiding and moving a category
    // keep the menu open, as the current client does, so it is shut by hand for everything else.
    private void PrepareMenus()
    {
        foreach (var menu in (PopupMenu[])[Menu, ColourMenu, SendToMenu])
        {
            menu.AddThemeFontOverride("font", GetThemeFont("font", "Label"));
            menu.AddThemeFontSizeOverride("font_size", GetThemeFontSize("font_size", "Label"));
        }

        Menu.HideOnItemSelection = false;
        Menu.HideOnCheckableItemSelection = false;
        for (var colourIndex = 0; colourIndex < NotePalette.Size; colourIndex++)
        {
            ColourMenu.AddIconRadioCheckItem(
                NoteSwatches.Of(NotePalette.Rgb(colourIndex), PaletteSwatchSize),
                "",
                colourIndex);
        }

        Menu.IdPressed += ChooseFromMenu;
        ColourMenu.IdPressed += id =>
        {
            if (notes?.SetCategoryColour(menuCategoryId, (int)id) == true)
            {
                SaveChanges();
            }
        };
        SendToMenu.IdPressed += id =>
        {
            if (notes != null)
            {
                window?.Editor.MoveCategory(notes, menuCategoryId, (int)id);
                SaveChanges();
            }
        };
    }

    private void OpenMenu(int categoryId)
    {
        menuCategoryId = categoryId;
        if (FillMenu())
        {
            Menu.Popup(new Rect2I((Vector2I)GetViewport().GetMousePosition(), Vector2I.Zero));
        }
    }

    private bool FillMenu()
    {
        if (notes?.FindCategory(menuCategoryId) is not { } category)
        {
            Menu.Hide();
            return false;
        }

        Menu.Clear();
        Menu.AddItem(category.Name, (int)MenuItem.Heading);
        Menu.SetItemDisabled(Menu.ItemCount - 1, true);
        Menu.AddSeparator();
        Menu.AddCheckItem("Show on map", (int)MenuItem.ShowOnMap);
        Menu.SetItemChecked(Menu.ItemCount - 1, category.Visible);
        Menu.AddCheckItem("Show labels", (int)MenuItem.ShowLabels);
        Menu.SetItemChecked(Menu.ItemCount - 1, category.ShowLabels);
        Menu.AddSeparator();
        var canNest = notes.Depth(category.Id) < NoteLimits.Depth;
        AddMenuItem("New subcategory...", MenuItem.NewSubcategory, canNest);
        AddMenuItem("Import here...", MenuItem.ImportHere, canNest);
        AddMenuItem("Rename", MenuItem.Rename, true);
        Menu.AddSubmenuNodeItem("Colour", ColourMenu);
        Menu.AddSubmenuNodeItem("Send to", SendToMenu);
        AddMenuItem("Move up", MenuItem.MoveUp, notes.CanShiftCategory(category.Id, -1));
        AddMenuItem("Move down", MenuItem.MoveDown, notes.CanShiftCategory(category.Id, 1));
        Menu.AddSeparator();
        AddMenuItem("Export", MenuItem.Export, true);
        var duplicates = notes.CountDuplicatePins(category.Id);
        AddMenuItem(WorldMapWording.DuplicatePinsItem(duplicates), MenuItem.RemoveDuplicates, duplicates > 0);
        if (duplicates == 0)
        {
            Menu.SetItemTooltip(
                Menu.ItemCount - 1,
                "No pins here share a name and spot with another pin in the same category.");
        }

        var hasSubcategories = notes.HasChildren(category.Id);
        AddMenuItem("Delete category...", MenuItem.Delete, !hasSubcategories);
        if (hasSubcategories)
        {
            Menu.SetItemTooltip(Menu.ItemCount - 1, "Delete or move its subcategories first.");
        }

        for (var itemIndex = 0; itemIndex < ColourMenu.ItemCount; itemIndex++)
        {
            ColourMenu.SetItemChecked(itemIndex, ColourMenu.GetItemId(itemIndex) == category.ColourIndex);
        }

        SendToMenu.Clear();
        SendToMenu.AddItem("Top level", NoParent);
        SendToMenu.SetItemDisabled(0, !notes.CanMoveCategory(category.Id, NoParent));
        foreach (var target in notes.Tree())
        {
            SendToMenu.AddItem(target.Name, target.Id);
            var itemIndex = SendToMenu.ItemCount - 1;
            SendToMenu.SetItemIndent(itemIndex, notes.Depth(target.Id) - 1);
            SendToMenu.SetItemDisabled(itemIndex, !notes.CanMoveCategory(category.Id, target.Id));
        }

        return true;
    }

    private void AddMenuItem(string text, MenuItem item, bool enabled)
    {
        Menu.AddItem(text, (int)item);
        Menu.SetItemDisabled(Menu.ItemCount - 1, !enabled);
    }

    private void ChooseFromMenu(long id)
    {
        if (notes?.FindCategory(menuCategoryId) is not { } category || window == null)
        {
            return;
        }

        var item = (MenuItem)id;
        switch (item)
        {
            case MenuItem.ShowOnMap:
                category.Visible = !category.Visible;
                notes.Changed = true;
                break;
            case MenuItem.ShowLabels:
                category.ShowLabels = !category.ShowLabels;
                notes.Changed = true;
                break;
            case MenuItem.MoveUp or MenuItem.MoveDown:
                notes.ShiftCategory(category.Id, item == MenuItem.MoveUp ? -1 : 1);
                break;
            case MenuItem.NewSubcategory:
                OpenNewPrompt(category.Id);
                break;
            case MenuItem.ImportHere:
                OpenImportPrompt(category.Id);
                break;
            case MenuItem.Rename:
                StartRename(category.Id);
                break;
            case MenuItem.Export:
                Export(category.Id);
                break;
            case MenuItem.RemoveDuplicates:
                AskToRemoveDuplicates(category);
                break;
            case MenuItem.Delete:
                PromptNode<Label>(DeletePrompt, "Question").Text = WorldMapWording.DeleteCategoryQuestion(category);
                DeletePrompt.Visible = true;
                break;
            case MenuItem.Heading:
                break;
            default:
                throw new UnreachableException();
        }

        SaveChanges();
        if (item is MenuItem.ShowOnMap or MenuItem.ShowLabels or MenuItem.MoveUp or MenuItem.MoveDown)
        {
            FillMenu();
        }
        else
        {
            Menu.Hide();
        }
    }

    private void Export(int categoryId)
    {
        if (notes?.FindCategory(categoryId) == null)
        {
            return;
        }

        if (NoteShareCode.Export(notes, categoryId) is not { } code)
        {
            window?.ShowStatus("Export failed.", Now);
            return;
        }

        DisplayServer.ClipboardSet(code);
        PromptNode<Label>(ExportPrompt, "Message").Text = NoteShareCode.CopiedMessage(code.Length);
        PromptNode<Label>(ExportPrompt, "TooLong").Visible = code.Length > NoteShareCode.DiscordLimit;
        ExportPrompt.Visible = true;
    }

    private void DeleteCategory()
    {
        DeletePrompt.Visible = false;
        if (notes != null)
        {
            window?.Editor.DeleteCategory(notes, menuCategoryId);
            SaveChanges();
        }
    }

    private void AskToRemoveDuplicates(NoteCategory category)
    {
        PromptNode<Label>(DuplicatesPrompt, "Question").Text = WorldMapWording.DuplicatePinsQuestion(
            notes!.CountDuplicatePins(category.Id),
            category.Name,
            notes.HasChildren(category.Id));
        DuplicatesPrompt.Visible = true;
    }

    private void RemoveDuplicates()
    {
        DuplicatesPrompt.Visible = false;
        if (notes != null)
        {
            window?.Editor.RemoveDuplicatePins(notes, menuCategoryId);
            SaveChanges();
        }
    }

    // A prompt about a category that has since gone, or that can no longer be done, closes by itself.
    private void ClosePromptsThatLostTheirCategory(NoteSet shown)
    {
        var category = shown.FindCategory(menuCategoryId);
        if (DeletePrompt.Visible && (category == null || shown.HasChildren(menuCategoryId)))
        {
            DeletePrompt.Visible = false;
        }

        if (DuplicatesPrompt.Visible && (category == null || shown.CountDuplicatePins(menuCategoryId) == 0))
        {
            DuplicatesPrompt.Visible = false;
        }

        if (ImportPrompt.Visible && importDraft?.DestinationGone != false)
        {
            ImportPrompt.Visible = false;
            importDraft = null;
        }
    }

    private void ConnectNewPrompt()
    {
        NewName.TextChanged += _ => ShowNewPrompt();
        NewName.TextSubmitted += _ => CreateCategory();
        PromptNode<Button>(NewPrompt, "Buttons/Create").Pressed += CreateCategory;
        PromptNode<Button>(NewPrompt, "Buttons/Cancel").Pressed += () => NewPrompt.Visible = false;
        for (var colourIndex = 0; colourIndex < NotePalette.Size; colourIndex++)
        {
            var chosen = colourIndex;
            PromptNode<Button>(NewPrompt, $"Palette/Swatch{colourIndex}").Pressed += () =>
            {
                newColour = chosen;
                ShowNewPrompt();
            };
        }
    }

    private void OpenNewPrompt(int parentId)
    {
        newParentId = parentId;
        newColour = 0;
        NewName.Text = "";
        NewPrompt.Visible = true;
        NewName.GrabFocus();
        ShowNewPrompt();
    }

    // The chosen colour has a gold border round its swatch.
    private void ShowNewPrompt()
    {
        PromptNode<Button>(NewPrompt, "Buttons/Create").Disabled = NewName.Text.Length == 0;
        for (var colourIndex = 0; colourIndex < NotePalette.Size; colourIndex++)
        {
            var rgb = NotePalette.Rgb(colourIndex);
            var swatch = new StyleBoxFlat
            {
                BgColor = Color.Color8((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb),
                BorderColor = Gold,
            };
            swatch.SetBorderWidthAll(colourIndex == newColour ? 2 : 0);
            var button = PromptNode<Button>(NewPrompt, $"Palette/Swatch{colourIndex}");
            foreach (var style in (string[])["normal", "hover", "pressed", "focus"])
            {
                button.AddThemeStyleboxOverride(style, swatch);
            }
        }
    }

    private void CreateCategory()
    {
        if (notes == null || window == null || NewName.Text.Length == 0)
        {
            return;
        }

        NewPrompt.Visible = false;
        window.Editor.CreateCategory(notes, NewName.Text, newColour, newParentId);
        SaveChanges();
    }

    private void ConnectImportPrompt()
    {
        ImportCode.TextChanged += () =>
        {
            importDraft?.ChangeText(ImportCode.Text);
            if (importDraft != null)
            {
                ImportRootName.Text = importDraft.RootName;
            }

            ShowImportPrompt();
        };
        ImportRootName.TextChanged += name =>
        {
            if (importDraft != null)
            {
                importDraft.RootName = name;
            }

            ShowImportPrompt();
        };
        PromptNode<Button>(ImportPrompt, "Merge").Pressed += () => FinishImport(importDraft?.Merge());
        PromptNode<Button>(ImportPrompt, "Buttons/AddAsNew").Pressed += () => FinishImport(importDraft?.AddAsNew());
        PromptNode<Button>(ImportPrompt, "Buttons/Add").Pressed += () => FinishImport(importDraft?.Add());
        PromptNode<Button>(ImportPrompt, "Buttons/Cancel").Pressed += () =>
        {
            ImportPrompt.Visible = false;
            importDraft = null;
        };
    }

    // A code already on the clipboard is pasted in, so sharing one is copy, Import, Add.
    private void OpenImportPrompt(int parentId)
    {
        if (notes == null)
        {
            return;
        }

        importDraft = new NoteImportDraft(notes, parentId, DisplayServer.ClipboardGet());
        ImportCode.Text = importDraft.Text;
        ImportRootName.Text = importDraft.RootName;
        ImportPrompt.Visible = true;
        ImportCode.GrabFocus();
        ShowImportPrompt();
    }

    private void ShowImportPrompt()
    {
        if (importDraft is not { } draft)
        {
            return;
        }

        var destination = PromptNode<Label>(ImportPrompt, "Destination");
        destination.Visible = draft.DestinationName != null;
        destination.Text = $"Import into \"{draft.DestinationName}\"";
        PromptNode<Label>(ImportPrompt, "Preview").Text = draft.Preview;
        PromptNode<Label>(ImportPrompt, "Preview").Visible = draft.Preview.Length > 0;
        PromptNode<Label>(ImportPrompt, "Conflict").Text = draft.ConflictMessage;
        foreach (var path in (string[])["Conflict", "Merge", "Rename", "Buttons/AddAsNew"])
        {
            PromptNode<Control>(ImportPrompt, path).Visible = draft.HasConflict;
        }

        PromptNode<Button>(ImportPrompt, "Buttons/AddAsNew").Disabled = !draft.CanAddAsNew;
        PromptNode<Button>(ImportPrompt, "Buttons/Add").Visible = !draft.HasConflict;
        PromptNode<Button>(ImportPrompt, "Buttons/Add").Disabled = !draft.IsReady;
        PromptNode<Label>(ImportPrompt, "Error").Text = draft.Error;
        PromptNode<Label>(ImportPrompt, "Error").Visible = draft.Error.Length > 0;
    }

    private void FinishImport(NoteImportResult? result)
    {
        if (result == null || importDraft == null || window == null)
        {
            return;
        }

        if (window.Editor.FinishImport(result, importDraft.ParentId))
        {
            ImportPrompt.Visible = false;
            importDraft = null;
            SaveChanges();
            return;
        }

        ShowImportPrompt();
    }

    private enum MenuItem
    {
        Heading,
        ShowOnMap,
        ShowLabels,
        NewSubcategory,
        ImportHere,
        Rename,
        MoveUp,
        MoveDown,
        Export,
        RemoveDuplicates,
        Delete,
    }
}
