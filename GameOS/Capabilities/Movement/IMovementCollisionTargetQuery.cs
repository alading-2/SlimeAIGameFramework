using System.Collections.Generic;
using SlimeAI.GameOS.Runtime.Entity;

namespace SlimeAI.GameOS.Capabilities.Movement;

/// <summary>
/// Movement 碰撞候选目标查询接口；Capability 内核只依赖 IEntity。
/// </summary>
public interface IMovementCollisionTargetQuery
{
    /// <summary>
    /// 查询一次 from -> intended 位移可能命中的目标候选。
    /// </summary>
    /// <param name="source">移动源实体。</param>
    /// <param name="movementParams">本次移动参数。</param>
    /// <param name="from">本帧起点。</param>
    /// <param name="intended">本帧预期终点。</param>
    IReadOnlyList<IEntity> GetCandidates(
        IEntity source,
        in MovementParams movementParams,
        Vector2Value from,
        Vector2Value intended);
}
