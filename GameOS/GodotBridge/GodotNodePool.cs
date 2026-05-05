using System;
using System.Collections.Generic;
using Godot;
using SkilmeAI.GameOS.Runtime.Pool;

namespace SkilmeAI.GameOS.GodotBridge;

/// <summary>
/// Object pool for Godot Node instances with SceneTree parking and collision isolation.
/// </summary>
/// <typeparam name="T">Pooled Godot node type.</typeparam>
public sealed class GodotNodePool<T> : IObjectPool where T : Node
{
    private readonly Stack<T> idle;
    private readonly HashSet<T> active = new();
    private readonly Func<T> createFunc;
    private readonly Action<T>? resetMethod;
    private readonly GodotNodePoolConfig config;

    private int totalCreated;
    private int totalAcquired;
    private int totalReused;
    private int totalCreatedOnAcquire;
    private int totalReleased;
    private int totalDiscarded;

    /// <summary>
    /// Creates and registers a Godot Node pool.
    /// </summary>
    /// <param name="createFunc">Factory used to instantiate nodes.</param>
    /// <param name="config">Pool configuration.</param>
    /// <param name="resetMethod">Optional reset callback invoked during release.</param>
    public GodotNodePool(Func<T> createFunc, GodotNodePoolConfig config, Action<T>? resetMethod = null)
    {
        this.createFunc = createFunc ?? throw new ArgumentNullException(nameof(createFunc));
        this.config = config;
        this.resetMethod = resetMethod;
        idle = new Stack<T>(Math.Max(0, config.InitialSize));

        Warmup(config.InitialSize);
        ObjectPoolManager.RegisterPool(this);
    }

    /// <inheritdoc />
    public string PoolName => config.Name;

    /// <summary>
    /// Number of idle nodes.
    /// </summary>
    public int Count => idle.Count;

    /// <summary>
    /// Number of acquired nodes.
    /// </summary>
    public int ActiveCount => active.Count;

    /// <summary>
    /// Creates idle nodes ahead of gameplay.
    /// </summary>
    /// <param name="count">Number of nodes to create.</param>
    public void Warmup(int count)
    {
        for (var i = 0; i < count; i++)
        {
            if (IsIdleFull())
            {
                return;
            }

            PushIdle(CreateNew());
        }
    }

    /// <summary>
    /// Acquires a node and activates it immediately.
    /// </summary>
    public T Get()
    {
        return Get(activateNode: true);
    }

    /// <summary>
    /// Acquires a node with optional delayed activation.
    /// </summary>
    /// <param name="activateNode">Whether to restore processing, visibility and collision immediately.</param>
    public T Get(bool activateNode)
    {
        T node;
        if (idle.Count > 0)
        {
            node = idle.Pop();
            totalReused++;
        }
        else
        {
            node = CreateNew();
            totalCreatedOnAcquire++;
        }

        active.Add(node);
        ObjectPoolManager.MapObject(node, PoolName);
        totalAcquired++;

        node.SetMeta("GameOSPoolName", PoolName);
        node.SetMeta("GameOSInPool", false);
        ReattachToTree(node);

        if (activateNode)
        {
            Activate(node);
        }

        return node;
    }

    /// <summary>
    /// Activates a delayed-acquired node after caller-side initialization.
    /// </summary>
    /// <param name="node">Node acquired from this pool.</param>
    public void Activate(T node)
    {
        ArgumentNullException.ThrowIfNull(node);
        if (!active.Contains(node))
        {
            return;
        }

        ReattachToTree(node);
        ApplyActiveState(node);

        if (node is IPoolable poolable)
        {
            poolable.OnPoolAcquire();
        }
    }

    /// <summary>
    /// Releases a node back to idle state.
    /// </summary>
    /// <param name="node">Node acquired from this pool.</param>
    public void Release(T node)
    {
        ArgumentNullException.ThrowIfNull(node);
        if (!active.Remove(node))
        {
            return;
        }

        totalReleased++;
        ObjectPoolManager.UnmapObject(node);

        if (node is IPoolable poolable)
        {
            poolable.OnPoolRelease();
        }

        resetMethod?.Invoke(node);

        if (node is IPoolable poolableReset)
        {
            poolableReset.OnPoolReset();
        }

        if (IsIdleFull())
        {
            totalDiscarded++;
            Discard(node);
            return;
        }

        PushIdle(node);
    }

    /// <inheritdoc />
    public bool ReleaseObject(object instance)
    {
        if (instance is not T typedNode)
        {
            return false;
        }

        Release(typedNode);
        return true;
    }

    /// <summary>
    /// Releases every active node.
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
    /// Clears idle nodes.
    /// </summary>
    public void Clear()
    {
        while (idle.Count > 0)
        {
            Discard(idle.Pop());
        }
    }

    /// <inheritdoc />
    public void Destroy()
    {
        ReleaseAll();
        Clear();
        ObjectPoolManager.UnregisterPool(this);
    }

    /// <inheritdoc />
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
        var node = createFunc();
        node.Name = $"{PoolName}_{Guid.NewGuid().ToString()[..8]}";
        node.SetMeta("GameOSPoolName", PoolName);
        totalCreated++;
        return node;
    }

    private void PushIdle(T node)
    {
        node.SetMeta("GameOSInPool", true);
        ApplyInactiveState(node);
        idle.Push(node);
    }

    private void ApplyInactiveState(T node)
    {
        node.ProcessMode = Node.ProcessModeEnum.Disabled;
        if (node is CanvasItem item)
        {
            item.Visible = false;
        }

        if (node is Node2D node2D)
        {
            node2D.GlobalPosition = config.ParkingPosition;
            if (node2D.IsInsideTree())
            {
                node2D.ForceUpdateTransform();
            }
        }

        GodotCollisionIsolation.DisableDeferred(node);

        if (ShouldDetach(node))
        {
            node.GetParent()?.RemoveChild(node);
        }
    }

    private static void ApplyActiveState(T node)
    {
        node.ProcessMode = Node.ProcessModeEnum.Inherit;
        if (node is CanvasItem item)
        {
            item.Visible = true;
        }

        if (node is Node2D node2D && node2D.IsInsideTree())
        {
            node2D.ForceUpdateTransform();
        }

        GodotCollisionIsolation.Restore(node);
    }

    private void ReattachToTree(T node)
    {
        if (node.GetParent() != null)
        {
            return;
        }

        if (config.ActiveParent == null)
        {
            return;
        }

        config.ActiveParent.AddChild(node);
        GodotCollisionIsolation.DisableDirect(node);
    }

    private bool ShouldDetach(Node node)
    {
        return config.DetachCollisionObjects && GodotCollisionIsolation.NeedsTreeDetach(node);
    }

    private void Discard(T node)
    {
        if (node.GetParent() == null)
        {
            node.Free();
            return;
        }

        node.QueueFree();
    }

    private bool IsIdleFull()
    {
        return config.MaxSize >= 0 && idle.Count >= config.MaxSize;
    }
}
