using TheLastGate.Characters;
using TheLastGate.Hotbar;
using TheLastGate.Items;
using TheLastGate.Net;

namespace TheLastGate.Tests;

public class HotbarButtonsTests
{
    [Theory]
    [InlineData(0, "C-1")]
    [InlineData(4, "C-5")]
    [InlineData(5, "C-Q")]
    [InlineData(9, "C-T")]
    [InlineData(10, "C-A")]
    [InlineData(14, "C-G")]
    [InlineData(15, "C-Z")]
    [InlineData(19, "C-B")]
    public void KeyLabel_NamesTheControlKeyForEachButton(int slot, string label)
    {
        Assert.Equal(label, HotbarButtons.KeyLabel(slot));
    }

    [Theory]
    [InlineData('1', 0)]
    [InlineData('5', 4)]
    [InlineData('Q', 5)]
    [InlineData('G', 14)]
    [InlineData('B', 19)]
    public void SlotForKey_FindsTheButtonForEachKey(char key, int slot)
    {
        Assert.Equal(slot, HotbarButtons.SlotForKey(key));
    }

    [Theory]
    [InlineData('6')]
    [InlineData('P')]
    [InlineData('H')]
    public void SlotForKey_IgnoresKeysWithoutAButton(char key)
    {
        Assert.Null(HotbarButtons.SlotForKey(key));
    }

    [Fact]
    public void Name_ShowsADashOnAnUnassignedButton()
    {
        Assert.Equal("-", HotbarButtons.Name(CharacterSettings.Unassigned, "Light"));
    }

    [Fact]
    public void Name_ShowsTheStoredNameOfAnAssignedButton()
    {
        Assert.Equal("Light", HotbarButtons.Name(50, "Light"));
    }

    [Theory]
    [InlineData(100, "Item 0")]
    [InlineData(105, "Item 5")]
    [InlineData(159, "Item 59")]
    public void ItemName_NamesInventorySlotsByNumber(int action, string name)
    {
        Assert.Equal(name, HotbarButtons.ItemName(action));
    }

    [Theory]
    [InlineData(200, "*Helmet")]
    [InlineData(207, "*Offhan")]
    [InlineData(208, "*Weapon")]
    [InlineData(212, "*Tarot2")]
    public void ItemName_NamesEquipmentSlotsByGear(int action, string name)
    {
        Assert.Equal(name, HotbarButtons.ItemName(action));
    }

    [Fact]
    public void GearKeySprite_ShowsTheKeyOfTheButtonThatUsesTheGear()
    {
        var actions = new CharacterSettings().HotbarActions;
        actions[3] = 208;

        Assert.Equal(4014, HotbarButtons.GearKeySprite(actions, WornSlot.MainHand));
        Assert.Null(HotbarButtons.GearKeySprite(actions, WornSlot.Head));
    }

    [Fact]
    public void GearKeySprite_ShowsTheLastButtonWhenSeveralUseTheGear()
    {
        var actions = new CharacterSettings().HotbarActions;
        actions[3] = 208;
        actions[12] = 208;

        Assert.Equal(4023, HotbarButtons.GearKeySprite(actions, WornSlot.MainHand));
    }

    [Fact]
    public void InventoryKeySprite_ShowsTheKeyOfTheButtonThatUsesTheInventorySlot()
    {
        var actions = new CharacterSettings().HotbarActions;
        actions[5] = 105;

        Assert.Equal(4016, HotbarButtons.InventoryKeySprite(actions, 5));
        Assert.Null(HotbarButtons.InventoryKeySprite(actions, 6));
    }

    [Fact]
    public void UsePacket_UsesASkillOnTheSelectedCharacter()
    {
        Assert.Equal(ClientPackets.ThreeValues(ClientCommand.Skill, 50, 345, 1), HotbarButtons.UsePacket(50, 345));
    }

    [Fact]
    public void UsePacket_UsesAnInventoryItemWhereItIs()
    {
        Assert.Equal(InventoryPackets.For(InventoryAction.UseItem, 3, 345), HotbarButtons.UsePacket(103, 345));
    }

    [Fact]
    public void UsePacket_UsesAnEquippedItemWhereItIs()
    {
        Assert.Equal(InventoryPackets.For(InventoryAction.UseGear, 8, 345), HotbarButtons.UsePacket(208, 345));
    }

    [Theory]
    [InlineData(CharacterSettings.Unassigned)]
    [InlineData(60)]
    [InlineData(99)]
    public void UsePacket_SendsNothingForAnEmptyOrUnknownButton(int action)
    {
        Assert.Null(HotbarButtons.UsePacket(action, 345));
    }
}
