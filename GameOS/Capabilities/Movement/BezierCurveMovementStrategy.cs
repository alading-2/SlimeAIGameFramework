using System;
using SkilmeAI.GameOS.Runtime.Data;
using SkilmeAI.GameOS.Runtime.Entity;

namespace SkilmeAI.GameOS.Capabilities.Movement;

/// <summary>
/// BezierCurve 策略：按固定控制点从起点移动到终点。
/// </summary>
public sealed class BezierCurveMovementStrategy : IMovementStrategy
{
    private Vector2Value[] points = [];
    private float progress;

    /// <inheritdoc />
    public void OnEnter(IEntity entity, Data data, in MovementParams movementParams)
    {
        var startPosition = data.Get<Vector2Value>(MovementDataKeys.Position, Vector2Value.Zero);
        var sourcePoints = movementParams.BezierPoints;
        var targetPosition = movementParams.TargetPosition ?? startPosition;
        var pointCount = Math.Max(2, (sourcePoints?.Count ?? 0) + 1);

        points = new Vector2Value[pointCount];
        points[0] = startPosition;

        if (sourcePoints is { Count: > 0 })
        {
            for (var i = 0; i < sourcePoints.Count; i++)
            {
                points[i + 1] = sourcePoints[i];
            }
        }
        else
        {
            points[1] = targetPosition;
        }

        points[^1] = targetPosition;
        progress = 0f;
    }

    /// <inheritdoc />
    public MovementUpdateResult Update(IEntity entity, Data data, float delta, in MovementParams movementParams)
    {
        if (delta <= 0f || points.Length < 2)
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

        var nextProgress = MathF.Min(progress + (delta / duration), 1f);
        var position = data.Get<Vector2Value>(MovementDataKeys.Position, Vector2Value.Zero);
        var nextPosition = Evaluate(points, nextProgress);
        var displacement = nextPosition - position;
        var velocity = displacement / delta;
        data.Set(MovementDataKeys.Velocity, velocity);
        progress = nextProgress;

        var facingDirection = EvaluateTangent(points, nextProgress).Normalized();
        var result = MovementUpdateResult.Continue(displacement.Length, facingDirection);
        return progress >= 1f ? MovementUpdateResult.Complete() : result;
    }

    private float ResolveDuration(in MovementParams movementParams)
    {
        if (movementParams.MaxDuration > 0f)
        {
            return movementParams.MaxDuration;
        }

        var speed = movementParams.Speed > 0f ? movementParams.Speed : 0f;
        if (speed <= 0f || points.Length < 2)
        {
            return 0f;
        }

        return ApproximateLength(points) / speed;
    }

    private static Vector2Value Evaluate(ReadOnlySpan<Vector2Value> controlPoints, float t)
    {
        Span<Vector2Value> work = stackalloc Vector2Value[controlPoints.Length];
        controlPoints.CopyTo(work);
        for (var level = controlPoints.Length - 1; level > 0; level--)
        {
            for (var i = 0; i < level; i++)
            {
                work[i] = Lerp(work[i], work[i + 1], t);
            }
        }

        return work[0];
    }

    private static Vector2Value EvaluateTangent(ReadOnlySpan<Vector2Value> controlPoints, float t)
    {
        if (controlPoints.Length < 2)
        {
            return Vector2Value.Zero;
        }

        Span<Vector2Value> derivative = stackalloc Vector2Value[controlPoints.Length - 1];
        var order = controlPoints.Length - 1;
        for (var i = 0; i < derivative.Length; i++)
        {
            derivative[i] = (controlPoints[i + 1] - controlPoints[i]) * order;
        }

        return Evaluate(derivative, t);
    }

    private static float ApproximateLength(ReadOnlySpan<Vector2Value> controlPoints)
    {
        var previous = Evaluate(controlPoints, 0f);
        var length = 0f;
        for (var i = 1; i <= 24; i++)
        {
            var current = Evaluate(controlPoints, i / 24f);
            length += Vector2Value.Distance(previous, current);
            previous = current;
        }

        return length;
    }

    private static Vector2Value Lerp(Vector2Value from, Vector2Value to, float t)
    {
        return from + ((to - from) * t);
    }
}
