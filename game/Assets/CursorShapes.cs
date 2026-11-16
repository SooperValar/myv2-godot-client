using System.Collections.Generic;
using Godot;
using TheLastGate.Cursors;

namespace TheLastGate.Assets;

public static class CursorShapes
{
    private const string ImagePathFormat = "res://images/cursors/cursor{0}.png";

    // Shapes Godot's own controls never ask for, so text fields and splitters keep their usual cursors.
    private static readonly Dictionary<MouseCursor, Input.CursorShape> Shapes = new()
    {
        [MouseCursor.None] = Input.CursorShape.Arrow,
        [MouseCursor.Take] = Input.CursorShape.Vsplit,
        [MouseCursor.Drop] = Input.CursorShape.Hsplit,
        [MouseCursor.Use] = Input.CursorShape.Help,
        [MouseCursor.Give] = Input.CursorShape.Cross,
        [MouseCursor.Walk] = Input.CursorShape.Move,
        [MouseCursor.Hit] = Input.CursorShape.Wait,
        [MouseCursor.Swap] = Input.CursorShape.Busy,
        [MouseCursor.Select] = Input.CursorShape.Bdiagsize,
        [MouseCursor.Trash] = Input.CursorShape.Fdiagsize,
    };

    public static Input.CursorShape For(MouseCursor cursor) => Shapes[cursor];

    public static MouseCursor CursorFor(Input.CursorShape shape)
    {
        foreach (var (cursor, cursorShape) in Shapes)
        {
            if (cursorShape == shape)
            {
                return cursor;
            }
        }

        return MouseCursor.None;
    }

    public static void Register(float scale)
    {
        foreach (var (cursor, shape) in Shapes)
        {
            var image = GD.Load<Texture2D>(string.Format(ImagePathFormat, (int)cursor)).GetImage();
            image.Resize(
                Mathf.RoundToInt(image.GetWidth() * scale),
                Mathf.RoundToInt(image.GetHeight() * scale),
                Image.Interpolation.Nearest);
            Input.SetCustomMouseCursor(ImageTexture.CreateFromImage(image), shape, Vector2.Zero);
        }
    }

    public static void Clear()
    {
        foreach (var shape in Shapes.Values)
        {
            Input.SetCustomMouseCursor(null, shape);
        }

        Input.SetDefaultCursorShape();
    }
}
