using System.Diagnostics;
using GdUnit4;
using TheLastGate.Net;
using TheLastGate.Profiling;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Profiling;

[TestSuite]
public class PacketCounterTests
{
    private static readonly long Second = Stopwatch.Frequency;

    [TestCase]
    public void Packets_AreCountedBySenderAndType()
    {
        var counter = new PacketCounter();
        counter.Count("ItemLooks", Packet(ClientCommand.LookItem), Second);
        counter.Count("ItemLooks", Packet(ClientCommand.LookItem), Second);
        counter.Count("Player", Packet(ClientCommand.Move), Second);

        var rows = counter.Snapshot(Second);

        AssertThat(rows.Select(row => (row.Sender, row.Type, row.Total)))
            .ContainsExactly(("ItemLooks", "LookItem", 2L), ("Player", "Move", 1L));
    }

    [TestCase]
    public void TextPackets_FoldIntoOneType()
    {
        var counter = new PacketCounter();
        counter.Count("BuffWatch", Packet(ClientCommand.Input1), Second);
        counter.Count("BuffWatch", Packet(ClientCommand.Input8), Second);

        var row = counter.Snapshot(Second).Single();

        AssertThat(row.Type).IsEqual("Input (text)");
        AssertThat(row.Total).IsEqual(2);
    }

    [TestCase]
    public void AByteThatIsNoCommand_IsNamedByValue() =>
        AssertThat(PacketCounter.TypeName(200)).IsEqual("Unknown 0xC8");

    [TestCase]
    public void PerSecond_CoversTheWindowAndTotal_CoversTheSession()
    {
        var counter = new PacketCounter();
        counter.Count("NameRequests", Packet(ClientCommand.AutoLook), 0);
        for (var packet = 0; packet < 4; packet++)
        {
            counter.Count("NameRequests", Packet(ClientCommand.AutoLook), 3 * Second);
        }

        var row = counter.Snapshot(4 * Second).Single();

        AssertThat(row.PerSecond).IsEqual(2.0);
        AssertThat(row.Total).IsEqual(5);
        AssertThat(counter.PerSecond("NameRequests", 4 * Second)).IsEqual(2.0);
        AssertThat(counter.PerSecond("Player", 4 * Second)).IsEqual(0.0);
    }

    [TestCase]
    public void Senders_ListsEachSenderOnce()
    {
        var counter = new PacketCounter();
        counter.Count("Player", Packet(ClientCommand.Move), 0);
        counter.Count("Player", Packet(ClientCommand.Turn), 0);
        counter.Count("AutoWalk", Packet(ClientCommand.Move), 0);

        AssertThat(counter.Senders).ContainsExactlyInAnyOrder("Player", "AutoWalk");
    }

    private static byte[] Packet(ClientCommand command)
    {
        var packet = new byte[Protocol.PacketSize];
        packet[0] = (byte)command;
        return packet;
    }
}
