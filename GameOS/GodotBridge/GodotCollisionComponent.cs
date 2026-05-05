using System;
using Godot;
using SkilmeAI.GameOS.Capabilities.Collision;
using SkilmeAI.GameOS.Runtime.Entity;

namespace SkilmeAI.GameOS.GodotBridge;

/// <summary>
/// 桥接 Entity 根节点 `Area2D` 的碰撞信号到 `GameEventType.Collision`。
/// </summary>
public partial class GodotCollisionComponent : Node, IGodotComponent
{
    private IEntity? entity;
    private Area2D? area;

    /// <summary>
    /// 是否忽略同队目标。
    /// </summary>
    [Export]
    public bool IgnoreSameTeam { get; set; } = false;

    /// <inheritdoc />
    public void OnComponentRegistered(IEntity entity, Node entityNode)
    {
        this.entity = entity;
        if (entityNode is not Area2D entityArea)
        {
            return;
        }

        area = entityArea;
        GodotCollisionBridge.SyncCollisionObjectData(entity, entityArea);
        area.BodyEntered += OnBodyEntered;
        area.BodyExited += OnBodyExited;
        area.AreaEntered += OnAreaEntered;
        area.AreaExited += OnAreaExited;
    }

    /// <inheritdoc />
    public void OnComponentUnregistered(IEntity? entity, Node? entityNode)
    {
        if (area != null && GodotObject.IsInstanceValid(area))
        {
            area.BodyEntered -= OnBodyEntered;
            area.BodyExited -= OnBodyExited;
            area.AreaEntered -= OnAreaEntered;
            area.AreaExited -= OnAreaExited;
        }

        this.entity = null;
        area = null;
    }

    /// <summary>
    /// 手动发射进入事件，供 smoke 或测试绕开物理帧时序。
    /// </summary>
    public bool EmitEntered(Node targetNode)
    {
        return entity != null && GodotCollisionBridge.EmitEntered(entity, targetNode, CreatePolicy());
    }

    /// <summary>
    /// 手动发射离开事件，供 smoke 或测试绕开物理帧时序。
    /// </summary>
    public bool EmitExited(Node targetNode)
    {
        return entity != null && GodotCollisionBridge.EmitExited(entity, targetNode, CreatePolicy());
    }

    private CollisionFilterPolicy CreatePolicy()
    {
        return new CollisionFilterPolicy(IgnoreSameTeam: IgnoreSameTeam);
    }

    private void OnBodyEntered(Node2D body)
    {
        EmitEntered(body);
    }

    private void OnBodyExited(Node2D body)
    {
        EmitExited(body);
    }

    private void OnAreaEntered(Area2D other)
    {
        EmitEntered(other);
    }

    private void OnAreaExited(Area2D other)
    {
        EmitExited(other);
    }
}
