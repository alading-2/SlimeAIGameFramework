using SlimeAI.GameOS.Capabilities.Movement;
using SlimeAI.GameOS.Runtime.Data;
using SlimeAI.GameOS.Runtime.Entity;

namespace SlimeAI.GameOS.Capabilities.Effect;

/// <summary>
/// Effect Capability 使用的运行时 DataKey。
/// </summary>
public static class EffectDataKeys
{
    /// <summary>效果资源路径。</summary>
    public static readonly DataKey<string> ScenePath = DataKey.Create<string>("Effect.ScenePath",
        defaultValue: string.Empty,
        category: EffectCategory.Basic);

    /// <summary>效果名称。</summary>
    public static readonly DataKey<string> Name = DataKey.Create<string>("Effect.Name",
        defaultValue: string.Empty,
        category: EffectCategory.Basic);

    /// <summary>效果播放动画名；为空时由 Godot 视觉节点使用当前动画或第一个可用动画。</summary>
    public static readonly DataKey<string> AnimationName = DataKey.Create<string>("Effect.AnimationName",
        defaultValue: string.Empty,
        category: EffectCategory.Basic);

    /// <summary>来源实体。</summary>
    public static readonly DataKey<IEntity?> SourceEntity = DataKey.Create<IEntity?>("Effect.SourceEntity",
        category: EffectCategory.Runtime);

    /// <summary>关联技能实体。</summary>
    public static readonly DataKey<IEntity?> AbilityEntity = DataKey.Create<IEntity?>("Effect.AbilityEntity",
        category: EffectCategory.Runtime);

    /// <summary>目标实体。</summary>
    public static readonly DataKey<IEntity?> TargetEntity = DataKey.Create<IEntity?>("Effect.TargetEntity",
        category: EffectCategory.Runtime);

    /// <summary>效果位置。</summary>
    public static readonly DataKey<Vector2Value> Position = DataKey.Create<Vector2Value>("Effect.Position",
        defaultValue: Vector2Value.Zero,
        category: EffectCategory.Runtime);

    /// <summary>持续时间，-1 表示不自动结束。</summary>
    public static readonly DataKey<float> Duration = DataKey.Create<float>("Effect.Duration",
        defaultValue: -1f,
        category: EffectCategory.Basic,
        minValue: -1f);

    /// <summary>
    /// 显式触发静态 DataKey 注册。
    /// </summary>
    public static void RegisterAll()
    {
        _ = ScenePath;
        _ = Name;
        _ = AnimationName;
        _ = SourceEntity;
        _ = AbilityEntity;
        _ = TargetEntity;
        _ = Position;
        _ = Duration;
    }
}
