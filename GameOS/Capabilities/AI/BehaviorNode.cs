namespace SkilmeAI.GameOS.Capabilities.AI;

/// <summary>
/// AI 行为树节点基类。
/// </summary>
public abstract class BehaviorNode
{
    /// <summary>
    /// 创建行为节点。
    /// </summary>
    /// <param name="name">调试名称。</param>
    protected BehaviorNode(string name = "")
    {
        Name = name;
    }

    /// <summary>调试名称。</summary>
    public string Name { get; }

    /// <summary>
    /// 执行节点。
    /// </summary>
    /// <param name="context">AI 上下文。</param>
    public abstract AIState Evaluate(AIContext context);

    /// <summary>
    /// 重置节点运行态。
    /// </summary>
    /// <param name="context">AI 上下文。</param>
    public virtual void Reset(AIContext? context = null) { }
}
