using System;
using System.Collections.Generic;

namespace OpcUaViewer.Core.Contracts;

public record MonitoredNodeInfo(string Name, string NodeIdStr);

public class NodeValueEventArgs(string name, object? rawValue, string statusCode, DateTime timestamp) : EventArgs
{
    public string   Name       { get; } = name;
    public object?  RawValue   { get; } = rawValue;
    public string   StrValue   { get; } = rawValue?.ToString() ?? "";
    public string   StatusCode { get; } = statusCode;
    public DateTime Timestamp  { get; } = timestamp;
}

public class OpcUaConnectionException(string message, Exception? inner = null)
    : Exception(message, inner);
