using System;
using System.Buffers.Binary;

namespace TheLastGate.Characters;

// The settings block of a character file: name, description and changed flag (read by
// CharacterRecord), then five int32 toggles at 324 and twenty { char name[8], int32 action }
// hotbar entries at 344.
public sealed class CharacterSettings
{
    public const int HotbarSlots = 20;
    public const int Unassigned = -1;

    private const int TogglesOffset = 324;
    private const int ToggleCount = 5;
    private const int HotbarOffset = 344;
    private const int HotbarNameLength = 8;
    private const int HotbarEntrySize = 12;
    private const string EmptyHotbarName = "-";

    public bool HideWalls { get; set; } = true;
    public bool ShowNames { get; set; } = true;
    public bool ShowHealthPercent { get; set; } = true;
    public bool ShowHealthBars { get; set; }
    public bool ShowStatBase { get; set; }

    // A skill number below 60, 100 + an inventory slot, or 200 + an equipment slot.
    public int[] HotbarActions { get; } = CreateFilled(Unassigned);
    public string[] HotbarNames { get; } = CreateFilled(EmptyHotbarName);

    public static CharacterSettings Parse(ReadOnlySpan<byte> stored)
    {
        var toggles = new bool[ToggleCount];
        for (var toggleIndex = 0; toggleIndex < ToggleCount; toggleIndex++)
        {
            toggles[toggleIndex] =
                BinaryPrimitives.ReadInt32LittleEndian(stored[(TogglesOffset + toggleIndex * sizeof(int))..]) != 0;
        }

        var settings = new CharacterSettings
        {
            HideWalls = toggles[0],
            ShowNames = toggles[1],
            ShowHealthPercent = toggles[2],
            ShowHealthBars = toggles[3],
            ShowStatBase = toggles[4],
        };

        for (var slot = 0; slot < HotbarSlots; slot++)
        {
            var entry = stored.Slice(HotbarOffset + slot * HotbarEntrySize, HotbarEntrySize);
            settings.HotbarNames[slot] = NulTerminatedText.Read(entry[..HotbarNameLength]);
            settings.HotbarActions[slot] = BinaryPrimitives.ReadInt32LittleEndian(entry[HotbarNameLength..]);
        }

        return settings;
    }

    public void Write(Span<byte> stored)
    {
        bool[] toggles = [HideWalls, ShowNames, ShowHealthPercent, ShowHealthBars, ShowStatBase];
        for (var toggleIndex = 0; toggleIndex < ToggleCount; toggleIndex++)
        {
            BinaryPrimitives.WriteInt32LittleEndian(
                stored[(TogglesOffset + toggleIndex * sizeof(int))..],
                toggles[toggleIndex] ? 1 : 0);
        }

        for (var slot = 0; slot < HotbarSlots; slot++)
        {
            var entry = stored.Slice(HotbarOffset + slot * HotbarEntrySize, HotbarEntrySize);
            NulTerminatedText.Write(entry[..HotbarNameLength], HotbarNames[slot]);
            BinaryPrimitives.WriteInt32LittleEndian(entry[HotbarNameLength..], HotbarActions[slot]);
        }
    }

    private static T[] CreateFilled<T>(T value)
    {
        var values = new T[HotbarSlots];
        Array.Fill(values, value);
        return values;
    }
}
