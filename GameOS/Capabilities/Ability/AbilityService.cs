using System;
using System.Collections.Generic;
using SlimeAI.GameOS.Capabilities.Ability.Events;
using SlimeAI.GameOS.Capabilities.Damage;
using SlimeAI.GameOS.Capabilities.Feature;
using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Timer;

namespace SlimeAI.GameOS.Capabilities.Ability;

/// <summary>
/// Ability Capability 最小运行时服务，负责正式技能触发提交。
/// </summary>
public sealed class AbilityService
{
    private readonly TimerManager timerManager;

    /// <summary>进程级默认 AbilityService。</summary>
    public static AbilityService Instance { get; } = new(TimerManager.Instance);

    /// <summary>
    /// 创建 AbilityService。
    /// </summary>
    /// <param name="timerManager">周期伤害使用的计时器管理器。</param>
    public AbilityService(TimerManager timerManager)
    {
        this.timerManager = timerManager;
        AbilityDataKeys.RegisterAll();
        DamageDataKeys.RegisterAll();
    }

    /// <summary>
    /// 尝试触发技能。
    /// </summary>
    /// <param name="context">施法上下文。</param>
    public AbilityTriggerReport TryTrigger(AbilityCastContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var check = CheckCanUse(context);
        if (check.Result != AbilityTriggerResult.Success)
        {
            EmitFailed(context, check.Result, check.Message);
            return check;
        }

        ConsumeCharges(context.Ability);
        StartCooldown(context.Ability);
        context.Ability.Data.Set(AbilityDataKeys.IsActive, true);
        context.Ability.Events.Publish(new Activated(context));

        var executed = ExecuteFeatureOrDamage(context);
        context.Ability.Data.Set(AbilityDataKeys.IsActive, false);
        context.Ability.Events.Publish(new Executed(context, executed));

        return new AbilityTriggerReport(AbilityTriggerResult.Success, executed, string.Empty);
    }

    /// <summary>
    /// 推进技能运行时冷却。
    /// </summary>
    /// <param name="abilities">技能实体集合。</param>
    /// <param name="delta">经过秒数。</param>
    public void TickCooldowns(IEnumerable<IEntity> abilities, float delta)
    {
        ArgumentNullException.ThrowIfNull(abilities);
        if (delta <= 0f)
        {
            return;
        }

        foreach (var ability in abilities)
        {
            var remaining = ability.Data.Get<float>(AbilityDataKeys.CooldownRemaining, 0f);
            if (remaining <= 0f)
            {
                continue;
            }

            ability.Data.Set(AbilityDataKeys.CooldownRemaining, MathF.Max(0f, remaining - delta));
        }
    }

    /// <summary>
    /// 推进自动触发技能，并触发 Periodic 模式的技能。
    /// </summary>
    /// <param name="contexts">外部系统已准备好的施法上下文集合。</param>
    /// <param name="delta">经过秒数。</param>
    public IReadOnlyList<AbilityTriggerReport> TickAutoTriggers(IEnumerable<AbilityCastContext> contexts, float delta)
    {
        ArgumentNullException.ThrowIfNull(contexts);
        var reports = new List<AbilityTriggerReport>();
        if (delta < 0f)
        {
            return reports;
        }

        foreach (var context in contexts)
        {
            TickCooldown(context.Ability, delta);
            var triggerMode = context.Ability.Data.Get<AbilityTriggerMode>(
                AbilityDataKeys.TriggerMode,
                AbilityTriggerMode.None);
            if ((triggerMode & AbilityTriggerMode.Periodic) == 0)
            {
                continue;
            }

            if (context.Ability.Data.Get<float>(AbilityDataKeys.CooldownRemaining, 0f) > 0f)
            {
                continue;
            }

            reports.Add(TryTrigger(context));
        }

        return reports;
    }

    private static AbilityTriggerReport CheckCanUse(AbilityCastContext context)
    {
        if (context.Caster.Data.Get<bool>(DamageDataKeys.IsDead, false))
        {
            return Fail(AbilityTriggerResult.FailCasterDead, "Caster is dead.");
        }

        if (!context.Ability.Data.Get<bool>(AbilityDataKeys.IsEnabled, true))
        {
            return Fail(AbilityTriggerResult.FailDisabled, "Ability is disabled.");
        }

        if (context.Ability.Data.Get<bool>(AbilityDataKeys.IsActive, false))
        {
            return Fail(AbilityTriggerResult.FailActive, "Ability is active.");
        }

        if (context.Ability.Data.Get<float>(AbilityDataKeys.CooldownRemaining, 0f) > 0f)
        {
            return Fail(AbilityTriggerResult.FailCooldown, "Ability is on cooldown.");
        }

        if (context.Ability.Data.Get<bool>(AbilityDataKeys.UsesCharges, false)
            && context.Ability.Data.Get<int>(AbilityDataKeys.CurrentCharges, 0) <= 0)
        {
            return Fail(AbilityTriggerResult.FailNoCharge, "Ability has no charge.");
        }

        var targetSelection = context.Ability.Data.Get<AbilityTargetSelection>(
            AbilityDataKeys.TargetSelection,
            AbilityTargetSelection.None);
        var hasEntityTarget = context.Targets != null && context.Targets.Count > 0;
        var hasPointTarget = context.TargetPosition.HasValue;
        if (targetSelection == AbilityTargetSelection.Entity && !hasEntityTarget)
        {
            return Fail(AbilityTriggerResult.FailNoTarget, "Ability requires entity target.");
        }

        if (targetSelection == AbilityTargetSelection.Point && !hasPointTarget)
        {
            return Fail(AbilityTriggerResult.FailNoTarget, "Ability requires point target.");
        }

        if (targetSelection == AbilityTargetSelection.EntityOrPoint && !hasEntityTarget && !hasPointTarget)
        {
            return Fail(AbilityTriggerResult.FailNoTarget, "Ability requires entity or point target.");
        }

        return new AbilityTriggerReport(AbilityTriggerResult.Success, null, string.Empty);
    }

    private static AbilityTriggerReport Fail(AbilityTriggerResult result, string message)
    {
        return new AbilityTriggerReport(result, null, message);
    }

    private static void ConsumeCharges(IEntity ability)
    {
        if (!ability.Data.Get<bool>(AbilityDataKeys.UsesCharges, false))
        {
            return;
        }

        var charges = ability.Data.Get<int>(AbilityDataKeys.CurrentCharges, 0);
        ability.Data.Set(AbilityDataKeys.CurrentCharges, Math.Max(0, charges - 1));
    }

    private static void StartCooldown(IEntity ability)
    {
        var cooldown = ability.Data.Get<float>(AbilityDataKeys.Cooldown, 0f);
        if (cooldown > 0f)
        {
            ability.Data.Set(AbilityDataKeys.CooldownRemaining, cooldown);
        }
    }

    private static void TickCooldown(IEntity ability, float delta)
    {
        if (delta <= 0f)
        {
            return;
        }

        var remaining = ability.Data.Get<float>(AbilityDataKeys.CooldownRemaining, 0f);
        if (remaining <= 0f)
        {
            return;
        }

        ability.Data.Set(AbilityDataKeys.CooldownRemaining, MathF.Max(0f, remaining - delta));
    }

    private AbilityExecutedResult ExecuteFeatureOrDamage(AbilityCastContext context)
    {
        var handlerId = context.Ability.Data.Get(AbilityDataKeys.FeatureHandlerId, string.Empty);
        if (string.IsNullOrEmpty(handlerId))
        {
            return ExecuteDamage(context);
        }

        context.Ability.Data.Set(FeatureDataKeys.HandlerId, handlerId);
        var featureContext = new FeatureContext
        {
            Owner = context.Caster,
            Feature = context.Ability,
            Definition = new FeatureDefinition
            {
                FeatureId = context.Ability.EntityId.Value,
                HandlerId = handlerId
            },
            ActivationData = context
        };
        FeatureService.Instance.Activate(featureContext);
        FeatureService.Instance.End(featureContext, FeatureEndReason.Completed);

        if (featureContext.ExecuteResult is AbilityExecutedResult result)
        {
            return result;
        }

        return ExecuteDamage(context);
    }

    private AbilityExecutedResult ExecuteDamage(AbilityCastContext context)
    {
        var damage = context.Ability.Data.Get<float>(AbilityDataKeys.Damage, 0f);
        if (damage <= 0f || context.Targets == null || context.Targets.Count <= 0)
        {
            return new AbilityExecutedResult { TargetsHit = context.Targets?.Count ?? 0 };
        }

        var options = new DamageApplyOptions(damage)
        {
            Attacker = context.Caster,
            Type = context.DamageType,
            Tags = context.DamageTags | DamageTags.Ability
        };
        var interval = context.Ability.Data.Get<float>(AbilityDataKeys.DamageInterval, 0f);
        var repeatCount = context.Ability.Data.Get<int>(AbilityDataKeys.DamageRepeatCount, 1);
        var immediate = context.Ability.Data.Get<bool>(AbilityDataKeys.ApplyImmediateDamage, true);
        var result = interval > 0f && repeatCount != 1
            ? DamageTool.ApplyPeriodic(
                context.Targets,
                options,
                new DamageRepeatOptions(interval)
                {
                    RepeatCount = repeatCount,
                    ApplyImmediately = immediate
                },
                timerManager)
            : DamageTool.Apply(context.Targets, options);

        var totalDamage = 0f;
        for (var i = 0; i < result.Results.Count; i++)
        {
            totalDamage += result.Results[i].Info.FinalDamage;
        }

        return new AbilityExecutedResult
        {
            TargetsHit = result.TargetCount,
            TotalDamage = totalDamage,
            DamageResult = result
        };
    }

    private static void EmitFailed(AbilityCastContext context, AbilityTriggerResult result, string message)
    {
        context.Ability.Events.Publish(new Failed(context, result, message));
    }
}
