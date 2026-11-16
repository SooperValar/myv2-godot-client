using TheLastGate.Cursors;
using TheLastGate.Items;

namespace TheLastGate.Tests;

public class HeldItemTests
{
    [Theory]
    [InlineData(MouseCursor.Drop, true)]
    [InlineData(MouseCursor.Swap, true)]
    [InlineData(MouseCursor.Use, true)]
    [InlineData(MouseCursor.None, false)]
    [InlineData(MouseCursor.Take, false)]
    [InlineData(MouseCursor.Give, false)]
    [InlineData(MouseCursor.Walk, false)]
    [InlineData(MouseCursor.Hit, false)]
    [InlineData(MouseCursor.Select, false)]
    [InlineData(MouseCursor.Trash, false)]
    public void IsHighlighted_WhenTheCursorWouldPutItSomewhere(MouseCursor cursor, bool highlighted)
    {
        Assert.Equal(highlighted, HeldItem.IsHighlighted(cursor));
    }
}
