using System;

namespace OpcUaViewer.Core.Contracts;

public record TagInfo(string Name, string Address);

public class TagValueEventArgs(string name, object? rawValue, string statusCode, DateTime timestamp) : EventArgs
{
    public string   Name       { get; } = name;
    public object?  RawValue   { get; } = rawValue;
    public string   StrValue   { get; } = rawValue?.ToString() ?? "";
    public string   StatusCode { get; } = statusCode;
    public DateTime Timestamp  { get; } = timestamp;
}
