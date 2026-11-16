using TheLastGate.Chat;

namespace TheLastGate.Tests;

public class TabCompletionTests
{
    [Fact]
    public void Complete_FinishesTheWordBeforeTheCaretWithAWordFromASentLine()
    {
        var completion = new TabCompletion();
        completion.Learn("hello world");

        Assert.Equal(("say hello ", 10), completion.Complete("say he", 6));
    }

    [Fact]
    public void Complete_PrefersTheMostRecentlySentWord()
    {
        var completion = new TabCompletion();
        completion.Learn("help");
        completion.Learn("hello");

        Assert.Equal(("hello ", 6), completion.Complete("he", 2));
    }

    [Fact]
    public void Complete_AgainTriesTheNextMatchingWord()
    {
        var completion = new TabCompletion();
        completion.Learn("help");
        completion.Learn("hello");
        var first = completion.Complete("he", 2)!.Value;

        Assert.Equal(("help ", 5), completion.Complete(first.Text, first.Caret));
    }

    [Fact]
    public void Complete_AfterTheLastMatchGoesBackToWhatWasTyped()
    {
        var completion = new TabCompletion();
        completion.Learn("hello");
        var first = completion.Complete("he", 2)!.Value;

        Assert.Equal(("he", 2), completion.Complete(first.Text, first.Caret));
        Assert.Equal(("hello ", 6), completion.Complete("he", 2));
    }

    [Fact]
    public void Complete_StartsOverOnceTheLineHasChanged()
    {
        var completion = new TabCompletion();
        completion.Learn("help");
        completion.Learn("hello");
        completion.Complete("he", 2);

        Assert.Equal(("say hello ", 10), completion.Complete("say he", 6));
    }

    [Fact]
    public void Complete_FindsNothingWithoutAMatchingWord()
    {
        var completion = new TabCompletion();
        completion.Learn("hello");

        Assert.Null(completion.Complete("wo", 2));
    }

    [Fact]
    public void Complete_FindsNothingWithoutAWordBeforeTheCaret()
    {
        var completion = new TabCompletion();
        completion.Learn("hello");

        Assert.Null(completion.Complete("say ", 4));
    }

    [Fact]
    public void Complete_NeedsAWordLongerThanWhatWasTyped()
    {
        var completion = new TabCompletion();
        completion.Learn("he");

        Assert.Null(completion.Complete("he", 2));
    }

    [Fact]
    public void Complete_MatchesCase()
    {
        var completion = new TabCompletion();
        completion.Learn("Hello");

        Assert.Null(completion.Complete("he", 2));
    }

    [Fact]
    public void Learn_CountsHashAsPartOfAWord()
    {
        var completion = new TabCompletion();
        completion.Learn("#autodepot on");

        Assert.Equal(("#autodepot ", 11), completion.Complete("#au", 3));
    }

    [Fact]
    public void Learn_SplitsWordsAtDigitsAndPunctuation()
    {
        var completion = new TabCompletion();
        completion.Learn("give 5gold, now");

        Assert.Equal(("gold ", 5), completion.Complete("go", 2));
    }

    [Fact]
    public void Learn_MovesARepeatedWordToTheFront()
    {
        var completion = new TabCompletion();
        completion.Learn("help");
        completion.Learn("hello");
        completion.Learn("help");

        Assert.Equal(("help ", 5), completion.Complete("he", 2));
    }
}
