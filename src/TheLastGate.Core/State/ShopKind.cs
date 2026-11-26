namespace TheLastGate.State;

public enum ShopKind
{
    Merchant,
    BlackStronghold,
    Casino,
    Contract,
    Experience,
    WeaponSmith,
    ArmourSmith,
    Depot,
    Trash,

    // MyV2's bank depot, opened as a shop window; its prices are the rent per day.
    Bank,

    // A MyV2 grave: everything the dead character carried, wore and held, in two pages.
    Grave,
}
