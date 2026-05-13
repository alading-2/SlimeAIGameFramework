using SlimeAI.GameOS.Runtime.Entity;

namespace SlimeAI.GameOS.Capabilities.Movement;

/// <summary>
/// Movement 停止回调上下文。
/// </summary>
public readonly record struct MovementStopContext(
    IEntity Entity,
    MovementParams Params,
    MovementStopReason Reason,
    Vector2Value FinalPosition);
