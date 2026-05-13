using System.Collections.Generic;

namespace SlimeAI.GameOS.Capabilities.AI;

/// <summary>
/// AI 组合节点基类。
/// </summary>
public abstract class CompositeNode : BehaviorNode
{
    private readonly List<BehaviorNode> children = new();

    /// <summary>
    /// 创建组合节点。
    /// </summary>
    /// <param name="name">调试名称。</param>
    protected CompositeNode(string name = "") : base(name) { }

    /// <summary>子节点只读视图。</summary>
    public IReadOnlyList<BehaviorNode> Children => children;

    /// <summary>
    /// 添加子节点。
    /// </summary>
    /// <param name="child">子节点。</param>
    public CompositeNode Add(BehaviorNode child)
    {
        children.Add(child);
        return this;
    }

    /// <inheritdoc />
    public override void Reset(AIContext? context = null)
    {
        for (var i = 0; i < children.Count; i++)
        {
            children[i].Reset(context);
        }
    }
}
