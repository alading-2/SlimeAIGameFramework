using System;
using Godot;
using SlimeAI.GameOS.Runtime.Entity;

namespace SlimeAI.GameOS.GodotBridge;

/// <summary>
/// Godot SceneTree 与 GameOS Runtime 生命周期的桥接入口。
/// </summary>
public static class GameOSGodotBridge
{
    /// <summary>
    /// 默认 GodotBridge context。旧 static facade 全部转发到这里。
    /// </summary>
    public static GodotBridgeContext DefaultContext { get; } = new();

    /// <summary>
    /// 注册 Godot Entity，并可递归注册其子 Component。
    /// </summary>
    /// <param name="entityNode">Entity 所在 Godot 节点。</param>
    /// <param name="entity">运行时 Entity 契约。</param>
    /// <param name="registerComponents">是否扫描并注册子 Component。</param>
    public static bool RegisterEntity(Node entityNode, IEntity entity, bool registerComponents = true)
    {
        return DefaultContext.RegisterEntity(entityNode, entity, registerComponents);
    }

    /// <summary>
    /// 使用显式 context 注册 Godot Entity。
    /// </summary>
    public static bool RegisterEntity(GodotBridgeContext context, Node entityNode, IEntity entity, bool registerComponents = true)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.RegisterEntity(entityNode, entity, registerComponents);
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
        return DefaultContext.UnregisterEntity(entityNode, entity, unregisterComponents, destroyRuntimeEntity);
    }

    /// <summary>
    /// 使用显式 context 注销 Godot Entity。
    /// </summary>
    public static bool UnregisterEntity(
        GodotBridgeContext context,
        Node entityNode,
        IEntity entity,
        bool unregisterComponents = true,
        bool destroyRuntimeEntity = true)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.UnregisterEntity(entityNode, entity, unregisterComponents, destroyRuntimeEntity);
    }

    /// <summary>
    /// 通过 Godot 队列销毁 Entity 节点。
    /// </summary>
    /// <param name="entityNode">Entity 所在 Godot 节点。</param>
    /// <param name="entity">运行时 Entity 契约。</param>
    public static bool DestroyEntity(Node entityNode, IEntity entity)
    {
        return DefaultContext.DestroyEntity(entityNode, entity);
    }

    /// <summary>
    /// 注册 Entity 的子 Component。
    /// </summary>
    /// <param name="entityNode">Entity 所在 Godot 节点。</param>
    /// <param name="entity">运行时 Entity 契约。</param>
    public static int RegisterComponents(Node entityNode, IEntity entity)
    {
        return DefaultContext.RegisterComponents(entityNode, entity);
    }

    /// <summary>
    /// 注销 Entity 的子 Component。
    /// </summary>
    /// <param name="entityNode">Entity 所在 Godot 节点。</param>
    /// <param name="entity">运行时 Entity 契约。</param>
    public static int UnregisterComponents(Node entityNode, IEntity entity)
    {
        return DefaultContext.UnregisterComponents(entityNode, entity);
    }

    /// <summary>
    /// 按 EntityId 查找 Godot Entity 节点。
    /// </summary>
    /// <param name="entityId">稳定运行时 EntityId。</param>
    public static Node? GetEntityNode(EntityId entityId)
    {
        return DefaultContext.GetEntityNode(entityId);
    }

    /// <summary>
    /// 读取当前桥接统计。
    /// </summary>
    public static GodotBridgeStats GetStats()
    {
        return DefaultContext.GetStats();
    }

    internal static bool IsComponentNode(Node node)
    {
        return node is IGodotComponent || node.GetType().Name.EndsWith("Component", StringComparison.Ordinal);
    }
}
