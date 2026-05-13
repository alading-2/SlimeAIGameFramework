using SlimeAI.GameOS.Runtime.Data;

namespace SlimeAI.GameOS.Capabilities.AI;

/// <summary>
/// 可复用敌人行为树积木块。
/// </summary>
public static class EnemyBehaviorBlocks
{
    /// <summary>
    /// 攻击分支：索敌、范围检测、发出普通攻击请求。
    /// </summary>
    /// <param name="targetSearchRange">索敌范围，-1 表示不限距离。</param>
    /// <param name="attackRangeDataKey">攻击范围 DataKey。</param>
    /// <param name="defaultAttackRange">DataKey 未配置时的回退攻击范围，-1 表示不限距离。</param>
    public static BehaviorNode AttackBranch(
        float targetSearchRange = -1f,
        DataKey<float>? attackRangeDataKey = null,
        float defaultAttackRange = -1f)
    {
        return new SequenceNode("Attack")
            .Add(new FindNearestTargetAction(targetSearchRange))
            .Add(attackRangeDataKey == null
                ? new IsTargetInRangeCondition(defaultAttackRange)
                : new IsTargetInRangeCondition(attackRangeDataKey, defaultAttackRange))
            .Add(new RequestAttackAction());
    }

    /// <summary>
    /// 追逐分支：索敌后写入 AI 移动意图。
    /// </summary>
    /// <param name="targetSearchRange">索敌范围，-1 表示不限距离。</param>
    /// <param name="speedMultiplier">写入 `AIMoveSpeedMultiplier` 的速度倍率。</param>
    public static BehaviorNode ChaseBranch(float targetSearchRange = -1f, float speedMultiplier = 1f)
    {
        return new SequenceNode("Chase")
            .Add(new FindNearestTargetAction(targetSearchRange))
            .Add(new MoveToTargetAction(speedMultiplier));
    }

    /// <summary>
    /// 巡逻分支：无目标时兜底写入确定性巡逻移动意图。
    /// </summary>
    /// <param name="reachDistance">距离巡逻点多近时视为到达。</param>
    /// <param name="speedMultiplier">写入 `AIMoveSpeedMultiplier` 的速度倍率。</param>
    public static BehaviorNode PatrolBranch(float reachDistance = 1f, float speedMultiplier = 1f)
    {
        return new PatrolAction(reachDistance, speedMultiplier);
    }

    /// <summary>
    /// 自动施法分支：推进外部准备好的 Periodic Ability 上下文。
    /// </summary>
    public static BehaviorNode AbilityAutoTriggerBranch()
    {
        return new SequenceNode("Ability Auto Trigger")
            .Add(new TickAbilityAutoTriggersAction());
    }
}
