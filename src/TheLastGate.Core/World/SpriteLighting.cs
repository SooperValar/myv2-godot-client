namespace TheLastGate.World;

public static class SpriteLighting
{
    public const int DarkestLight = 15;

    private const float DefaultGamma = 5000;
    private const float ShaderGammaBase = 4880;
    private const float GammaEffect = DefaultGamma - ShaderGammaBase;

    public static float Brightness(int light) => light > 0 ? GammaEffect / (light * light + GammaEffect) : 1f;
}
