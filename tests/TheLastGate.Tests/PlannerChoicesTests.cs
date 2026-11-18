using TheLastGate.Planner;

namespace TheLastGate.Tests;

public sealed class PlannerChoicesTests : IDisposable
{
    private readonly string folder = Directory.CreateTempSubdirectory("planner-choices").FullName;

    private string FilePath => Path.Combine(folder, PlannerChoicesStore.FileName);

    public void Dispose() => Directory.Delete(folder, true);

    [Fact]
    public void AMissingFile_HasNoChoices()
    {
        var loaded = PlannerChoicesStore.Load(FilePath, 1234);

        Assert.Null(loaded.Weapon);
        Assert.Null(loaded.Offhand);
    }

    [Fact]
    public void SavedChoices_LoadBack()
    {
        Assert.True(PlannerChoicesStore.Save(FilePath, 1234, Choices(CombatWeapon.Axe, CombatOffhand.Shield)));

        var loaded = PlannerChoicesStore.Load(FilePath, 1234);

        Assert.Equal(CombatWeapon.Axe, loaded.Weapon);
        Assert.Equal(CombatOffhand.Shield, loaded.Offhand);
    }

    [Fact]
    public void Characters_AreKeptApart()
    {
        PlannerChoicesStore.Save(FilePath, 11, Choices(CombatWeapon.Dagger, CombatOffhand.DualWield));
        PlannerChoicesStore.Save(FilePath, 22, Choices(CombatWeapon.Staff, CombatOffhand.None));
        PlannerChoicesStore.Save(FilePath, 11, Choices(CombatWeapon.Sword, CombatOffhand.Shield));

        var first = PlannerChoicesStore.Load(FilePath, 11);
        var second = PlannerChoicesStore.Load(FilePath, 22);

        Assert.Equal(CombatWeapon.Sword, first.Weapon);
        Assert.Equal(CombatOffhand.Shield, first.Offhand);
        Assert.Equal(CombatWeapon.Staff, second.Weapon);
        Assert.Equal(CombatOffhand.None, second.Offhand);
        Assert.Null(PlannerChoicesStore.Load(FilePath, 33).Weapon);
    }

    [Fact]
    public void OutOfRangeValues_AreIgnored()
    {
        WriteWords(1, 1, 1234, (uint)Enum.GetValues<CombatWeapon>().Length, unchecked((uint)-5));

        var loaded = PlannerChoicesStore.Load(FilePath, 1234);

        Assert.Null(loaded.Weapon);
        Assert.Null(loaded.Offhand);
    }

    [Fact]
    public void ADamagedFile_IsReplaced()
    {
        File.WriteAllText(FilePath, "not a planner file");
        Assert.Null(PlannerChoicesStore.Load(FilePath, 1234).Weapon);

        Assert.True(PlannerChoicesStore.Save(FilePath, 1234, Choices(CombatWeapon.Spear, CombatOffhand.None)));

        var loaded = PlannerChoicesStore.Load(FilePath, 1234);
        Assert.Equal(CombatWeapon.Spear, loaded.Weapon);
        Assert.Equal(CombatOffhand.None, loaded.Offhand);
    }

    [Fact]
    public void ACharacterNotCreatedYet_IsNotSaved()
    {
        Assert.False(PlannerChoicesStore.Save(FilePath, 0, Choices(CombatWeapon.Axe, CombatOffhand.None)));
        Assert.Null(PlannerChoicesStore.Load(FilePath, 0).Weapon);
    }

    [Fact]
    public void SpellsAndCards_RoundTrip()
    {
        var saved = Choices(CombatWeapon.Sword, CombatOffhand.Shield);
        saved.SpellToggles = 0x5;
        saved.TarotCards = (1ul << 22) | (1ul << 43);

        Assert.True(PlannerChoicesStore.Save(FilePath, 7, saved));
        var loaded = PlannerChoicesStore.Load(FilePath, 7);

        Assert.Equal(CombatWeapon.Sword, loaded.Weapon);
        Assert.Equal(CombatOffhand.Shield, loaded.Offhand);
        Assert.Equal(0x5u, loaded.SpellToggles);
        Assert.Equal(saved.TarotCards, loaded.TarotCards);
    }

    [Fact]
    public void AFormatOneFile_KeepsWeaponAndOffhand()
    {
        WriteFormatOneFile();

        var loaded = PlannerChoicesStore.Load(FilePath, 7);

        Assert.Equal(CombatWeapon.Sword, loaded.Weapon);
        Assert.Equal(CombatOffhand.Shield, loaded.Offhand);
        Assert.Equal(0u, loaded.SpellToggles);
        Assert.Equal(0ul, loaded.TarotCards);
    }

    [Fact]
    public void SavingOverAFormatOneFile_KeepsItsCharacters()
    {
        WriteFormatOneFile();

        Assert.True(PlannerChoicesStore.Save(FilePath, 9, Choices(CombatWeapon.Axe, CombatOffhand.None)));

        var loaded = PlannerChoicesStore.Load(FilePath, 7);
        Assert.Equal(CombatWeapon.Sword, loaded.Weapon);
        Assert.Equal(CombatOffhand.Shield, loaded.Offhand);
        Assert.Equal(CombatWeapon.Axe, PlannerChoicesStore.Load(FilePath, 9).Weapon);
    }

    [Fact]
    public void Corruptors_RoundTrip()
    {
        var saved = Choices(CombatWeapon.Sword, CombatOffhand.Shield);
        saved.CorruptorTicks = 0x5;
        saved.CorruptorCounts[0] = 2;
        saved.CorruptorCounts[1] = 4;
        saved.CorruptorCounts[2] = 7;
        saved.CorruptorCounts[TreeEffects.CorruptorCount - 1] = 20;

        Assert.True(PlannerChoicesStore.Save(FilePath, 7, saved));
        var loaded = PlannerChoicesStore.Load(FilePath, 7);

        Assert.Equal(0x5u, loaded.CorruptorTicks);
        Assert.Equal(2, loaded.CorruptorCounts[0]);
        Assert.Equal(4, loaded.CorruptorCounts[1]);
        Assert.Equal(7, loaded.CorruptorCounts[2]);
        Assert.Equal(0, loaded.CorruptorCounts[3]);
        Assert.Equal(20, loaded.CorruptorCounts[TreeEffects.CorruptorCount - 1]);
    }

    [Fact]
    public void AFormatTwoFile_LoadsWithoutCorruptors()
    {
        WriteWords(2, 1, 7, (uint)CombatWeapon.Sword, (uint)CombatOffhand.Shield, 0x5, 0x400000, 0x800);

        var loaded = PlannerChoicesStore.Load(FilePath, 7);

        Assert.Equal(CombatWeapon.Sword, loaded.Weapon);
        Assert.Equal(CombatOffhand.Shield, loaded.Offhand);
        Assert.Equal(0x5u, loaded.SpellToggles);
        Assert.Equal((0x800ul << 32) | 0x400000, loaded.TarotCards);
        Assert.Equal(0u, loaded.CorruptorTicks);
        Assert.All(loaded.CorruptorCounts, count => Assert.Equal(0, count));
    }

    [Fact]
    public void CorruptorCounts_AreKeptBetweenOneTickedAnd20()
    {
        var saved = Choices(CombatWeapon.Sword, CombatOffhand.Shield);
        saved.CorruptorTicks = 0x7;
        saved.CorruptorCounts[0] = 0;
        saved.CorruptorCounts[1] = 21;
        saved.CorruptorCounts[2] = 5;
        saved.CorruptorCounts[3] = 30;

        Assert.True(PlannerChoicesStore.Save(FilePath, 7, saved));
        var loaded = PlannerChoicesStore.Load(FilePath, 7);

        Assert.Equal([1, 20, 5, 20], loaded.CorruptorCounts.Take(4));
    }

    [Fact]
    public void Items_RoundTrip()
    {
        var saved = Choices(CombatWeapon.Sword, CombatOffhand.Shield);
        saved.EnchantTicks = 0x41;
        saved.EnchantCounts[0] = 2;
        saved.EnchantCounts[6] = 5;
        saved.GearTicks = 0x100001;

        Assert.True(PlannerChoicesStore.Save(FilePath, 7, saved));
        var loaded = PlannerChoicesStore.Load(FilePath, 7);

        Assert.Equal(0x41u, loaded.EnchantTicks);
        Assert.Equal(2, loaded.EnchantCounts[0]);
        Assert.Equal(5, loaded.EnchantCounts[6]);
        Assert.Equal(0, loaded.EnchantCounts[1]);
        Assert.Equal(0x100001u, loaded.GearTicks);
    }

    [Fact]
    public void AFormatThreeFile_LoadsWithoutItems()
    {
        WriteWords(
            3,
            1,
            7,
            (uint)CombatWeapon.Sword,
            (uint)CombatOffhand.Shield,
            0x5,
            0x400000,
            0x800,
            0x3,
            0x0402,
            0,
            0,
            0,
            0,
            0,
            0);

        var loaded = PlannerChoicesStore.Load(FilePath, 7);

        Assert.Equal(CombatWeapon.Sword, loaded.Weapon);
        Assert.Equal(0x5u, loaded.SpellToggles);
        Assert.Equal(0x3u, loaded.CorruptorTicks);
        Assert.Equal(2, loaded.CorruptorCounts[0]);
        Assert.Equal(4, loaded.CorruptorCounts[1]);
        Assert.Equal(0u, loaded.EnchantTicks);
        Assert.Equal(0u, loaded.GearTicks);
        Assert.All(loaded.EnchantCounts, count => Assert.Equal(0, count));
    }

    [Fact]
    public void EnchantCounts_AreKeptBetweenOneTickedAnd20()
    {
        var saved = Choices(CombatWeapon.Sword, CombatOffhand.Shield);
        saved.EnchantTicks = 0x7;
        saved.EnchantCounts[0] = 0;
        saved.EnchantCounts[1] = 21;
        saved.EnchantCounts[2] = 5;
        saved.EnchantCounts[3] = 30;

        Assert.True(PlannerChoicesStore.Save(FilePath, 7, saved));
        var loaded = PlannerChoicesStore.Load(FilePath, 7);

        Assert.Equal([1, 20, 5, 20], loaded.EnchantCounts.Take(4));
    }

    [Fact]
    public void AFullFile_DropsTheOldestCharacter()
    {
        for (uint character = 1; character <= 256; character++)
        {
            PlannerChoicesStore.Save(FilePath, character, Choices(CombatWeapon.Axe, CombatOffhand.None));
        }

        PlannerChoicesStore.Save(FilePath, 1000, Choices(CombatWeapon.Staff, CombatOffhand.None));

        Assert.Null(PlannerChoicesStore.Load(FilePath, 1).Weapon);
        Assert.Equal(CombatWeapon.Axe, PlannerChoicesStore.Load(FilePath, 2).Weapon);
        Assert.Equal(CombatWeapon.Staff, PlannerChoicesStore.Load(FilePath, 1000).Weapon);
    }

    [Fact]
    public void TheFile_IsTheCurrentClientsLayout()
    {
        PlannerChoicesStore.Save(FilePath, 7180, Choices(CombatWeapon.Axe, CombatOffhand.None));

        var bytes = File.ReadAllBytes(FilePath);

        Assert.Equal(88, bytes.Length);
        Assert.Equal([4u, 1u, 7180u, (uint)CombatWeapon.Axe, 0u], Words(bytes).Take(5));
    }

    private static PlannerChoices Choices(CombatWeapon weapon, CombatOffhand offhand) =>
        new() { Weapon = weapon, Offhand = offhand };

    private static IEnumerable<uint> Words(byte[] bytes) =>
        Enumerable.Range(0, bytes.Length / 4).Select(index => BitConverter.ToUInt32(bytes, index * 4));

    private void WriteFormatOneFile() => WriteWords(1, 1, 7, (uint)CombatWeapon.Sword, (uint)CombatOffhand.Shield);

    private void WriteWords(params uint[] words) =>
        File.WriteAllBytes(FilePath, words.SelectMany(BitConverter.GetBytes).ToArray());
}
