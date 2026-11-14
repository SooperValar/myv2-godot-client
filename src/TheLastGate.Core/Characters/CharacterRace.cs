namespace TheLastGate.Characters;

// The server's race number is template + game mode * 10000 + season * 100000.
public readonly record struct CharacterRace(CharacterClass Class, Body Body, GameMode GameMode, Season Season)
{
    private const int GameModeFactor = 10000;
    private const int SeasonFactor = 100000;

    // Indexed by class, then body. Both Lycanthrope bodies share one template.
    private static readonly int[,] Templates =
    {
        { 4, 5 }, { 6, 7 }, { 8, 9 }, { 12, 13 }, { 14, 15 }, { 16, 17 },
        { 10, 11 }, { 18, 19 }, { 20, 21 }, { 22, 23 }, { 2, 3 }, { 1554, 1554 },
    };

    public int ToNumber() =>
        Templates[(int)Class, (int)Body] + (int)GameMode * GameModeFactor + (int)Season * SeasonFactor;

    // A template that isn't listed is read as a masculine Templar.
    public static CharacterRace FromNumber(int number)
    {
        var template = number % GameModeFactor;
        var gameMode = (GameMode)(number / GameModeFactor % 10);
        var season = (Season)(number / SeasonFactor);
        for (var classIndex = 0; classIndex < Templates.GetLength(0); classIndex++)
        {
            for (var bodyIndex = 0; bodyIndex < Templates.GetLength(1); bodyIndex++)
            {
                if (Templates[classIndex, bodyIndex] == template)
                {
                    return new CharacterRace((CharacterClass)classIndex, (Body)bodyIndex, gameMode, season);
                }
            }
        }

        return new CharacterRace(CharacterClass.Templar, Body.Masculine, gameMode, season);
    }
}
