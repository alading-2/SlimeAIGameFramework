using System;
using System.Collections.Generic;

namespace SlimeAI.GameOS.Runtime.Pool;

/// <summary>
/// Global registry for named runtime object pools.
/// </summary>
public static class ObjectPoolManager
{
    private static readonly Dictionary<string, IObjectPool> Pools = new(StringComparer.Ordinal);
    private static readonly Dictionary<object, string> ObjectToPool = new();
    private static readonly object LockObject = new();

    /// <summary>
    /// Registers a pool by name.
    /// </summary>
    /// <param name="pool">Pool instance.</param>
    public static void Register<T>(ObjectPool<T> pool) where T : class
    {
        RegisterPool(pool);
    }

    /// <summary>
    /// Registers a non-generic pool handle by name.
    /// </summary>
    /// <param name="pool">Pool instance.</param>
    public static void RegisterPool(IObjectPool pool)
    {
        ArgumentNullException.ThrowIfNull(pool);
        lock (LockObject)
        {
            Pools[pool.PoolName] = pool;
        }
    }

    /// <summary>
    /// Unregisters a pool.
    /// </summary>
    /// <param name="pool">Pool instance.</param>
    public static void Unregister<T>(ObjectPool<T> pool) where T : class
    {
        UnregisterPool(pool);
    }

    /// <summary>
    /// Unregisters a non-generic pool handle.
    /// </summary>
    /// <param name="pool">Pool instance.</param>
    public static void UnregisterPool(IObjectPool pool)
    {
        ArgumentNullException.ThrowIfNull(pool);
        lock (LockObject)
        {
            Pools.Remove(pool.PoolName);
        }
    }

    /// <summary>
    /// Maps an acquired object to its pool name.
    /// </summary>
    /// <param name="instance">Pooled instance.</param>
    /// <param name="poolName">Pool name.</param>
    public static void MapObject(object instance, string poolName)
    {
        lock (LockObject)
        {
            ObjectToPool[instance] = poolName;
        }
    }

    /// <summary>
    /// Removes a pooled object mapping.
    /// </summary>
    /// <param name="instance">Pooled instance.</param>
    public static void UnmapObject(object instance)
    {
        lock (LockObject)
        {
            ObjectToPool.Remove(instance);
        }
    }

    /// <summary>
    /// Returns an object to its source pool when known.
    /// </summary>
    /// <param name="instance">Pooled instance.</param>
    public static bool ReturnToPool(object instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        IObjectPool? pool;
        lock (LockObject)
        {
            if (!ObjectToPool.TryGetValue(instance, out var poolName) || !Pools.TryGetValue(poolName, out pool))
            {
                return false;
            }
        }

        return pool.ReleaseObject(instance);
    }

    /// <summary>
    /// Gets a typed pool by name.
    /// </summary>
    /// <param name="name">Pool name.</param>
    public static ObjectPool<T>? GetPool<T>(string name) where T : class
    {
        lock (LockObject)
        {
            return Pools.TryGetValue(name, out var pool) ? pool as ObjectPool<T> : null;
        }
    }

    /// <summary>
    /// Gets a non-generic pool by name.
    /// </summary>
    /// <param name="name">Pool name.</param>
    public static IObjectPool? GetNamedPool(string name)
    {
        lock (LockObject)
        {
            return Pools.GetValueOrDefault(name);
        }
    }

    /// <summary>
    /// Returns stats for all registered pools.
    /// </summary>
    public static Dictionary<string, PoolStats> GetAllStats()
    {
        lock (LockObject)
        {
            var result = new Dictionary<string, PoolStats>(StringComparer.Ordinal);
            foreach (var pair in Pools)
            {
                result[pair.Key] = pair.Value.GetStats();
            }

            return result;
        }
    }

    /// <summary>
    /// Destroys all pools and mappings.
    /// </summary>
    public static void DestroyAll()
    {
        List<IObjectPool> pools;
        lock (LockObject)
        {
            pools = new List<IObjectPool>(Pools.Values);
            Pools.Clear();
            ObjectToPool.Clear();
        }

        for (var i = 0; i < pools.Count; i++)
        {
            pools[i].Destroy();
        }
    }
}
