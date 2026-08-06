using System;

namespace OpcUaViewer.Core.Contracts;

public record CycleCompletedEventArgs(string PartNumber, double CycleSeconds);

/// <summary>
/// Optional interface for data sources that provide explicit cycle times
/// (e.g. CSV source). When a source implements this, StatsTab uses the
/// exact duration instead of measuring elapsed wall-clock time.
/// </summary>
public interface ICycleSource
{
    event EventHandler<CycleCompletedEventArgs>? CycleCompleted;
}
