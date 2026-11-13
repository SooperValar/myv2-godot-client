using TheLastGate.Assets;

namespace TheLastGate.Tests;

public class SpriteCacheTests
{
    [Fact]
    public void TryGet_ReturnsAnAddedValue()
    {
        var cache = new SpriteCache<string>(100);
        cache.Add(1, "one", 10);

        Assert.True(cache.TryGet(1, out var value));
        Assert.Equal("one", value);
        Assert.False(cache.TryGet(2, out _));
    }

    [Fact]
    public void Add_OverBudget_EvictsTheLeastRecentlyUsed()
    {
        var evicted = new List<string>();
        var cache = new SpriteCache<string>(20, evicted.Add);
        cache.Add(1, "one", 10);
        cache.Add(2, "two", 10);
        cache.TryGet(1, out _);

        cache.Add(3, "three", 10);

        Assert.Equal(["two"], evicted);
        Assert.True(cache.TryGet(1, out _));
        Assert.False(cache.TryGet(2, out _));
        Assert.True(cache.TryGet(3, out _));
    }

    [Fact]
    public void Add_LargerThanTheBudget_KeepsTheNewestValue()
    {
        var cache = new SpriteCache<string>(20);
        cache.Add(1, "one", 10);

        cache.Add(2, "huge", 50);

        Assert.False(cache.TryGet(1, out _));
        Assert.True(cache.TryGet(2, out _));
    }

    [Fact]
    public void Add_RejectsASpriteAlreadyCached()
    {
        var cache = new SpriteCache<string>(100);
        cache.Add(1, "one", 10);

        Assert.Throws<ArgumentException>(() => cache.Add(1, "again", 10));
    }

    [Fact]
    public void Clear_EvictsEverythingAndFreesTheBudget()
    {
        var evicted = new List<string>();
        var cache = new SpriteCache<string>(20, evicted.Add);
        cache.Add(1, "one", 10);
        cache.Add(2, "two", 10);

        cache.Clear();
        cache.Add(3, "three", 10);
        cache.Add(4, "four", 10);

        Assert.Equal(["one", "two"], evicted.Order());
        Assert.False(cache.TryGet(1, out _));
        Assert.False(cache.TryGet(2, out _));
        Assert.True(cache.TryGet(3, out _));
        Assert.True(cache.TryGet(4, out _));
    }
}
