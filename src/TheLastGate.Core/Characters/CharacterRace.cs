namespace TheLastGate.Characters;

// The MyV2 server's race number is its character template. It makes new characters only from the six starting
// templates and ignores game modes and seasons, which The Last Gate added above the template.
public readonly record struct CharacterRace(CharacterClass Class, Body Body, GameMode GameMode, Season Season)
{
    private const int GameModeFactor = 10000;
    private const int SeasonFactor = 100000;

    // Indexed by class, then body. The Last Gate's own classes have no MyV2 template.
    private const int NoTemplate = -1;

    private static readonly int[,] Templates =
    {
        { 3, 77 }, { 2, 76 }, { 4, 78 }, { 544, 549 }, { NoTemplate, NoTemplate }, { 547, 552 },
        { 13, 79 }, { 546, 551 }, { NoTemplate, NoTemplate }, { 545, 550 }, { NoTemplate, NoTemplate },
        { NoTemplate, NoTemplate },
    };

    public int ToNumber() => Templates[(int)Class, (int)Body];

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
