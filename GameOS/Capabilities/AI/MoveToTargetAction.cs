using SlimeAI.GameOS.Capabilities.Movement;
using SlimeAI.GameOS.Runtime.Entity;

namespace SlimeAI.GameOS.Capabilities.AI;

/// <summary>
/// 向当前目标写入 AI 移动意图。
/// </summary>
public sealed class MoveToTargetAction : BehaviorNode
{
    private readonly float speedMultiplier;

    /// <summary>
    /// 创建移动到目标动作。
    /// </summary>
    /// <param name="speedMultiplier">写入 `AIMoveSpeedMultiplier` 的速度倍率。</param>
    public MoveToTargetAction(float speedMultiplier = 1f) : base("MoveToTarget")
    {
        this.speedMultiplier = speedMultiplier;
    }

    /// <inheritdoc />
    public override AIState Evaluate(AIContext context)
    {
        var origin = context.Entity.Data.Get<Vector2Value>(MovementDataKeys.Position, Vector2Value.Zero);
        if (!TryGetTargetPosition(context, out var targetPosition))
        {
            context.Entity.Data.Set(MovementDataKeys.AIMoveDirection, Vector2Value.Zero);
            return AIState.Failure;
        }

        var direction = (targetPosition - origin).Normalized();
        context.Entity.Data.Set(MovementDataKeys.AIMoveDirection, direction);
        context.Entity.Data.Set(MovementDataKeys.AIMoveSpeedMultiplier, speedMultiplier);
        return direction == Vector2Value.Zero ? AIState.Failure : AIState.Success;
    }

    private static bool TryGetTargetPosition(AIContext context, out Vector2Value targetPosition)
    {
        var targetId = context.Entity.Data.Get<EntityId?>(AIDataKeys.TargetEntity, null);
        var target = targetId.HasValue ? EntityManager.Get(targetId.Value) : null;
        if (target != null)
        {
            targetPosition = target.Data.Get<Vector2Value>(MovementDataKeys.Position, Vector2Value.Zero);
            return true;
        }

        if (context.Entity.Data.Get<bool>(AIDataKeys.HasTargetPosition, false))
        {
            targetPosition = context.Entity.Data.Get<Vector2Value>(AIDataKeys.TargetPosition, Vector2Value.Zero);
            return true;
        }

        targetPosition = Vector2Value.Zero;
        return false;
    }
}
