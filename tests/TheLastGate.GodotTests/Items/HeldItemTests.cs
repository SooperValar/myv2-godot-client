using GdUnit4;
using TheLastGate.Cursors;
using TheLastGate.Items;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Items;

[TestSuite]
public class HeldItemTests
{
    [TestCase(MouseCursor.Drop, true)]
    [TestCase(MouseCursor.Swap, true)]
    [TestCase(MouseCursor.Use, true)]
    [TestCase(MouseCursor.None, false)]
    [TestCase(MouseCursor.Take, false)]
    [TestCase(MouseCursor.Give, false)]
    [TestCase(MouseCursor.Walk, false)]
    [TestCase(MouseCursor.Hit, false)]
    [TestCase(MouseCursor.Select, false)]
    [TestCase(MouseCursor.Trash, false)]
    public void IsHighlighted_WhenTheCursorWouldPutItSomewhere(MouseCursor cursor, bool highlighted)
    {
        AssertThat(HeldItem.IsHighlighted(cursor)).IsEqual(highlighted);
    }
}
