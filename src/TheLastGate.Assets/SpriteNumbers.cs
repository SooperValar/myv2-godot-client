namespace TheLastGate.Assets;

public static class SpriteNumbers
{
    public const int MaxSprite = 2000 + 128 * 1024;

    // The client's own interface art is asked for at this offset, above every number the MyV2 server sends, and is
    // read from the client's archives at the number less the offset.
    public const int ClientArtBase = 100000;

    // bmplib only holds sprites below this number.
    public const int BmpSlotCount = 40000;
}
