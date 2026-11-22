namespace TheLastGate.Profiling;

public sealed record PacketTraffic(string Sender, string Type, double PerSecond, long Total);
