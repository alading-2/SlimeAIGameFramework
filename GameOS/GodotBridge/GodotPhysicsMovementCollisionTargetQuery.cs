using System;
using System.Collections.Generic;
using Godot;
using SkilmeAI.GameOS.Capabilities.Collision;
using SkilmeAI.GameOS.Capabilities.Movement;
using SkilmeAI.GameOS.Runtime.Entity;

namespace SkilmeAI.GameOS.GodotBridge;

/// <summary>
/// 使用 Godot Physics 2D 做 Movement 碰撞 broadphase 查询。
/// </summary>
public sealed class GodotPhysicsMovementCollisionTargetQuery : IMovementCollisionTargetQuery
{
    private const int MaxPhysicsResults = 32;

    private readonly List<IEntity> candidates = new();
    private readonly HashSet<string> candidateIds = new(StringComparer.Ordinal);
    private readonly MovementCollisionRuntimeTargetQuery runtimeFallback = new();

    /// <summary>最近一次查询是否成功访问 Godot Physics。</summary>
    public bool LastQueryUsedPhysics { get; private set; }

    /// <summary>最近一次查询由 Godot Physics 返回的候选实体数量。</summary>
    public int LastPhysicsCandidateCount { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<IEntity> GetCandidates(
        IEntity source,
        in MovementParams movementParams,
        Vector2Value from,
        Vector2Value intended)
    {
        candidates.Clear();
        candidateIds.Clear();
        LastQueryUsedPhysics = false;
        LastPhysicsCandidateCount = 0;

        AddPhysicsCandidates(source, in movementParams, from, intended);
        AddRuntimeFallbackCandidates(source, in movementParams, from, intended);
        return candidates;
    }

    private void AddPhysicsCandidates(
        IEntity source,
        in MovementParams movementParams,
        Vector2Value from,
        Vector2Value intended)
    {
        if (GodotNodeRegistry.GetNodeById(source.EntityId) is not Node2D sourceNode || !sourceNode.IsInsideTree())
        {
            return;
        }

        if (sourceNode is CollisionObject2D collisionObject)
        {
            GodotCollisionBridge.SyncCollisionObjectData(source, collisionObject);
        }

        var distance = Vector2Value.Distance(from, intended);
        if (distance <= float.Epsilon)
        {
            return;
        }

        var world = sourceNode.GetWorld2D();
        var space = world.DirectSpaceState;
        var sourceRadius = ResolveSourceRadius(source, in movementParams);
        var midpoint = new Vector2((from.X + intended.X) * 0.5f, (from.Y + intended.Y) * 0.5f);
        var shape = new CircleShape2D
        {
            Radius = (distance * 0.5f) + sourceRadius
        };

        var parameters = new PhysicsShapeQueryParameters2D
        {
            Shape = shape,
            Transform = new Transform2D(0f, midpoint),
            CollisionMask = source.Data.Get<uint>(CollisionDataKeys.CollisionMask, uint.MaxValue),
            CollideWithAreas = true,
            CollideWithBodies = true
        };

        var hits = space.IntersectShape(parameters, MaxPhysicsResults);
        LastQueryUsedPhysics = true;
        for (var i = 0; i < hits.Count; i++)
        {
            if (!hits[i].TryGetValue("collider", out var colliderValue))
            {
                continue;
            }

            if (colliderValue.AsGodotObject() is not Node collider)
            {
                continue;
            }

            var entity = GodotCollisionBridge.ResolveOwningEntity(collider);
            if (entity == null)
            {
                continue;
            }

            SyncPhysicsEntityData(entity, collider);
            if (TryAddCandidate(entity))
            {
                LastPhysicsCandidateCount++;
            }
        }
    }

    private void AddRuntimeFallbackCandidates(
        IEntity source,
        in MovementParams movementParams,
        Vector2Value from,
        Vector2Value intended)
    {
        var fallbackCandidates = runtimeFallback.GetCandidates(source, in movementParams, from, intended);
        for (var i = 0; i < fallbackCandidates.Count; i++)
        {
            TryAddCandidate(fallbackCandidates[i]);
        }
    }

    private bool TryAddCandidate(IEntity entity)
    {
        if (!candidateIds.Add(entity.EntityId))
        {
            return false;
        }

        candidates.Add(entity);
        return true;
    }

    private static float ResolveSourceRadius(IEntity source, in MovementParams movementParams)
    {
        var collisionParams = movementParams.CollisionParams;
        if (collisionParams?.SourceRadiusOverride >= 0f)
        {
            return collisionParams.Value.SourceRadiusOverride;
        }

        return MathF.Max(0f, source.Data.Get<float>(CollisionDataKeys.CollisionRadius, 0f));
    }

    private static void SyncPhysicsEntityData(IEntity entity, Node node)
    {
        var current = node;
        while (current != null)
        {
            if (current is CollisionObject2D collisionObject)
            {
                GodotCollisionBridge.SyncCollisionObjectData(entity, collisionObject);
                return;
            }

            current = current.GetParent();
        }
    }
}
