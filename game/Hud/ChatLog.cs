using System;
using Godot;
using TheLastGate.Assets;
using TheLastGate.State;

namespace TheLastGate.Hud;

public partial class ChatLog : Control
{
    private const int VisibleLines = 22;
    private const int StatusFont = 1;

    private readonly LogScroll scroll = new();
    private ClassicFonts fonts = null!;
    private bool hasLines;
    private TextLog? log;
    private RichTextLabel text = null!;

    public override void _Ready() => text = GetNode<RichTextLabel>("Text");

    public override void _ExitTree()
    {
        if (log != null)
        {
            log.LineAdded -= Add;
        }
    }

    public override void _Process(double delta)
    {
        var linesBack = scroll.LinesBack;
        scroll.Advance(TimeSpan.FromSeconds(delta));
        if (scroll.LinesBack != linesBack)
        {
            ScrollToShown();
        }
    }

    public override void _GuiInput(InputEvent inputEvent)
    {
        if (inputEvent is not InputEventMouseButton { Pressed: true } button)
        {
            return;
        }

        var lines = button.ButtonIndex switch
        {
            MouseButton.WheelUp => LogScroll.WheelLines,
            MouseButton.WheelDown => -LogScroll.WheelLines,
            _ => 0,
        };
        if (lines != 0)
        {
            AcceptEvent();
            ScrollBy(lines);
        }
    }

    public override void _UnhandledKeyInput(InputEvent inputEvent)
    {
        if (inputEvent is not InputEventKey { Pressed: true } key)
        {
            return;
        }

        var lines = key.Keycode switch
        {
            Key.Pageup => LogScroll.PageLines,
            Key.Pagedown => -LogScroll.PageLines,
            _ => 0,
        };
        if (lines != 0)
        {
            GetViewport().SetInputAsHandled();
            ScrollBy(lines);
        }
    }

    public void Show(TextLog shownLog, ClassicFonts classicFonts)
    {
        fonts = classicFonts;
        log = shownLog;
        text.AddThemeFontOverride("normal_font", fonts.For(StatusFont));
        text.AddThemeFontSizeOverride("normal_font_size", ClassicFonts.FontSize);
        foreach (var line in log.Lines)
        {
            Append(line);
        }

        log.LineAdded += Add;
        scroll.LinesChanged(0, OldestLinesBack());
        ScrollToShown();
    }

    private void Add(LogLine line)
    {
        var linesBefore = text.GetLineCount();
        Append(line);
        var linesAdded = text.GetLineCount() - linesBefore;
        if (text.GetParagraphCount() > TextLog.MaxLines)
        {
            text.RemoveParagraph(0);
        }

        scroll.LinesChanged(linesAdded, OldestLinesBack());
        ScrollToShown();
    }

    private void Append(LogLine line)
    {
        if (hasLines)
        {
            text.Newline();
        }

        text.PushFont(fonts.For(line.Font), ClassicFonts.FontSize);
        text.AddText(line.Text);
        text.Pop();
        hasLines = true;
    }

    private void ScrollBy(int lines)
    {
        scroll.Scroll(lines);
        ScrollToShown();
    }

    private int OldestLinesBack() => Math.Max(0, text.GetLineCount() - VisibleLines);

    private void ScrollToShown() => text.ScrollToLine(Math.Max(0, OldestLinesBack() - scroll.LinesBack));
}
