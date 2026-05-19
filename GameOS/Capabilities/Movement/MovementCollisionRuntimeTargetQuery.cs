using System.Collections.Generic;
using SlimeAI.GameOS.Runtime.Entity;

namespace SlimeAI.GameOS.Capabilities.Movement;

/// <summary>
/// 默认 Runtime 碰撞候选目标查询：返回当前 EntityManager 快照。
/// 纯 Runtime / 无 physics 环境使用；
/// Godot 项目必须注入 GodotPhysicsMovementCollisionTargetQuery。
/// </summary>
public sealed class MovementCollisionRuntimeTargetQuery : IMovementCollisionTargetQuery
{
    /// <inheritdoc />
    public IReadOnlyList<IEntity> GetCandidates(
        IEntity source,
        in MovementParams movementParams,
        Vector2Value from,
        Vector2Value intended)
    {
        return EntityManager.GetAll();
    }
}
