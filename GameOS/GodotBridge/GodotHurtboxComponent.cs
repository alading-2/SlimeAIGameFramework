using Godot;
using SkilmeAI.GameOS.Capabilities.Collision;
using SkilmeAI.GameOS.Runtime.Entity;

namespace SkilmeAI.GameOS.GodotBridge;

/// <summary>
/// 桥接独立 `Area2D` Hurtbox 节点到 `Collision.Events.HurtboxEntered` / `HurtboxExited`。
/// </summary>
public partial class GodotHurtboxComponent : Area2D, IGodotComponent
{
    private IEntity? entity;

    /// <summary>
    /// 是否忽略同队目标。
    /// </summary>
    [Export]
    public bool IgnoreSameTeam { get; set; } = true;

    /// <inheritdoc />
    public void OnComponentRegistered(IEntity entity, Node entityNode)
    {
        this.entity = entity;
        GodotCollisionBridge.SyncCollisionObjectData(entity, this);
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
        AreaEntered += OnAreaEntered;
        AreaExited += OnAreaExited;
    }

    /// <inheritdoc />
    public void OnComponentUnregistered(IEntity? entity, Node? entityNode)
    {
        BodyEntered -= OnBodyEntered;
        BodyExited -= OnBodyExited;
        AreaEntered -= OnAreaEntered;
        AreaExited -= OnAreaExited;
        this.entity = null;
    }

    /// <summary>
    /// 手动发射 Hurtbox 进入事件，供 smoke 或测试绕开物理帧时序。
    /// </summary>
    public bool EmitEntered(Node targetNode)
    {
        return entity != null && GodotCollisionBridge.EmitHurtboxEntered(entity, targetNode, CreatePolicy());
    }

    /// <summary>
    /// 手动发射 Hurtbox 离开事件，供 smoke 或测试绕开物理帧时序。
    /// </summary>
    public bool EmitExited(Node targetNode)
    {
        return entity != null && GodotCollisionBridge.EmitHurtboxExited(entity, targetNode, CreatePolicy());
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
