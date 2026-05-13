using SlimeAI.GameOS.Capabilities.Damage;
using SlimeAI.GameOS.Capabilities.Movement;

namespace SlimeAI.GameOS.Capabilities.AI;

/// <summary>
/// AI 行为树最小 Tick 服务。
/// </summary>
public sealed class AIService
{
    /// <summary>进程级默认 AIService。</summary>
    public static AIService Instance { get; } = new();

    /// <summary>
    /// 创建 AIService，并确保 AI DataKey 已注册。
    /// </summary>
    public AIService()
    {
        AIDataKeys.RegisterAll();
    }

    /// <summary>
    /// Tick 行为树。
    /// </summary>
    /// <param name="context">AI 上下文。</param>
    /// <param name="root">行为树根节点。</param>
    public AIState Tick(AIContext context, BehaviorNode root)
    {
        if (!context.Entity.Data.Get<bool>(AIDataKeys.IsEnabled, true)
            || context.Entity.Data.Get<bool>(DamageDataKeys.IsDead, false))
        {
            context.Entity.Data.Set(AIDataKeys.IsAttackRequested, false);
            context.Entity.Data.Set(MovementDataKeys.AIMoveDirection, Vector2Value.Zero);
            return AIState.Failure;
        }

        context.Entity.Data.Set(AIDataKeys.IsAttackRequested, false);
        return root.Evaluate(context);
    }
}
