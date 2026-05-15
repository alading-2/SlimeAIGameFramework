using System;
using System.Collections.Generic;
using SlimeAI.GameOS.Runtime.Pool;

namespace SlimeAI.GameOS.Runtime.World;

/// <summary>
/// RuntimeWorld 持有的对象池注册表状态。
/// </summary>
public sealed class ObjectPoolManagerState : IObjectPoolManager
{
    private readonly Dictionary<string, IObjectPool> pools = new(StringComparer.Ordinal);
    private readonly Dictionary<object, string> objectToPool = new();
    private readonly object lockObject = new();

    /// <inheritdoc />
    public void Register<T>(ObjectPool<T> pool) where T : class
    {
        RegisterPool(pool);
    }

    /// <inheritdoc />
    public void RegisterPool(IObjectPool pool)
    {
        ArgumentNullException.ThrowIfNull(pool);
        lock (lockObject)
        {
            pools[pool.PoolName] = pool;
        }
    }

    /// <inheritdoc />
    public void Unregister<T>(ObjectPool<T> pool) where T : class
    {
        UnregisterPool(pool);
    }

    /// <inheritdoc />
    public void UnregisterPool(IObjectPool pool)
    {
        ArgumentNullException.ThrowIfNull(pool);
        lock (lockObject)
        {
            pools.Remove(pool.PoolName);
        }
    }

    /// <inheritdoc />
    public void MapObject(object instance, string poolName)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentException.ThrowIfNullOrWhiteSpace(poolName);
        lock (lockObject)
        {
            objectToPool[instance] = poolName;
        }
    }

    /// <inheritdoc />
    public void UnmapObject(object instance)
    {
        ArgumentNullException.ThrowIfNull(instance);
        lock (lockObject)
        {
            objectToPool.Remove(instance);
        }
    }

    /// <inheritdoc />
    public bool ReturnToPool(object instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        IObjectPool? pool;
        lock (lockObject)
        {
            if (!objectToPool.TryGetValue(instance, out var poolName) || !pools.TryGetValue(poolName, out pool))
            {
                return false;
            }
        }

        return pool.ReleaseObject(instance);
    }

    /// <inheritdoc />
    public ObjectPool<T>? GetPool<T>(string name) where T : class
    {
        lock (lockObject)
        {
            return pools.TryGetValue(name, out var pool) ? pool as ObjectPool<T> : null;
        }
    }

    /// <inheritdoc />
    public IObjectPool? GetNamedPool(string name)
    {
        lock (lockObject)
        {
            return pools.GetValueOrDefault(name);
        }
    }

    /// <inheritdoc />
    public Dictionary<string, PoolStats> GetAllStats()
    {
        lock (lockObject)
        {
            var result = new Dictionary<string, PoolStats>(StringComparer.Ordinal);
            foreach (var pair in pools)
            {
                result[pair.Key] = pair.Value.GetStats();
            }

            return result;
        }
    }

    /// <inheritdoc />
    public void Clear()
    {
        List<IObjectPool> snapshot;
        lock (lockObject)
        {
            snapshot = new List<IObjectPool>(pools.Values);
            pools.Clear();
            objectToPool.Clear();
        }

        for (var i = 0; i < snapshot.Count; i++)
        {
            snapshot[i].Destroy();
        }
    }
}
