using TheLastGate.Net;
using TheLastGate.State;

namespace TheLastGate.Tests;

public class HelpPagesTests
{
    private const byte Commands = (byte)HelpPageNumber.Commands;
    private const int TextLength = 56;

    [Fact]
    public void Page_HoldsTheHeadingsRowsAndLinesOfAFinishedPage()
    {
        var state = new GameState();
        var processor = new TickProcessor(state);
        HelpPageNumber? arrived = null;
        state.HelpPages.PageArrived += number => arrived = number;

        processor.Process(Piece(HelpPieceKind.Start, 0, 0, 0, 0, ""));
        processor.Process(Piece(HelpPieceKind.Heading, 0, 0, 0, 0, "Command"));
        processor.Process(Piece(HelpPieceKind.Heading, 15, 1, 1, 0, "Exp"));
        processor.Process(Piece(HelpPieceKind.Cell, 0, 0, 2, 0, "#who"));
        processor.Process(Piece(HelpPieceKind.Cell, 0, 1, 2, 0, "who is online"));
        processor.Process(Piece(HelpPieceKind.Line, 0, 0, 3, 0, "Most commands are toggles."));
        Assert.Null(arrived);
        processor.Process(Piece(HelpPieceKind.End, 1, 0, 0, 0, ""));

        Assert.Equal(HelpPageNumber.Commands, arrived);
        var page = state.HelpPages.Page(HelpPageNumber.Commands)!;
        Assert.Equal([new HelpHeading("Command", false, 0), new HelpHeading("Exp", true, 1.5f)], page.Headings);
        Assert.Equal(2, page.Rows[0].Font);
        Assert.Equal(["#who", "who is online"], page.Rows[0].Cells);
        Assert.Equal(new HelpLine(3, "Most commands are toggles."), page.Lines[0]);
    }

    [Fact]
    public void Page_HoldsItsSubTabsInTheOrderTheyCame()
    {
        var state = new GameState();
        var processor = new TickProcessor(state);

        processor.Process(Piece(HelpPieceKind.Start, 1, 0, 0, 0, ""));
        processor.Process(Piece(HelpPieceKind.SubTab, 0, 0, 0, 0, "Attribute Rings"));
        processor.Process(Piece(HelpPieceKind.SubTab, 1, 0, 0, 0, "Skill Rings"));
        processor.Process(Piece(HelpPieceKind.End, 0, 0, 0, 0, ""));

        var page = state.HelpPages.Page(HelpPageNumber.Commands)!;
        Assert.Equal(1, page.Argument);
        Assert.Equal([new HelpSubTab(0, "Attribute Rings"), new HelpSubTab(1, "Skill Rings")], page.SubTabs);
    }

    [Fact]
    public void Page_JoinsTheTextOfALongCell()
    {
        var state = new GameState();
        var processor = new TickProcessor(state);
        var first = new string('a', TextLength);

        processor.Process(Piece(HelpPieceKind.Start, 0, 0, 0, 0, ""));
        processor.Process(Piece(HelpPieceKind.Cell, 0, 1, 1, 0, first));
        processor.Process(Piece(HelpPieceKind.Cell, 0, 1, 1, 1, "end"));
        processor.Process(Piece(HelpPieceKind.End, 1, 0, 0, 0, ""));

        Assert.Equal(first + "end", state.HelpPages.Page(HelpPageNumber.Commands)!.Rows[0].Cells[1]);
    }

    [Fact]
    public void Page_StartsAgainWhenTheServerSendsItAgain()
    {
        var state = new GameState();
        var processor = new TickProcessor(state);

        processor.Process(Piece(HelpPieceKind.Start, 0, 0, 0, 0, ""));
        processor.Process(Piece(HelpPieceKind.Cell, 0, 0, 1, 0, "old"));
        processor.Process(Piece(HelpPieceKind.End, 1, 0, 0, 0, ""));
        processor.Process(Piece(HelpPieceKind.Start, 7, 0, 0, 0, ""));
        processor.Process(Piece(HelpPieceKind.End, 0, 0, 0, 0, ""));

        var page = state.HelpPages.Page(HelpPageNumber.Commands)!;
        Assert.Empty(page.Rows);
        Assert.Equal(7, page.Argument);
    }

    [Fact]
    public void For_AsksForAPageAndItsArgument()
    {
        var packet = HelpPackets.For(HelpPageNumber.Quest, 0x8003);

        Assert.Equal((byte)ClientCommand.Help, packet[0]);
        Assert.Equal((byte)HelpPageNumber.Quest, packet[1]);
        Assert.Equal(0, packet[2]);
        Assert.Equal([0x03, 0x80, 0, 0], packet[3..7]);
    }

    private static byte[] Piece(HelpPieceKind kind, int row, int column, int font, int part, string text) =>
        TestCommand.Payload(
            TestCommand.Of(ServerCommand.Help)
                .Byte(Commands)
                .Byte((byte)kind)
                .UInt16((ushort)row)
                .Byte((byte)column)
                .Byte((byte)font)
                .Byte((byte)part)
                .Text(text, TextLength));
}
