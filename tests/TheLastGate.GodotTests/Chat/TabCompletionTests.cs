using GdUnit4;
using TheLastGate.Chat;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Chat;

[TestSuite]
public class TabCompletionTests
{
    [TestCase]
    public void Complete_FinishesTheWordBeforeTheCaretWithAWordFromASentLine()
    {
        var completion = new TabCompletion();
        completion.Learn("hello world");

        AssertThat(completion.Complete("say he", 6)).IsEqual(("say hello ", 10));
    }

    [TestCase]
    public void Complete_PrefersTheMostRecentlySentWord()
    {
        var completion = new TabCompletion();
        completion.Learn("help");
        completion.Learn("hello");

        AssertThat(completion.Complete("he", 2)).IsEqual(("hello ", 6));
    }

    [TestCase]
    public void Complete_AgainTriesTheNextMatchingWord()
    {
        var completion = new TabCompletion();
        completion.Learn("help");
        completion.Learn("hello");
        var first = completion.Complete("he", 2)!.Value;

        AssertThat(completion.Complete(first.Text, first.Caret)).IsEqual(("help ", 5));
    }

    [TestCase]
    public void Complete_AfterTheLastMatchGoesBackToWhatWasTyped()
    {
        var completion = new TabCompletion();
        completion.Learn("hello");
        var first = completion.Complete("he", 2)!.Value;

        AssertThat(completion.Complete(first.Text, first.Caret)).IsEqual(("he", 2));
        AssertThat(completion.Complete("he", 2)).IsEqual(("hello ", 6));
    }

    [TestCase]
    public void Complete_StartsOverOnceTheLineHasChanged()
    {
        var completion = new TabCompletion();
        completion.Learn("help");
        completion.Learn("hello");
        completion.Complete("he", 2);

        AssertThat(completion.Complete("say he", 6)).IsEqual(("say hello ", 10));
    }

    [TestCase]
    public void Complete_FindsNothingWithoutAMatchingWord()
    {
        var completion = new TabCompletion();
        completion.Learn("hello");

        AssertThat(completion.Complete("wo", 2)).IsNull();
    }

    [TestCase]
    public void Complete_FindsNothingWithoutAWordBeforeTheCaret()
    {
        var completion = new TabCompletion();
        completion.Learn("hello");

        AssertThat(completion.Complete("say ", 4)).IsNull();
    }

    [TestCase]
    public void Complete_NeedsAWordLongerThanWhatWasTyped()
    {
        var completion = new TabCompletion();
        completion.Learn("he");

        AssertThat(completion.Complete("he", 2)).IsNull();
    }

    [TestCase]
    public void Complete_MatchesCase()
    {
        var completion = new TabCompletion();
        completion.Learn("Hello");

        AssertThat(completion.Complete("he", 2)).IsNull();
    }

    [TestCase]
    public void Learn_CountsHashAsPartOfAWord()
    {
        var completion = new TabCompletion();
        completion.Learn("#autodepot on");

        AssertThat(completion.Complete("#au", 3)).IsEqual(("#autodepot ", 11));
    }

    [TestCase]
    public void Learn_SplitsWordsAtDigitsAndPunctuation()
    {
        var completion = new TabCompletion();
        completion.Learn("give 5gold, now");

        AssertThat(completion.Complete("go", 2)).IsEqual(("gold ", 5));
    }

    [TestCase]
    public void Learn_MovesARepeatedWordToTheFront()
    {
        var completion = new TabCompletion();
        completion.Learn("help");
        completion.Learn("hello");
        completion.Learn("help");

        AssertThat(completion.Complete("he", 2)).IsEqual(("help ", 5));
    }
}
