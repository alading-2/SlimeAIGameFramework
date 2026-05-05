using System;
using System.Collections.Generic;
using SkilmeAI.GameOS.Capabilities.Damage;
using SkilmeAI.GameOS.Capabilities.Movement;
using SkilmeAI.GameOS.Runtime.Entity;
using SkilmeAI.GameOS.Runtime.Event;
using SkilmeAI.GameOS.Runtime.Timer;

namespace SkilmeAI.GameOS.Capabilities.Attack;

/// <summary>
/// 普通攻击最小 Runtime 服务，消费攻击请求并通过 DamageService 结算。
/// </summary>
public sealed class AttackService
{
    private readonly TimerManager timerManager;
    private readonly DamageService damageService;
    private readonly Dictionary<string, Subscription> subscriptions = new(StringComparer.Ordinal);

    /// <summary>进程级默认 AttackService。</summary>
    public static AttackService Instance { get; } = new(TimerManager.Instance, DamageService.Instance);

    /// <summary>
    /// 创建普通攻击服务。
    /// </summary>
    /// <param name="timerManager">前摇、后摇和冷却使用的计时器管理器。</param>
    /// <param name="damageService">伤害结算入口。</param>
    public AttackService(TimerManager timerManager, DamageService damageService)
    {
        this.timerManager = timerManager;
        this.damageService = damageService;
        AttackDataKeys.RegisterAll();
        DamageDataKeys.RegisterAll();
    }

    /// <summary>
    /// 订阅实体局部攻击请求事件。
    /// </summary>
    /// <param name="entity">攻击者实体。</param>
    public void Register(IEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        if (subscriptions.ContainsKey(entity.EntityId))
        {
            return;
        }

        void OnRequested(GameEventType.Attack.RequestedEventData data) => TryRequest(data);
        void OnCancelRequested(GameEventType.Attack.CancelRequestedEventData data) => Cancel(data.Attacker, data.Reason);
        entity.Events.On<GameEventType.Attack.RequestedEventData>(GameEventType.Attack.Requested, OnRequested);
        entity.Events.On<GameEventType.Attack.CancelRequestedEventData>(GameEventType.Attack.CancelRequested, OnCancelRequested);
        subscriptions[entity.EntityId] = new Subscription(entity, OnRequested, OnCancelRequested);
    }

    /// <summary>
    /// 取消订阅实体攻击请求事件。
    /// </summary>
    /// <param name="entity">攻击者实体。</param>
    public void Unregister(IEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        if (!subscriptions.Remove(entity.EntityId, out var subscription))
        {
            return;
        }

        subscription.Entity.Events.Off(GameEventType.Attack.Requested, subscription.OnRequested);
        subscription.Entity.Events.Off(GameEventType.Attack.CancelRequested, subscription.OnCancelRequested);
    }

    /// <summary>
    /// 清空所有订阅。
    /// </summary>
    public void Clear()
    {
        foreach (var subscription in subscriptions.Values)
        {
            subscription.Entity.Events.Off(GameEventType.Attack.Requested, subscription.OnRequested);
            subscription.Entity.Events.Off(GameEventType.Attack.CancelRequested, subscription.OnCancelRequested);
        }

        subscriptions.Clear();
    }

    /// <summary>
    /// 尝试处理一次攻击请求。
    /// </summary>
    /// <param name="request">攻击请求 payload。</param>
    public AttackTriggerReport TryRequest(GameEventType.Attack.RequestedEventData request)
    {
        var validation = Validate(request);
        if (validation.Result != AttackTriggerResult.Success)
        {
            EmitCancelledForValidation(request, validation.Result);
            return validation;
        }

        StartAttack(request);
        return new AttackTriggerReport(AttackTriggerResult.Success, null, string.Empty);
    }

    /// <summary>
    /// 取消指定攻击者的当前攻击流程。
    /// </summary>
    /// <param name="attacker">攻击者。</param>
    /// <param name="reason">取消原因。</param>
    public void Cancel(IEntity attacker, AttackCancelReason reason = AttackCancelReason.ExternalCancel)
    {
        ArgumentNullException.ThrowIfNull(attacker);
        timerManager.CancelByTag(GetTimerTag(attacker));
        SetIdle(attacker);
        attacker.Data.Set(AttackDataKeys.CooldownRemaining, 0f);
        attacker.Events.Emit(GameEventType.Attack.Cancelled, new GameEventType.Attack.CancelledEventData(attacker, reason));
    }

    private void StartAttack(GameEventType.Attack.RequestedEventData request)
    {
        var attacker = request.Attacker;
        timerManager.CancelByTag(GetTimerTag(attacker));
        attacker.Data.Set(AttackDataKeys.IsAttacking, true);
        attacker.Data.Set(AttackDataKeys.State, AttackState.WindUp);
        attacker.Events.Emit(GameEventType.Attack.Started, new GameEventType.Attack.StartedEventData(attacker, request.Target));
        StartCooldown(attacker);

        var windUpTime = attacker.Data.Get<float>(AttackDataKeys.WindUpTime, 0f);
        if (windUpTime > 0f)
        {
            timerManager.Delay(windUpTime)
                .WithTag(GetTimerTag(attacker))
                .OnComplete(() => ResolveHit(request));
            return;
        }

        ResolveHit(request);
    }

    private void ResolveHit(GameEventType.Attack.RequestedEventData request)
    {
        var attacker = request.Attacker;
        var target = request.Target;
        if (target == null || target.Data.Get<bool>(DamageDataKeys.IsDead, false))
        {
            Cancel(attacker, target == null ? AttackCancelReason.TargetInvalid : AttackCancelReason.TargetDead);
            return;
        }

        if (attacker.Data.Get<bool>(DamageDataKeys.IsDead, false))
        {
            Cancel(attacker, AttackCancelReason.SelfDead);
            return;
        }

        var damage = attacker.Data.Get<float>(AttackDataKeys.Damage, 0f);
        var result = damageService.Process(new DamageInfo
        {
            Attacker = attacker,
            Victim = target,
            Damage = damage,
            Type = DamageType.Physical,
            Tags = DamageTags.Attack
        });
        attacker.Events.Emit(
            GameEventType.Attack.Finished,
            new GameEventType.Attack.FinishedEventData(attacker, target, result.Applied));
        StartRecovery(attacker);
    }

    private void StartRecovery(IEntity attacker)
    {
        var recoveryTime = attacker.Data.Get<float>(AttackDataKeys.RecoveryTime, 0f);
        if (recoveryTime <= 0f)
        {
            SetIdle(attacker);
            return;
        }

        attacker.Data.Set(AttackDataKeys.State, AttackState.Recovery);
        timerManager.Delay(recoveryTime)
            .WithTag(GetTimerTag(attacker))
            .OnComplete(() => SetIdle(attacker));
    }

    private void StartCooldown(IEntity attacker)
    {
        var interval = attacker.Data.Get<float>(AttackDataKeys.Interval, 0f);
        if (interval <= 0f)
        {
            attacker.Data.Set(AttackDataKeys.CooldownRemaining, 0f);
            return;
        }

        attacker.Data.Set(AttackDataKeys.CooldownRemaining, interval);
        timerManager.Delay(interval)
            .WithTag(GetTimerTag(attacker))
            .OnUpdate(progress => attacker.Data.Set(AttackDataKeys.CooldownRemaining, MathF.Max(0f, interval * (1f - progress))))
            .OnComplete(() => attacker.Data.Set(AttackDataKeys.CooldownRemaining, 0f));
    }

    private AttackTriggerReport Validate(GameEventType.Attack.RequestedEventData request)
    {
        var attacker = request.Attacker;
        var target = request.Target;
        if (target == null)
        {
            return Fail(AttackTriggerResult.FailNoTarget, "Attack requires target.");
        }

        if (attacker.Data.Get<bool>(DamageDataKeys.IsDead, false))
        {
            return Fail(AttackTriggerResult.FailSelfDead, "Attacker is dead.");
        }

        if (target.Data.Get<bool>(DamageDataKeys.IsDead, false))
        {
            return Fail(AttackTriggerResult.FailTargetDead, "Target is dead.");
        }

        if (!attacker.Data.Get<bool>(AttackDataKeys.CanAttack, true))
        {
            return Fail(AttackTriggerResult.FailDisabled, "Attacker cannot attack.");
        }

        if (attacker.Data.Get<bool>(AttackDataKeys.IsAttacking, false)
            || attacker.Data.Get<AttackState>(AttackDataKeys.State, AttackState.Idle) != AttackState.Idle)
        {
            return Fail(AttackTriggerResult.FailActive, "Attack is active.");
        }

        if (attacker.Data.Get<float>(AttackDataKeys.CooldownRemaining, 0f) > 0f)
        {
            return Fail(AttackTriggerResult.FailCooldown, "Attack is on cooldown.");
        }

        var range = attacker.Data.Get<float>(AttackDataKeys.Range, 100f);
        var origin = attacker.Data.Get<Vector2Value>(MovementDataKeys.Position, Vector2Value.Zero);
        var targetPosition = target.Data.Get<Vector2Value>(MovementDataKeys.Position, request.TargetPosition);
        if (range >= 0f && Vector2Value.Distance(origin, targetPosition) > range)
        {
            return Fail(AttackTriggerResult.FailOutOfRange, "Target is out of range.");
        }

        return new AttackTriggerReport(AttackTriggerResult.Success, null, string.Empty);
    }

    private static AttackTriggerReport Fail(AttackTriggerResult result, string message)
    {
        return new AttackTriggerReport(result, null, message);
    }

    private static void EmitCancelledForValidation(GameEventType.Attack.RequestedEventData request, AttackTriggerResult result)
    {
        var reason = result switch
        {
            AttackTriggerResult.FailSelfDead => AttackCancelReason.SelfDead,
            AttackTriggerResult.FailTargetDead => AttackCancelReason.TargetDead,
            AttackTriggerResult.FailDisabled => AttackCancelReason.SelfDisabled,
            AttackTriggerResult.FailNoTarget => AttackCancelReason.TargetInvalid,
            AttackTriggerResult.FailOutOfRange => AttackCancelReason.TargetOutOfRange,
            _ => (AttackCancelReason?)null
        };
        if (reason.HasValue)
        {
            request.Attacker.Events.Emit(
                GameEventType.Attack.Cancelled,
                new GameEventType.Attack.CancelledEventData(request.Attacker, reason.Value));
        }
    }

    private static void SetIdle(IEntity attacker)
    {
        attacker.Data.Set(AttackDataKeys.IsAttacking, false);
        attacker.Data.Set(AttackDataKeys.State, AttackState.Idle);
    }

    private static string GetTimerTag(IEntity attacker)
    {
        return $"attack:{attacker.EntityId}";
    }

    private sealed record Subscription(
        IEntity Entity,
        Action<GameEventType.Attack.RequestedEventData> OnRequested,
        Action<GameEventType.Attack.CancelRequestedEventData> OnCancelRequested);
}
