using System.Collections.Generic;
using SlimeAI.GameOS.Capabilities.Ability;

namespace SlimeAI.GameOS.Capabilities.AI;

/// <summary>
/// 为 AI 自动施法准备带自动索敌结果的 AbilityCastContext。
/// </summary>
public sealed class PrepareAbilityAutoTargetContextsAction : BehaviorNode
{
    /// <summary>
    /// 创建自动索敌上下文准备动作。
    /// </summary>
    public PrepareAbilityAutoTargetContextsAction() : base("PrepareAbilityAutoTargetContexts") { }

    /// <inheritdoc />
    public override AIState Evaluate(AIContext context)
    {
        if (context.AutoTargetAbilities == null || context.AutoTargetAbilities.Count == 0)
        {
            context.AbilityContexts = null;
            return AIState.Failure;
        }

        var abilityContexts = new List<AbilityCastContext>();
        for (var i = 0; i < context.AutoTargetAbilities.Count; i++)
        {
            var ability = context.AutoTargetAbilities[i];
            if (AbilityTargetingTool.TryBuildContext(context.Entity, ability, out var abilityContext)
                && abilityContext != null)
            {
                abilityContexts.Add(abilityContext);
            }
        }

        context.AbilityContexts = abilityContexts;
        return abilityContexts.Count > 0 ? AIState.Success : AIState.Failure;
    }
}
