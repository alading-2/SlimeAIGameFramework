using SlimeAI.GameOS.Capabilities.Collision;
using SlimeAI.GameOS.Capabilities.Damage;
using SlimeAI.GameOS.Capabilities.Movement;
using SlimeAI.GameOS.Runtime.Entity;

namespace SlimeAI.GameOS.Capabilities.AI;

/// <summary>
/// 从 Runtime Entity 快照中查找最近目标。
/// </summary>
public sealed class FindNearestTargetAction : BehaviorNode
{
    private readonly float range;
    private readonly bool excludeSameTeam;

    /// <summary>
    /// 创建最近目标查询动作。
    /// </summary>
    /// <param name="range">查询半径，-1 表示不限距离。</param>
    /// <param name="excludeSameTeam">是否过滤同队目标。</param>
    public FindNearestTargetAction(float range = -1f, bool excludeSameTeam = true) : base("FindNearestTarget")
    {
        this.range = range;
        this.excludeSameTeam = excludeSameTeam;
    }

    /// <inheritdoc />
    public override AIState Evaluate(AIContext context)
    {
        var origin = context.Entity.Data.Get<Vector2Value>(MovementDataKeys.Position, Vector2Value.Zero);
        var selfTeam = context.Entity.Data.Get<int>(CollisionDataKeys.Team, 0);
        IEntity? bestTarget = null;
        var bestDistance = float.MaxValue;
        var entities = EntityManager.GetAll();

        for (var i = 0; i < entities.Count; i++)
        {
            var candidate = entities[i];
            if (ReferenceEquals(candidate, context.Entity)
                || candidate.Data.Get<bool>(DamageDataKeys.IsDead, false))
            {
                continue;
            }

            if (excludeSameTeam
                && selfTeam != 0
                && candidate.Data.Get<int>(CollisionDataKeys.Team, 0) == selfTeam)
            {
                continue;
            }

            var candidatePosition = candidate.Data.Get<Vector2Value>(MovementDataKeys.Position, Vector2Value.Zero);
            var distance = Vector2Value.Distance(origin, candidatePosition);
            if ((range >= 0f && distance > range) || distance >= bestDistance)
            {
                continue;
            }

            bestDistance = distance;
            bestTarget = candidate;
        }

        if (bestTarget == null)
        {
            context.Entity.Data.Remove(AIDataKeys.TargetEntity);
            context.Entity.Data.Set(AIDataKeys.HasTargetPosition, false);
            return AIState.Failure;
        }

        context.Entity.Data.Set<EntityId?>(AIDataKeys.TargetEntity, bestTarget.EntityId);
        context.Entity.Data.Set(AIDataKeys.TargetPosition, bestTarget.Data.Get<Vector2Value>(MovementDataKeys.Position, Vector2Value.Zero));
        context.Entity.Data.Set(AIDataKeys.HasTargetPosition, true);
        return AIState.Success;
    }
}
