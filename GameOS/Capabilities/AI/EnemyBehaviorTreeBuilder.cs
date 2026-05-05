namespace SkilmeAI.GameOS.Capabilities.AI;

/// <summary>
/// 敌人行为树预制工厂。
/// </summary>
public static class EnemyBehaviorTreeBuilder
{
    /// <summary>
    /// 构建标准近战敌人树：攻击优先，其次追逐，最后巡逻。
    /// </summary>
    /// <param name="targetSearchRange">攻击和追逐共用的索敌范围，-1 表示不限距离。</param>
    /// <param name="defaultAttackRange">攻击范围 DataKey 未配置时的回退范围。</param>
    /// <param name="chaseSpeedMultiplier">追逐速度倍率。</param>
    /// <param name="patrolReachDistance">巡逻到达判定距离。</param>
    /// <param name="patrolSpeedMultiplier">巡逻速度倍率。</param>
    public static BehaviorNode BuildMeleeEnemyTree(
        float targetSearchRange = -1f,
        float defaultAttackRange = -1f,
        float chaseSpeedMultiplier = 1f,
        float patrolReachDistance = 1f,
        float patrolSpeedMultiplier = 1f)
    {
        return new SelectorNode("Melee Enemy")
            .Add(EnemyBehaviorBlocks.AttackBranch(targetSearchRange, AIDataKeys.AttackRange, defaultAttackRange))
            .Add(EnemyBehaviorBlocks.ChaseBranch(targetSearchRange, chaseSpeedMultiplier))
            .Add(EnemyBehaviorBlocks.PatrolBranch(patrolReachDistance, patrolSpeedMultiplier));
    }

    /// <summary>
    /// 构建会推进 Periodic Ability 的近战敌人树。
    /// </summary>
    /// <param name="targetSearchRange">攻击和追逐共用的索敌范围，-1 表示不限距离。</param>
    /// <param name="defaultAttackRange">攻击范围 DataKey 未配置时的回退范围。</param>
    /// <param name="chaseSpeedMultiplier">追逐速度倍率。</param>
    /// <param name="patrolReachDistance">巡逻到达判定距离。</param>
    /// <param name="patrolSpeedMultiplier">巡逻速度倍率。</param>
    public static BehaviorNode BuildAbilityMeleeEnemyTree(
        float targetSearchRange = -1f,
        float defaultAttackRange = -1f,
        float chaseSpeedMultiplier = 1f,
        float patrolReachDistance = 1f,
        float patrolSpeedMultiplier = 1f)
    {
        return new SelectorNode("Ability Melee Enemy")
            .Add(EnemyBehaviorBlocks.AbilityAutoTriggerBranch())
            .Add(EnemyBehaviorBlocks.AttackBranch(targetSearchRange, AIDataKeys.AttackRange, defaultAttackRange))
            .Add(EnemyBehaviorBlocks.ChaseBranch(targetSearchRange, chaseSpeedMultiplier))
            .Add(EnemyBehaviorBlocks.PatrolBranch(patrolReachDistance, patrolSpeedMultiplier));
    }

    /// <summary>
    /// 构建纯巡逻树。
    /// </summary>
    /// <param name="reachDistance">巡逻到达判定距离。</param>
    /// <param name="speedMultiplier">巡逻速度倍率。</param>
    public static BehaviorNode BuildPatrolOnlyTree(float reachDistance = 1f, float speedMultiplier = 1f)
    {
        return EnemyBehaviorBlocks.PatrolBranch(reachDistance, speedMultiplier);
    }

    /// <summary>
    /// 构建只追逐不攻击的树，无法索敌时回退巡逻。
    /// </summary>
    /// <param name="targetSearchRange">索敌范围，-1 表示不限距离。</param>
    /// <param name="chaseSpeedMultiplier">追逐速度倍率。</param>
    /// <param name="patrolReachDistance">巡逻到达判定距离。</param>
    /// <param name="patrolSpeedMultiplier">巡逻速度倍率。</param>
    public static BehaviorNode BuildChaserTree(
        float targetSearchRange = -1f,
        float chaseSpeedMultiplier = 1f,
        float patrolReachDistance = 1f,
        float patrolSpeedMultiplier = 1f)
    {
        return new SelectorNode("Chaser")
            .Add(EnemyBehaviorBlocks.ChaseBranch(targetSearchRange, chaseSpeedMultiplier))
            .Add(EnemyBehaviorBlocks.PatrolBranch(patrolReachDistance, patrolSpeedMultiplier));
    }
}
