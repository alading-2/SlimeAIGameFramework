using SlimeAI.GameOS.Capabilities.Attack.Events;
using SlimeAI.GameOS.Capabilities.Movement;
using SlimeAI.GameOS.Runtime.Entity;

namespace SlimeAI.GameOS.Capabilities.AI;

/// <summary>
/// 发出攻击请求并写入停步朝向意图。
/// </summary>
public sealed class RequestAttackAction : BehaviorNode
{
    /// <summary>
    /// 创建攻击请求动作。
    /// </summary>
    public RequestAttackAction() : base("RequestAttack") { }

    /// <inheritdoc />
    public override AIState Evaluate(AIContext context)
    {
        if (!TryGetTarget(context, out var target, out var targetPosition))
        {
            context.Entity.Data.Set(MovementDataKeys.AIMoveSpeedMultiplier, 0f);
            return AIState.Failure;
        }

        var origin = context.Entity.Data.Get<Vector2Value>(MovementDataKeys.Position, Vector2Value.Zero);
        var faceDirection = (targetPosition - origin).Normalized();
        context.Entity.Data.Set(MovementDataKeys.AIMoveDirection, faceDirection);
        context.Entity.Data.Set(MovementDataKeys.AIMoveSpeedMultiplier, 0f);
        context.Entity.Data.Set(AIDataKeys.IsAttackRequested, true);
        context.Entity.Events.Publish(new Requested(context.Entity, target, targetPosition));
        return AIState.Running;
    }

    private static bool TryGetTarget(AIContext context, out IEntity? target, out Vector2Value targetPosition)
    {
        var targetId = context.Entity.Data.Get<EntityId?>(AIDataKeys.TargetEntity, null);
        target = targetId.HasValue ? EntityManager.Get(targetId.Value) : null;
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
