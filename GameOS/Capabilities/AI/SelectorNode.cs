namespace SlimeAI.GameOS.Capabilities.AI;

/// <summary>
/// 选择节点：任一子节点成功则成功。
/// </summary>
public sealed class SelectorNode : CompositeNode
{
    /// <summary>
    /// 创建选择节点。
    /// </summary>
    /// <param name="name">调试名称。</param>
    public SelectorNode(string name = "Selector") : base(name) { }

    /// <inheritdoc />
    public override AIState Evaluate(AIContext context)
    {
        for (var i = 0; i < Children.Count; i++)
        {
            var state = Children[i].Evaluate(context);
            if (state != AIState.Failure)
            {
                return state;
            }
        }

        return AIState.Failure;
    }
}
