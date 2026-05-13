using System;
using Godot;
using SlimeAI.GameOS.Capabilities.Collision;
using SlimeAI.GameOS.Capabilities.Collision.Events;
using SlimeAI.GameOS.Runtime.Entity;

namespace SlimeAI.GameOS.GodotBridge;

/// <summary>
/// Godot 2D 碰撞节点到 Collision Capability 的桥接工具。
/// </summary>
public static class GodotCollisionBridge
{
    /// <summary>
    /// 从 Godot 碰撞节点解析所属 GameOS Entity。
    /// </summary>
    public static IEntity? ResolveOwningEntity(Node? node)
    {
        var current = node;
        while (current != null)
        {
            if (current is IEntity entity)
            {
                return entity;
            }

            current = current.GetParent();
        }

        return null;
    }

    /// <summary>
    /// 从 Godot `CollisionObject2D` 同步 layer / mask / radius 到 Runtime Data。
    /// </summary>
    public static void SyncCollisionObjectData(IEntity entity, CollisionObject2D collisionObject)
    {
        entity.Data.Set(CollisionDataKeys.CollisionLayer, (uint)collisionObject.CollisionLayer);
        entity.Data.Set(CollisionDataKeys.CollisionMask, (uint)collisionObject.CollisionMask);
        var radius = ResolveCollisionRadius(collisionObject);
        if (radius > 0f)
        {
            entity.Data.Set(CollisionDataKeys.CollisionRadius, radius);
        }
    }

    /// <summary>
    /// 发射碰撞进入事件。
    /// </summary>
    public static bool EmitEntered(IEntity source, Node targetNode, CollisionFilterPolicy policy = default)
    {
        var target = ResolveOwningEntity(targetNode);
        if (target == null)
        {
            return false;
        }

        return new CollisionSystem().EmitEntered(source, target, policy);
    }

    /// <summary>
    /// 发射碰撞离开事件。
    /// </summary>
    public static bool EmitExited(IEntity source, Node targetNode, CollisionFilterPolicy policy = default)
    {
        var target = ResolveOwningEntity(targetNode);
        if (target == null)
        {
            return false;
        }

        return new CollisionSystem().EmitExited(source, target, policy);
    }

    /// <summary>
    /// 发射 Hurtbox 进入事件。
    /// </summary>
    public static bool EmitHurtboxEntered(IEntity source, Node targetNode, CollisionFilterPolicy policy = default)
    {
        var target = ResolveOwningEntity(targetNode);
        if (target == null)
        {
            return false;
        }

        var collisionSystem = new CollisionSystem();
        if (!collisionSystem.CanCollide(source, target, policy))
        {
            return false;
        }

        var contact = new CollisionContact(
            source,
            target,
            source.Data.Get<uint>(CollisionDataKeys.CollisionLayer, 0u),
            target.Data.Get<uint>(CollisionDataKeys.CollisionLayer, 0u));
        source.Events.Publish(new HurtboxEntered(contact));
        return true;
    }

    /// <summary>
    /// 发射 Hurtbox 离开事件。
    /// </summary>
    public static bool EmitHurtboxExited(IEntity source, Node targetNode, CollisionFilterPolicy policy = default)
    {
        var target = ResolveOwningEntity(targetNode);
        if (target == null)
        {
            return false;
        }

        var collisionSystem = new CollisionSystem();
        if (!collisionSystem.CanCollide(source, target, policy))
        {
            return false;
        }

        var contact = new CollisionContact(
            source,
            target,
            source.Data.Get<uint>(CollisionDataKeys.CollisionLayer, 0u),
            target.Data.Get<uint>(CollisionDataKeys.CollisionLayer, 0u));
        source.Events.Publish(new HurtboxExited(contact));
        return true;
    }

    private static float ResolveCollisionRadius(Node node)
    {
        var radius = 0f;
        ResolveCollisionRadiusRecursive(node, ref radius);
        return radius;
    }

    private static void ResolveCollisionRadiusRecursive(Node node, ref float radius)
    {
        if (node is CollisionShape2D collisionShape)
        {
            radius = MathF.Max(radius, ResolveShapeRadius(collisionShape.Shape));
        }
        else if (node is CollisionPolygon2D collisionPolygon)
        {
            radius = MathF.Max(radius, ResolvePolygonRadius(collisionPolygon));
        }

        for (var i = 0; i < node.GetChildCount(); i++)
        {
            ResolveCollisionRadiusRecursive(node.GetChild(i), ref radius);
        }
    }

    private static float ResolveShapeRadius(Shape2D? shape)
    {
        return shape switch
        {
            CircleShape2D circle => circle.Radius,
            RectangleShape2D rectangle => MathF.Max(rectangle.Size.X, rectangle.Size.Y) * 0.5f,
            CapsuleShape2D capsule => MathF.Max(capsule.Radius, capsule.Height * 0.5f),
            _ => 0f
        };
    }

    private static float ResolvePolygonRadius(CollisionPolygon2D polygon)
    {
        var points = polygon.Polygon;
        var radius = 0f;
        for (var i = 0; i < points.Length; i++)
        {
            radius = MathF.Max(radius, points[i].Length());
        }

        return radius;
    }
}
