using System;
using SkilmeAI.GameOS.Capabilities.Collision;

namespace SkilmeAI.GameOS.Capabilities.Movement;

/// <summary>
/// 单次 Movement 的纯运行时碰撞策略配置。
/// </summary>
public readonly record struct MovementCollisionParams
{
    /// <summary>
    /// 带默认值的 struct 需要显式无参构造函数。
    /// </summary>
    public MovementCollisionParams()
    {
    }

    /// <summary>基础 layer / mask / team 过滤策略。</summary>
    public CollisionFilterPolicy FilterPolicy { get; init; } = CollisionFilterPolicy.Default;

    /// <summary>目标匹配模式。</summary>
    public MovementCollisionTargetMatchMode TargetMatchMode { get; init; } = MovementCollisionTargetMatchMode.Any;

    /// <summary>TargetMatchMode 为 SpecificEntity 时使用的目标实体 Id。</summary>
    public string? SpecificTargetEntityId { get; init; } = null;

    /// <summary>覆盖移动实体半径，-1 表示读取 CollisionDataKeys.CollisionRadius。</summary>
    public float SourceRadiusOverride { get; init; } = -1f;

    /// <summary>覆盖目标实体半径，-1 表示读取 CollisionDataKeys.CollisionRadius。</summary>
    public float TargetRadiusOverride { get; init; } = -1f;

    /// <summary>有效碰撞累计到多少次后停止，-1 表示只通知不停止。</summary>
    public int StopAfterCollisionCount { get; init; } = -1;

    /// <summary>因碰撞触发停止后是否销毁实体。</summary>
    public bool DestroyOnStop { get; init; } = false;

    /// <summary>是否发出 movement:collision 事件。</summary>
    public bool EmitCollisionEvent { get; init; } = true;

    /// <summary>有效碰撞时的本地回调。</summary>
    public Action<MovementCollisionContext>? OnCollision { get; init; } = null;
}
