using System;
using System.Collections.Generic;
using Godot;
using SkilmeAI.GameOS.Capabilities.Collision;
using SkilmeAI.GameOS.Capabilities.Damage;
using SkilmeAI.GameOS.Runtime.Entity;
using SkilmeAI.GameOS.Runtime.Event;
using SkilmeAI.GameOS.Runtime.Timer;

namespace SkilmeAI.GameOS.GodotBridge;

/// <summary>
/// Godot 接触伤害组件：消费 Hurtbox 事件并通过 DamageService 结算伤害。
/// </summary>
public partial class GodotContactDamageComponent : Node, IGodotComponent
{
    private readonly Dictionary<string, GameTimer> contactTimers = new(StringComparer.Ordinal);
    private IEntity? entity;
    private Action<GameEventType.Collision.HurtboxEnteredEventData>? hurtboxEnteredHandler;
    private Action<GameEventType.Collision.HurtboxExitedEventData>? hurtboxExitedHandler;

    /// <summary>是否忽略同队接触目标。</summary>
    [Export]
    public bool IgnoreSameTeam { get; set; } = true;

    /// <summary>进入时是否立即造成一次伤害。</summary>
    [Export]
    public bool ApplyOnEnter { get; set; } = true;

    /// <summary>是否在接触期间循环造成伤害。</summary>
    [Export]
    public bool RepeatWhileContact { get; set; } = true;

    /// <inheritdoc />
    public void OnComponentRegistered(IEntity entity, Node entityNode)
    {
        this.entity = entity;
        DamageDataKeys.RegisterAll();
        hurtboxEnteredHandler = OnHurtboxEntered;
        hurtboxExitedHandler = OnHurtboxExited;
        entity.Events.On(GameEventType.Collision.HurtboxEntered, hurtboxEnteredHandler);
        entity.Events.On(GameEventType.Collision.HurtboxExited, hurtboxExitedHandler);
    }

    /// <inheritdoc />
    public void OnComponentUnregistered(IEntity? entity, Node? entityNode)
    {
        if (this.entity != null && hurtboxEnteredHandler != null)
        {
            this.entity.Events.Off(GameEventType.Collision.HurtboxEntered, hurtboxEnteredHandler);
        }

        if (this.entity != null && hurtboxExitedHandler != null)
        {
            this.entity.Events.Off(GameEventType.Collision.HurtboxExited, hurtboxExitedHandler);
        }

        CancelAllTimers();
        hurtboxEnteredHandler = null;
        hurtboxExitedHandler = null;
        this.entity = null;
    }

    private void OnHurtboxEntered(GameEventType.Collision.HurtboxEnteredEventData data)
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
        contactTimers[data.Contact.Target.EntityId] = TimerManager.Instance
            .Loop(interval)
            .OnLoop(() => ApplyContactDamage(data.Contact.Target));
    }

    private void OnHurtboxExited(GameEventType.Collision.HurtboxExitedEventData data)
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

        DamageService.Instance.Process(new DamageInfo
        {
            Attacker = attacker,
            Victim = entity,
            Damage = damage,
            Type = DamageType.Physical,
            Tags = DamageTags.Attack | DamageTags.Contact
        });
    }

    private void CancelTimer(string entityId)
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
