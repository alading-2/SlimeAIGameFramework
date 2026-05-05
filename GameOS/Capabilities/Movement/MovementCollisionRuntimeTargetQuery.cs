using System.Collections.Generic;
using SkilmeAI.GameOS.Runtime.Entity;

namespace SkilmeAI.GameOS.Capabilities.Movement;

/// <summary>
/// 默认 Runtime 查询：返回当前 EntityManager 快照。
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
