using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace TheLastGate.Notes;

public enum NoteCodeStatus
{
    Ok,
    NotACode,
    Damaged,
    TooLarge,
}

// A category and its subcategories as text to paste: "TLGMAP2:" and the base64 of the zlib-compressed tree. The
// current client's first codes, "TLGMAP1:", held one category without subcategories or note colours.
public static class NoteShareCode
{
    public const int DiscordLimit = 2000;
    public const string TooLongForDiscord = "Too long for one Discord message; it will still work on the wiki.";

    private const string Prefix = "TLGMAP2:";
    private const string FirstPrefix = "TLGMAP1:";
    private const int FirstLimit = 256 * 1024;
    private const int TreeLimit = 16 * 1024 * 1024;
    private const int AdlerModulus = 65521;
    private const int ZlibHeaderSize = 2;
    private const int AdlerSize = 4;

    public static bool HasPrefix(string text) =>
        text.StartsWith(Prefix, StringComparison.Ordinal) || text.StartsWith(FirstPrefix, StringComparison.Ordinal);

    public static string? Export(NoteSet set, int categoryId)
    {
        var tree = new List<byte>();
        if (categoryId == 0 || !NoteCodec.EncodeTree(set, categoryId, tree))
        {
            return null;
        }

        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.SmallestSize, true))
        {
            zlib.Write(tree.ToArray());
        }

        return Prefix + Convert.ToBase64String(compressed.GetBuffer(), 0, (int)compressed.Length);
    }

    // Spaces and line breaks are ignored, so a code broken up by a chat window still imports.
    public static NoteCodeStatus Import(string code, out NoteSet? imported)
    {
        imported = null;
        var stripped = new string(code.Where(character => !char.IsWhiteSpace(character)).ToArray());
        var isFirstVersion = stripped.StartsWith(FirstPrefix, StringComparison.Ordinal);
        if (!isFirstVersion && !stripped.StartsWith(Prefix, StringComparison.Ordinal))
        {
            return NoteCodeStatus.NotACode;
        }

        var encoded = stripped[Prefix.Length..];
        var compressed = new byte[encoded.Length / 4 * 3];
        if (encoded.Length == 0 || !Convert.TryFromBase64String(encoded, compressed, out var compressedSize))
        {
            return NoteCodeStatus.Damaged;
        }

        var status = Inflate(
            compressed.AsSpan(0, compressedSize),
            isFirstVersion ? FirstLimit : TreeLimit,
            out var raw);
        if (status != NoteCodeStatus.Ok)
        {
            return status;
        }

        var offset = 0;
        NoteDecodeResult result;
        if (isFirstVersion)
        {
            result = NoteCodec.DecodeCategory(raw, ref offset, out var category);
            if (result == NoteDecodeResult.Decoded)
            {
                imported = new NoteSet();
                imported.Attach(category!, 0);
            }
        }
        else
        {
            result = NoteCodec.DecodeTree(raw, ref offset, true, out imported);
        }

        if (result == NoteDecodeResult.Decoded && offset != raw.Length)
        {
            result = NoteDecodeResult.Damaged;
        }

        if (result != NoteDecodeResult.Decoded)
        {
            imported = null;
            return result == NoteDecodeResult.TooLarge ? NoteCodeStatus.TooLarge : NoteCodeStatus.Damaged;
        }

        imported!.Changed = false;
        return NoteCodeStatus.Ok;
    }

    public static string Describe(NoteSet imported)
    {
        var categories = imported.Categories;
        var rootName = categories.Count > 0 ? categories[0].Name : "";
        var categoryWord = categories.Count == 1 ? "category" : "categories";
        var pins = categories.Sum(category => category.Count(NoteKind.Pin));
        var labels = categories.Sum(category => category.Count(NoteKind.Text));
        var paths = categories.Sum(category => category.Count(NoteKind.Path));
        return $"{rootName}: {categories.Count} {categoryWord}, {pins} pins, {labels} labels, {paths} paths";
    }

    public static string Problem(NoteCodeStatus status) => status switch
    {
        NoteCodeStatus.NotACode => "That is not a world map code.",
        NoteCodeStatus.Damaged => "This code is damaged or incomplete.",
        NoteCodeStatus.TooLarge => "This code is too large to import.",
        _ => throw new UnreachableException(),
    };

    public static string CopiedMessage(int characters) => $"Copied {characters} characters to the clipboard.";

    // Reading one byte past the limit tells a payload that is too big from one that ends exactly at it. .NET doesn't
    // reliably reject a stream cut short, so zlib's Adler-32 check at the end is compared here.
    private static NoteCodeStatus Inflate(ReadOnlySpan<byte> compressed, int limit, out byte[] raw)
    {
        raw = [];
        if (compressed.Length < ZlibHeaderSize + AdlerSize)
        {
            return NoteCodeStatus.Damaged;
        }

        try
        {
            using var zlib = new ZLibStream(new MemoryStream(compressed.ToArray()), CompressionMode.Decompress);
            using var output = new MemoryStream();
            var chunk = new byte[16384];
            int read;
            while (output.Length <= limit && (read = zlib.Read(
                chunk,
                0,
                (int)Math.Min(chunk.Length, limit + 1 - output.Length))) > 0)
            {
                output.Write(chunk, 0, read);
            }

            if (output.Length > limit)
            {
                return NoteCodeStatus.TooLarge;
            }

            raw = output.ToArray();
        }
        catch (InvalidDataException)
        {
            return NoteCodeStatus.Damaged;
        }

        return Adler32(raw) == BinaryPrimitives.ReadUInt32BigEndian(compressed[^AdlerSize..])
            ? NoteCodeStatus.Ok
            : NoteCodeStatus.Damaged;
    }

    private static uint Adler32(ReadOnlySpan<byte> bytes)
    {
        uint low = 1;
        uint high = 0;
        foreach (var value in bytes)
        {
            low = (low + value) % AdlerModulus;
            high = (high + low) % AdlerModulus;
        }

        return (high << 16) | low;
    }
}
