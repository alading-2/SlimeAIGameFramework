using System;
using System.Collections.Generic;
using Godot;

namespace SlimeAI.GameOS.GodotBridge;

/// <summary>
/// Godot Node 运行时注册表，用于桥接 SceneTree 与 GameOS Runtime Id。
/// </summary>
public static class GodotNodeRegistry
{
    private static readonly Dictionary<string, Node> Nodes = new(StringComparer.Ordinal);

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
    /// 清空注册表。
    /// </summary>
    public static void Clear()
    {
        Nodes.Clear();
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
