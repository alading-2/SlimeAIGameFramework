using Godot;

namespace SlimeAI.GameOS.GodotBridge;

/// <summary>
/// Configuration for a Godot Node object pool.
/// </summary>
public readonly record struct GodotNodePoolConfig
{
    /// <summary>
    /// Pool name used by global return-to-pool mapping.
    /// </summary>
    public string Name { get; init; } = "UnnamedGodotNodePool";

    /// <summary>
    /// Number of nodes created during construction.
    /// </summary>
    public int InitialSize { get; init; } = 0;

    /// <summary>
    /// Maximum idle nodes retained by the pool. -1 means unlimited.
    /// </summary>
    public int MaxSize { get; init; } = -1;

    /// <summary>
    /// Parent used when an idle detached node is acquired again.
    /// </summary>
    public Node? ActiveParent { get; init; }

    /// <summary>
    /// Parking position applied before a physics node is detached from SceneTree.
    /// </summary>
    public Vector2 ParkingPosition { get; init; } = new(1000000f, 1000000f);

    /// <summary>
    /// Whether pooled CollisionObject2D roots are detached while idle.
    /// </summary>
    public bool DetachCollisionObjects { get; init; } = true;

    /// <summary>
    /// Creates a default config.
    /// </summary>
    public GodotNodePoolConfig()
    {
    }
}
