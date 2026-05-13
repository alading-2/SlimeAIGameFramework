using SlimeAI.GameOS.Runtime.Data;
using SlimeAI.GameOS.Runtime.Entity;

namespace SlimeAI.GameOS.Capabilities.Movement;

/// <summary>
/// AttachToHost 策略：跟随目标实体 Position，并叠加固定偏移。
/// </summary>
public sealed class AttachToHostMovementStrategy : IMovementStrategy
{
    /// <inheritdoc />
    public MovementUpdateResult Update(IEntity entity, Data data, float delta, in MovementParams movementParams)
    {
        if (delta <= 0f || string.IsNullOrWhiteSpace(movementParams.TargetEntityId))
        {
            data.Set(MovementDataKeys.Velocity, Vector2Value.Zero);
            return MovementUpdateResult.Complete();
        }

        var target = EntityManager.Get(movementParams.TargetEntityId);
        if (target == null)
        {
            data.Set(MovementDataKeys.Velocity, Vector2Value.Zero);
            return MovementUpdateResult.Complete();
        }

        var position = data.Get<Vector2Value>(MovementDataKeys.Position, Vector2Value.Zero);
        var targetPosition = target.Data.Get<Vector2Value>(MovementDataKeys.Position, Vector2Value.Zero) + movementParams.AttachOffset;
        var displacement = targetPosition - position;
        data.Set(MovementDataKeys.Velocity, displacement / delta);
        return MovementUpdateResult.Continue(0f);
    }
}
