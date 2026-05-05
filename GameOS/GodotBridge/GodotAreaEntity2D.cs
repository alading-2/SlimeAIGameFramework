using Godot;
using SkilmeAI.GameOS.Capabilities.Collision;
using SkilmeAI.GameOS.Capabilities.Movement;
using SkilmeAI.GameOS.Runtime.Data;
using SkilmeAI.GameOS.Runtime.Entity;
using SkilmeAI.GameOS.Runtime.Event;

namespace SkilmeAI.GameOS.GodotBridge;

/// <summary>
/// 可直接挂到 2D 物理场景的 `Area2D` GameOS Entity 基类。
/// </summary>
public partial class GodotAreaEntity2D : Area2D, IEntity
{
    private readonly EventBus events = new();

    /// <summary>
    /// 创建带局部 Data 和 EventBus 的 2D Area Entity。
    /// </summary>
    public GodotAreaEntity2D()
    {
        Data = new Data(new EventDataChangeSink(events));
    }

    /// <summary>
    /// 可选稳定 Id；留空时使用 Godot InstanceId。
    /// </summary>
    [Export]
    public string EntityIdOverride { get; set; } = string.Empty;

    /// <summary>
    /// 进入 SceneTree 时是否扫描并注册子 Component。
    /// </summary>
    [Export]
    public bool AutoRegisterComponents { get; set; } = true;

    /// <inheritdoc />
    public string EntityId => string.IsNullOrWhiteSpace(EntityIdOverride)
        ? GodotNodeRegistry.GetNodeInstanceId(this)
        : EntityIdOverride;

    /// <inheritdoc />
    public Data Data { get; }

    /// <inheritdoc />
    public EventBus Events => events;

    /// <inheritdoc />
    public override void _EnterTree()
    {
        if (GameOSGodotBridge.RegisterEntity(this, this, AutoRegisterComponents))
        {
            Data.Set(MovementDataKeys.Position, new Vector2Value(Position.X, Position.Y));
            GodotCollisionBridge.SyncCollisionObjectData(this, this);
            OnGameOSEntityRegistered();
        }
    }

    /// <inheritdoc />
    public override void _ExitTree()
    {
        if (GameOSGodotBridge.UnregisterEntity(this, this, unregisterComponents: true, destroyRuntimeEntity: true))
        {
            OnGameOSEntityUnregistered();
        }
    }

    /// <summary>
    /// 通过 GameOS 生命周期销毁节点。
    /// </summary>
    public void DestroyEntity()
    {
        GameOSGodotBridge.DestroyEntity(this, this);
    }

    /// <summary>
    /// Entity 已纳入 GameOS 注册表。
    /// </summary>
    protected virtual void OnGameOSEntityRegistered()
    {
    }

    /// <summary>
    /// Entity 已从 GameOS 注册表移除。
    /// </summary>
    protected virtual void OnGameOSEntityUnregistered()
    {
    }
}
