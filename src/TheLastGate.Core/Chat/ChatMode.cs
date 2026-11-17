using System;

namespace TheLastGate.Chat;

public enum ChatKey
{
    Enter,
    Escape,
    Slash,
    Older,
    Newer,
}

public enum ChatAction
{
    None,
    Open,
    OpenWithSlash,
    OpenWithOlder,
    Send,
    Cancel,
}

public sealed class ChatMode
{
    public bool EnterToTalk { get; set; }

    public bool Active { get; private set; }

    public bool TakesTyping => throw new NotImplementedException();

    public bool RunsHotkeys => throw new NotImplementedException();

    public ChatAction Press(ChatKey key) => throw new NotImplementedException();
}
