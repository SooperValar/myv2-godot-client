using GdUnit4;
using TheLastGate.Map;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Map;

[TestSuite]
public class WorldMapPictureTests
{
    private const int BytesPerPixel = 4;

    [TestCase]
    public void Render_SpreadsMapYAcrossAndMapXDownOverTheWorldsTopHalf()
    {
        var map = new ExploredMap();
        map.SetTile(3, 5, 0x112233);
        map.SetTile(1023, 1023, 0x445566);
        map.SetTile(1024, 0, 0x778899);
        var rgba = new byte[WorldMapPicture.Width * WorldMapPicture.Height * BytesPerPixel];

        WorldMapPicture.Render(map, rgba);

        AssertThat(WorldMapPicture.Width).IsEqual(1024);
        AssertThat(WorldMapPicture.Height).IsEqual(1024);
        AssertThat(Pixel(rgba, 5, 3)).IsEqual(new byte[] { 0x11, 0x22, 0x33, 255 });
        AssertThat(Pixel(rgba, 1023, 1023)).IsEqual(new byte[] { 0x44, 0x55, 0x66, 255 });
        AssertThat(Pixel(rgba, 0, 0)).IsEqual(new byte[] { 0, 0, 0, 255 });
    }

    private static byte[] Pixel(byte[] rgba, int across, int down) =>
        rgba.AsSpan((down * WorldMapPicture.Width + across) * BytesPerPixel, BytesPerPixel).ToArray();
}
