using SkilmeAI.GameOS.Runtime.Data;
using SkilmeAI.GameOS.Runtime.Entity;

namespace SkilmeAI.GameOS.Capabilities.Movement;

/// <summary>
/// PlayerInput 策略：消费外部输入方向 DataKey，输出基础速度。
/// </summary>
public sealed class PlayerInputMovementStrategy : IMovementStrategy
{
    /// <inheritdoc />
    public MovementUpdateResult Update(IEntity entity, Data data, float delta, in MovementParams movementParams)
    {
        return InputDrivenMovement.UpdateFromDirection(
            data,
            delta,
            data.Get<Vector2Value>(MovementDataKeys.InputDirection, Vector2Value.Zero),
            data.Get<float>(MovementDataKeys.MoveSpeed, 0f));
    }
}

/// <summary>
/// AIControlled 策略：消费 AI 方向和速度倍率，输出基础速度。
/// </summary>
public sealed class AIControlledMovementStrategy : IMovementStrategy
{
    /// <inheritdoc />
    public MovementUpdateResult Update(IEntity entity, Data data, float delta, in MovementParams movementParams)
    {
        var multiplier = data.Get<float>(MovementDataKeys.AIMoveSpeedMultiplier, 1f);
        var speed = data.Get<float>(MovementDataKeys.MoveSpeed, 0f) * multiplier;
        return InputDrivenMovement.UpdateFromDirection(
            data,
            delta,
            data.Get<Vector2Value>(MovementDataKeys.AIMoveDirection, Vector2Value.Zero),
            speed);
    }
}

internal static class InputDrivenMovement
{
    public static MovementUpdateResult UpdateFromDirection(Data data, float delta, Vector2Value direction, float speed)
    {
        if (!data.Get<bool>(MovementDataKeys.CanMoveInput, true) || speed <= 0f)
        {
            data.Set(MovementDataKeys.Velocity, Vector2Value.Zero);
            return MovementUpdateResult.Continue();
        }

        var normalizedDirection = direction.Normalized();
        if (normalizedDirection == Vector2Value.Zero)
        {
            data.Set(MovementDataKeys.Velocity, Vector2Value.Zero);
            return MovementUpdateResult.Continue();
        }

        data.Set(MovementDataKeys.LastMoveDirection, normalizedDirection);
        var velocity = normalizedDirection * speed;
        data.Set(MovementDataKeys.Velocity, velocity);
        return MovementUpdateResult.Continue(velocity.Length * delta, normalizedDirection);
    }
}
