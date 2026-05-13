using System;
using SlimeAI.GameOS.Runtime.Data;
using SlimeAI.GameOS.Runtime.Entity;

namespace SlimeAI.GameOS.Capabilities.Movement;

/// <summary>
/// Boomerang 策略：去程曲线、停顿、返程曲线三阶段运动。
/// </summary>
public sealed class BoomerangMovementStrategy : IMovementStrategy
{
    private const float DefaultSpeed = 300f;
    private const float DefaultArcHeightRatio = 0.22f;
    private const float MinArcHeight = 24f;
    private const float MaxAutoArcHeight = 180f;

    private Vector2Value launchPoint;
    private Vector2Value phaseStartPoint;
    private Vector2Value outboundTargetPoint;
    private bool returning;
    private float pauseTimer;
    private float phaseProgress;

    /// <inheritdoc />
    public void OnEnter(IEntity entity, Data data, in MovementParams movementParams)
    {
        launchPoint = data.Get<Vector2Value>(MovementDataKeys.Position, Vector2Value.Zero);
        phaseStartPoint = launchPoint;
        outboundTargetPoint = movementParams.TargetPosition ?? launchPoint;
        returning = false;
        pauseTimer = 0f;
        phaseProgress = 0f;
    }

    /// <inheritdoc />
    public MovementUpdateResult Update(IEntity entity, Data data, float delta, in MovementParams movementParams)
    {
        if (delta <= 0f)
        {
            data.Set(MovementDataKeys.Velocity, Vector2Value.Zero);
            return MovementUpdateResult.Continue();
        }

        if (pauseTimer > 0f)
        {
            pauseTimer = MathF.Max(pauseTimer - delta, 0f);
            data.Set(MovementDataKeys.Velocity, Vector2Value.Zero);
            return MovementUpdateResult.Continue();
        }

        var position = data.Get<Vector2Value>(MovementDataKeys.Position, Vector2Value.Zero);
        var phaseEndPoint = ResolvePhaseEndPoint(movementParams);
        var reachDistance = MathF.Max(movementParams.ReachDistance, 0.001f);
        if (Vector2Value.Distance(position, phaseEndPoint) <= reachDistance)
        {
            return HandlePhaseArrival(data, position, movementParams, preserveVelocity: false);
        }

        var chordLength = Vector2Value.Distance(phaseStartPoint, phaseEndPoint);
        var speed = ResolveCurrentSpeed(movementParams, chordLength);
        if (chordLength <= 0.001f || speed <= 0f)
        {
            data.Set(MovementDataKeys.Velocity, Vector2Value.Zero);
            return HandlePhaseArrival(data, position, movementParams, preserveVelocity: false);
        }

        var duration = chordLength / speed;
        var nextProgress = MathF.Min(phaseProgress + (delta / MathF.Max(duration, 0.001f)), 1f);
        var nextPosition = EvaluateQuadraticArc(phaseStartPoint, phaseEndPoint, ResolveArcHeight(chordLength, movementParams), nextProgress, movementParams.BoomerangIsClockwise);
        var displacement = nextPosition - position;
        var velocity = displacement / delta;

        data.Set(MovementDataKeys.Velocity, velocity);
        phaseProgress = nextProgress;

        if (phaseProgress >= 1f)
        {
            return HandlePhaseArrival(data, nextPosition, movementParams, preserveVelocity: true);
        }

        return MovementUpdateResult.Continue(displacement.Length, velocity.Normalized());
    }

    private Vector2Value ResolvePhaseEndPoint(in MovementParams movementParams)
    {
        if (!returning)
        {
            return outboundTargetPoint;
        }

        if (!string.IsNullOrWhiteSpace(movementParams.TargetEntityId))
        {
            var target = EntityManager.Get(movementParams.TargetEntityId);
            if (target != null)
            {
                return target.Data.Get<Vector2Value>(MovementDataKeys.Position, launchPoint);
            }
        }

        return launchPoint;
    }

    private MovementUpdateResult HandlePhaseArrival(Data data, Vector2Value position, in MovementParams movementParams, bool preserveVelocity)
    {
        if (!preserveVelocity)
        {
            data.Set(MovementDataKeys.Velocity, Vector2Value.Zero);
        }

        if (!returning)
        {
            returning = true;
            pauseTimer = MathF.Max(movementParams.BoomerangPauseTime, 0f);
            phaseStartPoint = position;
            phaseProgress = 0f;
            return MovementUpdateResult.Continue();
        }

        return MovementUpdateResult.Complete();
    }

    private float ResolveCurrentSpeed(in MovementParams movementParams, float travelLength)
    {
        var speed = movementParams.Speed > 0f ? movementParams.Speed : 0f;
        if (speed <= 0f && movementParams.MaxDuration > 0f && travelLength > 0.001f)
        {
            var flyDuration = movementParams.MaxDuration - MathF.Max(movementParams.BoomerangPauseTime, 0f);
            speed = flyDuration > 0.001f ? travelLength / (flyDuration * 0.5f) : 0f;
        }

        if (speed <= 0f)
        {
            speed = DefaultSpeed;
        }

        if (returning)
        {
            speed *= movementParams.BoomerangReturnSpeedMultiplier > 0f
                ? movementParams.BoomerangReturnSpeedMultiplier
                : 1f;
        }

        return speed;
    }

    private static float ResolveArcHeight(float chordLength, in MovementParams movementParams)
    {
        if (movementParams.BoomerangArcHeight > 0f)
        {
            return MathF.Min(movementParams.BoomerangArcHeight, chordLength * 0.45f);
        }

        return MathF.Min(MathF.Max(chordLength * DefaultArcHeightRatio, MinArcHeight), MaxAutoArcHeight);
    }

    private static Vector2Value EvaluateQuadraticArc(Vector2Value start, Vector2Value end, float arcHeight, float t, bool clockwise)
    {
        var chord = end - start;
        var normal = clockwise
            ? new Vector2Value(-chord.Y, chord.X).Normalized()
            : new Vector2Value(chord.Y, -chord.X).Normalized();
        var control = start + (chord * 0.5f) + (normal * arcHeight);
        var oneMinusT = 1f - t;
        return (start * (oneMinusT * oneMinusT))
            + (control * (2f * oneMinusT * t))
            + (end * (t * t));
    }
}
