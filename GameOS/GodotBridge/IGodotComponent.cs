using Godot;
using SkilmeAI.GameOS.Runtime.Entity;

namespace SkilmeAI.GameOS.GodotBridge;

/// <summary>
/// Godot Node Component 的最小生命周期协议。
/// </summary>
public interface IGodotComponent
{
    /// <summary>
    /// Component 已绑定到所属 Entity。
    /// </summary>
    /// <param name="entity">所属运行时 Entity。</param>
    /// <param name="entityNode">所属 Godot 节点。</param>
    void OnComponentRegistered(IEntity entity, Node entityNode)
    {
    }

    /// <summary>
    /// Component 即将从所属 Entity 解绑。
    /// </summary>
    /// <param name="entity">所属运行时 Entity。</param>
    /// <param name="entityNode">所属 Godot 节点。</param>
    void OnComponentUnregistered(IEntity? entity, Node? entityNode)
    {
    }
}
