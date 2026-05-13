using System;
using SlimeAI.GameOS.Capabilities.Damage.Events;

namespace SlimeAI.GameOS.Capabilities.Damage;

/// <summary>
/// 治疗服务，作为恢复生命值的统一入口。
/// </summary>
public sealed class HealService
{
    /// <summary>进程级默认 HealService。</summary>
    public static HealService Instance { get; } = new();

    /// <summary>
    /// 创建 HealService，并确保 Damage DataKey 已注册。
    /// </summary>
    public HealService()
    {
        DamageDataKeys.RegisterAll();
    }

    /// <summary>
    /// 处理一次治疗。
    /// </summary>
    /// <param name="info">治疗上下文。</param>
    public HealResult Process(HealInfo info)
    {
        ArgumentNullException.ThrowIfNull(info);
        ResetRuntimeState(info);

        if (info.Amount <= 0f)
        {
            return Block(info, "Heal amount must be greater than zero.");
        }

        if (info.Target.Data.Get<bool>(DamageDataKeys.IsDead, false))
        {
            return Block(info, "Target is dead.");
        }

        var maxHp = info.Target.Data.Get<float>(DamageDataKeys.MaxHp, 0f);
        if (maxHp <= 0f)
        {
            return Block(info, "Target max HP must be greater than zero.");
        }

        var newHp = MathF.Min(maxHp, info.OldHp + info.Amount);
        info.NewHp = newHp;
        info.FinalAmount = MathF.Max(0f, newHp - info.OldHp);
        if (info.FinalAmount <= 0f)
        {
            return Block(info, "Target HP is full.");
        }

        if (!info.IsSimulation)
        {
            ApplyHeal(info);
        }

        return new HealResult(true, info, info.OldHp, info.NewHp, string.Empty);
    }

    private static void ResetRuntimeState(HealInfo info)
    {
        info.OldHp = DamageService.ReadCurrentHp(info.Target);
        info.NewHp = info.OldHp;
        info.FinalAmount = 0f;
        info.IsBlocked = false;
        info.Logs.Clear();
    }

    private static HealResult Block(HealInfo info, string message)
    {
        info.IsBlocked = true;
        info.AddLog(message);
        return new HealResult(false, info, info.OldHp, info.NewHp, message);
    }

    private static void ApplyHeal(HealInfo info)
    {
        info.Target.Data.Set(DamageDataKeys.CurrentHp, info.NewHp);
        info.Target.Data.Add(DamageDataKeys.TotalHealingReceived, info.FinalAmount);
        info.Healer?.Data.Add(DamageDataKeys.TotalHealingDone, info.FinalAmount);

        info.Target.Events.Publish(new HealthChanged(info.Target, info.OldHp, info.NewHp));
        info.Target.Events.Publish(new Healed(info.Target, info.FinalAmount, info));
    }
}
