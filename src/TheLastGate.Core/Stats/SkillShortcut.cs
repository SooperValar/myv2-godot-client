using System;
using TheLastGate.State;

namespace TheLastGate.Stats;

public sealed class SkillShortcut
{
    public int? Selected { get; private set; }

    public LogLine Toggle(int skillNumber, string name, uint kindred) => throw new NotImplementedException();
}
