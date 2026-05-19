using Godot;
using SlimeAI.GameOS.Capabilities.Ability;
using SlimeAI.GameOS.Capabilities.AI;
using SlimeAI.GameOS.Capabilities.Movement;
using SlimeAI.GameOS.Runtime.Entity;

namespace SlimeAI.GameOS.GodotBridge;

/// <summary>
/// Godot AI bridge：驱动纯 Runtime 行为树 Tick，并把 AI 参数写入 Entity Data。
/// </summary>
public partial class GodotAIComponent : Node, IGodotComponent
{
    private AIContext? context;
    private IEntity? entity;
    private BehaviorNode? root;

    /// <summary>是否自动在 `_Process` 中推进 AI。</summary>
    [Export]
    public bool AutoTick { get; set; } = true;

    /// <summary>注册时是否把导出参数写入 Runtime Data。</summary>
    [Export]
    public bool ApplyExportedDataOnRegister { get; set; } = true;

    /// <summary>注册时是否把当前位置写为巡逻中心。</summary>
    [Export]
    public bool UseNodePositionAsPatrolCenter { get; set; } = true;

    /// <summary>内置行为树类型。</summary>
    [Export]
    public GodotAIBehaviorTreeKind BehaviorTreeKind { get; set; } = GodotAIBehaviorTreeKind.Melee;

    /// <summary>AI 是否启用。</summary>
    [Export]
    public bool IsEnabled { get; set; } = true;

    /// <summary>索敌范围，-1 表示不限距离。</summary>
    [Export]
    public float TargetSearchRange { get; set; } = -1f;

    /// <summary>攻击范围，-1 表示不限距离。</summary>
    [Export]
    public float AttackRange { get; set; } = 100f;

    /// <summary>追逐速度倍率。</summary>
    [Export]
    public float ChaseSpeedMultiplier { get; set; } = 1f;

    /// <summary>巡逻半径，单位像素。</summary>
    [Export]
    public float PatrolRadius { get; set; } = 500f;

    /// <summary>巡逻等待时间，单位秒。</summary>
    [Export]
    public float PatrolWaitTime { get; set; } = 2f;

    /// <summary>巡逻到达判定距离。</summary>
    [Export]
    public float PatrolReachDistance { get; set; } = 1f;

    /// <summary>巡逻速度倍率。</summary>
    [Export]
    public float PatrolSpeedMultiplier { get; set; } = 1f;

    /// <summary>最近一次 Tick 返回状态。</summary>
    public AIState LastState { get; private set; } = AIState.Success;

    /// <inheritdoc />
    public void OnComponentRegistered(IEntity entity, Node entityNode)
    {
        this.entity = entity;
        context = new AIContext { Entity = entity, AbilityService = AbilityService.Instance };
        AIDataKeys.RegisterAll();
        MovementDataKeys.RegisterAll();
        if (ApplyExportedDataOnRegister)
        {
            ApplyExportedData(entity, entityNode);
        }

        SetBehaviorTree(BuildExportedTree());
    }

    /// <inheritdoc />
    public void OnComponentUnregistered(IEntity? entity, Node? entityNode)
    {
        root?.Reset(context);
        root = null;
        context = null;
        this.entity = null;
        LastState = AIState.Success;
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        if (AutoTick)
        {
            TickAI((float)delta);
        }
    }

    /// <summary>
    /// 手动设置行为树，供测试或游戏特定逻辑覆盖默认树。
    /// </summary>
    /// <param name="root">行为树根节点。</param>
    public void SetBehaviorTree(BehaviorNode root)
    {
        this.root?.Reset(context);
        this.root = root;
    }

    /// <summary>
    /// 手动推进一次 AI Tick。
    /// </summary>
    /// <param name="delta">帧间隔秒数。</param>
    public AIState TickAI(float delta)
    {
        if (entity == null || context == null || root == null)
        {
            LastState = AIState.Failure;
            return LastState;
        }

        context.Delta = delta;
        LastState = AIService.Default.Tick(context, root);
        return LastState;
    }

    private void ApplyExportedData(IEntity entity, Node entityNode)
    {
        entity.Data.Set(AIDataKeys.IsEnabled, IsEnabled);
        entity.Data.Set(AIDataKeys.AttackRange, AttackRange);
        entity.Data.Set(AIDataKeys.PatrolRadius, PatrolRadius);
        entity.Data.Set(AIDataKeys.PatrolWaitTime, PatrolWaitTime);
        entity.Data.Set(MovementDataKeys.AIMoveSpeedMultiplier, ChaseSpeedMultiplier);
        if (UseNodePositionAsPatrolCenter && entityNode is Node2D node2D)
        {
            entity.Data.Set(AIDataKeys.PatrolCenter, new Vector2Value(node2D.Position.X, node2D.Position.Y));
        }
    }

    private BehaviorNode BuildExportedTree()
    {
        return BehaviorTreeKind switch
        {
            GodotAIBehaviorTreeKind.AbilityMelee => EnemyBehaviorTreeBuilder.BuildAbilityMeleeEnemyTree(
                TargetSearchRange,
                AttackRange,
                ChaseSpeedMultiplier,
                PatrolReachDistance,
                PatrolSpeedMultiplier),
            GodotAIBehaviorTreeKind.PatrolOnly => EnemyBehaviorTreeBuilder.BuildPatrolOnlyTree(
                PatrolReachDistance,
                PatrolSpeedMultiplier),
            GodotAIBehaviorTreeKind.Chaser => EnemyBehaviorTreeBuilder.BuildChaserTree(
                TargetSearchRange,
                ChaseSpeedMultiplier,
                PatrolReachDistance,
                PatrolSpeedMultiplier),
            _ => EnemyBehaviorTreeBuilder.BuildMeleeEnemyTree(
                TargetSearchRange,
                AttackRange,
                ChaseSpeedMultiplier,
                PatrolReachDistance,
                PatrolSpeedMultiplier)
        };
    }
}
