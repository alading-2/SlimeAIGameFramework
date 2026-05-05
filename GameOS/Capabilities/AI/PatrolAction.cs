using System;
using SkilmeAI.GameOS.Capabilities.Movement;

namespace SkilmeAI.GameOS.Capabilities.AI;

/// <summary>
/// 确定性巡逻动作：在巡逻中心两侧切换目标点，并向 Movement 写入 AI 移动意图。
/// </summary>
public sealed class PatrolAction : BehaviorNode
{
    private readonly float reachDistance;
    private readonly float speedMultiplier;

    /// <summary>
    /// 创建巡逻动作。
    /// </summary>
    /// <param name="reachDistance">距离巡逻点多近时视为到达。</param>
    /// <param name="speedMultiplier">写入 `AIMoveSpeedMultiplier` 的速度倍率。</param>
    public PatrolAction(float reachDistance = 1f, float speedMultiplier = 1f) : base("Patrol")
    {
        this.reachDistance = MathF.Max(0f, reachDistance);
        this.speedMultiplier = speedMultiplier;
    }

    /// <inheritdoc />
    public override AIState Evaluate(AIContext context)
    {
        var data = context.Entity.Data;
        var waitRemaining = data.Get<float>(AIDataKeys.PatrolWaitRemaining, 0f);
        if (waitRemaining > 0f)
        {
            waitRemaining = MathF.Max(0f, waitRemaining - context.Delta);
            data.Set(AIDataKeys.PatrolWaitRemaining, waitRemaining);
            data.Set(MovementDataKeys.AIMoveDirection, Vector2Value.Zero);
            data.Set(MovementDataKeys.AIMoveSpeedMultiplier, 0f);
            if (waitRemaining > 0f)
            {
                return AIState.Running;
            }

            SelectNextPatrolTarget(context);
        }

        if (!data.Get<bool>(AIDataKeys.HasPatrolTargetPosition, false))
        {
            SelectNextPatrolTarget(context);
        }

        var origin = data.Get<Vector2Value>(MovementDataKeys.Position, Vector2Value.Zero);
        var target = data.Get<Vector2Value>(AIDataKeys.PatrolTargetPosition, origin);
        var offset = target - origin;
        if (offset.Length <= reachDistance)
        {
            data.Set(MovementDataKeys.AIMoveDirection, Vector2Value.Zero);
            data.Set(MovementDataKeys.AIMoveSpeedMultiplier, 0f);
            data.Set(AIDataKeys.PatrolWaitRemaining, data.Get<float>(AIDataKeys.PatrolWaitTime, 0f));
            data.Set(AIDataKeys.HasPatrolTargetPosition, false);
            return AIState.Running;
        }

        data.Set(MovementDataKeys.AIMoveDirection, offset.Normalized());
        data.Set(MovementDataKeys.AIMoveSpeedMultiplier, speedMultiplier);
        return AIState.Running;
    }

    private static void SelectNextPatrolTarget(AIContext context)
    {
        var data = context.Entity.Data;
        var center = data.Get<Vector2Value>(
            AIDataKeys.PatrolCenter,
            data.Get<Vector2Value>(MovementDataKeys.Position, Vector2Value.Zero));
        var radius = data.Get<float>(AIDataKeys.PatrolRadius, 0f);
        var directionSign = data.Get<int>(AIDataKeys.PatrolDirectionSign, 1);
        if (directionSign == 0)
        {
            directionSign = 1;
        }

        var normalizedSign = directionSign > 0 ? 1 : -1;
        var target = new Vector2Value(center.X + (radius * normalizedSign), center.Y);
        data.Set(AIDataKeys.PatrolTargetPosition, target);
        data.Set(AIDataKeys.HasPatrolTargetPosition, true);
        data.Set(AIDataKeys.PatrolDirectionSign, -normalizedSign);
    }
}
