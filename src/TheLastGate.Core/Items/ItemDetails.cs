using System;
using System.Collections.Generic;

namespace TheLastGate.Items;

public enum ItemStatKind
{
    Plain,
    Percent,
    Soulstone,
    Regen,
    Duration,
    Single,
    Rank,
}

// Percent values are in tenths of a percent; a regen's Passive is the total and Active the amount per second.
public sealed record ItemStat(
    string Name,
    ItemStatKind Kind,
    int Passive,
    int Active = 0,
    int? Required = null,
    string RankName = "");

[Flags]
public enum ItemModsApplied
{
    None = 0,
    Whetstoned = 1 << 0,
    Soulstoned = 1 << 1,
    Catalyzed = 1 << 2,
    Enchanted = 1 << 3,
    Focused = 1 << 4,
    Augmented = 1 << 5,
    Easier = 1 << 6,
    Corrupted = 1 << 7,
    Legacy = 1 << 8,
}

[Flags]
public enum ItemModsPossible
{
    None = 0,
    CanWhetstone = 1 << 0,
    CanSoulstone = 1 << 1,
    CannotSoulstone = 1 << 2,
    CanEnchant = 1 << 3,
    CannotEnchant = 1 << 4,
    CanOsiris = 1 << 5,
}

// Line numbers index the reply the details were parsed from.
public sealed class ItemDetails
{
    public string Name { get; init; } = "";

    public int? DescriptionLine { get; init; }

    public bool SoulstoneTable { get; init; }

    public IReadOnlyList<ItemStat> Stats { get; init; } = [];

    public ItemModsApplied ModsApplied { get; init; }

    public ItemModsPossible ModsPossible { get; init; }

    public int? Condition { get; init; }

    public int? Level { get; init; }

    public int KillsToLevel { get; init; }

    public int? Blessings { get; init; }

    public int KillsToBlessing { get; init; }

    public int? SellSilver { get; init; }

    public IReadOnlyList<int> EffectLines { get; init; } = [];

    public IReadOnlyList<int> OtherLines { get; init; } = [];
}
