using System;
using System.Collections.Generic;
using SlimeAI.GameOS.Runtime.Data;
using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Schedule;
using MovementEvents = SlimeAI.GameOS.Capabilities.Movement.Events;

namespace SlimeAI.GameOS.Capabilities.Movement;

/// <summary>
/// Movement Capability 纯 C# 调度系统。
/// </summary>
public sealed class MovementSystem : IRuntimeSystem
{
    private sealed class ActiveMovement
    {
        public required IEntity Entity { get; init; }
        public required IMovementStrategy Strategy { get; init; }
        public required MovementCollisionPolicy CollisionPolicy { get; init; }
        public MovementParams Params { get; set; }
    }

    private readonly Dictionary<EntityId, ActiveMovement> activeMovements = new();
    private readonly IMovementCollisionTargetQuery collisionTargetQuery;

    /// <summary>
    /// 创建 MovementSystem，并确保内置策略和 DataKey 已注册。
    /// </summary>
    /// <param name="collisionTargetQuery">Movement 碰撞候选目标查询；null 时使用 Runtime EntityManager 扫描。</param>
    public MovementSystem(IMovementCollisionTargetQuery? collisionTargetQuery = null)
    {
        this.collisionTargetQuery = collisionTargetQuery ?? new MovementCollisionRuntimeTargetQuery();
        MovementDataKeys.RegisterAll();
        MovementStrategyRegistry.RegisterBuiltIns();
    }

    /// <summary>
    /// 启动一次实体移动。
    /// </summary>
    /// <param name="entity">目标实体。</param>
    /// <param name="movementParams">移动参数。</param>
    public bool Start(IEntity entity, MovementParams movementParams)
    {
        ArgumentNullException.ThrowIfNull(entity);
        if (movementParams.Mode == MoveMode.None)
        {
            return Stop(entity, MovementStopReason.External);
        }

        var strategy = MovementStrategyRegistry.Create(movementParams.Mode);
        if (strategy == null)
        {
            return false;
        }

        if (activeMovements.ContainsKey(entity.EntityId))
        {
            Stop(entity, MovementStopReason.Replaced);
        }

        movementParams.ElapsedTime = 0f;
        movementParams.TraveledDistance = 0f;
        var collisionPolicy = new MovementCollisionPolicy(collisionTargetQuery);
        collisionPolicy.Reset(in movementParams);
        strategy.OnEnter(entity, entity.Data, in movementParams);
        activeMovements[entity.EntityId] = new ActiveMovement
        {
            Entity = entity,
            Strategy = strategy,
            CollisionPolicy = collisionPolicy,
            Params = movementParams
        };

        entity.Data.Set(MovementDataKeys.IsMoving, true);
        entity.Events.Publish(new MovementEvents.Started(entity, movementParams));
        return true;
    }

    /// <summary>
    /// 停止实体移动。
    /// </summary>
    /// <param name="entity">目标实体。</param>
    /// <param name="reason">停止原因。</param>
    public bool Stop(IEntity entity, MovementStopReason reason = MovementStopReason.External)
    {
        ArgumentNullException.ThrowIfNull(entity);
        if (!activeMovements.TryGetValue(entity.EntityId, out var active))
        {
            return false;
        }

        Finish(entity, active, reason);
        return true;
    }

    /// <summary>
    /// 推进所有注册实体的移动。
    /// </summary>
    /// <param name="delta">帧间隔秒数。</param>
    public void Tick(float delta)
    {
        if (delta <= 0f || activeMovements.Count == 0)
        {
            return;
        }

        var keys = new List<EntityId>(activeMovements.Keys);
        for (var i = 0; i < keys.Count; i++)
        {
            if (activeMovements.TryGetValue(keys[i], out var active))
                TickEntity(active.Entity, delta);
        }
    }

    /// <summary>
    /// 判断实体当前是否有活动移动。
    /// </summary>
    public bool IsMoving(IEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        return activeMovements.ContainsKey(entity.EntityId);
    }

    /// <summary>
    /// 清空系统状态。
    /// </summary>
    public void Clear()
    {
        activeMovements.Clear();
    }

    private void TickEntity(IEntity entity, float delta)
    {
        if (!activeMovements.TryGetValue(entity.EntityId, out var active))
        {
            return;
        }

        var movementParams = active.Params;
        movementParams.ElapsedTime += delta;

        var result = active.Strategy.Update(entity, entity.Data, delta, in movementParams);
        var distance = ApplyVelocity(entity, active, delta, movementParams, out var stopReason, out var destroyAfterStop);
        movementParams.TraveledDistance += distance;
        active.Params = movementParams;

        if (result.HasFacingDirection)
        {
            entity.Data.Set(MovementDataKeys.FacingDirection, result.FacingDirection.Normalized());
        }
        else if (movementParams.FaceVelocity)
        {
            var velocity = entity.Data.Get<Vector2Value>(MovementDataKeys.Velocity, Vector2Value.Zero);
            if (velocity != Vector2Value.Zero)
            {
                entity.Data.Set(MovementDataKeys.FacingDirection, velocity.Normalized());
            }
        }

        if (stopReason.HasValue)
        {
            Finish(entity, active, stopReason.Value);
            if (destroyAfterStop)
            {
                EntityManager.Destroy(entity);
            }

            return;
        }

        if (movementParams.MaxDuration >= 0f && movementParams.ElapsedTime >= movementParams.MaxDuration)
        {
            Finish(entity, active, MovementStopReason.MaxDuration);
            return;
        }

        if (result.IsCompleted)
        {
            Finish(entity, active, MovementStopReason.Completed);
        }
    }

    private float ApplyVelocity(
        IEntity entity,
        ActiveMovement active,
        float delta,
        in MovementParams movementParams,
        out MovementStopReason? stopReason,
        out bool destroyAfterStop)
    {
        stopReason = null;
        destroyAfterStop = false;
        var data = entity.Data;
        var velocity = data.Get<Vector2Value>(MovementDataKeys.Velocity, Vector2Value.Zero);
        var displacement = velocity * delta;
        var distance = displacement.Length;

        if (movementParams.MaxDistance >= 0f)
        {
            var remainingDistance = movementParams.MaxDistance - movementParams.TraveledDistance;
            if (remainingDistance <= 0f)
            {
                data.Set(MovementDataKeys.Velocity, Vector2Value.Zero);
                stopReason = MovementStopReason.MaxDistance;
                return 0f;
            }

            if (distance > remainingDistance)
            {
                var direction = displacement.Normalized();
                displacement = direction * remainingDistance;
                distance = remainingDistance;
                data.Set(MovementDataKeys.Velocity, direction * (remainingDistance / delta));
                stopReason = MovementStopReason.MaxDistance;
            }
        }

        var position = data.Get<Vector2Value>(MovementDataKeys.Position, Vector2Value.Zero);
        var intendedPosition = position + displacement;
        var dispatchedTargets = new HashSet<EntityId>();
        while (active.CollisionPolicy.TryAcceptFirstHit(
                   entity,
                   movementParams.Mode,
                   in movementParams,
                   position,
                   intendedPosition,
                   out var collisionContext))
        {
            if (!dispatchedTargets.Add(collisionContext.Target.EntityId))
            {
                break;
            }

            DispatchMovementCollision(entity, in collisionContext);

            if (collisionContext.WillStop)
            {
                distance = Vector2Value.Distance(position, collisionContext.ContactPosition);
                data.Set(MovementDataKeys.Position, collisionContext.ContactPosition);
                data.Set(MovementDataKeys.Velocity, Vector2Value.Zero);
                stopReason = MovementStopReason.Collision;
                destroyAfterStop = collisionContext.Params.CollisionParams?.DestroyOnStop == true;
                return distance;
            }
        }

        data.Set(MovementDataKeys.Position, intendedPosition);
        return distance;
    }

    private static void DispatchMovementCollision(IEntity entity, in MovementCollisionContext context)
    {
        var collisionParams = context.Params.CollisionParams;
        collisionParams?.OnCollision?.Invoke(context);
        if (collisionParams?.EmitCollisionEvent != false)
        {
            entity.Events.Publish(new MovementEvents.Collision(context));
        }
    }

    private void Finish(IEntity entity, ActiveMovement active, MovementStopReason reason)
    {
        activeMovements.Remove(entity.EntityId);
        entity.Data.Set(MovementDataKeys.Velocity, Vector2Value.Zero);
        entity.Data.Set(MovementDataKeys.IsMoving, false);

        var finalPosition = entity.Data.Get<Vector2Value>(MovementDataKeys.Position, Vector2Value.Zero);
        var context = new MovementStopContext(entity, active.Params, reason, finalPosition);
        active.Strategy.OnStop(entity, entity.Data, in context);
        entity.Events.Publish(new MovementEvents.Stopped(context));
    }
}
