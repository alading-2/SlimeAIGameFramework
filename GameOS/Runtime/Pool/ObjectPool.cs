using System;
using System.Collections.Generic;

namespace SlimeAI.GameOS.Runtime.Pool;

/// <summary>
/// Type-safe object pool for pure runtime objects.
/// </summary>
public sealed class ObjectPool<T> : IObjectPool where T : class
{
    private readonly Stack<T> idle;
    private readonly HashSet<T> active = new();
    private readonly Func<T> createFunc;
    private readonly Action<T>? resetMethod;
    private readonly ObjectPoolConfig config;

    private int totalCreated;
    private int totalAcquired;
    private int totalReused;
    private int totalCreatedOnAcquire;
    private int totalReleased;
    private int totalDiscarded;

    /// <summary>
    /// Creates and registers an object pool.
    /// </summary>
    /// <param name="createFunc">Factory used when the pool must grow.</param>
    /// <param name="config">Pool configuration.</param>
    /// <param name="resetMethod">Optional reset callback invoked on release.</param>
    public ObjectPool(Func<T> createFunc, ObjectPoolConfig config, Action<T>? resetMethod = null)
    {
        this.createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
        this.config = config;
        this.resetMethod = resetMethod;
        idle = new Stack<T>(Math.Max(0, config.InitialSize));

        Warmup(config.InitialSize);
        ObjectPoolManager.Register(this);
    }

    /// <summary>
    /// Pool name.
    /// </summary>
    public string PoolName => config.Name;

    /// <summary>
    /// Number of idle instances.
    /// </summary>
    public int Count => idle.Count;

    /// <summary>
    /// Number of acquired instances.
    /// </summary>
    public int ActiveCount => active.Count;

    /// <summary>
    /// Pre-allocates idle instances.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    public void Warmup(int count)
    {
        for (var i = 0; i < count; i++)
        {
            if (IsIdleFull())
            {
                return;
            }

            idle.Push(CreateNew());
        }
    }

    /// <summary>
    /// Acquires an instance.
    /// </summary>
    public T Get()
    {
        T instance;
        if (idle.Count > 0)
        {
            instance = idle.Pop();
            totalReused++;
        }
        else
        {
            instance = CreateNew();
            totalCreatedOnAcquire++;
        }

        active.Add(instance);
        ObjectPoolManager.MapObject(instance, PoolName);
        totalAcquired++;

        if (instance is IPoolable poolable)
        {
            poolable.OnPoolAcquire();
        }

        return instance;
    }

    /// <summary>
    /// Releases an instance back to the pool.
    /// </summary>
    /// <param name="instance">Instance acquired from this pool.</param>
    public void Release(T instance)
    {
        ArgumentNullException.ThrowIfNull(instance);
        if (!active.Remove(instance))
        {
            return;
        }

        totalReleased++;
        ObjectPoolManager.UnmapObject(instance);

        if (instance is IPoolable poolable)
        {
            poolable.OnPoolRelease();
        }

        resetMethod?.Invoke(instance);

        if (instance is IPoolable poolableReset)
        {
            poolableReset.OnPoolReset();
        }

        if (IsIdleFull())
        {
            totalDiscarded++;
            DisposeInstance(instance);
            return;
        }

        idle.Push(instance);
    }

    /// <inheritdoc />
    public bool ReleaseObject(object instance)
    {
        if (instance is not T typedInstance)
        {
            return false;
        }

        Release(typedInstance);
        return true;
    }

    /// <summary>
    /// Releases every active instance.
    /// </summary>
    public void ReleaseAll()
    {
        var snapshot = new List<T>(active);
        for (var i = 0; i < snapshot.Count; i++)
        {
            Release(snapshot[i]);
        }
    }

    /// <summary>
    /// Applies an action to an active snapshot.
    /// </summary>
    /// <param name="action">Action invoked for each active instance.</param>
    public void ForEachActive(Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        var snapshot = new List<T>(active);
        for (var i = 0; i < snapshot.Count; i++)
        {
            action(snapshot[i]);
        }
    }

    /// <summary>
    /// Clears idle instances.
    /// </summary>
    public void Clear()
    {
        while (idle.Count > 0)
        {
            DisposeInstance(idle.Pop());
        }
    }

    /// <summary>
    /// Destroys the pool and unregisters it.
    /// </summary>
    public void Destroy()
    {
        ReleaseAll();
        Clear();
        ObjectPoolManager.Unregister(this);
    }

    /// <summary>
    /// Returns current statistics.
    /// </summary>
    public PoolStats GetStats()
    {
        return new PoolStats(
            PoolName,
            idle.Count,
            active.Count,
            totalCreated,
            totalAcquired,
            totalReused,
            totalCreatedOnAcquire,
            totalReleased,
            totalDiscarded);
    }

    private T CreateNew()
    {
        totalCreated++;
        return createFunc();
    }

    private bool IsIdleFull()
    {
        return config.MaxSize >= 0 && idle.Count >= config.MaxSize;
    }

    private static void DisposeInstance(T instance)
    {
        if (instance is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
