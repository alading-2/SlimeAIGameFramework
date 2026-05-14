using SlimeAI.GameOS.Capabilities.Movement;
using SlimeAI.GameOS.Runtime.Entity;

namespace SlimeAI.GameOS.Capabilities.Effect;

/// <summary>
/// 效果生成参数。
/// </summary>
public sealed class EffectSpawnOptions
{
    /// <summary>来源实体。</summary>
    public required IEntity Source { get; init; }

    /// <summary>关联技能实体。</summary>
    public IEntity? Ability { get; init; }

    /// <summary>目标实体。</summary>
    public IEntity? Target { get; init; }

    /// <summary>效果实体 Id；为 <see cref="Runtime.Entity.EntityId.Empty"/> 时由 EntityManager 生成。</summary>
    public EntityId EntityId { get; init; }

    /// <summary>效果资源路径。</summary>
    public string? ScenePath { get; init; }

    /// <summary>效果名称。</summary>
    public string? Name { get; init; }

    /// <summary>播放动画名；为空时由视觉节点使用当前动画或第一个可用动画。</summary>
    public string? AnimationName { get; init; }

    /// <summary>生成位置。</summary>
    public Vector2Value Position { get; init; } = Vector2Value.Zero;

    /// <summary>持续时间，-1 表示不自动结束。</summary>
    public float Duration { get; init; } = -1f;
}
