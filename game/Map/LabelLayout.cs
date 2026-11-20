using System;
using System.Collections.Generic;

namespace TheLastGate.Map;

public readonly record struct LabelRect(float Left, float Top, float Right, float Bottom)
{
    // Rectangles that only touch along an edge leave each other free.
    public bool Overlaps(LabelRect other) =>
        Left < other.Right && other.Left < Right && Top < other.Bottom && other.Top < Bottom;
}

// Finds room for names beside markers without covering anything already placed.
public sealed class LabelLayout(int capacity = LabelLayout.DefaultCapacity)
{
    public const int DefaultCapacity = 4096;

    private readonly List<LabelRect> occupied = [];

    public int Count => occupied.Count;

    public bool Reserve(LabelRect rect)
    {
        if (occupied.Count >= capacity)
        {
            return false;
        }

        occupied.Add(rect);
        return true;
    }

    // Right and left of the anchor read best, then the four diagonals, then above and below. A diagonal spot tucks a
    // corner in by half the clearance on each axis, which clears a diamond marker, plus the marker's own pixel.
    public LabelRect? PlaceBeside(float anchorX, float anchorY, float clearance, float width, float height)
    {
        if (occupied.Count >= capacity)
        {
            return null;
        }

        var diagonal = MathF.Ceiling(clearance / 2) + 1;
        var centredX = anchorX - width / 2;
        var centredY = anchorY - height / 2;
        (float Left, float Top)[] spots =
        [
            (anchorX + clearance, centredY),
            (anchorX - clearance - width, centredY),
            (anchorX + diagonal, anchorY - diagonal - height),
            (anchorX + diagonal, anchorY + diagonal),
            (anchorX - diagonal - width, anchorY - diagonal - height),
            (anchorX - diagonal - width, anchorY + diagonal),
            (centredX, anchorY - clearance - height),
            (centredX, anchorY + clearance),
        ];
        foreach (var (spotLeft, spotTop) in spots)
        {
            var left = MathF.Floor(spotLeft);
            var top = MathF.Floor(spotTop);
            var candidate = new LabelRect(left, top, left + width, top + height);
            if (!occupied.Exists(taken => taken.Overlaps(candidate)))
            {
                occupied.Add(candidate);
                return candidate;
            }
        }

        return null;
    }
}
