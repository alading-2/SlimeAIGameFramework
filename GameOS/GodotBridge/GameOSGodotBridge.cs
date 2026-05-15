using System;
using Godot;
using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.World;

namespace SlimeAI.GameOS.GodotBridge;

/// <summary>
/// Godot SceneTree 与 GameOS Runtime 生命周期的桥接入口。
/// </summary>
public static class GameOSGodotBridge
{
    /// <summary>
    /// 注册 Godot Entity，并可递归注册其子 Component。
    /// </summary>
    /// <param name="entityNode">Entity 所在 Godot 节点。</param>
    /// <param name="entity">运行时 Entity 契约。</param>
    /// <param name="registerComponents">是否扫描并注册子 Component。</param>
    public static bool RegisterEntity(Node entityNode, IEntity entity, bool registerComponents = true)
    {
        ArgumentNullException.ThrowIfNull(entityNode);
        ArgumentNullException.ThrowIfNull(entity);

        var nodeRegistered = GodotNodeRegistry.Register(entityNode, entity.EntityId.Value);
        var entityRegistered = EntityManager.Register(entity);
        if (registerComponents && (nodeRegistered || entityRegistered))
        {
            RegisterComponents(entityNode, entity);
        }

        return nodeRegistered || entityRegistered;
    }

    /// <summary>
    /// 注销 Godot Entity。
    /// </summary>
    /// <param name="entityNode">Entity 所在 Godot 节点。</param>
    /// <param name="entity">运行时 Entity 契约。</param>
    /// <param name="unregisterComponents">是否同步注销子 Component。</param>
    /// <param name="destroyRuntimeEntity">是否从 Runtime EntityManager 移除。</param>
    public static bool UnregisterEntity(
        Node entityNode,
        IEntity entity,
        bool unregisterComponents = true,
        bool destroyRuntimeEntity = true)
    {
        ArgumentNullException.ThrowIfNull(entityNode);
        ArgumentNullException.ThrowIfNull(entity);

        if (unregisterComponents)
        {
            UnregisterComponents(entityNode, entity);
        }

        var entityDestroyed = destroyRuntimeEntity && EntityManager.Destroy(entity.EntityId);
        var nodeUnregistered = GodotNodeRegistry.Unregister(entityNode, entity.EntityId.Value);
        return entityDestroyed || nodeUnregistered;
    }

    /// <summary>
    /// 通过 Godot 队列销毁 Entity 节点。
    /// </summary>
    /// <param name="entityNode">Entity 所在 Godot 节点。</param>
    /// <param name="entity">运行时 Entity 契约。</param>
    public static bool DestroyEntity(Node entityNode, IEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entityNode);
        ArgumentNullException.ThrowIfNull(entity);

        if (entityNode.IsInsideTree())
        {
            if (!entityNode.IsQueuedForDeletion())
            {
                entityNode.QueueFree();
            }

            return true;
        }

        return UnregisterEntity(entityNode, entity);
    }

    /// <summary>
    /// 注册 Entity 的子 Component。
    /// </summary>
    /// <param name="entityNode">Entity 所在 Godot 节点。</param>
    /// <param name="entity">运行时 Entity 契约。</param>
    public static int RegisterComponents(Node entityNode, IEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entityNode);
        ArgumentNullException.ThrowIfNull(entity);

        var count = 0;
        var children = entityNode.FindChildren("*", "Node", recursive: true, owned: false);
        foreach (var child in children)
        {
            if (child is Node componentNode && IsComponentNode(componentNode))
            {
                if (RegisterComponent(entityNode, entity, componentNode))
                {
                    count++;
                }
            }
        }

        return count;
    }

    /// <summary>
    /// 注销 Entity 的子 Component。
    /// </summary>
    /// <param name="entityNode">Entity 所在 Godot 节点。</param>
    /// <param name="entity">运行时 Entity 契约。</param>
    public static int UnregisterComponents(Node entityNode, IEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entityNode);
        ArgumentNullException.ThrowIfNull(entity);

        var count = 0;
        var componentIds = GodotNodeRegistry.GetAdaptersByEntity(entity.EntityId);

        for (var i = 0; i < componentIds.Count; i++)
        {
            var componentId = componentIds[i];
            var componentNode = GodotNodeRegistry.GetNodeById(componentId);
            if (componentNode is IGodotComponent component)
            {
                using var guard = RuntimeWorld.Default.Commands.EnterGuard("godot-bridge-callback");
                component.OnComponentUnregistered(entity, entityNode);
            }

            GodotNodeRegistry.UnregisterAdapter(entity.EntityId, componentId);
            if (componentNode != null)
            {
                GodotNodeRegistry.Unregister(componentNode, componentId);
            }

            count++;
        }

        return count;
    }

    /// <summary>
    /// 按 EntityId 查找 Godot Entity 节点。
    /// </summary>
    /// <param name="entityId">稳定运行时 EntityId。</param>
    public static Node? GetEntityNode(EntityId entityId)
    {
        return GodotNodeRegistry.GetNodeById(entityId.Value);
    }

    /// <summary>
    /// 读取当前桥接统计。
    /// </summary>
    public static GodotBridgeStats GetStats()
    {
        var nodes = GodotNodeRegistry.GetAllNodes();
        var entityCount = 0;
        var componentCount = 0;
        for (var i = 0; i < nodes.Count; i++)
        {
            if (nodes[i] is IEntity)
            {
                entityCount++;
            }

            if (IsComponentNode(nodes[i]))
            {
                componentCount++;
            }
        }

        return new GodotBridgeStats(nodes.Count, entityCount, componentCount);
    }

    private static bool RegisterComponent(Node entityNode, IEntity entity, Node componentNode)
    {
        var componentId = GodotNodeRegistry.GetNodeInstanceId(componentNode);
        var nodeRegistered = GodotNodeRegistry.Register(componentNode, componentId);
        var adapterRegistered = GodotNodeRegistry.RegisterAdapter(entity.EntityId, componentId);

        if (nodeRegistered && componentNode is IGodotComponent component)
        {
            using var guard = RuntimeWorld.Default.Commands.EnterGuard("godot-bridge-callback");
            component.OnComponentRegistered(entity, entityNode);
        }

        return nodeRegistered || adapterRegistered;
    }

    private static bool IsComponentNode(Node node)
    {
        return node is IGodotComponent || node.GetType().Name.EndsWith("Component", StringComparison.Ordinal);
    }
}
