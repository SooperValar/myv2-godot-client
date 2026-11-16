using System.Collections.Generic;

namespace TheLastGate.Hud;

public sealed record BuffIconLists(IReadOnlyList<BuffIcon> Buffs, IReadOnlyList<BuffIcon> Debuffs);
