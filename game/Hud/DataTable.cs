using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

namespace TheLastGate.Hud;

// The first column holds each row's name, after its swatch; the rest are right-aligned values.
[Tool]
public partial class DataTable : VBoxContainer
{
    private const string RowStyle = "TableRow";
    private const string HoverRowStyle = "TableRowHover";
    private static readonly Vector2 SwatchSize = new(6, 6);

    private readonly List<Control> rows = [];
    private IReadOnlyList<TableColumn> columns = [];

    public event Action<int>? RowClicked;

    public override void _Ready() => ThemeTypeVariation = "DataTable";

    public void SetColumns(IReadOnlyList<TableColumn> shownColumns)
    {
        columns = shownColumns;
        foreach (var child in GetChildren())
        {
            child.Free();
        }

        rows.Clear();
        if (columns.Any(column => column.Group != null))
        {
            AddChild(GroupRow());
        }

        AddChild(HeaderRow());
    }

    public void ShowRows(IReadOnlyList<TableRow> shownRows)
    {
        while (rows.Count < shownRows.Count)
        {
            var row = BodyRow();
            rows.Add(row);
            AddChild(row);
        }

        for (var index = 0; index < rows.Count; index++)
        {
            rows[index].Visible = index < shownRows.Count;
            if (index < shownRows.Count)
            {
                Fill(rows[index], shownRows[index]);
            }
        }
    }

    private static void Fill(Control row, TableRow shown)
    {
        var cells = row.GetChild(0);
        var nameCell = cells.GetChild(0);
        var swatch = nameCell.GetChild<ColorRect>(0);
        swatch.Visible = shown.Swatch != null;
        swatch.Color = shown.Swatch ?? Colors.Transparent;
        ShowCell(nameCell.GetChild<Label>(1), shown.Cells[0]);
        for (var column = 1; column < shown.Cells.Count; column++)
        {
            ShowCell(cells.GetChild<Label>(column), shown.Cells[column]);
        }
    }

    private static void ShowCell(Label label, TableCell cell)
    {
        label.Text = cell.Text;
        label.ThemeTypeVariation = cell.Tone switch
        {
            CellTone.Normal => "TableCell",
            CellTone.Dim => "TableCellDim",
            CellTone.Positive => "TableCellPositive",
            CellTone.Negative => "TableCellNegative",
            CellTone.Zero => "TableCellZero",
            _ => throw new UnreachableException(),
        };
    }

    private Control GroupRow()
    {
        var groups = new HBoxContainer { ThemeTypeVariation = "TableCells" };
        var padded = new MarginContainer();
        padded.AddChild(groups);
        foreach (var run in GroupRuns())
        {
            var caption = new PanelContainer
            {
                ThemeTypeVariation = run.Group == null ? "" : "TableGroup",
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                SizeFlagsStretchRatio = run.Width,
                SelfModulate = run.Group == null ? Colors.Transparent : Colors.White,
            };
            caption.AddChild(
                new Label
                {
                    Text = run.Group ?? "",
                    ThemeTypeVariation = "TableGroupCaption",
                    HorizontalAlignment = HorizontalAlignment.Center,
                });
            groups.AddChild(caption);
        }

        var row = new PanelContainer { ThemeTypeVariation = RowStyle, SelfModulate = Colors.Transparent };
        row.AddChild(padded);
        return row;
    }

    private IEnumerable<(string? Group, float Width)> GroupRuns()
    {
        var runs = new List<(string? Group, float Width)>();
        foreach (var column in columns)
        {
            if (runs.Count > 0 && runs[^1].Group == column.Group)
            {
                runs[^1] = (column.Group, runs[^1].Width + column.Width);
            }
            else
            {
                runs.Add((column.Group, column.Width));
            }
        }

        return runs;
    }

    private Control HeaderRow()
    {
        var row = new PanelContainer { ThemeTypeVariation = "TableHeaderRow" };
        var cells = new HBoxContainer { ThemeTypeVariation = "TableCells" };
        row.AddChild(cells);
        for (var column = 0; column < columns.Count; column++)
        {
            var header = CellLabel(column == 0 || columns[column].LeftAligned, columns[column].Width);
            header.Text = columns[column].Header;
            header.ThemeTypeVariation = "TableHeader";
            cells.AddChild(header);
        }

        return row;
    }

    private Control BodyRow()
    {
        var row = new PanelContainer { ThemeTypeVariation = RowStyle, MouseFilter = MouseFilterEnum.Pass };
        row.MouseEntered += () => row.ThemeTypeVariation = HoverRowStyle;
        row.MouseExited += () => row.ThemeTypeVariation = RowStyle;
        row.GuiInput += inputEvent =>
        {
            if (inputEvent is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
            {
                RowClicked?.Invoke(rows.IndexOf(row));
            }
        };
        var cells = new HBoxContainer { ThemeTypeVariation = "TableCells" };
        row.AddChild(cells);

        var nameCell = new HBoxContainer
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsStretchRatio = columns[0].Width,
        };
        nameCell.AddChild(
            new ColorRect
            {
                CustomMinimumSize = SwatchSize,
                SizeFlagsVertical = SizeFlags.ShrinkCenter,
            });
        nameCell.AddChild(CellLabel(true, 1));
        cells.AddChild(nameCell);
        foreach (var column in columns.Skip(1))
        {
            cells.AddChild(CellLabel(column.LeftAligned, column.Width));
        }

        return row;
    }

    // A label that trims instead of growing keeps every row's columns at the same widths.
    private static Label CellLabel(bool isName, float width) => new()
    {
        HorizontalAlignment = isName ? HorizontalAlignment.Left : HorizontalAlignment.Right,
        SizeFlagsHorizontal = SizeFlags.ExpandFill,
        SizeFlagsStretchRatio = width,
        TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis,
    };
}
