using System.Collections.Generic;
using SlimeAI.GameOS.Runtime.Entity;

namespace SlimeAI.GameOS.Capabilities.Ability;

/// <summary>
/// Ability 目标候选查询接口。Capability-owned selector：Ability 持有自己的目标查询逻辑。
/// </summary>
public interface IAbilityTargetQuery
{
    /// <summary>返回 Ability 可索敌的候选实体列表。</summary>
    IReadOnlyList<IEntity> GetCandidates(IEntity caster);
}
