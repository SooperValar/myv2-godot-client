using System.Collections.Generic;

namespace TheLastGate.State;

public sealed class ItemInfoReply
{
    public int Where { get; internal set; }

    public int Slot { get; internal set; }

    public List<LogLine> Lines { get; } = [];

    public bool Complete { get; internal set; }
}
