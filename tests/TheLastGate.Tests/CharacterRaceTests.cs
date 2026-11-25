using TheLastGate.Characters;

namespace TheLastGate.Tests;

public class CharacterRaceTests
{
    [Theory]
    [InlineData(CharacterClass.Templar, Body.Masculine, 3)]
    [InlineData(CharacterClass.Mercenary, Body.Masculine, 2)]
    [InlineData(CharacterClass.Harakim, Body.Masculine, 4)]
    [InlineData(CharacterClass.Templar, Body.Feminine, 77)]
    [InlineData(CharacterClass.Mercenary, Body.Feminine, 76)]
    [InlineData(CharacterClass.Harakim, Body.Feminine, 78)]
    public void ToNumber_GivesTheMyV2StartingTemplate(CharacterClass characterClass, Body body, int template)
    {
        Assert.Equal(template, new CharacterRace(characterClass, body, GameMode.Hard, Season.Current).ToNumber());
    }

    [Theory]
    [InlineData(544, CharacterClass.ArchTemplar, Body.Masculine)]
    [InlineData(550, CharacterClass.ArchHarakim, Body.Feminine)]
    [InlineData(13, CharacterClass.Seyan, Body.Masculine)]
    [InlineData(552, CharacterClass.Warrior, Body.Feminine)]
    public void FromNumber_ReadsAMyV2CharactersClassAndBody(int template, CharacterClass characterClass, Body body)
    {
        Assert.Equal(
            new CharacterRace(characterClass, body, GameMode.Normal, Season.Legacy),
            CharacterRace.FromNumber(template));
    }

    [Theory]
    [MemberData(nameof(MyV2Classes))]
    public void FromNumber_ReversesToNumber(CharacterClass characterClass, Body body)
    {
        var race = new CharacterRace(characterClass, body, GameMode.Normal, Season.Legacy);

        Assert.Equal(race, CharacterRace.FromNumber(race.ToNumber()));
    }

    [Fact]
    public void FromNumber_TreatsAnUnknownTemplateAsAMasculineTemplar()
    {
        Assert.Equal(
            new CharacterRace(CharacterClass.Templar, Body.Masculine, GameMode.Normal, Season.Legacy),
            CharacterRace.FromNumber(99));
    }

    public static TheoryData<CharacterClass, Body> MyV2Classes()
    {
        var data = new TheoryData<CharacterClass, Body>();
        CharacterClass[] classes =
        [
            CharacterClass.Templar, CharacterClass.Mercenary, CharacterClass.Harakim, CharacterClass.ArchTemplar,
            CharacterClass.Warrior, CharacterClass.Seyan, CharacterClass.Sorcerer, CharacterClass.ArchHarakim,
        ];
        foreach (var characterClass in classes)
        {
            data.Add(characterClass, Body.Masculine);
            data.Add(characterClass, Body.Feminine);
        }

        return data;
    }
}
