namespace SlimeAI.GameOS.Runtime.Pool;

/// <summary>
/// Non-generic pool handle used by global pool registries.
/// </summary>
public interface IObjectPool
{
    /// <summary>
    /// Global pool name.
    /// </summary>
    string PoolName { get; }

    /// <summary>
    /// Releases an object when it belongs to this pool.
    /// </summary>
    /// <param name="instance">Instance mapped to this pool.</param>
    bool ReleaseObject(object instance);

    /// <summary>
    /// Returns current statistics.
    /// </summary>
    PoolStats GetStats();

    /// <summary>
    /// Destroys the pool and releases retained objects.
    /// </summary>
    void Destroy();
}
