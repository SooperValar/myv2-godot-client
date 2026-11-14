namespace TheLastGate.Net;

public readonly record struct ReceivedTick(byte[] Payload, long ReceivedAtTimestamp);
