using System;
using TheLastGate.State;
using TheLastGate.World;

namespace TheLastGate.Hud;

public sealed class LookPanel
{
    public static readonly TimeSpan ShowFor = TimeSpan.FromSeconds(10);

    public CharacterLook? Shown => throw new NotImplementedException();

    public static SpriteTint PortraitTint(LookFlags flags) => throw new NotImplementedException();

    public void Show(CharacterLook look) => throw new NotImplementedException();

    public void Advance(TimeSpan elapsed) => throw new NotImplementedException();
}
