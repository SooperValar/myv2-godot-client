using System;

namespace TheLastGate.Net;

public sealed class ProtocolDesyncException(string message) : Exception(message);
