using System;
using System.Collections.Generic;
using SlimeAI.GameOS.Runtime.World;

namespace SlimeAI.GameOS.Runtime.Pool;

/// <summary>
/// Global registry for named runtime object pools.
/// </summary>
public static class ObjectPoolManager
{
    /// <summary>
    /// Registers a pool by name.
    /// </summary>
    /// <param name="pool">Pool instance.</param>
    public static void Register<T>(ObjectPool<T> pool) where T : class
    {
        RuntimeWorld.Default.Pools.Register(pool);
    }

    /// <summary>
    /// Registers a non-generic pool handle by name.
    /// </summary>
    /// <param name="pool">Pool instance.</param>
    public static void RegisterPool(IObjectPool pool)
    {
        RuntimeWorld.Default.Pools.RegisterPool(pool);
    }

    /// <summary>
    /// Unregisters a pool.
    /// </summary>
    /// <param name="pool">Pool instance.</param>
    public static void Unregister<T>(ObjectPool<T> pool) where T : class
    {
        RuntimeWorld.Default.Pools.Unregister(pool);
    }

    /// <summary>
    /// Unregisters a non-generic pool handle.
    /// </summary>
    /// <param name="pool">Pool instance.</param>
    public static void UnregisterPool(IObjectPool pool)
    {
        RuntimeWorld.Default.Pools.UnregisterPool(pool);
    }

    /// <summary>
    /// Maps an acquired object to its pool name.
    /// </summary>
    /// <param name="instance">Pooled instance.</param>
    /// <param name="poolName">Pool name.</param>
    public static void MapObject(object instance, string poolName)
    {
        RuntimeWorld.Default.Pools.MapObject(instance, poolName);
    }

    /// <summary>
    /// Removes a pooled object mapping.
    /// </summary>
    /// <param name="instance">Pooled instance.</param>
    public static void UnmapObject(object instance)
    {
        RuntimeWorld.Default.Pools.UnmapObject(instance);
    }

    /// <summary>
    /// Returns an object to its source pool when known.
    /// </summary>
    /// <param name="instance">Pooled instance.</param>
    public static bool ReturnToPool(object instance)
    {
        return RuntimeWorld.Default.Pools.ReturnToPool(instance);
    }

    /// <summary>
    /// Gets a typed pool by name.
    /// </summary>
    /// <param name="name">Pool name.</param>
    public static ObjectPool<T>? GetPool<T>(string name) where T : class
    {
        return RuntimeWorld.Default.Pools.GetPool<T>(name);
    }

    /// <summary>
    /// Gets a non-generic pool by name.
    /// </summary>
    /// <param name="name">Pool name.</param>
    public static IObjectPool? GetNamedPool(string name)
    {
        return RuntimeWorld.Default.Pools.GetNamedPool(name);
    }

    /// <summary>
    /// Returns stats for all registered pools.
    /// </summary>
    public static Dictionary<string, PoolStats> GetAllStats()
    {
        return RuntimeWorld.Default.Pools.GetAllStats();
    }

    /// <summary>
    /// Destroys all pools and mappings.
    /// </summary>
    public static void DestroyAll()
    {
        RuntimeWorld.Default.Pools.Clear();
    }
}
