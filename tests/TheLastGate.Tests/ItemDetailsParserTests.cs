using System.Globalization;
using TheLastGate.Assets;
using TheLastGate.Items;
using TheLastGate.State;

namespace TheLastGate.Tests;

// Stat rows are built the way the server's printf formats write them.
public class ItemDetailsParserTests
{
    private const string TableHeader = "Stat              Passiv   Active   Requi";

    [Fact]
    public void Parse_ReadsAWeapon()
    {
        var details = ItemDetailsParser.Parse(
        [
            Line(TextColour.Lime, "Steel Sword:"),
            Line(TextColour.Yellow, "A finely crafted steel sword."),
            Line(TextColour.Orange, TableHeader),
            Line(TextColour.Yellow, PlainWithRequired("Strength", 2, 4, 30)),
            Line(TextColour.Yellow, PlainWithRequired("Sword", 0, 0, 25)),
            Line(TextColour.Yellow, Plain("Weapon Value", 24, 24)),
            Line(TextColour.Yellow, WholePercent("Crit Bonus", 5, 5)),
            Line(TextColour.Yellow, TenthsPercent("Dmg Bonus", 1, 5, 2, 5)),
            Line(TextColour.Yellow, Rank("Min. Rank", 5, "Sergeant")),
            Line(TextColour.Orange, "Can be Whetstoned."),
            Line(TextColour.Red, "Cannot be Soulstoned."),
            Line(TextColour.Purple, "Has been Enchanted."),
            Line(TextColour.Yellow, "It's fairly old."),
            Line(TextColour.Lime, "You estimate this could sell for 12G 34S."),
        ]);

        Assert.Equal("Steel Sword", details.Name);
        Assert.Equal(1, details.DescriptionLine);
        Assert.False(details.SoulstoneTable);
        Assert.Equal(
            [
                new ItemStat("Strength", ItemStatKind.Plain, 2, 4, 30),
                new ItemStat("Sword", ItemStatKind.Plain, 0, 0, 25),
                new ItemStat("Weapon Value", ItemStatKind.Plain, 24, 24),
                new ItemStat("Crit Bonus", ItemStatKind.Percent, 50, 50),
                new ItemStat("Dmg Bonus", ItemStatKind.Percent, 15, 25),
                new ItemStat("Min. Rank", ItemStatKind.Rank, 5, RankName: "Sergeant"),
            ],
            details.Stats);
        Assert.Equal(ItemModsPossible.CanWhetstone | ItemModsPossible.CannotSoulstone, details.ModsPossible);
        Assert.Equal(ItemModsApplied.Enchanted, details.ModsApplied);
        Assert.Equal(2, details.Condition);
        Assert.Equal(1234, details.SellSilver);
        Assert.Empty(details.EffectLines);
        Assert.Empty(details.OtherLines);
    }

    [Fact]
    public void Parse_ReadsASoulstoneTable()
    {
        var details = ItemDetailsParser.Parse(
        [
            Line(TextColour.Lime, "Soulstone:"),
            Line(TextColour.Yellow, "A glowing stone."),
            Line(TextColour.Orange, "Stat               Adds"),
            Line(TextColour.Yellow, Adds("Dagger", 3)),
            Line(TextColour.Yellow, Plain("Armor Value", 2, 2)),
        ]);

        Assert.True(details.SoulstoneTable);
        Assert.Equal(
            [new ItemStat("Dagger", ItemStatKind.Soulstone, 3), new ItemStat("Armor Value", ItemStatKind.Plain, 2, 2)],
            details.Stats);
    }

    [Fact]
    public void Parse_ReadsRegenDurationAndSingleValues()
    {
        var details = ItemDetailsParser.Parse(
        [
            Line(TextColour.Lime, "Healing Potion:"),
            Line(TextColour.Yellow, "Restores health over time."),
            Line(TextColour.Orange, TableHeader),
            Line(TextColour.Yellow, Regen("HP Regen", 120, 4)),
            Line(TextColour.Yellow, Seconds("Duration", 30)),
            Line(TextColour.Yellow, Single("Complexity", 12)),
        ]);

        Assert.Equal(
            [
                new ItemStat("HP Regen", ItemStatKind.Regen, 120, 4),
                new ItemStat("Duration", ItemStatKind.Duration, 30),
                new ItemStat("Complexity", ItemStatKind.Single, 12),
            ],
            details.Stats);
    }

    [Fact]
    public void Parse_KeepsEffectsBeforeTheTableAndUnrecognisedLinesAfterIt()
    {
        var details = ItemDetailsParser.Parse(
        [
            Line(TextColour.Lime, "Ring of Warmth:"),
            Line(TextColour.Yellow, "A warm ring."),
            Line(TextColour.Purple, "20% more maximum Endurance."),
            Line(TextColour.Orange, TableHeader),
            Line(TextColour.Yellow, Plain("Armor Value", 1, 1)),
            Line(TextColour.Green, "Level 3. Next level in 120 kills."),
            Line(TextColour.Pink, "Something the parser has never seen."),
        ]);

        Assert.Equal([2], details.EffectLines);
        Assert.Single(details.Stats);
        Assert.Equal(3, details.Level);
        Assert.Equal(120, details.KillsToLevel);
        Assert.Equal([6], details.OtherLines);
    }

    [Fact]
    public void Parse_LeavesOutTheComparisonWithTheHeldItem()
    {
        var details = ItemDetailsParser.Parse(
        [
            Line(TextColour.Lime, "Steel Sword:"),
            Line(TextColour.Yellow, "A sword."),
            Line(TextColour.Yellow, " "),
            Line(TextColour.Yellow, "You compare it with a Bronze Sword:"),
            Line(TextColour.Yellow, "A Steel Sword is the better weapon."),
            Line(TextColour.Yellow, "No difference as armor."),
            Line(TextColour.Orange, TableHeader),
            Line(TextColour.Yellow, Plain("Weapon Value", 24, 24)),
        ]);

        Assert.Empty(details.EffectLines);
        Assert.Empty(details.OtherLines);
        Assert.Single(details.Stats);
    }

    [Fact]
    public void Parse_LeavesValuesThatWereNotShownEmpty()
    {
        var details = ItemDetailsParser.Parse([Line(TextColour.Lime, "Talisman:")]);

        Assert.Equal("Talisman", details.Name);
        Assert.Null(details.DescriptionLine);
        Assert.Null(details.Condition);
        Assert.Null(details.Level);
        Assert.Null(details.Blessings);
        Assert.Null(details.SellSilver);
    }

    [Fact]
    public void Parse_NamesAContractAndKeepsItsText()
    {
        var details = ItemDetailsParser.Parse(
        [
            Line(TextColour.Yellow, "A contract. It reads:"),
            Line(TextColour.Yellow, " "),
            Line(TextColour.Green, "Clear the area of all enemies."),
            Line(TextColour.Yellow, " "),
            Line(TextColour.Green, "  + Enemies have more gold"),
            Line(TextColour.Green, "A contract for a mercenary."),
        ]);

        Assert.Equal("Contract", details.Name);
        Assert.Empty(details.Stats);
        Assert.Equal([2, 4, 5], details.EffectLines);
    }

    [Theory]
    [InlineData("3 more blessings. Next in 40 kills.", 3, 40)]
    [InlineData("1 more blessing. Next in 15 kills.", 1, 15)]
    public void Parse_ReadsBlessings(string text, int blessings, int kills)
    {
        var details = ItemDetailsParser.Parse(
            [Line(TextColour.Lime, "Godly Blade:"), Line(TextColour.Yellow, "Blessed."), Line(TextColour.Green, text)]);

        Assert.Equal(blessings, details.Blessings);
        Assert.Equal(kills, details.KillsToBlessing);
    }

    [Fact]
    public void Parse_ReadsEveryAppliedModAndTheWorstCondition()
    {
        var details = ItemDetailsParser.Parse(
        [
            Line(TextColour.Lime, "Old Axe:"),
            Line(TextColour.Yellow, "Heavy."),
            Line(TextColour.Purple, "Has been Whetstoned."),
            Line(TextColour.Purple, "Has been Soulstoned."),
            Line(TextColour.Purple, "Has been Catalyzed."),
            Line(TextColour.Purple, "Has been Enchanted."),
            Line(TextColour.Purple, "Has been Focused."),
            Line(TextColour.Purple, "Has been Augmented."),
            Line(TextColour.Purple, "Has been made easier to use."),
            Line(TextColour.Purple, "Has been Corrupted."),
            Line(TextColour.Purple, "Legacy item."),
            Line(TextColour.Orange, "Can be Enchanted."),
            Line(TextColour.Orange, "Can be Soulstoned."),
            Line(TextColour.Orange, "May be further enhanced by Osiris."),
            Line(TextColour.Red, "Cannot be Enchanted."),
            Line(TextColour.Yellow, "It is very old and battered."),
        ]);

        Assert.Equal(
            ItemModsApplied.Whetstoned | ItemModsApplied.Soulstoned | ItemModsApplied.Catalyzed
            | ItemModsApplied.Enchanted | ItemModsApplied.Focused | ItemModsApplied.Augmented | ItemModsApplied.Easier
            | ItemModsApplied.Corrupted | ItemModsApplied.Legacy,
            details.ModsApplied);
        Assert.Equal(
            ItemModsPossible.CanEnchant | ItemModsPossible.CanSoulstone | ItemModsPossible.CanOsiris
            | ItemModsPossible.CannotEnchant,
            details.ModsPossible);
        Assert.Equal(4, details.Condition);
    }

    [Fact]
    public void Parse_ReadsNegativeValues()
    {
        var details = ItemDetailsParser.Parse(
        [
            Line(TextColour.Lime, "Cursed Ring:"),
            Line(TextColour.Yellow, "Cold."),
            Line(TextColour.Orange, TableHeader),
            Line(TextColour.Yellow, PlainWithRequired("Agility", -3, -5, 10)),
            Line(TextColour.Yellow, WholePercent("Crit Bonus", -2, -4)),
        ]);

        Assert.Equal(
            [
                new ItemStat("Agility", ItemStatKind.Plain, -3, -5, 10),
                new ItemStat("Crit Bonus", ItemStatKind.Percent, -20, -40),
            ],
            details.Stats);
    }

    [Fact]
    public void Parse_EndsTheTableAtTheFirstRowItCannotRead()
    {
        var details = ItemDetailsParser.Parse(
        [
            Line(TextColour.Lime, "Strange Gem:"),
            Line(TextColour.Yellow, "Odd."),
            Line(TextColour.Orange, TableHeader),
            Line(TextColour.Yellow, Plain("Armor Value", 1, 1)),
            Line(TextColour.Yellow, "Hums quietly."),
            Line(TextColour.Yellow, Plain("Weapon Value", 2, 2)),
        ]);

        Assert.Single(details.Stats);
        Assert.Equal([4, 5], details.OtherLines);
    }

    [Theory]
    [InlineData("Steel Sword:", TextColour.Lime, true)]
    [InlineData("A contract. It reads:", TextColour.Yellow, true)]
    [InlineData("A finely crafted sword:", TextColour.Yellow, false)]
    [InlineData("Steel Sword", TextColour.Lime, false)]
    [InlineData(":", TextColour.Lime, false)]
    public void IsReplyStart_SpotsTheFirstLineOfALookReply(string text, TextColour font, bool start)
    {
        Assert.Equal(start, ItemDetailsParser.IsReplyStart(text, (int)font));
    }

    private static LogLine Line(TextColour font, string text) => new(text, (int)font);

    private static string Name(string name) => name.PadRight(16)[..16];

    private static string Signed(int value, int width) =>
        (value >= 0 ? "+" + Whole(value) : Whole(value)).PadLeft(width);

    private static string Whole(int value) => value.ToString(CultureInfo.InvariantCulture);

    private static string Adds(string name, int adds) => $"{Name(name)}    {Signed(adds, 2)}";

    private static string PlainWithRequired(string name, int passive, int active, int required) =>
        $"{Name(name)}   {Signed(passive, 4)}     {Signed(active, 4)}     {Whole(required).PadLeft(3)}";

    private static string Plain(string name, int passive, int active) =>
        $"{Name(name)}   {Signed(passive, 4)}     {Signed(active, 4)}";

    private static string WholePercent(string name, int passive, int active) =>
        $"{Name(name)}   {Signed(passive, 4)}%    {Signed(active, 4)}%";

    private static string TenthsPercent(string name, int passive, int passiveTenth, int active, int activeTenth) =>
        $"{Name(name)}  {Signed(passive, 2)}.{passiveTenth}%   {Signed(active, 2)}.{activeTenth}%";

    private static string Regen(string name, int total, int perSecond) =>
        $"{Name(name)}  {Signed(total, 4)} ({Signed(perSecond, 4)}/s)";

    private static string Seconds(string name, int seconds) => $"{Name(name)}   {Whole(seconds).PadLeft(4)} seconds";

    private static string Single(string name, int value) => $"{Name(name)}   {Whole(value).PadLeft(4)}";

    private static string Rank(string name, int rank, string rankName) =>
        $"{Name(name)}   {Whole(rank).PadLeft(4)} ({rankName})";
}
