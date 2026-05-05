namespace SkilmeAI.GameOS.Runtime.Pool;

/// <summary>
/// Object pool configuration.
/// </summary>
public readonly record struct ObjectPoolConfig
{
    /// <summary>
    /// Number of instances created during construction.
    /// </summary>
    public int InitialSize { get; init; } = 0;

    /// <summary>
    /// Maximum idle instances retained by the pool. -1 means unlimited.
    /// </summary>
    public int MaxSize { get; init; } = -1;

    /// <summary>
    /// Global pool name used by ObjectPoolManager.
    /// </summary>
    public string Name { get; init; } = "UnnamedPool";

    /// <summary>
    /// Creates a default config.
    /// </summary>
    public ObjectPoolConfig()
    {
    }
}
