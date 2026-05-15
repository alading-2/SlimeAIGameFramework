using System;
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
    private static readonly Dictionary<string, Node> Nodes = new(StringComparer.Ordinal);
    private static readonly Dictionary<EntityId, List<string>> EntityToAdapterIds = new();
    private static readonly Dictionary<string, EntityId> AdapterIdToEntity = new(StringComparer.Ordinal);

    /// <summary>
    /// 注册 Node。
    /// </summary>
    /// <param name="node">Godot 节点。</param>
    /// <param name="nodeId">稳定运行时 Id；为空时使用 Godot InstanceId。</param>
    public static bool Register(Node node, string? nodeId = null)
    {
        ArgumentNullException.ThrowIfNull(node);
        var id = string.IsNullOrWhiteSpace(nodeId) ? GetNodeInstanceId(node) : nodeId;
        if (Nodes.ContainsKey(id))
        {
            return false;
        }

        Nodes.Add(id, node);
        return true;
    }

    /// <summary>
    /// 注销 Node。
    /// </summary>
    /// <param name="node">Godot 节点。</param>
    /// <param name="nodeId">注册时使用的稳定 Id。</param>
    public static bool Unregister(Node node, string? nodeId = null)
    {
        ArgumentNullException.ThrowIfNull(node);
        var id = string.IsNullOrWhiteSpace(nodeId) ? GetNodeInstanceId(node) : nodeId;
        return Nodes.Remove(id);
    }

    /// <summary>
    /// 按稳定 Id 查找 Node。
    /// </summary>
    /// <param name="nodeId">注册时使用的稳定 Id。</param>
    public static Node? GetNodeById(string nodeId)
    {
        return Nodes.GetValueOrDefault(nodeId);
    }

    /// <summary>
    /// 按类型返回 Node 快照。
    /// </summary>
    public static IReadOnlyList<T> GetNodesByType<T>() where T : Node
    {
        var result = new List<T>();
        foreach (var node in Nodes.Values)
        {
            if (node is T typedNode)
            {
                result.Add(typedNode);
            }
        }

        return result;
    }

    /// <summary>
    /// 返回全部 Node 快照。
    /// </summary>
    public static IReadOnlyList<Node> GetAllNodes()
    {
        return new List<Node>(Nodes.Values);
    }

    /// <summary>
    /// 判断稳定 Id 是否已注册。
    /// </summary>
    public static bool IsRegistered(string nodeId)
    {
        return Nodes.ContainsKey(nodeId);
    }

    /// <summary>
    /// 把 <paramref name="adapterId"/> 注册到 <paramref name="entityId"/> 名下，
    /// 替代旧 Relationship 层的 <c>EntityToComponent</c>。
    /// </summary>
    public static bool RegisterAdapter(EntityId entityId, string adapterId)
    {
        if (entityId.IsEmpty || string.IsNullOrWhiteSpace(adapterId))
        {
            return false;
        }

        if (AdapterIdToEntity.ContainsKey(adapterId))
        {
            return false;
        }

        if (!EntityToAdapterIds.TryGetValue(entityId, out var list))
        {
            list = new List<string>();
            EntityToAdapterIds[entityId] = list;
        }

        list.Add(adapterId);
        AdapterIdToEntity[adapterId] = entityId;
        return true;
    }

    /// <summary>
    /// 解除 <paramref name="adapterId"/> 与 <paramref name="entityId"/> 的关联。
    /// </summary>
    public static bool UnregisterAdapter(EntityId entityId, string adapterId)
    {
        if (entityId.IsEmpty || string.IsNullOrWhiteSpace(adapterId))
        {
            return false;
        }

        if (!EntityToAdapterIds.TryGetValue(entityId, out var list))
        {
            return false;
        }

        var removed = list.Remove(adapterId);
        if (list.Count == 0)
        {
            EntityToAdapterIds.Remove(entityId);
        }

        AdapterIdToEntity.Remove(adapterId);
        return removed;
    }

    /// <summary>
    /// 判断 <paramref name="entityId"/> 是否注册有 <paramref name="adapterId"/>。
    /// </summary>
    public static bool IsAdapterRegistered(EntityId entityId, string adapterId)
    {
        return !entityId.IsEmpty
            && AdapterIdToEntity.TryGetValue(adapterId, out var owner)
            && owner.Equals(entityId);
    }

    /// <summary>
    /// 返回 <paramref name="entityId"/> 下挂的 adapter id 列表快照。
    /// </summary>
    public static IReadOnlyList<string> GetAdaptersByEntity(EntityId entityId)
    {
        if (entityId.IsEmpty || !EntityToAdapterIds.TryGetValue(entityId, out var list) || list.Count == 0)
        {
            return Array.Empty<string>();
        }

        return new List<string>(list);
    }

    /// <summary>
    /// 清空注册表。
    /// </summary>
    public static void Clear()
    {
        Nodes.Clear();
        EntityToAdapterIds.Clear();
        AdapterIdToEntity.Clear();
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
