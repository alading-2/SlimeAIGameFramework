using Godot;
using SlimeAI.GameOS.Runtime.Data;
using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Event;

namespace SlimeAI.GameOS.GodotBridge;

/// <summary>
/// 可直接挂到 Godot 场景的 GameOS Entity 基类。
/// </summary>
public partial class GodotEntity : Node, IEntity
{
    private readonly EntityEventBus events;

    /// <summary>
    /// 创建带局部 Data 和 EventBus 的 Godot Entity。
    /// </summary>
    public GodotEntity()
    {
        events = new EntityEventBus($"entity:{GodotNodeRegistry.GetNodeInstanceId(this)}", WorldEvents.World);
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
    public IEventBus Events => events;

    /// <inheritdoc />
    public override void _EnterTree()
    {
        if (GameOSGodotBridge.RegisterEntity(this, this, AutoRegisterComponents))
        {
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
