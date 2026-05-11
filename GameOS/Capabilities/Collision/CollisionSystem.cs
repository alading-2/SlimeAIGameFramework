using System;
using SkilmeAI.GameOS.Capabilities.Collision.Events;
using SkilmeAI.GameOS.Runtime.Entity;

namespace SkilmeAI.GameOS.Capabilities.Collision;

/// <summary>
/// Collision Capability 纯 C# 事件和过滤系统。
/// </summary>
public sealed class CollisionSystem
{
    /// <summary>
    /// 创建 CollisionSystem，并确保 DataKey 已注册。
    /// </summary>
    public CollisionSystem()
    {
        CollisionDataKeys.RegisterAll();
    }

    /// <summary>
    /// 判断两个实体是否满足碰撞过滤策略。
    /// </summary>
    public bool CanCollide(IEntity source, IEntity target, CollisionFilterPolicy policy = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        if (policy == default)
        {
            policy = CollisionFilterPolicy.Default;
        }

        if (policy.IgnoreSelf && source.EntityId == target.EntityId)
        {
            return false;
        }

        if (policy.IgnoreSameTeam)
        {
            var sourceTeam = source.Data.Get<int>(CollisionDataKeys.Team, 0);
            var targetTeam = target.Data.Get<int>(CollisionDataKeys.Team, 0);
            if (sourceTeam != 0 && sourceTeam == targetTeam)
            {
                return false;
            }
        }

        if (!policy.RequireLayerMaskMatch)
        {
            return true;
        }

        var sourceMask = source.Data.Get<uint>(CollisionDataKeys.CollisionMask, 0u);
        var targetLayer = target.Data.Get<uint>(CollisionDataKeys.CollisionLayer, 0u);
        return (sourceMask & targetLayer) != 0u;
    }

    /// <summary>
    /// 发布碰撞进入事件。
    /// </summary>
    public bool EmitEntered(IEntity source, IEntity target, CollisionFilterPolicy policy = default)
    {
        if (!CanCollide(source, target, policy))
        {
            return false;
        }

        var contact = CreateContact(source, target);
        source.Events.Publish(new Entered(contact));
        return true;
    }

    /// <summary>
    /// 发布碰撞离开事件。
    /// </summary>
    public bool EmitExited(IEntity source, IEntity target, CollisionFilterPolicy policy = default)
    {
        if (!CanCollide(source, target, policy))
        {
            return false;
        }

        var contact = CreateContact(source, target);
        source.Events.Publish(new Exited(contact));
        return true;
    }

    private static CollisionContact CreateContact(IEntity source, IEntity target)
    {
        return new CollisionContact(
            source,
            target,
            source.Data.Get<uint>(CollisionDataKeys.CollisionLayer, 0u),
            target.Data.Get<uint>(CollisionDataKeys.CollisionLayer, 0u));
    }
}
