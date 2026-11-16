using System;

namespace TheLastGate.Chat;

public sealed class ChatLineView
{
    public const int MaxLength = 115;
    public const int VisibleLength = 48;

    public (string Text, int CursorColumn) Show(string text, int caret) => throw new NotImplementedException();
}
