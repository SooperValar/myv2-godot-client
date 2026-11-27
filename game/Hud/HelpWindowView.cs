using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using TheLastGate.Assets;
using TheLastGate.State;

namespace TheLastGate.Hud;

// MyV2's Help window: pages the server builds (help.c), one tab each; a quest's row opens the quest in detail.
[Tool]
public partial class HelpWindowView : CenterContainer, IHudContextUser
{
    private const int CloseSprite = SpriteNumbers.ClientArtBase + 18042;
    private const int RedFont = 0;
    private const int GreenFont = 2;
    private const int BlueFont = 3;

    private static readonly Rect2 CloseRegion = new(283, 0, 14, 14);

    private static readonly Dictionary<HelpPageNumber, float[]> ColumnWidths = new()
    {
        [HelpPageNumber.Commands] = [1.4f, 2.6f],
        [HelpPageNumber.Quests] = [0.3f, 1.3f, 1.5f, 1.6f, 0.6f],
    };

    private HelpPageNumber tab = HelpPageNumber.Commands;
    private bool allQuests;
    private int? shownQuest;

    public HelpWindowView() => AddToGroup(HudContext.Group);

    public event Action? ClosePressed;

    public event Action<HelpPageNumber, int>? PageRequested;

    private DataTable Table => GetNode<DataTable>("%Table");

    private VBoxContainer Lines => GetNode<VBoxContainer>("%Lines");

    public void ShowContext(HudContext context)
    {
        if (context.Sprites?.GetTexture(CloseSprite) is { } sheet)
        {
            GetNode<TextureButton>("Window/Close").TextureNormal =
                new AtlasTexture { Atlas = sheet, Region = CloseRegion };
        }
    }

    public override void _Ready()
    {
        if (Engine.IsEditorHint())
        {
            return;
        }

        GetNode<TextureButton>("Window/Close").Pressed += () => ClosePressed?.Invoke();
        GetNode<Button>("%CommandsTab").Pressed += () => Open(HelpPageNumber.Commands);
        GetNode<Button>("%QuestsTab").Pressed += () => Open(HelpPageNumber.Quests);
        GetNode<Button>("%Back").Pressed += () => Open(HelpPageNumber.Quests);
        GetNode<Button>("%AllQuests").Toggled += shown =>
        {
            allQuests = shown;
            Open(HelpPageNumber.Quests);
        };
        Table.RowClicked += OpenQuest;
    }

    public void ShowAllQuests(bool shown)
    {
        allQuests = shown;
        GetNode<Button>("%AllQuests").SetPressedNoSignal(shown);
    }

    public void Open(HelpPageNumber page)
    {
        tab = page;
        shownQuest = null;
        Visible = true;
        GetNode<Button>("%CommandsTab").ButtonPressed = page == HelpPageNumber.Commands;
        GetNode<Button>("%QuestsTab").ButtonPressed = page == HelpPageNumber.Quests;
        GetNode<Button>("%AllQuests").Visible = page == HelpPageNumber.Quests;
        GetNode<Button>("%Back").Visible = false;
        PageRequested?.Invoke(page, page == HelpPageNumber.Quests && allQuests ? HelpPages.AllQuests : 0);
    }

    public void OpenQuest(int row)
    {
        if (tab != HelpPageNumber.Quests || shownQuest != null)
        {
            return;
        }

        shownQuest = row + 1;
        GetNode<Button>("%Back").Visible = true;
        GetNode<Button>("%AllQuests").Visible = false;
        PageRequested?.Invoke(HelpPageNumber.Quest, row + 1 + (allQuests ? HelpPages.QuestOfAll : 0));
    }

    public void ShowPage(HelpPageNumber number, HelpPage page)
    {
        var wanted = shownQuest != null ? HelpPageNumber.Quest : tab;
        if (number != wanted || !Visible)
        {
            return;
        }

        var table = number != HelpPageNumber.Quest;
        Table.Visible = table;
        if (table)
        {
            var widths = ColumnWidths[number];
            Table.SetColumns(
                page.Headings.Select(
                        (heading, column) => new TableColumn(
                            heading.Text,
                            column < widths.Length ? widths[column] : 1,
                            LeftAligned: !heading.RightAligned))
                    .ToArray());
            Table.ShowRows(page.Rows.Select(row => Row(row, page.Headings.Count)).ToArray());
        }

        foreach (var child in Lines.GetChildren())
        {
            child.QueueFree();
        }

        foreach (var line in page.Lines)
        {
            Lines.AddChild(
                new Label
                {
                    Text = line.Text,
                    ThemeTypeVariation = TypeVariation(Tone(line.Font)),
                    AutowrapMode = TextServer.AutowrapMode.WordSmart,
                });
        }

        GetNode<ScrollContainer>("%Scroll").ScrollVertical = 0;
    }

    private static TableRow Row(HelpRow row, int columns)
    {
        var tone = Tone(row.Font);
        return new TableRow(
            Enumerable.Range(0, columns)
                .Select(column => new TableCell(column < row.Cells.Count ? row.Cells[column] : "", tone))
                .ToArray());
    }

    private static CellTone Tone(int font) => font switch
    {
        RedFont => CellTone.Negative,
        GreenFont => CellTone.Positive,
        BlueFont => CellTone.Dim,
        _ => CellTone.Normal,
    };

    private static string TypeVariation(CellTone tone) => tone switch
    {
        CellTone.Negative => "TableCellNegative",
        CellTone.Positive => "TableCellPositive",
        CellTone.Dim => "TableCellDim",
        _ => "TableCell",
    };
}
