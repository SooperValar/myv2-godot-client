namespace TheLastGate.State;

public readonly record struct PlayerTarget(
    int AttackCharacter,
    int GotoX,
    int GotoY,
    int MiscAction,
    int MiscTarget1,
    int MiscTarget2);
