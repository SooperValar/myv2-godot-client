using TheLastGate.Characters;

namespace TheLastGate.Tests;

public class CharacterRaceTests
{
    [Fact]
    public void ToNumber_AddsGameModeAndSeasonToTheTemplate()
    {
        Assert.Equal(
            4,
            new CharacterRace(CharacterClass.Templar, Body.Masculine, GameMode.Normal, Season.Legacy).ToNumber());
        Assert.Equal(
            110009,
            new CharacterRace(CharacterClass.Harakim, Body.Feminine, GameMode.Hard, Season.Current).ToNumber());
    }

    [Fact]
    public void FromNumber_ReadsClassBodyGameModeAndSeason()
    {
        Assert.Equal(
            new CharacterRace(CharacterClass.Harakim, Body.Feminine, GameMode.Hard, Season.Current),
            CharacterRace.FromNumber(110009));
    }

    [Theory]
    [MemberData(nameof(ClassesWithBothBodies))]
    public void FromNumber_ReversesToNumber(CharacterClass characterClass, Body body)
    {
        var race = new CharacterRace(characterClass, body, GameMode.Roguelike, Season.Current);

        Assert.Equal(race, CharacterRace.FromNumber(race.ToNumber()));
    }

    [Fact]
    public void FromNumber_GivesLycanthropesTheMasculineBody()
    {
        var race = new CharacterRace(CharacterClass.Lycanthrope, Body.Feminine, GameMode.Normal, Season.Legacy);

        Assert.Equal(Body.Masculine, CharacterRace.FromNumber(race.ToNumber()).Body);
    }

    [Fact]
    public void FromNumber_TreatsAnUnknownTemplateAsAMasculineTemplar()
    {
        Assert.Equal(
            new CharacterRace(CharacterClass.Templar, Body.Masculine, GameMode.Hard, Season.Legacy),
            CharacterRace.FromNumber(10099));
    }

    public static TheoryData<CharacterClass, Body> ClassesWithBothBodies()
    {
        var data = new TheoryData<CharacterClass, Body>();
        foreach (var characterClass in Enum.GetValues<CharacterClass>()
            .Where(value => value != CharacterClass.Lycanthrope))
        {
            data.Add(characterClass, Body.Masculine);
            data.Add(characterClass, Body.Feminine);
        }

        return data;
    }
}
