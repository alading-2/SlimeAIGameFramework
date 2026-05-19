using System.Collections.Generic;
using SlimeAI.GameOS.Runtime.Entity;

namespace SlimeAI.GameOS.Capabilities.AI;

/// <summary>
/// AI 目标候选查询接口。Capability-owned selector：AI 持有自己的目标查询逻辑。
/// </summary>
public interface IAITargetQuery
{
    /// <summary>返回 AI 可感知的候选实体列表。</summary>
    IReadOnlyList<IEntity> GetCandidates(IEntity self);
}
