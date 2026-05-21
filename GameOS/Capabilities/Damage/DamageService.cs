using System;
using System.Collections.Generic;
using System.Linq;
using SlimeAI.GameOS.Observation;
using SlimeAI.GameOS.Runtime.Entity;

namespace SlimeAI.GameOS.Capabilities.Damage;

/// <summary>
/// Damage Capability 最小运行时服务，作为生命值扣减和伤害事件的统一入口。
/// </summary>
public sealed class DamageService
{
    private static readonly GameOSContextLog Log = GameOSLog.For("DamageService");

    /// <summary>进程级默认 DamageService。</summary>
    public static DamageService Instance { get; } = new();

    /// <summary>进程级默认 DamageService；与 <see cref="Instance"/> 等价，语义更明确。</summary>
    public static DamageService Default => Instance;

    private readonly List<IDamageProcessor> processors = new();
    private readonly HealService healService;

    /// <summary>
    /// 创建 DamageService，并确保 Damage DataKey 已注册。
    /// </summary>
    /// <param name="healService">可选 HealService；为 null 时自动创建默认实例。</param>
    public DamageService(HealService? healService = null)
    {
        this.healService = healService ?? new HealService();
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
        var result = new DamageResult(applied, info, info.OldHp, info.NewHp, message);

        if (applied)
        {
            Log.Info(
                $"Damage applied: {info.Attacker?.EntityId.Value ?? "none"} -> {info.Victim.EntityId.Value}, " +
                $"raw={info.Damage:0.##}, final={info.FinalDamage:0.##}, hp={info.OldHp:0.##}->{info.NewHp:0.##}",
                new Dictionary<string, object?>
                {
                    ["attackerId"] = info.Attacker?.EntityId.Value,
                    ["victimId"] = info.Victim.EntityId.Value,
                    ["rawDamage"] = info.Damage,
                    ["finalDamage"] = info.FinalDamage,
                    ["oldHp"] = info.OldHp,
                    ["newHp"] = info.NewHp,
                    ["isCritical"] = info.IsCritical,
                    ["isFatal"] = info.IsFatal,
                });
        }
        else if (info.IsDodged)
        {
            Log.Info(
                $"Damage dodged: {info.Attacker?.EntityId.Value ?? "none"} -> {info.Victim.EntityId.Value}",
                new Dictionary<string, object?>
                {
                    ["attackerId"] = info.Attacker?.EntityId.Value,
                    ["victimId"] = info.Victim.EntityId.Value,
                    ["rawDamage"] = info.Damage,
                });
        }
        else if (info.IsBlocked)
        {
            Log.Info(
                $"Damage blocked: {info.Attacker?.EntityId.Value ?? "none"} -> {info.Victim.EntityId.Value}, reason={message}",
                new Dictionary<string, object?>
                {
                    ["attackerId"] = info.Attacker?.EntityId.Value,
                    ["victimId"] = info.Victim.EntityId.Value,
                    ["rawDamage"] = info.Damage,
                    ["blockReason"] = message,
                });
        }

        return result;
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
        Log.Debug($"Registered damage processor: {processor.GetType().Name} (priority={processor.Priority})");
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
        RegisterProcessor(new LifestealProcessor(healService));
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
