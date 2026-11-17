using TheLastGate.Characters;
using TheLastGate.Hotbar;
using TheLastGate.Items;
using TheLastGate.Net;

namespace TheLastGate.Tests;

public class HotbarButtonsTests
{
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
    public void GearKeyButton_FindsTheButtonThatUsesTheGear()
    {
        var actions = new CharacterSettings().HotbarActions;
        actions[3] = 208;

        Assert.Equal(3, HotbarButtons.GearKeyButton(actions, WornSlot.MainHand));
        Assert.Null(HotbarButtons.GearKeyButton(actions, WornSlot.Head));
    }

    [Fact]
    public void GearKeyButton_FindsTheLastButtonWhenSeveralUseTheGear()
    {
        var actions = new CharacterSettings().HotbarActions;
        actions[3] = 208;
        actions[12] = 208;

        Assert.Equal(12, HotbarButtons.GearKeyButton(actions, WornSlot.MainHand));
    }

    [Fact]
    public void InventoryKeyButton_FindsTheButtonThatUsesTheInventorySlot()
    {
        var actions = new CharacterSettings().HotbarActions;
        actions[5] = 105;

        Assert.Equal(5, HotbarButtons.InventoryKeyButton(actions, 5));
        Assert.Null(HotbarButtons.InventoryKeyButton(actions, 6));
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
