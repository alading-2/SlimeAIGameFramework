using System;
using System.Collections.Generic;
using Godot;
using SlimeAI.GameOS.Capabilities.Collision;
using SlimeAI.GameOS.Capabilities.Collision.Events;
using SlimeAI.GameOS.Capabilities.Damage;
using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Timer;

namespace SlimeAI.GameOS.GodotBridge;

/// <summary>
/// Godot 接触伤害组件：消费 Hurtbox 事件并通过 DamageService 结算伤害。
/// </summary>
public partial class GodotContactDamageComponent : Node, IGodotComponent
{
    private readonly Dictionary<EntityId, GameTimer> contactTimers = new();
    private IEntity? entity;
    private IDisposable? hurtboxEnteredToken;
    private IDisposable? hurtboxExitedToken;

    /// <summary>是否忽略同队接触目标。</summary>
    [Export]
    public bool IgnoreSameTeam { get; set; } = true;

    /// <summary>进入时是否立即造成一次伤害。</summary>
    [Export]
    public bool ApplyOnEnter { get; set; } = true;

    /// <summary>是否在接触期间循环造成伤害。</summary>
    [Export]
    public bool RepeatWhileContact { get; set; } = true;

    /// <summary>当前 Adapter 使用的伤害服务；默认是进程级兼容入口，验证代码可替换。</summary>
    public DamageService DamageService { get; set; } = SlimeAI.GameOS.Capabilities.Damage.DamageService.Default;

    /// <summary>当前 Adapter 使用的计时器管理器；默认是进程级兼容入口，验证代码可替换。</summary>
    public TimerManager TimerManager { get; set; } = SlimeAI.GameOS.Runtime.Timer.TimerManager.Instance;

    /// <inheritdoc />
    public void OnComponentRegistered(IEntity entity, Node entityNode)
    {
        this.entity = entity;
        DamageDataKeys.RegisterAll();
        hurtboxEnteredToken = entity.Events.Subscribe<HurtboxEntered>(OnHurtboxEntered);
        hurtboxExitedToken = entity.Events.Subscribe<HurtboxExited>(OnHurtboxExited);
    }

    /// <inheritdoc />
    public void OnComponentUnregistered(IEntity? entity, Node? entityNode)
    {
        hurtboxEnteredToken?.Dispose();
        hurtboxExitedToken?.Dispose();
        hurtboxEnteredToken = null;
        hurtboxExitedToken = null;

        CancelAllTimers();
        this.entity = null;
    }

    private void OnHurtboxEntered(HurtboxEntered data)
    {
        if (entity == null || !CanReceiveContactDamage(entity, data.Contact.Target))
        {
            return;
        }

        if (ApplyOnEnter)
        {
            ApplyContactDamage(data.Contact.Target);
        }

        if (!RepeatWhileContact || contactTimers.ContainsKey(data.Contact.Target.EntityId))
        {
            return;
        }

        var interval = MathF.Max(
            0.01f,
            data.Contact.Target.Data.Get<float>(DamageDataKeys.ContactDamageInterval, 1f));
        contactTimers[data.Contact.Target.EntityId] = TimerManager
            .Loop(interval)
            .OnLoop(() => ApplyContactDamage(data.Contact.Target));
    }

    private void OnHurtboxExited(HurtboxExited data)
    {
        CancelTimer(data.Contact.Target.EntityId);
    }

    private bool CanReceiveContactDamage(IEntity victim, IEntity attacker)
    {
        if (victim.Data.Get<bool>(DamageDataKeys.IsDead, false))
        {
            return false;
        }

        if (!IgnoreSameTeam)
        {
            return true;
        }

        var victimTeam = victim.Data.Get<int>(CollisionDataKeys.Team, 0);
        var attackerTeam = attacker.Data.Get<int>(CollisionDataKeys.Team, 0);
        return victimTeam == 0 || attackerTeam == 0 || victimTeam != attackerTeam;
    }

    private void ApplyContactDamage(IEntity attacker)
    {
        if (entity == null || !CanReceiveContactDamage(entity, attacker))
        {
            return;
        }

        var damage = attacker.Data.Get<float>(DamageDataKeys.ContactDamage, 0f);
        if (damage <= 0f)
        {
            return;
        }

        DamageService.Process(new DamageInfo
        {
            Attacker = attacker,
            Victim = entity,
            Damage = damage,
            Type = DamageType.Physical,
            Tags = DamageTags.Attack | DamageTags.Contact
        });
    }

    private void CancelTimer(EntityId entityId)
    {
        if (!contactTimers.TryGetValue(entityId, out var timer))
        {
            return;
        }

        timer.Cancel();
        contactTimers.Remove(entityId);
    }

    private void CancelAllTimers()
    {
        foreach (var timer in contactTimers.Values)
        {
            timer.Cancel();
        }

        contactTimers.Clear();
    }
}
