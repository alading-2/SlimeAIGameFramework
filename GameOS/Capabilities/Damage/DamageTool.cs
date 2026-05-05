using System;
using System.Collections.Generic;
using SkilmeAI.GameOS.Runtime.Entity;
using SkilmeAI.GameOS.Runtime.Timer;

namespace SkilmeAI.GameOS.Capabilities.Damage;

/// <summary>
/// 多目标和周期伤害工具。
/// </summary>
public static class DamageTool
{
    /// <summary>
    /// 对一组目标立即应用伤害。
    /// </summary>
    /// <param name="targets">目标集合。</param>
    /// <param name="options">伤害参数。</param>
    /// <param name="service">伤害服务；为空时使用默认实例。</param>
    public static DamageApplyResult Apply(
        IEnumerable<IEntity> targets,
        DamageApplyOptions options,
        DamageService? service = null)
    {
        ArgumentNullException.ThrowIfNull(targets);
        service ??= DamageService.Instance;

        var result = new DamageApplyResult();
        foreach (var target in targets)
        {
            result.TargetCount++;
            var damageResult = service.Process(new DamageInfo
            {
                Attacker = options.Attacker,
                Victim = target,
                Damage = options.Damage,
                Type = options.Type,
                Tags = options.Tags,
                IsSimulation = options.IsSimulation
            });
            result.Results.Add(damageResult);
            if (damageResult.Applied)
            {
                result.AppliedCount++;
            }
        }

        return result;
    }

    /// <summary>
    /// 对一组目标应用周期伤害。
    /// </summary>
    /// <param name="targets">目标集合。</param>
    /// <param name="options">伤害参数。</param>
    /// <param name="repeat">周期参数。</param>
    /// <param name="timerManager">计时器管理器；为空时使用默认实例。</param>
    /// <param name="service">伤害服务；为空时使用默认实例。</param>
    public static DamageApplyResult ApplyPeriodic(
        IEnumerable<IEntity> targets,
        DamageApplyOptions options,
        DamageRepeatOptions repeat,
        TimerManager? timerManager = null,
        DamageService? service = null)
    {
        ArgumentNullException.ThrowIfNull(targets);
        timerManager ??= TimerManager.Instance;
        service ??= DamageService.Instance;

        var snapshot = SnapshotTargets(targets);
        var result = repeat.ApplyImmediately
            ? Apply(snapshot, options, service)
            : new DamageApplyResult { TargetCount = snapshot.Count };

        var remainingRepeats = repeat.ApplyImmediately && repeat.RepeatCount > 0
            ? repeat.RepeatCount - 1
            : repeat.RepeatCount;
        if (remainingRepeats == 0)
        {
            return result;
        }

        var interval = MathF.Max(0.01f, repeat.Interval);
        var timer = remainingRepeats < 0
            ? timerManager.Loop(interval, repeat.UseUnscaledTime)
            : timerManager.Repeat(interval, remainingRepeats, useUnscaledTime: repeat.UseUnscaledTime);
        timer.OnLoop(() => Apply(snapshot, options, service));
        if (!string.IsNullOrEmpty(repeat.TimerTag))
        {
            timer.WithTag(repeat.TimerTag);
        }

        result.Timer = timer;
        return result;
    }

    private static List<IEntity> SnapshotTargets(IEnumerable<IEntity> targets)
    {
        var snapshot = new List<IEntity>();
        foreach (var target in targets)
        {
            snapshot.Add(target);
        }

        return snapshot;
    }
}
