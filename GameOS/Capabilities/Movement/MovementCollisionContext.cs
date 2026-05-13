using SlimeAI.GameOS.Runtime.Entity;

namespace SlimeAI.GameOS.Capabilities.Movement;

/// <summary>
/// 一次有效 Movement 碰撞上下文。
/// </summary>
public readonly record struct MovementCollisionContext(
    MoveMode Mode,
    IEntity Source,
    IEntity Target,
    int CollisionCount,
    bool WillStop,
    MovementParams Params,
    Vector2Value FromPosition,
    Vector2Value IntendedPosition,
    Vector2Value ContactPosition);
