using System.Collections.Generic;
using Godot;
using TheLastGate.Assets;
using TheLastGate.Updates;

namespace TheLastGate.Launcher;

public static class ChangelogText
{
    private const int HeadingSizeIncrease = 2;
    private const string Bullet = "  • ";

    private static readonly Color HeadingColour = new(0.117647f, 0.505882f, 0.670588f);

    public static void Fill(RichTextLabel label, IEnumerable<AvailableRelease> releases, ILauncherFontSet fonts)
    {
        label.Clear();
        label.AddThemeFontOverride("normal_font", fonts.Regular);
        label.AddThemeFontSizeOverride("normal_font_size", fonts.Size);

        foreach (var release in releases)
        {
            var notes = release.Notes;
            label.PushFont(fonts.Bold, fonts.Size + HeadingSizeIncrease);
            label.PushColor(HeadingColour);
            label.AddText(
                notes.Date is { } date ? $"{release.Version} ({date:yyyy-MM-dd})" : release.Version.ToString());
            label.Pop();
            label.Pop();
            label.Newline();

            foreach (var section in notes.Sections)
            {
                if (section.Title.Length > 0)
                {
                    label.PushFont(fonts.Bold, fonts.Size);
                    label.AddText(section.Title);
                    label.Pop();
                    label.Newline();
                }

                foreach (var item in section.Items)
                {
                    label.AddText(Bullet + item);
                    label.Newline();
                }
            }

            label.Newline();
        }
    }
}
