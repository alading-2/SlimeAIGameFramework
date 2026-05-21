using System.Collections.Generic;
using Godot;
using SlimeAI.GameOS.Runtime.Entity;

namespace SlimeAI.GameOS.GodotBridge;

/// <summary>
/// Godot Node 运行时注册表，用于桥接 SceneTree 与 GameOS Runtime Id。
/// </summary>
/// <remarks>
/// 同时维护 entity -> adapter ids 的 typed registry，替代旧
/// <c>RelationshipType.EntityToComponent</c> 在 framework Relationship 层的存储；
/// 此 registry 不进入 <see cref="LifecycleTree"/>。
/// </remarks>
public static class GodotNodeRegistry
{
    /// <summary>
    /// 注册 Node。
    /// </summary>
    /// <param name="node">Godot 节点。</param>
    /// <param name="nodeId">稳定运行时 Id；为空时使用 Godot InstanceId。</param>
    public static bool Register(Node node, string? nodeId = null)
    {
        return GameOSGodotBridge.DefaultContext.Registry.Register(node, nodeId);
    }

    /// <summary>
    /// 注销 Node。
    /// </summary>
    /// <param name="node">Godot 节点。</param>
    /// <param name="nodeId">注册时使用的稳定 Id。</param>
    public static bool Unregister(Node node, string? nodeId = null)
    {
        return GameOSGodotBridge.DefaultContext.Registry.Unregister(node, nodeId);
    }

    /// <summary>
    /// 按稳定 Id 查找 Node。
    /// </summary>
    /// <param name="nodeId">注册时使用的稳定 Id。</param>
    public static Node? GetNodeById(string nodeId)
    {
        return GameOSGodotBridge.DefaultContext.Registry.GetNodeById(nodeId);
    }

    /// <summary>
    /// 按类型返回 Node 快照。
    /// </summary>
    public static IReadOnlyList<T> GetNodesByType<T>() where T : Node
    {
        return GameOSGodotBridge.DefaultContext.Registry.GetNodesByType<T>();
    }

    /// <summary>
    /// 返回全部 Node 快照。
    /// </summary>
    public static IReadOnlyList<Node> GetAllNodes()
    {
        return GameOSGodotBridge.DefaultContext.Registry.GetAllNodes();
    }

    /// <summary>
    /// 判断稳定 Id 是否已注册。
    /// </summary>
    public static bool IsRegistered(string nodeId)
    {
        return GameOSGodotBridge.DefaultContext.Registry.IsRegistered(nodeId);
    }

    /// <summary>
    /// 把 <paramref name="adapterId"/> 注册到 <paramref name="entityId"/> 名下，
    /// 替代旧 Relationship 层的 <c>EntityToComponent</c>。
    /// </summary>
    public static bool RegisterAdapter(EntityId entityId, string adapterId, string? componentType = null)
    {
        return GameOSGodotBridge.DefaultContext.Registry.RegisterAdapter(entityId, adapterId, componentType);
    }

    /// <summary>
    /// 解除 <paramref name="adapterId"/> 与 <paramref name="entityId"/> 的关联。
    /// </summary>
    public static bool UnregisterAdapter(EntityId entityId, string adapterId)
    {
        return GameOSGodotBridge.DefaultContext.Registry.UnregisterAdapter(entityId, adapterId);
    }

    /// <summary>
    /// 判断 <paramref name="entityId"/> 是否注册有 <paramref name="adapterId"/>。
    /// </summary>
    public static bool IsAdapterRegistered(EntityId entityId, string adapterId)
    {
        return GameOSGodotBridge.DefaultContext.Registry.IsAdapterRegistered(entityId, adapterId);
    }

    /// <summary>
    /// 返回 <paramref name="entityId"/> 下挂的 adapter id 列表快照。
    /// </summary>
    public static IReadOnlyList<string> GetAdaptersByEntity(EntityId entityId)
    {
        return GameOSGodotBridge.DefaultContext.Registry.GetAdaptersByEntity(entityId);
    }

    /// <summary>
    /// 清空注册表。
    /// </summary>
    public static void Clear()
    {
        GameOSGodotBridge.DefaultContext.Registry.Clear();
    }

    /// <summary>
    /// 读取 Godot InstanceId 字符串。
    /// </summary>
    /// <param name="node">Godot 节点。</param>
    public static string GetNodeInstanceId(Node node)
    {
        ArgumentNullException.ThrowIfNull(node);
        return node.GetInstanceId().ToString();
    }
}
