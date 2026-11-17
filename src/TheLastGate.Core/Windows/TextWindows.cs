using System.Collections.Generic;
using TheLastGate.Net;
using TheLastGate.State;

namespace TheLastGate.Windows;

public readonly record struct TextWindowAction(IReadOnlyList<byte[]> Packets, bool PlaysClick);

// The message of the day, the new player welcome and the tutorials share one spot and one set of buttons. Books are
// left out: the server never opens one.
public sealed class TextWindows
{
    public const int ClosedPauseTicks = 24;

    private const int FirstPage = 1;
    private const int SkipTutorials = 16;

    private static readonly TextWindowAction Nothing = new([], false);

    private int pause;

    public bool MessageOfTheDayOpen { get; private set; }

    public bool WelcomeOpen { get; private set; }

    public int? Tutorial { get; private set; }

    public int TutorialPage { get; private set; } = FirstPage;

    public bool AnyOpen => MessageOfTheDayOpen || WelcomeOpen || Tutorial != null;

    public void Show(WindowRequest request)
    {
        switch (request.Kind)
        {
            case WindowKind.MessageOfTheDay:
                MessageOfTheDayOpen = true;
                break;
            case WindowKind.NewPlayerWelcome:
                WelcomeOpen = true;
                break;
            case WindowKind.Tutorial when request.Number is >= 1 and <= TutorialPages.Tutorials:
                OpenTutorial(request.Number);
                break;
        }
    }

    // The close button tells the server the welcome was skipped and the tutorial read, so neither comes back.
    public TextWindowAction Close()
    {
        var packets = new List<byte[]>();
        if (WelcomeOpen)
        {
            packets.Add(Seen(SkipTutorials));
        }

        if (Tutorial is { } tutorial)
        {
            packets.Add(Seen(tutorial));
        }

        CloseAll();
        pause = ClosedPauseTicks;
        return new TextWindowAction(packets, false);
    }

    public TextWindowAction Ok()
    {
        MessageOfTheDayOpen = false;
        pause = ClosedPauseTicks;
        return Nothing;
    }

    public TextWindowAction Left()
    {
        if (WelcomeOpen)
        {
            WelcomeOpen = false;
            OpenTutorial(1);
            return new TextWindowAction([], true);
        }

        if (Tutorial == null)
        {
            return Nothing;
        }

        TutorialPage--;
        if (TutorialPage > FirstPage)
        {
            return new TextWindowAction([], true);
        }

        TutorialPage = FirstPage;
        return Nothing;
    }

    public TextWindowAction Right()
    {
        if (WelcomeOpen)
        {
            WelcomeOpen = false;
            pause = ClosedPauseTicks;
            return new TextWindowAction([Seen(SkipTutorials)], false);
        }

        if (Tutorial == null)
        {
            return Nothing;
        }

        TutorialPage++;
        if (TutorialPage < TutorialPages.Pages)
        {
            return new TextWindowAction([], true);
        }

        TutorialPage = TutorialPages.Pages;
        return Nothing;
    }

    // Escape tells the server nothing, so a tutorial closed this way comes back.
    public bool CloseByEscape()
    {
        var anyOpen = AnyOpen;
        CloseAll();
        return anyOpen;
    }

    public void CancelPause() => pause = 0;

    // For a short while after a window is closed, anything the server opens is closed again straight away.
    public void EndOfTick()
    {
        if (pause > 0)
        {
            pause--;
            CloseAll();
        }
    }

    private static byte[] Seen(int value) => ClientPackets.OneValue(ClientCommand.MessageOfTheDay, value);

    private void OpenTutorial(int tutorial)
    {
        Tutorial = tutorial;
        TutorialPage = FirstPage;
    }

    private void CloseAll()
    {
        MessageOfTheDayOpen = false;
        WelcomeOpen = false;
        Tutorial = null;
    }
}
