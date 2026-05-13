using Godot;
using SlimeAI.GameOS.Runtime.Pool;

namespace SlimeAI.GameOS.GodotBridge;

/// <summary>
/// Convenience entry points for Godot Node pools.
/// </summary>
public static class GodotNodePoolManager
{
    /// <summary>
    /// Gets a typed Godot Node pool by name.
    /// </summary>
    /// <param name="name">Pool name.</param>
    public static GodotNodePool<T>? GetPool<T>(string name) where T : Node
    {
        return ObjectPoolManager.GetNamedPool(name) as GodotNodePool<T>;
    }

    /// <summary>
    /// Returns a node to its mapped pool.
    /// </summary>
    /// <param name="node">Pooled node instance.</param>
    public static bool ReturnToPool(Node node)
    {
        return ObjectPoolManager.ReturnToPool(node);
    }
}
