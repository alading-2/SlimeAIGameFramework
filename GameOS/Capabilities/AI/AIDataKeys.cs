using SlimeAI.GameOS.Capabilities.Movement;
using SlimeAI.GameOS.Runtime.Data;
using SlimeAI.GameOS.Runtime.Entity;

namespace SlimeAI.GameOS.Capabilities.AI;

/// <summary>
/// AI Capability 使用的运行时 DataKey。
/// </summary>
public static class AIDataKeys
{
    /// <summary>AI 是否启用。</summary>
    public static readonly DataKey<bool> IsEnabled = DataKey.Create<bool>("AI.IsEnabled",
        defaultValue: true,
        category: AICategory.Runtime);

    /// <summary>当前目标实体。</summary>
    public static readonly DataKey<IEntity?> TargetEntity = DataKey.Create<IEntity?>("AI.TargetEntity",
        defaultValue: null,
        category: AICategory.Runtime);

    /// <summary>当前目标位置。</summary>
    public static readonly DataKey<Vector2Value> TargetPosition = DataKey.Create<Vector2Value>("AI.TargetPosition",
        defaultValue: Vector2Value.Zero,
        category: AICategory.Runtime);

    /// <summary>是否存在有效目标位置。</summary>
    public static readonly DataKey<bool> HasTargetPosition = DataKey.Create<bool>("AI.HasTargetPosition",
        defaultValue: false,
        category: AICategory.Runtime);

    /// <summary>AI 是否已经发出攻击请求。</summary>
    public static readonly DataKey<bool> IsAttackRequested = DataKey.Create<bool>("AI.IsAttackRequested",
        defaultValue: false,
        category: AICategory.Runtime);

    /// <summary>攻击距离。</summary>
    public static readonly DataKey<float> AttackRange = DataKey.Create<float>("AI.AttackRange",
        defaultValue: 100f,
        category: AICategory.Basic,
        minValue: 0f,
        supportsModifiers: true);

    /// <summary>巡逻中心点。</summary>
    public static readonly DataKey<Vector2Value> PatrolCenter = DataKey.Create<Vector2Value>("AI.PatrolCenter",
        defaultValue: Vector2Value.Zero,
        category: AICategory.Basic);

    /// <summary>巡逻半径，单位像素。</summary>
    public static readonly DataKey<float> PatrolRadius = DataKey.Create<float>("AI.PatrolRadius",
        defaultValue: 500f,
        category: AICategory.Basic,
        minValue: 0f,
        supportsModifiers: true);

    /// <summary>到达巡逻点后的等待时间，单位秒。</summary>
    public static readonly DataKey<float> PatrolWaitTime = DataKey.Create<float>("AI.PatrolWaitTime",
        defaultValue: 2f,
        category: AICategory.Basic,
        minValue: 0f);

    /// <summary>当前巡逻目标点。</summary>
    public static readonly DataKey<Vector2Value> PatrolTargetPosition = DataKey.Create<Vector2Value>("AI.PatrolTargetPosition",
        defaultValue: Vector2Value.Zero,
        category: AICategory.Runtime);

    /// <summary>是否已有巡逻目标点。</summary>
    public static readonly DataKey<bool> HasPatrolTargetPosition = DataKey.Create<bool>("AI.HasPatrolTargetPosition",
        defaultValue: false,
        category: AICategory.Runtime);

    /// <summary>巡逻等待剩余时间，单位秒。</summary>
    public static readonly DataKey<float> PatrolWaitRemaining = DataKey.Create<float>("AI.PatrolWaitRemaining",
        defaultValue: 0f,
        category: AICategory.Runtime,
        minValue: 0f);

    /// <summary>巡逻目标选择方向，用于确定性地在中心点两侧切换。</summary>
    public static readonly DataKey<int> PatrolDirectionSign = DataKey.Create<int>("AI.PatrolDirectionSign",
        defaultValue: 1,
        category: AICategory.Runtime);

    /// <summary>
    /// 显式触发静态 DataKey 注册。
    /// </summary>
    public static void RegisterAll()
    {
        _ = IsEnabled;
        _ = TargetEntity;
        _ = TargetPosition;
        _ = HasTargetPosition;
        _ = IsAttackRequested;
        _ = AttackRange;
        _ = PatrolCenter;
        _ = PatrolRadius;
        _ = PatrolWaitTime;
        _ = PatrolTargetPosition;
        _ = HasPatrolTargetPosition;
        _ = PatrolWaitRemaining;
        _ = PatrolDirectionSign;
    }
}
