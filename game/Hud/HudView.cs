using System;
using System.IO;
using Godot;
using TheLastGate.Assets;
using TheLastGate.Config;
using TheLastGate.State;

namespace TheLastGate.Hud;

[Tool]
public partial class HudView : Control
{
    private const long EditorCacheBudgetBytes = 64L * 1024 * 1024;

    private bool darkMode;
    private SpriteLoader? editorSprites;
    private SpriteLoader? sprites;

    [Export]
    public bool DarkMode
    {
        get => darkMode;
        set
        {
            darkMode = value;
            ShowSprites(this);
        }
    }

    public SpriteLoader? Sprites
    {
        get => sprites;
        set
        {
            sprites = value;
            ShowSprites(this);
        }
    }

    public void Show(GameState state, ClassicFonts fonts) => GetNode<ChatLog>("%ChatLog").Show(state.Chat, fonts);

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
    }

    private static void ShowSprites(Node parent)
    {
        foreach (var child in parent.GetChildren())
        {
            if (child is ArchiveSprite archiveSprite)
            {
                archiveSprite.LoadSprite();
            }

            ShowSprites(child);
        }
    }
}
