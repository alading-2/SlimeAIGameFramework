using System;
using Godot;
using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.World;

namespace SlimeAI.GameOS.GodotBridge;

/// <summary>
/// GodotBridge 运行时上下文，绑定一个 RuntimeWorld 和一份 Node/Adapter registry。
/// </summary>
public sealed class GodotBridgeContext
{
    /// <summary>
    /// 创建 GodotBridge context。
    /// </summary>
    /// <param name="world">目标 RuntimeWorld；为空时使用默认 world。</param>
    /// <param name="registry">目标 registry；为空时创建独立实例。</param>
    public GodotBridgeContext(RuntimeWorld? world = null, GodotBridgeNodeRegistry? registry = null)
    {
        World = world ?? RuntimeWorld.Default;
        Registry = registry ?? new GodotBridgeNodeRegistry();
    }

    /// <summary>当前桥接写入的 RuntimeWorld。</summary>
    public RuntimeWorld World { get; }

    /// <summary>当前桥接使用的 Node/Adapter registry。</summary>
    public GodotBridgeNodeRegistry Registry { get; }

    /// <summary>
    /// 注册 Godot Entity，并可递归注册其子 Adapter。
    /// </summary>
    public bool RegisterEntity(Node entityNode, IEntity entity, bool registerComponents = true)
    {
        ArgumentNullException.ThrowIfNull(entityNode);
        ArgumentNullException.ThrowIfNull(entity);

        var nodeRegistered = Registry.Register(entityNode, entity.EntityId.Value);
        var entityRegistered = World.Entities.Register(entity);
        if (registerComponents && (nodeRegistered || entityRegistered))
        {
            RegisterComponents(entityNode, entity);
        }

        return nodeRegistered || entityRegistered;
    }

    /// <summary>
    /// 注销 Godot Entity。
    /// </summary>
    public bool UnregisterEntity(
        Node entityNode,
        IEntity entity,
        bool unregisterComponents = true,
        bool destroyRuntimeEntity = true)
    {
        ArgumentNullException.ThrowIfNull(entityNode);
        ArgumentNullException.ThrowIfNull(entity);

        if (!IsRegisteredEntityNode(entityNode, entity))
        {
            return false;
        }

        if (unregisterComponents)
        {
            UnregisterComponents(entityNode, entity);
        }

        var entityDestroyed = destroyRuntimeEntity && World.Entities.Destroy(entity.EntityId);
        var nodeUnregistered = Registry.Unregister(entityNode, entity.EntityId.Value);
        return entityDestroyed || nodeUnregistered;
    }

    /// <summary>
    /// 通过 Godot 队列销毁 Entity 节点。
    /// </summary>
    public bool DestroyEntity(Node entityNode, IEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entityNode);
        ArgumentNullException.ThrowIfNull(entity);

        if (entityNode.IsInsideTree())
        {
            UnregisterEntityImmediate(entityNode, entity);
            if (!entityNode.IsQueuedForDeletion())
            {
                entityNode.QueueFree();
            }

            return true;
        }

        return UnregisterEntity(entityNode, entity);
    }

    /// <summary>
    /// 注册 Entity 的子 Adapter。
    /// </summary>
    public int RegisterComponents(Node entityNode, IEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entityNode);
        ArgumentNullException.ThrowIfNull(entity);

        var count = 0;
        var children = entityNode.FindChildren("*", "Node", recursive: true, owned: false);
        foreach (var child in children)
        {
            if (child is Node componentNode && GameOSGodotBridge.IsComponentNode(componentNode))
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
    /// 注销 Entity 的子 Adapter。
    /// </summary>
    public int UnregisterComponents(Node entityNode, IEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entityNode);
        ArgumentNullException.ThrowIfNull(entity);

        var count = 0;
        var componentIds = Registry.GetAdaptersByEntity(entity.EntityId);

        for (var i = 0; i < componentIds.Count; i++)
        {
            var componentId = componentIds[i];
            var componentNode = Registry.GetNodeById(componentId);
            if (componentNode is IGodotComponent component)
            {
                using var guard = World.Commands.EnterGuard("godot-bridge-callback");
                component.OnComponentUnregistered(entity, entityNode);
            }

            Registry.UnregisterAdapter(entity.EntityId, componentId);
            if (componentNode != null)
            {
                Registry.Unregister(componentNode, componentId);
            }

            count++;
        }

        return count;
    }

    /// <summary>
    /// 按 EntityId 查找 Godot Entity 节点。
    /// </summary>
    public Node? GetEntityNode(EntityId entityId)
    {
        return Registry.GetNodeById(entityId.Value);
    }

    /// <summary>
    /// 读取当前桥接统计。
    /// </summary>
    public GodotBridgeStats GetStats()
    {
        var nodes = Registry.GetAllNodes();
        var entityCount = 0;
        var componentCount = 0;
        for (var i = 0; i < nodes.Count; i++)
        {
            if (nodes[i] is IEntity)
            {
                entityCount++;
            }

            if (GameOSGodotBridge.IsComponentNode(nodes[i]))
            {
                componentCount++;
            }
        }

        return new GodotBridgeStats(nodes.Count, entityCount, componentCount);
    }

    private bool RegisterComponent(Node entityNode, IEntity entity, Node componentNode)
    {
        var componentId = GodotNodeRegistry.GetNodeInstanceId(componentNode);
        var nodeRegistered = Registry.Register(componentNode, componentId);
        var adapterRegistered = Registry.RegisterAdapter(entity.EntityId, componentId, componentNode.GetType().Name);

        if (nodeRegistered && componentNode is IGodotComponent component)
        {
            using var guard = World.Commands.EnterGuard("godot-bridge-callback");
            component.OnComponentRegistered(entity, entityNode);
        }

        return nodeRegistered || adapterRegistered;
    }

    private bool UnregisterEntityImmediate(Node entityNode, IEntity entity)
    {
        if (!IsRegisteredEntityNode(entityNode, entity))
        {
            return false;
        }

        UnregisterComponents(entityNode, entity);
        var entityDestroyed = World.EntityRegistry.DestroyImmediate(entity.EntityId);
        var nodeUnregistered = Registry.Unregister(entityNode, entity.EntityId.Value);
        return entityDestroyed || nodeUnregistered;
    }

    private bool IsRegisteredEntityNode(Node entityNode, IEntity entity)
    {
        return Registry.GetNodeById(entity.EntityId.Value) == entityNode;
    }
}
