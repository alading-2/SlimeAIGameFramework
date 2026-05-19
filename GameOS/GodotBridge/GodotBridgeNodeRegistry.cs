using System;
using System.Collections.Generic;
using Godot;
using SlimeAI.GameOS.Runtime.Entity;

namespace SlimeAI.GameOS.GodotBridge;

/// <summary>
/// GodotBridge context 持有的 Node / Adapter 注册表。
/// </summary>
/// <remarks>
/// 该注册表只表达 GodotBridge adapter 绑定，不进入 LifecycleTree 或业务 DataKey。
/// </remarks>
public sealed class GodotBridgeNodeRegistry
{
    private readonly Dictionary<string, Node> nodes = new(StringComparer.Ordinal);
    private readonly Dictionary<EntityId, List<string>> entityToAdapterIds = new();
    private readonly Dictionary<string, EntityId> adapterIdToEntity = new(StringComparer.Ordinal);

    /// <summary>
    /// 注册 Node。
    /// </summary>
    public bool Register(Node node, string? nodeId = null)
    {
        ArgumentNullException.ThrowIfNull(node);
        var id = string.IsNullOrWhiteSpace(nodeId) ? GodotNodeRegistry.GetNodeInstanceId(node) : nodeId;
        if (nodes.ContainsKey(id))
        {
            return false;
        }

        nodes.Add(id, node);
        return true;
    }

    /// <summary>
    /// 注销 Node。
    /// </summary>
    public bool Unregister(Node node, string? nodeId = null)
    {
        ArgumentNullException.ThrowIfNull(node);
        var id = string.IsNullOrWhiteSpace(nodeId) ? GodotNodeRegistry.GetNodeInstanceId(node) : nodeId;
        return nodes.Remove(id);
    }

    /// <summary>
    /// 按稳定 Id 查找 Node。
    /// </summary>
    public Node? GetNodeById(string nodeId)
    {
        return nodes.GetValueOrDefault(nodeId);
    }

    /// <summary>
    /// 按类型返回 Node 快照。
    /// </summary>
    public IReadOnlyList<T> GetNodesByType<T>() where T : Node
    {
        var result = new List<T>();
        foreach (var node in nodes.Values)
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
    public IReadOnlyList<Node> GetAllNodes()
    {
        return new List<Node>(nodes.Values);
    }

    /// <summary>
    /// 判断稳定 Id 是否已注册。
    /// </summary>
    public bool IsRegistered(string nodeId)
    {
        return nodes.ContainsKey(nodeId);
    }

    /// <summary>
    /// 把 adapter id 注册到指定 Runtime Entity。
    /// </summary>
    public bool RegisterAdapter(EntityId entityId, string adapterId)
    {
        if (entityId.IsEmpty || string.IsNullOrWhiteSpace(adapterId))
        {
            return false;
        }

        if (adapterIdToEntity.ContainsKey(adapterId))
        {
            return false;
        }

        if (!entityToAdapterIds.TryGetValue(entityId, out var list))
        {
            list = new List<string>();
            entityToAdapterIds[entityId] = list;
        }

        list.Add(adapterId);
        adapterIdToEntity[adapterId] = entityId;
        return true;
    }

    /// <summary>
    /// 解除 adapter id 与 Runtime Entity 的关联。
    /// </summary>
    public bool UnregisterAdapter(EntityId entityId, string adapterId)
    {
        if (entityId.IsEmpty || string.IsNullOrWhiteSpace(adapterId))
        {
            return false;
        }

        if (!entityToAdapterIds.TryGetValue(entityId, out var list))
        {
            return false;
        }

        var removed = list.Remove(adapterId);
        if (list.Count == 0)
        {
            entityToAdapterIds.Remove(entityId);
        }

        adapterIdToEntity.Remove(adapterId);
        return removed;
    }

    /// <summary>
    /// 判断 adapter 是否属于指定 Runtime Entity。
    /// </summary>
    public bool IsAdapterRegistered(EntityId entityId, string adapterId)
    {
        return !entityId.IsEmpty
            && adapterIdToEntity.TryGetValue(adapterId, out var owner)
            && owner.Equals(entityId);
    }

    /// <summary>
    /// 返回指定 Runtime Entity 下挂的 adapter id 快照。
    /// </summary>
    public IReadOnlyList<string> GetAdaptersByEntity(EntityId entityId)
    {
        if (entityId.IsEmpty || !entityToAdapterIds.TryGetValue(entityId, out var list) || list.Count == 0)
        {
            return Array.Empty<string>();
        }

        return new List<string>(list);
    }

    /// <summary>
    /// 清空注册表。
    /// </summary>
    public void Clear()
    {
        nodes.Clear();
        entityToAdapterIds.Clear();
        adapterIdToEntity.Clear();
    }
}
