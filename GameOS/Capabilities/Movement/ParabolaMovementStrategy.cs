using SlimeAI.GameOS.Runtime.Data;
using SlimeAI.GameOS.Runtime.Entity;

namespace SlimeAI.GameOS.Capabilities.Movement;

/// <summary>
/// Parabola 策略：从起点到固定终点的单段抛物线轨迹。
/// </summary>
public sealed class ParabolaMovementStrategy : IMovementStrategy
{
    private Vector2Value startPosition;
    private Vector2Value targetPosition;
    private float progress;

    /// <inheritdoc />
    public void OnEnter(IEntity entity, Data data, in MovementParams movementParams)
    {
        startPosition = data.Get<Vector2Value>(MovementDataKeys.Position, Vector2Value.Zero);
        targetPosition = movementParams.TargetPosition ?? startPosition;
        progress = 0f;
    }

    /// <inheritdoc />
    public MovementUpdateResult Update(IEntity entity, Data data, float delta, in MovementParams movementParams)
    {
        if (delta <= 0f || targetPosition == startPosition)
        {
            data.Set(MovementDataKeys.Velocity, Vector2Value.Zero);
            return MovementUpdateResult.Continue();
        }

        var duration = ResolveDuration(movementParams);
        if (duration <= 0f)
        {
            data.Set(MovementDataKeys.Velocity, Vector2Value.Zero);
            return MovementUpdateResult.Complete();
        }

        var nextProgress = progress + (delta / duration);
        if (nextProgress > 1f)
        {
            nextProgress = 1f;
        }

        var position = data.Get<Vector2Value>(MovementDataKeys.Position, Vector2Value.Zero);
        var nextPosition = Evaluate(nextProgress, movementParams);
        var displacement = nextPosition - position;
        var velocity = displacement / delta;
        data.Set(MovementDataKeys.Velocity, velocity);
        progress = nextProgress;

        var result = MovementUpdateResult.Continue(displacement.Length, velocity.Normalized());
        return progress >= 1f ? MovementUpdateResult.Complete() : result;
    }

    private Vector2Value Evaluate(float t, in MovementParams movementParams)
    {
        var linear = startPosition + ((targetPosition - startPosition) * t);
        var height = ResolveApexHeight(movementParams);
        var arcOffset = 4f * height * t * (1f - t);
        return new Vector2Value(linear.X, linear.Y + arcOffset);
    }

    private float ResolveApexHeight(in MovementParams movementParams)
    {
        if (!movementParams.BowWorldUp)
        {
            return movementParams.ParabolaApexHeight;
        }

        return -System.MathF.Abs(movementParams.ParabolaApexHeight);
    }

    private float ResolveDuration(in MovementParams movementParams)
    {
        if (movementParams.MaxDuration > 0f)
        {
            return movementParams.MaxDuration;
        }

        var speed = movementParams.Speed > 0f ? movementParams.Speed : 0f;
        if (speed <= 0f)
        {
            return 0f;
        }

        return Vector2Value.Distance(startPosition, targetPosition) / speed;
    }
}
