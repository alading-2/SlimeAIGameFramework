using System;
using System.Collections.Generic;
using SlimeAI.GameOS.Capabilities.Collision;
using SlimeAI.GameOS.Runtime.Entity;

namespace SlimeAI.GameOS.Capabilities.Movement;

/// <summary>
/// Movement 纯运行时碰撞策略，负责过滤、去重、计数和首次命中扫描。
/// </summary>
public sealed class MovementCollisionPolicy
{
    private readonly CollisionSystem collisionSystem = new();
    private readonly HashSet<EntityId> acceptedTargets = new();
    private readonly IMovementCollisionTargetQuery targetQuery;
    private MovementCollisionParams? config;
    private int acceptedCollisionCount;

    /// <summary>
    /// 创建 Movement 碰撞策略。
    /// </summary>
    /// <param name="targetQuery">候选目标查询；null 时使用 EntityManager Runtime 扫描。</param>
    public MovementCollisionPolicy(IMovementCollisionTargetQuery? targetQuery = null)
    {
        this.targetQuery = targetQuery ?? new MovementCollisionRuntimeTargetQuery();
    }

    /// <summary>当前策略是否启用。</summary>
    public bool IsEnabled => config.HasValue;

    /// <summary>
    /// 用新的 MovementParams 重置单次移动内的碰撞状态。
    /// </summary>
    public void Reset(in MovementParams movementParams)
    {
        config = movementParams.CollisionParams;
        acceptedCollisionCount = 0;
        acceptedTargets.Clear();
    }

    /// <summary>
    /// 在 from -> intended 位移段上查找并接受首次有效碰撞。
    /// </summary>
    public bool TryAcceptFirstHit(
        IEntity source,
        MoveMode mode,
        in MovementParams movementParams,
        Vector2Value from,
        Vector2Value intended,
        out MovementCollisionContext context)
    {
        context = default;
        if (!config.HasValue || from == intended)
        {
            return false;
        }

        var currentConfig = config.Value;
        IEntity? bestTarget = null;
        var bestT = float.PositiveInfinity;
        var bestContact = intended;
        var targets = targetQuery.GetCandidates(source, in movementParams, from, intended);

        for (var i = 0; i < targets.Count; i++)
        {
            var target = targets[i];
            if (!CanConsiderTarget(source, target, movementParams, currentConfig))
            {
                continue;
            }

            var targetPosition = target.Data.Get<Vector2Value>(MovementDataKeys.Position, Vector2Value.Zero);
            var radius = ResolveRadius(source, currentConfig.SourceRadiusOverride)
                         + ResolveRadius(target, currentConfig.TargetRadiusOverride);
            if (!TrySegmentCircleHit(from, intended, targetPosition, radius, out var hitT))
            {
                continue;
            }

            if (hitT < bestT)
            {
                bestTarget = target;
                bestT = hitT;
                bestContact = from + ((intended - from) * hitT);
            }
        }

        if (bestTarget == null)
        {
            return false;
        }

        acceptedTargets.Add(bestTarget.EntityId);
        acceptedCollisionCount++;
        var willStop = currentConfig.StopAfterCollisionCount >= 0
                       && acceptedCollisionCount >= currentConfig.StopAfterCollisionCount;

        context = new MovementCollisionContext(
            mode,
            source,
            bestTarget,
            acceptedCollisionCount,
            willStop,
            movementParams,
            from,
            intended,
            bestContact);
        return true;
    }

    private bool CanConsiderTarget(
        IEntity source,
        IEntity target,
        in MovementParams movementParams,
        MovementCollisionParams currentConfig)
    {
        if (source.EntityId == target.EntityId)
        {
            return false;
        }

        if (acceptedTargets.Contains(target.EntityId))
        {
            return false;
        }

        if (!target.Data.Has(MovementDataKeys.Position))
        {
            return false;
        }

        if (!PassTargetMatch(target, movementParams, currentConfig))
        {
            return false;
        }

        return collisionSystem.CanCollide(source, target, currentConfig.FilterPolicy);
    }

    private static bool PassTargetMatch(
        IEntity target,
        in MovementParams movementParams,
        MovementCollisionParams currentConfig)
    {
        return currentConfig.TargetMatchMode switch
        {
            MovementCollisionTargetMatchMode.Any => true,
            MovementCollisionTargetMatchMode.TrackedTargetOnly =>
                movementParams.TargetEntityId.HasValue && target.EntityId == movementParams.TargetEntityId.Value,
            MovementCollisionTargetMatchMode.SpecificEntity =>
                currentConfig.SpecificTargetEntityId.HasValue && target.EntityId == currentConfig.SpecificTargetEntityId.Value,
            _ => false
        };
    }

    private static float ResolveRadius(IEntity entity, float overrideRadius)
    {
        if (overrideRadius >= 0f)
        {
            return overrideRadius;
        }

        return MathF.Max(0f, entity.Data.Get<float>(CollisionDataKeys.CollisionRadius, 0f));
    }

    private static bool TrySegmentCircleHit(
        Vector2Value from,
        Vector2Value to,
        Vector2Value center,
        float radius,
        out float hitT)
    {
        hitT = 0f;
        var segment = to - from;
        var fromToCenter = from - center;
        var radiusSquared = radius * radius;

        if ((fromToCenter.X * fromToCenter.X) + (fromToCenter.Y * fromToCenter.Y) <= radiusSquared)
        {
            return true;
        }

        var a = (segment.X * segment.X) + (segment.Y * segment.Y);
        if (a <= float.Epsilon)
        {
            return false;
        }

        var b = 2f * ((fromToCenter.X * segment.X) + (fromToCenter.Y * segment.Y));
        var c = ((fromToCenter.X * fromToCenter.X) + (fromToCenter.Y * fromToCenter.Y)) - radiusSquared;
        var discriminant = (b * b) - (4f * a * c);
        if (discriminant < 0f)
        {
            return false;
        }

        var sqrt = MathF.Sqrt(discriminant);
        var first = (-b - sqrt) / (2f * a);
        var second = (-b + sqrt) / (2f * a);
        if (first is >= 0f and <= 1f)
        {
            hitT = first;
            return true;
        }

        if (second is >= 0f and <= 1f)
        {
            hitT = second;
            return true;
        }

        return false;
    }
}
