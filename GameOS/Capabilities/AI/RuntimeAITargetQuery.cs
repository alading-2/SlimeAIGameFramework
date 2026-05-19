using System.Collections.Generic;
using SlimeAI.GameOS.Runtime.Entity;

namespace SlimeAI.GameOS.Capabilities.AI;

/// <summary>
/// 默认 Runtime AI 目标查询：返回当前 EntityManager 快照。
/// 纯 Runtime 环境无 physics broadphase，回退全量扫描；
/// Godot 项目可注入 physics-aware 实现替换。
/// </summary>
public sealed class RuntimeAITargetQuery : IAITargetQuery
{
    /// <inheritdoc />
    public IReadOnlyList<IEntity> GetCandidates(IEntity self)
        => EntityManager.GetAll();
}
