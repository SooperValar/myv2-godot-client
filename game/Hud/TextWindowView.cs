using Godot;
using TheLastGate.Assets;
using TheLastGate.Windows;

namespace TheLastGate.Hud;

// The message of the day, the new player welcome and the tutorials, drawn over each other in that order as the
// current client does.
public partial class TextWindowView : Control
{
    public void Show(TextWindows windows, MessageOfTheDayLines messageLines)
    {
        Visible = windows.AnyOpen;
        if (!Visible)
        {
            return;
        }

        GetNode<CanvasItem>("MessageOfTheDay").Visible = windows.MessageOfTheDayOpen;
        GetNode<CanvasItem>("Welcome").Visible = windows.WelcomeOpen;
        GetNode<CanvasItem>("OkButton").Visible = windows.MessageOfTheDayOpen;

        var lines = GetNode<Control>("Lines");
        lines.Visible = windows.MessageOfTheDayOpen || windows.WelcomeOpen;
        for (var index = 0; index < MessageOfTheDayLines.VisibleLines; index++)
        {
            var label = lines.GetChild<HudLabel>(index);
            var line = messageLines.Lines[index];
            label.Text = line.Text;
            label.Colour = (TextColour)line.Font;
        }

        var tutorial = GetNode<CanvasItem>("Tutorial");
        tutorial.Visible = windows.Tutorial != null;
        if (windows.Tutorial is not { } number)
        {
            return;
        }

        GetNode<ArchiveSprite>("Tutorial/Picture").SpriteNumber = TutorialPages.Picture(number, windows.TutorialPage);
        var text = TutorialPages.Text(number, windows.TutorialPage);
        var tutorialLines = GetNode<Control>("Tutorial/Lines");
        for (var index = 0; index < TutorialPages.LinesPerPage; index++)
        {
            tutorialLines.GetChild<Label>(index).Text = text[index];
        }
    }
}
