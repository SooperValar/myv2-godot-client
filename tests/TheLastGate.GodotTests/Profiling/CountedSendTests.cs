using System.Diagnostics;
using GdUnit4;
using TheLastGate.Net;
using TheLastGate.Profiling;
using static GdUnit4.Assertions;

namespace TheLastGate.GodotTests.Profiling;

[TestSuite]
public class CountedSendTests
{
    [TestCase]
    public void WithNoCounter_TheSenderIsReturnedUnchanged()
    {
        Action<byte[]> send = _ => { };

        AssertThat(ReferenceEquals(CountedSend.For(null, "Player", send), send)).IsTrue();
    }

    [TestCase]
    public void APacketThroughAWrappedSender_IsCountedOnceAndStillSent()
    {
        var counter = new PacketCounter();
        var sent = new List<byte[]>();
        var sendAction = CountedSend.For(counter, "GiveMore", sent.Add);
        var packet = new byte[Protocol.PacketSize];
        packet[0] = (byte)ClientCommand.Give;

        sendAction(packet);

        var row = counter.Snapshot(Stopwatch.GetTimestamp()).Single();
        AssertThat(row.Sender).IsEqual("GiveMore");
        AssertThat(row.Total).IsEqual(1);
        AssertThat(sent.Count).IsEqual(1);
    }
}
