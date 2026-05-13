namespace SlimeAI.GameOS.Capabilities.AI;

/// <summary>
/// AI 行为节点返回状态。
/// </summary>
public enum AIState
{
    /// <summary>节点执行失败。</summary>
    Failure = 0,

    /// <summary>节点执行成功。</summary>
    Success = 1,

    /// <summary>节点仍在执行。</summary>
    Running = 2
}
