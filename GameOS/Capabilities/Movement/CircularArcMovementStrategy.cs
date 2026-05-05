using System;
using SkilmeAI.GameOS.Runtime.Data;
using SkilmeAI.GameOS.Runtime.Entity;

namespace SkilmeAI.GameOS.Capabilities.Movement;

/// <summary>
/// CircularArc 策略：从起点到固定终点的单段圆弧轨迹。
/// </summary>
public sealed class CircularArcMovementStrategy : IMovementStrategy
{
    private Vector2Value startPosition;
    private Vector2Value targetPosition;
    private Vector2Value center;
    private float startAngle;
    private float sweepAngle;
    private float progress;
    private bool isValidArc;

    /// <inheritdoc />
    public void OnEnter(IEntity entity, Data data, in MovementParams movementParams)
    {
        startPosition = data.Get<Vector2Value>(MovementDataKeys.Position, Vector2Value.Zero);
        targetPosition = movementParams.TargetPosition ?? startPosition;
        progress = 0f;
        isValidArc = TryBuildArc(movementParams);
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
        var nextPosition = isValidArc
            ? EvaluateArc(nextProgress, movementParams.CircularArcRadius)
            : startPosition + ((targetPosition - startPosition) * nextProgress);
        var displacement = nextPosition - position;
        var velocity = displacement / delta;
        data.Set(MovementDataKeys.Velocity, velocity);
        progress = nextProgress;

        var result = MovementUpdateResult.Continue(displacement.Length, velocity.Normalized());
        return progress >= 1f ? MovementUpdateResult.Complete() : result;
    }

    private bool TryBuildArc(in MovementParams movementParams)
    {
        var chord = targetPosition - startPosition;
        var chordLength = chord.Length;
        var radius = movementParams.CircularArcRadius;
        if (radius <= 0f || chordLength <= 0.001f || radius < chordLength * 0.5f)
        {
            return false;
        }

        var midpoint = (startPosition + targetPosition) * 0.5f;
        var halfChord = chordLength * 0.5f;
        var centerDistance = MathF.Sqrt(MathF.Max(0f, (radius * radius) - (halfChord * halfChord)));
        var normal = new Vector2Value(-chord.Y / chordLength, chord.X / chordLength);
        var firstCenter = midpoint + (normal * centerDistance);
        var secondCenter = midpoint - (normal * centerDistance);

        if (movementParams.BowWorldUp)
        {
            center = firstCenter.Y > secondCenter.Y ? firstCenter : secondCenter;
        }
        else
        {
            center = movementParams.CircularArcClockwise ? firstCenter : secondCenter;
        }

        startAngle = MovementMath.AngleFromDirection(startPosition - center);
        var endAngle = MovementMath.AngleFromDirection(targetPosition - center);
        sweepAngle = ResolveSweep(startAngle, endAngle, movementParams);
        return MathF.Abs(sweepAngle) > 0.001f;
    }

    private static float ResolveSweep(float start, float end, in MovementParams movementParams)
    {
        var sweep = end - start;
        while (sweep <= -180f) sweep += 360f;
        while (sweep > 180f) sweep -= 360f;

        if (movementParams.BowWorldUp)
        {
            return sweep;
        }

        if (movementParams.CircularArcClockwise && sweep < 0f)
        {
            sweep += 360f;
        }
        else if (!movementParams.CircularArcClockwise && sweep > 0f)
        {
            sweep -= 360f;
        }

        return sweep;
    }

    private Vector2Value EvaluateArc(float t, float radius)
    {
        return center + (MovementMath.DirectionFromAngle(startAngle + (sweepAngle * t)) * radius);
    }

    private float ResolveDuration(in MovementParams movementParams)
    {
        if (movementParams.MaxDuration > 0f)
        {
            return movementParams.MaxDuration;
        }

        if (movementParams.Speed <= 0f)
        {
            return 0f;
        }

        var distance = isValidArc
            ? MathF.Abs(sweepAngle) * (MathF.PI / 180f) * movementParams.CircularArcRadius
            : Vector2Value.Distance(startPosition, targetPosition);
        return distance / movementParams.Speed;
    }
}
