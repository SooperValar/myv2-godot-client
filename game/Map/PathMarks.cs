using System;
using System.Collections.Generic;
using System.Numerics;

namespace TheLastGate.Map;

// The dashes of a path being drawn and the arrowheads that show which way a finished path runs.
public static class PathMarks
{
    private const float DashLength = 6;
    private const float DashGap = 4;
    private const float ChevronSpacing = 48;
    private const float ChevronSize = 3;

    public static IReadOnlyList<(Vector2 Start, Vector2 End)> Dashes(Vector2 start, Vector2 end)
    {
        var length = Vector2.Distance(start, end);
        if (length < 1)
        {
            return [];
        }

        var direction = (end - start) / length;
        var dashStep = DashLength + DashGap;
        var dashCount = (int)MathF.Ceiling(length / dashStep);
        var dashes = new List<(Vector2, Vector2)>(dashCount);
        for (var dashIndex = 0; dashIndex < dashCount; dashIndex++)
        {
            var distance = dashIndex * dashStep;
            var dashEnd = MathF.Min(distance + DashLength, length);
            dashes.Add((start + direction * distance, start + direction * dashEnd));
        }

        return dashes;
    }

    // The first sits half a spacing in, and none is cut off by the segment's end.
    public static IReadOnlyList<(Vector2 First, Vector2 Tip, Vector2 Last)> Chevrons(Vector2 start, Vector2 end)
    {
        var length = Vector2.Distance(start, end);
        if (length < ChevronSpacing)
        {
            return [];
        }

        var along = (end - start) / length;
        var across = new Vector2(-along.Y, along.X);
        var firstDistance = ChevronSpacing / 2;
        var chevronCount = (int)MathF.Ceiling((length - ChevronSize - firstDistance) / ChevronSpacing);
        var chevrons = new List<(Vector2, Vector2, Vector2)>(chevronCount);
        for (var chevronIndex = 0; chevronIndex < chevronCount; chevronIndex++)
        {
            var middle = start + along * (firstDistance + chevronIndex * ChevronSpacing);
            var back = middle - along * ChevronSize;
            chevrons.Add((back + across * ChevronSize, middle + along * ChevronSize, back - across * ChevronSize));
        }

        return chevrons;
    }
}
