namespace SkilmeAI.GameOS.Runtime.Pool;

/// <summary>
/// Runtime object pool statistics.
/// </summary>
public readonly record struct PoolStats(
    string PoolName,
    int Count,
    int ActiveCount,
    int TotalCreated,
    int TotalAcquired,
    int TotalReused,
    int TotalCreatedOnAcquire,
    int TotalReleased,
    int TotalDiscarded)
{
    /// <summary>
    /// Fraction of acquire calls served by idle instances.
    /// </summary>
    public float ReuseRate => TotalAcquired > 0 ? (float)TotalReused / TotalAcquired : 0f;
}
