using Godot;
using SkilmeAI.GameOS.Capabilities.Movement;
using SkilmeAI.GameOS.Runtime.Entity;

namespace SkilmeAI.GameOS.GodotBridge;

/// <summary>
/// 在 Godot `_Process` 中驱动 MovementSystem，并把 Runtime Position 同步到 Node2D。
/// </summary>
public partial class GodotMovementDriver : Node
{
    private readonly GodotPhysicsMovementCollisionTargetQuery physicsCollisionTargetQuery = new();
    private readonly MovementSystem movementSystem;

    /// <summary>
    /// 创建 Godot Movement Driver，并接入 Godot Physics broadphase 查询。
    /// </summary>
    public GodotMovementDriver()
    {
        movementSystem = new MovementSystem(physicsCollisionTargetQuery);
    }

    /// <summary>
    /// 是否自动在 `_Process` 中推进 Movement。
    /// </summary>
    [Export]
    public bool AutoTick { get; set; } = true;

    /// <summary>
    /// 当前 Driver 持有的 MovementSystem。
    /// </summary>
    public MovementSystem MovementSystem => movementSystem;

    /// <summary>
    /// Godot Physics Movement 碰撞候选查询器。
    /// </summary>
    public GodotPhysicsMovementCollisionTargetQuery PhysicsCollisionTargetQuery => physicsCollisionTargetQuery;

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        if (AutoTick)
        {
            TickMovement((float)delta);
        }
    }

    /// <summary>
    /// 手动推进 Movement 并同步 2D 节点位置，供测试或固定调度入口调用。
    /// </summary>
    /// <param name="delta">帧间隔秒数。</param>
    public void TickMovement(float delta)
    {
        movementSystem.Tick(delta);
        SyncNode2DPositions();
    }

    private static void SyncNode2DPositions()
    {
        var nodes = GodotNodeRegistry.GetNodesByType<Node2D>();
        for (var i = 0; i < nodes.Count; i++)
        {
            if (nodes[i] is not IEntity entity || !entity.Data.Has(MovementDataKeys.Position))
            {
                continue;
            }

            var position = entity.Data.Get<Vector2Value>(MovementDataKeys.Position, Vector2Value.Zero);
            nodes[i].Position = new Vector2(position.X, position.Y);
        }
    }
}
