using SkilmeAI.GameOS.Runtime.Data;
using SkilmeAI.GameOS.Runtime.Entity;

namespace SkilmeAI.GameOS.Capabilities.Movement;

/// <summary>
/// SineWave 策略：沿基础方向前进并叠加横向正弦偏移。
/// </summary>
public sealed class SineWaveMovementStrategy : IMovementStrategy
{
    private Vector2Value baseDirection;
    private Vector2Value perpendicularDirection;
    private float baseSpeed;

    /// <inheritdoc />
    public void OnEnter(IEntity entity, Data data, in MovementParams movementParams)
    {
        baseDirection = movementParams.Direction != Vector2Value.Zero
            ? movementParams.Direction.Normalized()
            : MovementMath.DirectionFromAngle(movementParams.Angle);
        perpendicularDirection = new Vector2Value(-baseDirection.Y, baseDirection.X);
        baseSpeed = MovementMath.ResolveSpeed(
            movementParams,
            data.Get<float>(MovementDataKeys.MoveSpeed, 0f));
    }

    /// <inheritdoc />
    public MovementUpdateResult Update(IEntity entity, Data data, float delta, in MovementParams movementParams)
    {
        if (delta <= 0f || baseSpeed <= 0f)
        {
            data.Set(MovementDataKeys.Velocity, Vector2Value.Zero);
            return MovementUpdateResult.Continue();
        }

        var fromTime = movementParams.ElapsedTime - delta;
        if (fromTime < 0f)
        {
            fromTime = 0f;
        }

        var toTime = movementParams.ElapsedTime;
        var previousOffset = MovementMath.SineOffset(
            movementParams.WaveAmplitude,
            movementParams.WaveFrequency,
            fromTime,
            movementParams.WavePhase);
        var nextOffset = MovementMath.SineOffset(
            movementParams.WaveAmplitude,
            movementParams.WaveFrequency,
            toTime,
            movementParams.WavePhase);
        var forwardDisplacement = baseDirection * (baseSpeed * delta);
        var sideDisplacement = perpendicularDirection * (nextOffset - previousOffset);
        var displacement = forwardDisplacement + sideDisplacement;
        var velocity = displacement / delta;

        data.Set(MovementDataKeys.Velocity, velocity);
        return MovementUpdateResult.Continue(displacement.Length, velocity.Normalized());
    }
}
