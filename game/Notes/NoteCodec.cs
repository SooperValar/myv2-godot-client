using System;
using System.Collections.Generic;
using System.Text;

namespace TheLastGate.Notes;

public enum NoteDecodeResult
{
    Decoded,
    Damaged,
    TooLarge,
}

// The current client's encoding, shared by its notes file and its share codes. A category body is a length-prefixed
// UTF-8 name, a colour byte and a varint note count; each note is a kind byte, a length-prefixed label, a colour byte
// (0xFF for the category's, in tree encodings only) and either a point or a varint segment count and two points per
// segment. Points are zigzag varint steps from the previous point in the same category, starting from (0, 0). A tree
// is a varint category count, then for each category in parent-first order a varint parent reference (0 for the top,
// otherwise 1 + the earlier category's position), a flags byte (1 visible, 2 labels shown) and its body.
public static class NoteCodec
{
    private const byte CategoryColourByte = 0xFF;
    private const byte VisibleFlag = 0x01;
    private const byte ShowLabelsFlag = 0x02;
    private const int VarintBitLimit = 35;
    private const string NamelessCategory = "Imported";

    public static void WriteVarint(List<byte> bytes, uint value)
    {
        while (value >= 0x80)
        {
            bytes.Add((byte)(value | 0x80));
            value >>= 7;
        }

        bytes.Add((byte)value);
    }

    public static bool TryReadVarint(ReadOnlySpan<byte> bytes, ref int offset, out uint value)
    {
        uint result = 0;
        for (var shift = 0; shift < VarintBitLimit && offset < bytes.Length; shift += 7)
        {
            var next = bytes[offset++];
            result |= (uint)(next & 0x7F) << shift;
            if ((next & 0x80) == 0)
            {
                value = result;
                return true;
            }
        }

        value = 0;
        return false;
    }

    public static void EncodeCategory(NoteCategory category, List<byte> bytes) => WriteBody(category, bytes, false);

    public static NoteDecodeResult DecodeCategory(
        ReadOnlySpan<byte> bytes,
        ref int offset,
        out NoteCategory? category)
    {
        var result = ReadBody(bytes, ref offset, false, out var body);
        category = result == NoteDecodeResult.Decoded
            ? NoteCategory.Detached(body.Name, body.ColourIndex, true, true, body.Notes)
            : null;
        return result;
    }

    public static bool EncodeTree(NoteSet set, int rootId, List<byte> bytes)
    {
        var ordered = set.Tree(rootId);
        if (rootId != 0 && ordered.Count == 0)
        {
            return false;
        }

        WriteVarint(bytes, (uint)ordered.Count);
        for (var position = 0; position < ordered.Count; position++)
        {
            var category = ordered[position];
            WriteVarint(bytes, ParentReference(ordered, position));
            bytes.Add((byte)((category.Visible ? VisibleFlag : 0) | (category.ShowLabels ? ShowLabelsFlag : 0)));
            WriteBody(category, bytes, true);
        }

        return true;
    }

    public static NoteDecodeResult DecodeTree(
        ReadOnlySpan<byte> bytes,
        ref int offset,
        bool singleRoot,
        out NoteSet? tree)
    {
        tree = null;
        if (!TryReadVarint(bytes, ref offset, out var categoryCount))
        {
            return NoteDecodeResult.Damaged;
        }

        if (categoryCount > NoteLimits.Categories)
        {
            return NoteDecodeResult.TooLarge;
        }

        if (singleRoot && categoryCount == 0)
        {
            return NoteDecodeResult.Damaged;
        }

        var decoded = new NoteSet();
        var assignedIds = new int[categoryCount];
        var roots = 0;
        for (var index = 0; index < categoryCount; index++)
        {
            if (!TryReadVarint(bytes, ref offset, out var parentReference) || offset >= bytes.Length)
            {
                return NoteDecodeResult.Damaged;
            }

            var flags = bytes[offset++];

            // References are 1-based, so anything above the current index points at this category or a later one.
            if (parentReference > index || (parentReference == 0 && ++roots > 1 && singleRoot))
            {
                return NoteDecodeResult.Damaged;
            }

            var result = ReadBody(bytes, ref offset, true, out var body);
            if (result != NoteDecodeResult.Decoded)
            {
                return result;
            }

            var category = NoteCategory.Detached(
                body.Name,
                body.ColourIndex,
                (flags & VisibleFlag) != 0,
                (flags & ShowLabelsFlag) != 0,
                body.Notes);
            var parentId = parentReference == 0 ? 0 : assignedIds[parentReference - 1];
            assignedIds[index] = decoded.Attach(category, parentId);
            if (assignedIds[index] == 0)
            {
                return NoteDecodeResult.Damaged;
            }
        }

        decoded.Changed = false;
        tree = decoded;
        return NoteDecodeResult.Decoded;
    }

    private static uint ParentReference(IReadOnlyList<NoteCategory> ordered, int position)
    {
        for (var earlier = position - 1; earlier >= 0; earlier--)
        {
            if (ordered[earlier].Id == ordered[position].ParentId)
            {
                return (uint)earlier + 1;
            }
        }

        return 0;
    }

    private static uint Zigzag(int value) => (uint)((value << 1) ^ (value >> 31));

    private static int Unzigzag(uint value) => (int)(value >> 1) ^ -(int)(value & 1);

    private static void WriteText(List<byte> bytes, string text)
    {
        var utf8 = Encoding.UTF8.GetBytes(text);
        bytes.Add((byte)utf8.Length);
        bytes.AddRange(utf8);
    }

    private static void WritePoint(List<byte> bytes, ref (int X, int Y) cursor, int tileX, int tileY)
    {
        WriteVarint(bytes, Zigzag(tileX - cursor.X));
        WriteVarint(bytes, Zigzag(tileY - cursor.Y));
        cursor = (tileX, tileY);
    }

    private static void WriteBody(NoteCategory category, List<byte> bytes, bool withColours)
    {
        var cursor = (0, 0);
        WriteText(bytes, category.Name);
        bytes.Add((byte)category.ColourIndex);
        WriteVarint(bytes, (uint)category.Notes.Count);
        foreach (var note in category.Notes)
        {
            bytes.Add((byte)note.Kind);
            WriteText(bytes, note.Label);
            if (withColours)
            {
                bytes.Add(
                    note.ColourIndex is >= 0 and < NotePalette.Size ? (byte)note.ColourIndex : CategoryColourByte);
            }

            if (note.Kind != NoteKind.Path)
            {
                WritePoint(bytes, ref cursor, note.TileX, note.TileY);
                continue;
            }

            WriteVarint(bytes, (uint)note.Segments.Count);
            foreach (var segment in note.Segments)
            {
                WritePoint(bytes, ref cursor, segment.StartX, segment.StartY);
                WritePoint(bytes, ref cursor, segment.EndX, segment.EndY);
            }
        }
    }

    private static bool TryReadText(ReadOnlySpan<byte> bytes, ref int offset, int maxBytes, out string text)
    {
        text = "";
        if (offset >= bytes.Length)
        {
            return false;
        }

        var length = bytes[offset++];
        if (length > maxBytes || bytes.Length - offset < length)
        {
            return false;
        }

        text = Encoding.UTF8.GetString(bytes.Slice(offset, length));
        offset += length;
        return true;
    }

    // A step may cross the whole map but must land on it.
    private static bool TryReadCoordinate(ReadOnlySpan<byte> bytes, ref int offset, ref int cursor)
    {
        if (!TryReadVarint(bytes, ref offset, out var encoded))
        {
            return false;
        }

        var step = Unzigzag(encoded);
        var position = cursor + step;
        if (step is < -NoteLimits.Tiles or > NoteLimits.Tiles || position is < 0 or >= NoteLimits.Tiles)
        {
            return false;
        }

        cursor = position;
        return true;
    }

    private static bool TryReadPoint(ReadOnlySpan<byte> bytes, ref int offset, ref (int X, int Y) cursor) =>
        TryReadCoordinate(bytes, ref offset, ref cursor.X) && TryReadCoordinate(bytes, ref offset, ref cursor.Y);

    private static NoteDecodeResult ReadBody(
        ReadOnlySpan<byte> bytes,
        ref int offset,
        bool withColours,
        out (string Name, int ColourIndex, List<Note> Notes) body)
    {
        body = ("", 0, new List<Note>());
        if (!TryReadText(bytes, ref offset, NoteLimits.CategoryNameBytes, out var name)
            || offset >= bytes.Length)
        {
            return NoteDecodeResult.Damaged;
        }

        var colourIndex = bytes[offset++];
        if (!TryReadVarint(bytes, ref offset, out var noteCount))
        {
            return NoteDecodeResult.Damaged;
        }

        if (noteCount > NoteLimits.NotesPerCategory)
        {
            return NoteDecodeResult.TooLarge;
        }

        var cursor = (0, 0);
        var segmentTotal = 0;
        for (var index = 0; index < noteCount; index++)
        {
            var result = ReadNote(bytes, ref offset, withColours, ref cursor, out var note);
            if (result != NoteDecodeResult.Decoded)
            {
                return result;
            }

            segmentTotal += note!.Segments.Count;
            if (segmentTotal > NoteLimits.SegmentsPerCategory)
            {
                return NoteDecodeResult.TooLarge;
            }

            body.Notes.Add(note);
        }

        body.Name = name.Length == 0 ? NamelessCategory : name;
        body.ColourIndex = colourIndex < NotePalette.Size ? colourIndex : 0;
        return NoteDecodeResult.Decoded;
    }

    private static NoteDecodeResult ReadNote(
        ReadOnlySpan<byte> bytes,
        ref int offset,
        bool withColours,
        ref (int X, int Y) cursor,
        out Note? note)
    {
        note = null;
        if (offset >= bytes.Length || bytes[offset] is < (byte)NoteKind.Pin or > (byte)NoteKind.Path)
        {
            return NoteDecodeResult.Damaged;
        }

        var kind = (NoteKind)bytes[offset++];
        if (!TryReadText(bytes, ref offset, NoteLimits.LabelBytes, out var label))
        {
            return NoteDecodeResult.Damaged;
        }

        var colourIndex = Note.CategoryColour;
        if (withColours)
        {
            if (offset >= bytes.Length)
            {
                return NoteDecodeResult.Damaged;
            }

            colourIndex = bytes[offset] < NotePalette.Size ? bytes[offset] : Note.CategoryColour;
            offset++;
        }

        if (kind != NoteKind.Path)
        {
            if ((kind == NoteKind.Text && label.Length == 0) || !TryReadPoint(bytes, ref offset, ref cursor))
            {
                return NoteDecodeResult.Damaged;
            }

            note = new Note(kind, label, cursor.X, cursor.Y, [], colourIndex);
            return NoteDecodeResult.Decoded;
        }

        if (!TryReadVarint(bytes, ref offset, out var segmentCount))
        {
            return NoteDecodeResult.Damaged;
        }

        if (segmentCount > NoteLimits.SegmentsPerPath)
        {
            return NoteDecodeResult.TooLarge;
        }

        if (segmentCount == 0)
        {
            return NoteDecodeResult.Damaged;
        }

        var segments = new PathSegment[segmentCount];
        for (var index = 0; index < segmentCount; index++)
        {
            if (!TryReadPoint(bytes, ref offset, ref cursor))
            {
                return NoteDecodeResult.Damaged;
            }

            var start = cursor;
            if (!TryReadPoint(bytes, ref offset, ref cursor))
            {
                return NoteDecodeResult.Damaged;
            }

            segments[index] = new PathSegment(start.X, start.Y, cursor.X, cursor.Y);
        }

        note = new Note(kind, label, segments[0].StartX, segments[0].StartY, segments, colourIndex);
        return NoteDecodeResult.Decoded;
    }
}
