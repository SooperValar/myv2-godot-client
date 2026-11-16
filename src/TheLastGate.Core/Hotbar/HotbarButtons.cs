using System;
using TheLastGate.Characters;
using TheLastGate.Items;
using TheLastGate.Net;

namespace TheLastGate.Hotbar;

public static class HotbarButtons
{
    private const string Keys = "12345QWERTASDFGZXCVB";
    private const int SkillLimit = 60;
    public const int FirstInventoryAction = 100;
    public const int FirstEquipmentAction = 200;

    private const int UsedFromHotbar = 1;

    public static string KeyLabel(int slot) => $"C-{Keys[slot]}";

    public static int? SlotForKey(char key) => Keys.IndexOf(key) is var slot and >= 0 ? slot : null;

    public static string Name(int action, string storedName) =>
        action == CharacterSettings.Unassigned ? "-" : storedName;

    public static string ItemName(int action) => throw new NotImplementedException();

    public static int? GearKeySprite(int[] actions, WornSlot slot) => throw new NotImplementedException();

    public static byte[]? UsePacket(int action, int selectedCharacter) => action switch
    {
        >= FirstEquipmentAction =>
            InventoryPackets.For(InventoryAction.UseGear, action - FirstEquipmentAction, selectedCharacter),
        >= FirstInventoryAction =>
            InventoryPackets.For(InventoryAction.UseItem, action - FirstInventoryAction, selectedCharacter),
        >= 0 and < SkillLimit =>
            ClientPackets.ThreeValues(ClientCommand.Skill, action, selectedCharacter, UsedFromHotbar),
        _ => null,
    };
}
