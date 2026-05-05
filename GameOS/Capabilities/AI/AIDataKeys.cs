using SkilmeAI.GameOS.Capabilities.Movement;
using SkilmeAI.GameOS.Runtime.Data;
using SkilmeAI.GameOS.Runtime.Entity;

namespace SkilmeAI.GameOS.Capabilities.AI;

/// <summary>
/// AI Capability 使用的运行时 DataKey。
/// </summary>
public static class AIDataKeys
{
    /// <summary>AI 是否启用。</summary>
    public static readonly DataMeta IsEnabled = DataRegistry.Register(new DataMeta
    {
        Key = "AI.IsEnabled",
        DisplayName = "AI Enabled",
        Type = typeof(bool),
        Category = AICategory.Runtime,
        DefaultValue = true,
        Description = "AI 行为树是否允许 Tick。"
    });

    /// <summary>当前目标实体。</summary>
    public static readonly DataMeta TargetEntity = DataRegistry.Register(new DataMeta
    {
        Key = "AI.TargetEntity",
        DisplayName = "Target Entity",
        Type = typeof(IEntity),
        Category = AICategory.Runtime,
        DefaultValue = null,
        Description = "AI 当前锁定的目标实体引用。"
    });

    /// <summary>当前目标位置。</summary>
    public static readonly DataMeta TargetPosition = DataRegistry.Register(new DataMeta
    {
        Key = "AI.TargetPosition",
        DisplayName = "Target Position",
        Type = typeof(Vector2Value),
        Category = AICategory.Runtime,
        DefaultValue = Vector2Value.Zero,
        Description = "AI 当前目标位置。"
    });

    /// <summary>是否存在有效目标位置。</summary>
    public static readonly DataMeta HasTargetPosition = DataRegistry.Register(new DataMeta
    {
        Key = "AI.HasTargetPosition",
        DisplayName = "Has Target Position",
        Type = typeof(bool),
        Category = AICategory.Runtime,
        DefaultValue = false,
        Description = "AI.TargetPosition 是否有效。"
    });

    /// <summary>AI 是否已经发出攻击请求。</summary>
    public static readonly DataMeta IsAttackRequested = DataRegistry.Register(new DataMeta
    {
        Key = "AI.IsAttackRequested",
        DisplayName = "Attack Requested",
        Type = typeof(bool),
        Category = AICategory.Runtime,
        DefaultValue = false,
        Description = "当前 Tick 是否已发出普通攻击请求。"
    });

    /// <summary>攻击距离。</summary>
    public static readonly DataMeta AttackRange = DataRegistry.Register(new DataMeta
    {
        Key = "AI.AttackRange",
        DisplayName = "Attack Range",
        Type = typeof(float),
        Category = AICategory.Basic,
        DefaultValue = 100f,
        MinValue = 0f,
        SupportModifiers = true,
        Description = "普通攻击请求使用的目标距离。"
    });

    /// <summary>巡逻中心点。</summary>
    public static readonly DataMeta PatrolCenter = DataRegistry.Register(new DataMeta
    {
        Key = "AI.PatrolCenter",
        DisplayName = "Patrol Center",
        Type = typeof(Vector2Value),
        Category = AICategory.Basic,
        DefaultValue = Vector2Value.Zero,
        Description = "AI 巡逻计算使用的中心点，通常为出生位置。"
    });

    /// <summary>巡逻半径，单位像素。</summary>
    public static readonly DataMeta PatrolRadius = DataRegistry.Register(new DataMeta
    {
        Key = "AI.PatrolRadius",
        DisplayName = "Patrol Radius",
        Type = typeof(float),
        Category = AICategory.Basic,
        DefaultValue = 500f,
        MinValue = 0f,
        SupportModifiers = true,
        Description = "AI 巡逻点相对中心点的最大半径。"
    });

    /// <summary>到达巡逻点后的等待时间，单位秒。</summary>
    public static readonly DataMeta PatrolWaitTime = DataRegistry.Register(new DataMeta
    {
        Key = "AI.PatrolWaitTime",
        DisplayName = "Patrol Wait Time",
        Type = typeof(float),
        Category = AICategory.Basic,
        DefaultValue = 2f,
        MinValue = 0f,
        Description = "AI 到达巡逻点后等待多久再选择下一个巡逻点。"
    });

    /// <summary>当前巡逻目标点。</summary>
    public static readonly DataMeta PatrolTargetPosition = DataRegistry.Register(new DataMeta
    {
        Key = "AI.PatrolTargetPosition",
        DisplayName = "Patrol Target Position",
        Type = typeof(Vector2Value),
        Category = AICategory.Runtime,
        DefaultValue = Vector2Value.Zero,
        Description = "AI 当前巡逻目标点。"
    });

    /// <summary>是否已有巡逻目标点。</summary>
    public static readonly DataMeta HasPatrolTargetPosition = DataRegistry.Register(new DataMeta
    {
        Key = "AI.HasPatrolTargetPosition",
        DisplayName = "Has Patrol Target Position",
        Type = typeof(bool),
        Category = AICategory.Runtime,
        DefaultValue = false,
        Description = "AI.PatrolTargetPosition 是否有效。"
    });

    /// <summary>巡逻等待剩余时间，单位秒。</summary>
    public static readonly DataMeta PatrolWaitRemaining = DataRegistry.Register(new DataMeta
    {
        Key = "AI.PatrolWaitRemaining",
        DisplayName = "Patrol Wait Remaining",
        Type = typeof(float),
        Category = AICategory.Runtime,
        DefaultValue = 0f,
        MinValue = 0f,
        Description = "当前巡逻等待阶段的剩余时间。"
    });

    /// <summary>巡逻目标选择方向，用于确定性地在中心点两侧切换。</summary>
    public static readonly DataMeta PatrolDirectionSign = DataRegistry.Register(new DataMeta
    {
        Key = "AI.PatrolDirectionSign",
        DisplayName = "Patrol Direction Sign",
        Type = typeof(int),
        Category = AICategory.Runtime,
        DefaultValue = 1,
        Description = "巡逻目标点选择方向，1 表示中心点右侧，-1 表示中心点左侧。"
    });

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
