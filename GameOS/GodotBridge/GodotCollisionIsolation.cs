using System;
using System.Collections.Generic;
using Godot;

namespace SkilmeAI.GameOS.GodotBridge;

/// <summary>
/// Collision isolation helpers for pooled Godot 2D nodes.
/// </summary>
public static class GodotCollisionIsolation
{
    private static readonly Dictionary<ulong, CollisionObjectSnapshot> CollisionSnapshots = new();

    /// <summary>
    /// Returns true when removing the node from SceneTree is the safest idle state.
    /// </summary>
    /// <param name="node">Root node being pooled.</param>
    public static bool NeedsTreeDetach(Node node)
    {
        ArgumentNullException.ThrowIfNull(node);
        return node is CollisionObject2D;
    }

    /// <summary>
    /// Disables collision participation with deferred property writes.
    /// </summary>
    /// <param name="node">Root node.</param>
    public static void DisableDeferred(Node node)
    {
        SetTreeActive(node, active: false, deferred: true, isolateLayerMask: true);
    }

    /// <summary>
    /// Disables collision participation immediately after a node enters SceneTree.
    /// </summary>
    /// <param name="node">Root node.</param>
    public static void DisableDirect(Node node)
    {
        SetTreeActive(node, active: false, deferred: false, isolateLayerMask: true);
    }

    /// <summary>
    /// Restores collision participation for a pooled node tree.
    /// </summary>
    /// <param name="node">Root node.</param>
    public static void Restore(Node node)
    {
        SetTreeActive(node, active: true, deferred: true, isolateLayerMask: true);
    }

    /// <summary>
    /// Applies a collision active state recursively.
    /// </summary>
    /// <param name="node">Root node.</param>
    /// <param name="active">Whether collision should participate in physics.</param>
    /// <param name="deferred">Whether to use Godot deferred property writes.</param>
    /// <param name="isolateLayerMask">Whether layer and mask are cleared while inactive.</param>
    public static void SetTreeActive(Node node, bool active, bool deferred, bool isolateLayerMask)
    {
        ArgumentNullException.ThrowIfNull(node);

        if (!active && node is CharacterBody2D body)
        {
            body.Velocity = Vector2.Zero;
        }

        if (node is CollisionObject2D collisionObject && isolateLayerMask)
        {
            SetCollisionObjectActive(collisionObject, active, deferred);
        }

        if (node is Area2D area)
        {
            SetAreaActive(area, active, deferred);
        }

        if (node is CollisionShape2D shape)
        {
            SetCollisionShapeActive(shape, active, deferred);
        }
        else if (node is CollisionPolygon2D polygon)
        {
            SetCollisionPolygonActive(polygon, active, deferred);
        }

        var children = node.GetChildren();
        foreach (var child in children)
        {
            if (child is Node childNode)
            {
                SetTreeActive(childNode, active, deferred, isolateLayerMask);
            }
        }
    }

    private static void SetCollisionObjectActive(CollisionObject2D collisionObject, bool active, bool deferred)
    {
        var id = collisionObject.GetInstanceId();
        if (active)
        {
            if (CollisionSnapshots.TryGetValue(id, out var snapshot))
            {
                SetCollisionLayerMask(collisionObject, snapshot.Layer, snapshot.Mask, deferred);
            }

            return;
        }

        if (!CollisionSnapshots.ContainsKey(id))
        {
            CollisionSnapshots[id] = new CollisionObjectSnapshot(
                collisionObject.CollisionLayer,
                collisionObject.CollisionMask);
        }

        SetCollisionLayerMask(collisionObject, layer: 0, mask: 0, deferred);
    }

    private static void SetCollisionLayerMask(CollisionObject2D collisionObject, uint layer, uint mask, bool deferred)
    {
        if (deferred)
        {
            collisionObject.SetDeferred(CollisionObject2D.PropertyName.CollisionLayer, layer);
            collisionObject.SetDeferred(CollisionObject2D.PropertyName.CollisionMask, mask);
            return;
        }

        collisionObject.CollisionLayer = layer;
        collisionObject.CollisionMask = mask;
    }

    private static void SetAreaActive(Area2D area, bool active, bool deferred)
    {
        if (deferred)
        {
            area.SetDeferred(Area2D.PropertyName.Monitoring, active);
            area.SetDeferred(Area2D.PropertyName.Monitorable, active);
            return;
        }

        area.Monitoring = active;
        area.Monitorable = active;
    }

    private static void SetCollisionShapeActive(CollisionShape2D shape, bool active, bool deferred)
    {
        if (deferred)
        {
            shape.SetDeferred(CollisionShape2D.PropertyName.Disabled, !active);
            return;
        }

        shape.Disabled = !active;
    }

    private static void SetCollisionPolygonActive(CollisionPolygon2D polygon, bool active, bool deferred)
    {
        if (deferred)
        {
            polygon.SetDeferred(CollisionPolygon2D.PropertyName.Disabled, !active);
            return;
        }

        polygon.Disabled = !active;
    }

    private readonly record struct CollisionObjectSnapshot(uint Layer, uint Mask);
}
