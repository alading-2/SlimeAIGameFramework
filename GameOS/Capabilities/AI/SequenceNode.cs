namespace SkilmeAI.GameOS.Capabilities.AI;

/// <summary>
/// 顺序节点：全部成功才成功。
/// </summary>
public sealed class SequenceNode : CompositeNode
{
    /// <summary>
    /// 创建顺序节点。
    /// </summary>
    /// <param name="name">调试名称。</param>
    public SequenceNode(string name = "Sequence") : base(name) { }

    /// <inheritdoc />
    public override AIState Evaluate(AIContext context)
    {
        for (var i = 0; i < Children.Count; i++)
        {
            var state = Children[i].Evaluate(context);
            if (state != AIState.Success)
            {
                return state;
            }
        }

        return AIState.Success;
    }
}
