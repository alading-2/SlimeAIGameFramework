using SlimeAI.GameOS.Runtime.Data;
using SlimeAI.GameOS.Runtime.Entity;

namespace SlimeAI.GameOS.Capabilities.Movement;

/// <summary>
/// Charge 策略：按方向或目标点输出直线速度。
/// </summary>
public sealed class ChargeMovementStrategy : IMovementStrategy
{
    /// <inheritdoc />
    public MovementUpdateResult Update(IEntity entity, Data data, float delta, in MovementParams movementParams)
    {
        if (delta <= 0f)
        {
            data.Set(MovementDataKeys.Velocity, Vector2Value.Zero);
            return MovementUpdateResult.Continue();
        }

        var position = data.Get<Vector2Value>(MovementDataKeys.Position, Vector2Value.Zero);
        var direction = ResolveDirection(position, movementParams);
        if (direction == Vector2Value.Zero)
        {
            data.Set(MovementDataKeys.Velocity, Vector2Value.Zero);
            return MovementUpdateResult.Complete();
        }

        var speed = movementParams.Speed > 0f
            ? movementParams.Speed
            : data.Get<float>(MovementDataKeys.MoveSpeed, 0f);
        if (speed <= 0f)
        {
            data.Set(MovementDataKeys.Velocity, Vector2Value.Zero);
            return MovementUpdateResult.Complete();
        }

        if (movementParams.TargetPosition.HasValue && movementParams.StopAtTarget)
        {
            var remaining = Vector2Value.Distance(position, movementParams.TargetPosition.Value);
            if (remaining <= movementParams.ReachDistance)
            {
                data.Set(MovementDataKeys.Velocity, Vector2Value.Zero);
                return MovementUpdateResult.Complete();
            }

            var maxStep = speed * delta;
            if (maxStep >= remaining)
            {
                data.Set(MovementDataKeys.Velocity, direction * (remaining / delta));
                return MovementUpdateResult.Complete();
            }
        }

        var velocity = direction * speed;
        data.Set(MovementDataKeys.Velocity, velocity);
        return MovementUpdateResult.Continue(velocity.Length * delta, direction);
    }

    private static Vector2Value ResolveDirection(Vector2Value position, in MovementParams movementParams)
    {
        if (movementParams.TargetPosition.HasValue)
        {
            return (movementParams.TargetPosition.Value - position).Normalized();
        }

        return movementParams.Direction.Normalized();
    }
}
