using GdUnit4;
using TheLastGate.Assets;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Assets;

[TestSuite]
public class SpriteCacheTests
{
    [TestCase]
    public void TryGet_ReturnsAnAddedValue()
    {
        var cache = new SpriteCache<string>(100);
        cache.Add(1, "one", 10);

        AssertThat(cache.TryGet(1, out var value)).IsTrue();
        AssertThat(value).IsEqual("one");
        AssertThat(cache.TryGet(2, out _)).IsFalse();
    }

    [TestCase]
    public void Add_OverBudget_EvictsTheLeastRecentlyUsed()
    {
        var evicted = new List<string>();
        var cache = new SpriteCache<string>(20, evicted.Add);
        cache.Add(1, "one", 10);
        cache.Add(2, "two", 10);
        cache.TryGet(1, out _);

        cache.Add(3, "three", 10);

        AssertThat(evicted).ContainsExactly("two");
        AssertThat(cache.TryGet(1, out _)).IsTrue();
        AssertThat(cache.TryGet(2, out _)).IsFalse();
        AssertThat(cache.TryGet(3, out _)).IsTrue();
    }

    [TestCase]
    public void Add_LargerThanTheBudget_KeepsTheNewestValue()
    {
        var cache = new SpriteCache<string>(20);
        cache.Add(1, "one", 10);

        cache.Add(2, "huge", 50);

        AssertThat(cache.TryGet(1, out _)).IsFalse();
        AssertThat(cache.TryGet(2, out _)).IsTrue();
    }

    [TestCase]
    public void Add_RejectsASpriteAlreadyCached()
    {
        var cache = new SpriteCache<string>(100);
        cache.Add(1, "one", 10);

        AssertThrown(() => cache.Add(1, "again", 10)).IsInstanceOf<ArgumentException>();
    }

    [TestCase]
    public void Clear_EvictsEverythingAndFreesTheBudget()
    {
        var evicted = new List<string>();
        var cache = new SpriteCache<string>(20, evicted.Add);
        cache.Add(1, "one", 10);
        cache.Add(2, "two", 10);

        cache.Clear();
        cache.Add(3, "three", 10);
        cache.Add(4, "four", 10);

        AssertThat(evicted).ContainsExactlyInAnyOrder("one", "two");
        AssertThat(cache.TryGet(1, out _)).IsFalse();
        AssertThat(cache.TryGet(2, out _)).IsFalse();
        AssertThat(cache.TryGet(3, out _)).IsTrue();
        AssertThat(cache.TryGet(4, out _)).IsTrue();
    }
}
