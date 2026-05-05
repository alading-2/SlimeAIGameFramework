using System;
using System.Collections.Generic;
using System.Linq;
using SkilmeAI.GameOS.Runtime.Entity;

namespace SkilmeAI.GameOS.Capabilities.Damage;

/// <summary>
/// Damage Capability 最小运行时服务，作为生命值扣减和伤害事件的统一入口。
/// </summary>
public sealed class DamageService
{
    /// <summary>进程级默认 DamageService。</summary>
    public static DamageService Instance { get; } = new();

    private readonly List<IDamageProcessor> processors = new();

    /// <summary>
    /// 创建 DamageService，并确保 Damage DataKey 已注册。
    /// </summary>
    public DamageService()
    {
        DamageDataKeys.RegisterAll();
        RegisterDefaultProcessors();
    }

    /// <summary>
    /// 处理一次伤害。
    /// </summary>
    /// <param name="info">伤害上下文。</param>
    public DamageResult Process(DamageInfo info)
    {
        ArgumentNullException.ThrowIfNull(info);
        ResetRuntimeState(info);

        for (var i = 0; i < processors.Count; i++)
        {
            processors[i].Process(info);
            if (info.IsEnd)
            {
                break;
            }
        }

        var applied = info.FinalDamage > 0f && !info.IsBlocked && !info.IsDodged;
        var message = applied ? string.Empty : ResolveBlockedMessage(info);
        return new DamageResult(applied, info, info.OldHp, info.NewHp, message);
    }

    /// <summary>
    /// 注册伤害处理器。
    /// </summary>
    /// <param name="processor">处理器。</param>
    public void RegisterProcessor(IDamageProcessor processor)
    {
        ArgumentNullException.ThrowIfNull(processor);
        processors.Add(processor);
        processors.Sort(static (a, b) => a.Priority.CompareTo(b.Priority));
    }

    /// <summary>
    /// 清空处理器，主要用于测试或替换完整管线。
    /// </summary>
    public void ClearProcessors()
    {
        processors.Clear();
    }

    /// <summary>
    /// 获取当前处理器快照。
    /// </summary>
    public IReadOnlyList<IDamageProcessor> GetProcessors()
    {
        return processors.ToArray();
    }

    /// <summary>
    /// 读取实体当前生命值；未初始化 CurrentHp 时回退 MaxHp。
    /// </summary>
    /// <param name="entity">目标实体。</param>
    public static float ReadCurrentHp(IEntity entity)
    {
        var currentHp = entity.Data.Get<float>(DamageDataKeys.CurrentHp, 0f);
        if (currentHp > 0f)
        {
            return currentHp;
        }

        return entity.Data.Get<float>(DamageDataKeys.MaxHp, 0f);
    }

    private void RegisterDefaultProcessors()
    {
        RegisterProcessor(new BaseDamageProcessor());
        RegisterProcessor(new DodgeProcessor());
        RegisterProcessor(new CriticalDamageProcessor());
        RegisterProcessor(new ShieldDamageProcessor());
        RegisterProcessor(new ArmorDamageProcessor());
        RegisterProcessor(new DamageTakenAmplificationProcessor());
        RegisterProcessor(new HealthExecutionProcessor());
        RegisterProcessor(new LifestealProcessor());
        RegisterProcessor(new DamageStatisticsProcessor());
    }

    private static void ResetRuntimeState(DamageInfo info)
    {
        info.FinalDamage = 0f;
        info.OldHp = ReadCurrentHp(info.Victim);
        info.NewHp = info.OldHp;
        info.IsBlocked = false;
        info.IsCritical = false;
        info.IsDodged = false;
        info.IsEnd = false;
        info.IsFatal = false;
        info.ArmorMultiplier = 1f;
        info.DamageTakenMultiplier = 1f;
        info.LifestealAmount = 0f;
        info.ShieldDamageAbsorbed = 0f;
        info.Logs.Clear();
    }

    private static string ResolveBlockedMessage(DamageInfo info)
    {
        if (info.IsDodged)
        {
            return "Damage was dodged.";
        }

        if (info.Logs.Count > 0)
        {
            return info.Logs[^1];
        }

        return "Damage was not applied.";
    }
}
