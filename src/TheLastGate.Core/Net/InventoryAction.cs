namespace TheLastGate.Net;

public enum InventoryAction
{
    SwapWithInventorySlot = 0,
    SwapWithGearSlot = 1,
    TakeMoney = 2,
    MoveStack = 3,
    LockItem = 4,
    UseGear = 5,
    UseItem = 6,
    LookAtGear = 7,
    LookAtItem = 8,
    // MyV2 reads 9 as Ctrl+click: one from the stack in that backpack slot goes into the hand.
    TakeOneFromStack = 9,

    // MyV2's own: the item in the hand is destroyed.
    TrashHeldItem = 10,
}
