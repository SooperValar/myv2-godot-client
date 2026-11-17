using TheLastGate.Map;

namespace TheLastGate.Tests;

public class WorldMapPictureTests
{
    private const int BytesPerPixel = 4;

    [Fact]
    public void Render_SpreadsMapYAcrossAndMapXDownOverTheWorldsTopHalf()
    {
        var map = new ExploredMap();
        map.SetTile(3, 5, 0x112233);
        map.SetTile(1023, 2047, 0x445566);
        map.SetTile(1024, 0, 0x778899);
        var rgba = new byte[WorldMapPicture.Width * WorldMapPicture.Height * BytesPerPixel];

        WorldMapPicture.Render(map, rgba);

        Assert.Equal(2048, WorldMapPicture.Width);
        Assert.Equal(1024, WorldMapPicture.Height);
        Assert.Equal(new byte[] { 0x11, 0x22, 0x33, 255 }, Pixel(rgba, 5, 3));
        Assert.Equal(new byte[] { 0x44, 0x55, 0x66, 255 }, Pixel(rgba, 2047, 1023));
        Assert.Equal(new byte[] { 0, 0, 0, 255 }, Pixel(rgba, 0, 0));
    }

    private static byte[] Pixel(byte[] rgba, int across, int down) =>
        rgba.AsSpan((down * WorldMapPicture.Width + across) * BytesPerPixel, BytesPerPixel).ToArray();
}
