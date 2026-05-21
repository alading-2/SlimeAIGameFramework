using System.Collections.Generic;
using SlimeAI.GameOS.Capabilities.Damage;
using SlimeAI.GameOS.Capabilities.Movement;
using SlimeAI.GameOS.Observation;

namespace SlimeAI.GameOS.Capabilities.AI;

/// <summary>
/// AI 行为树最小 Tick 服务。
/// </summary>
public sealed class AIService
{
    private static readonly GameOSContextLog Log = GameOSLog.For("AIService");

    /// <summary>进程级默认 AIService。</summary>
    public static AIService Instance { get; } = new();

    /// <summary>进程级默认 AIService；与 <see cref="Instance"/> 等价，语义更明确。</summary>
    public static AIService Default => Instance;

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
        var entityId = context.Entity.EntityId.Value;
        if (!context.Entity.Data.Get<bool>(AIDataKeys.IsEnabled, true)
            || context.Entity.Data.Get<bool>(DamageDataKeys.IsDead, false))
        {
            Log.Debug(
                $"AI Tick skipped: {entityId}, enabled={context.Entity.Data.Get<bool>(AIDataKeys.IsEnabled, true)}, dead={context.Entity.Data.Get<bool>(DamageDataKeys.IsDead, false)}",
                new Dictionary<string, object?>
                {
                    ["entityId"] = entityId,
                    ["isEnabled"] = context.Entity.Data.Get<bool>(AIDataKeys.IsEnabled, true),
                    ["isDead"] = context.Entity.Data.Get<bool>(DamageDataKeys.IsDead, false),
                });
            context.Entity.Data.Set(AIDataKeys.IsAttackRequested, false);
            context.Entity.Data.Set(MovementDataKeys.AIMoveDirection, Vector2Value.Zero);
            return AIState.Failure;
        }

        Log.Debug(
            $"AI Tick: {entityId}, root={root.GetType().Name}",
            new Dictionary<string, object?>
            {
                ["entityId"] = entityId,
                ["rootType"] = root.GetType().Name,
                ["abilityServiceNull"] = context.AbilityService == null,
            });

        if (context.AbilityService == null)
        {
            Log.Warn(
                $"AIContext.AbilityService 为 null ({entityId})，跳过 auto-trigger。请确保 GodotAIComponent 显式注入 AbilityService。",
                new Dictionary<string, object?>
                {
                    ["entityId"] = entityId,
                });
        }

        context.Entity.Data.Set(AIDataKeys.IsAttackRequested, false);
        var state = root.Evaluate(context);

        Log.Debug(
            $"AI Tick result: {entityId}, state={state}, attackRequested={context.Entity.Data.Get<bool>(AIDataKeys.IsAttackRequested, false)}",
            new Dictionary<string, object?>
            {
                ["entityId"] = entityId,
                ["state"] = state.ToString(),
                ["attackRequested"] = context.Entity.Data.Get<bool>(AIDataKeys.IsAttackRequested, false),
                ["moveDirection"] = context.Entity.Data.Get<Vector2Value>(MovementDataKeys.AIMoveDirection, Vector2Value.Zero).ToString(),
            });

        return state;
    }
}
