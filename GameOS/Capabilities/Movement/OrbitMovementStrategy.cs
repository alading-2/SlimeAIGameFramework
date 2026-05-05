using SkilmeAI.GameOS.Runtime.Data;
using SkilmeAI.GameOS.Runtime.Entity;

namespace SkilmeAI.GameOS.Capabilities.Movement;

/// <summary>
/// Orbit 策略：围绕固定圆心输出切向速度。
/// </summary>
public sealed class OrbitMovementStrategy : IMovementStrategy
{
    private float currentAngle;
    private float currentAngularSpeed;
    private float traveledAngle;

    /// <inheritdoc />
    public void OnEnter(IEntity entity, Data data, in MovementParams movementParams)
    {
        var position = data.Get<Vector2Value>(MovementDataKeys.Position, Vector2Value.Zero);
        var toSelf = position - movementParams.OrbitCenter;
        currentAngle = movementParams.OrbitInitAngle ?? MovementMath.AngleFromDirection(toSelf);
        currentAngularSpeed = ResolveAngularSpeed(movementParams);
        traveledAngle = 0f;
    }

    /// <inheritdoc />
    public MovementUpdateResult Update(IEntity entity, Data data, float delta, in MovementParams movementParams)
    {
        if (delta <= 0f || movementParams.OrbitRadius <= 0f || currentAngularSpeed <= 0f)
        {
            data.Set(MovementDataKeys.Velocity, Vector2Value.Zero);
            return MovementUpdateResult.Continue();
        }

        currentAngularSpeed += movementParams.OrbitAngularAcceleration * delta;
        if (currentAngularSpeed < 0f)
        {
            currentAngularSpeed = 0f;
        }

        var angleStep = currentAngularSpeed * delta;
        if (movementParams.OrbitTotalAngle >= 0f)
        {
            var remainingAngle = movementParams.OrbitTotalAngle - traveledAngle;
            if (remainingAngle <= 0f)
            {
                data.Set(MovementDataKeys.Velocity, Vector2Value.Zero);
                return MovementUpdateResult.Complete();
            }

            if (angleStep > remainingAngle)
            {
                angleStep = remainingAngle;
            }
        }

        currentAngle += movementParams.IsOrbitClockwise ? angleStep : -angleStep;
        traveledAngle += angleStep;

        var position = data.Get<Vector2Value>(MovementDataKeys.Position, Vector2Value.Zero);
        var targetPosition = movementParams.OrbitCenter
            + (MovementMath.DirectionFromAngle(currentAngle) * movementParams.OrbitRadius);
        var displacement = targetPosition - position;
        var velocity = displacement / delta;
        data.Set(MovementDataKeys.Velocity, velocity);

        var result = MovementUpdateResult.Continue(displacement.Length, velocity.Normalized());
        if (movementParams.OrbitTotalAngle >= 0f && traveledAngle >= movementParams.OrbitTotalAngle)
        {
            return MovementUpdateResult.Complete();
        }

        return result;
    }

    private static float ResolveAngularSpeed(in MovementParams movementParams)
    {
        if (movementParams.OrbitAngularSpeed > 0f)
        {
            return movementParams.OrbitAngularSpeed;
        }

        if (movementParams.OrbitTotalAngle > 0f && movementParams.MaxDuration > 0f)
        {
            return movementParams.OrbitTotalAngle / movementParams.MaxDuration;
        }

        return 0f;
    }
}
