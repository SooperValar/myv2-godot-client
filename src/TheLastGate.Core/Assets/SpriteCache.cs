using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TheLastGate.Assets;

public sealed class SpriteCache<TTexture>(long budgetBytes, Action<TTexture>? onEvicted = null)
{
    private readonly Dictionary<int, LinkedListNode<Entry>> nodesBySprite = new();
    private readonly LinkedList<Entry> recencyOrder = new();
    private long cachedBytes;

    public bool TryGet(int spriteNumber, [MaybeNullWhen(false)] out TTexture texture)
    {
        if (!nodesBySprite.TryGetValue(spriteNumber, out var node))
        {
            texture = default;
            return false;
        }

        recencyOrder.Remove(node);
        recencyOrder.AddFirst(node);
        texture = node.Value.Texture;
        return true;
    }

    public void Add(int spriteNumber, TTexture texture, long byteSize)
    {
        if (nodesBySprite.ContainsKey(spriteNumber))
        {
            throw new ArgumentException($"Sprite {spriteNumber} is already cached.", nameof(spriteNumber));
        }

        nodesBySprite[spriteNumber] = recencyOrder.AddFirst(new Entry(spriteNumber, texture, byteSize));
        cachedBytes += byteSize;
        while (cachedBytes > budgetBytes && recencyOrder.Count > 1)
        {
            Evict(recencyOrder.Last!);
        }
    }

    public void Clear()
    {
        while (recencyOrder.Last != null)
        {
            Evict(recencyOrder.Last);
        }
    }

    private void Evict(LinkedListNode<Entry> node)
    {
        recencyOrder.Remove(node);
        nodesBySprite.Remove(node.Value.SpriteNumber);
        cachedBytes -= node.Value.ByteSize;
        onEvicted?.Invoke(node.Value.Texture);
    }

    private sealed record Entry(int SpriteNumber, TTexture Texture, long ByteSize);
}
