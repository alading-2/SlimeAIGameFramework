using System;
using System.Collections.Generic;
using Godot;
using SkilmeAI.GameOS.Capabilities.Attack;
using SkilmeAI.GameOS.Capabilities.Movement;
using SkilmeAI.GameOS.Capabilities.Unit;
using SkilmeAI.GameOS.Runtime.Entity;
using SkilmeAI.GameOS.Runtime.Event;

namespace SkilmeAI.GameOS.GodotBridge;

/// <summary>
/// Godot 普通攻击桥：注册 AttackService，并把节点目标转换为 Runtime 攻击请求。
/// </summary>
public partial class GodotAttackComponent : Node, IGodotComponent
{
    private IEntity? entity;
    private Action<GameEventType.Attack.StartedEventData>? attackStartedHandler;
    private Action<GameEventType.Attack.CancelledEventData>? attackCancelledHandler;

    /// <summary>是否在组件注册时自动注册到默认 AttackService。</summary>
    [Export]
    public bool AutoRegisterService { get; set; } = true;

    /// <summary>是否在注册时把导出攻击参数写入 Runtime Data。</summary>
    [Export]
    public bool ApplyExportedDataOnRegister { get; set; } = true;

    /// <summary>当 Runtime Data 已有攻击参数时，是否优先保留已有值。</summary>
    [Export]
    public bool PreferExistingDataOnRegister { get; set; } = false;

    /// <summary>是否在攻击事件上请求播放 / 停止动画。</summary>
    [Export]
    public bool RequestAnimationEvents { get; set; } = true;

    /// <summary>攻击动画名。</summary>
    [Export]
    public string AttackAnimation { get; set; } = "attack1";

    /// <summary>是否从可用动画里自动选择 attack* 动画。</summary>
    [Export]
    public bool AutoSelectAttackAnimation { get; set; } = true;

    /// <summary>普通攻击伤害。</summary>
    [Export]
    public float Damage { get; set; } = 0f;

    /// <summary>普通攻击距离，-1 表示不限距离。</summary>
    [Export]
    public float Range { get; set; } = 100f;

    /// <summary>攻击间隔，单位秒。</summary>
    [Export]
    public float Interval { get; set; } = 1f;

    /// <summary>攻击前摇时间，单位秒。</summary>
    [Export]
    public float WindUpTime { get; set; } = 0f;

    /// <summary>攻击后摇时间，单位秒。</summary>
    [Export]
    public float RecoveryTime { get; set; } = 0f;

    /// <summary>当前是否允许攻击。</summary>
    [Export]
    public bool CanAttack { get; set; } = true;

    /// <inheritdoc />
    public void OnComponentRegistered(IEntity entity, Node entityNode)
    {
        this.entity = entity;
        AttackDataKeys.RegisterAll();
        if (ApplyExportedDataOnRegister)
        {
            ApplyExportedData(entity);
        }

        if (AutoRegisterService)
        {
            AttackService.Instance.Register(entity);
        }

        if (RequestAnimationEvents)
        {
            attackStartedHandler = OnAttackStarted;
            attackCancelledHandler = OnAttackCancelled;
            entity.Events.On(GameEventType.Attack.Started, attackStartedHandler);
            entity.Events.On(GameEventType.Attack.Cancelled, attackCancelledHandler);
        }
    }

    /// <inheritdoc />
    public void OnComponentUnregistered(IEntity? entity, Node? entityNode)
    {
        if (this.entity != null && AutoRegisterService)
        {
            if (HasRunningAttack(this.entity))
            {
                AttackService.Instance.Cancel(this.entity, AttackCancelReason.ExternalCancel);
            }

            AttackService.Instance.Unregister(this.entity);
        }

        if (this.entity != null && attackStartedHandler != null)
        {
            this.entity.Events.Off(GameEventType.Attack.Started, attackStartedHandler);
        }

        if (this.entity != null && attackCancelledHandler != null)
        {
            this.entity.Events.Off(GameEventType.Attack.Cancelled, attackCancelledHandler);
        }

        attackStartedHandler = null;
        attackCancelledHandler = null;
        this.entity = null;
    }

    /// <summary>
    /// 对指定 Godot 节点所属 Entity 发起普通攻击请求。
    /// </summary>
    /// <param name="targetNode">目标节点；可为目标 Entity 或其子节点。</param>
    public AttackTriggerReport RequestAttackNode(Node targetNode)
    {
        if (entity == null)
        {
            return new AttackTriggerReport(AttackTriggerResult.FailDisabled, null, "Attack component is not registered.");
        }

        var target = GodotCollisionBridge.ResolveOwningEntity(targetNode);
        if (target == null)
        {
            return new AttackTriggerReport(AttackTriggerResult.FailNoTarget, null, "Target node is not bound to a GameOS entity.");
        }

        return RequestAttack(target);
    }

    /// <summary>
    /// 对指定 Runtime Entity 发起普通攻击请求。
    /// </summary>
    /// <param name="target">目标实体。</param>
    public AttackTriggerReport RequestAttack(IEntity target)
    {
        if (entity == null)
        {
            return new AttackTriggerReport(AttackTriggerResult.FailDisabled, null, "Attack component is not registered.");
        }

        var targetPosition = target.Data.Get<Vector2Value>(MovementDataKeys.Position, Vector2Value.Zero);
        return AttackService.Instance.TryRequest(
            new GameEventType.Attack.RequestedEventData(entity, target, targetPosition));
    }

    /// <summary>
    /// 取消当前攻击流程。
    /// </summary>
    /// <param name="reason">取消原因。</param>
    public void CancelAttack(AttackCancelReason reason = AttackCancelReason.ExternalCancel)
    {
        if (entity == null)
        {
            return;
        }

        entity.Events.Emit(
            GameEventType.Attack.CancelRequested,
            new GameEventType.Attack.CancelRequestedEventData(entity, reason));
    }

    private void ApplyExportedData(IEntity entity)
    {
        SetExportedValue(entity, AttackDataKeys.Damage, Damage);
        SetExportedValue(entity, AttackDataKeys.Range, Range);
        SetExportedValue(entity, AttackDataKeys.Interval, Interval);
        SetExportedValue(entity, AttackDataKeys.WindUpTime, WindUpTime);
        SetExportedValue(entity, AttackDataKeys.RecoveryTime, RecoveryTime);
        SetExportedValue(entity, AttackDataKeys.CanAttack, CanAttack);
    }

    private void SetExportedValue<T>(IEntity entity, SkilmeAI.GameOS.Runtime.Data.DataMeta key, T value)
    {
        if (PreferExistingDataOnRegister && entity.Data.Has(key))
        {
            return;
        }

        entity.Data.Set(key, value);
    }

    private static bool HasRunningAttack(IEntity entity)
    {
        return entity.Data.Get<bool>(AttackDataKeys.IsAttacking, false)
            || entity.Data.Get<AttackState>(AttackDataKeys.State, AttackState.Idle) != AttackState.Idle
            || entity.Data.Get<float>(AttackDataKeys.CooldownRemaining, 0f) > 0f;
    }

    private void OnAttackStarted(GameEventType.Attack.StartedEventData data)
    {
        if (entity == null || data.Attacker.EntityId != entity.EntityId)
        {
            return;
        }

        var duration = entity.Data.Get<float>(AttackDataKeys.Interval, -1f);
        entity.Events.Emit(
            GameEventType.Unit.PlayAnimationRequested,
            new GameEventType.Unit.PlayAnimationRequestedEventData(
                entity,
                ResolveAttackAnimation(entity),
                ForceRestart: true,
                Duration: duration));
    }

    private void OnAttackCancelled(GameEventType.Attack.CancelledEventData data)
    {
        if (entity == null || data.Attacker.EntityId != entity.EntityId)
        {
            return;
        }

        entity.Events.Emit(
            GameEventType.Unit.StopAnimationRequested,
            new GameEventType.Unit.StopAnimationRequestedEventData(entity));
    }

    private string ResolveAttackAnimation(IEntity entity)
    {
        if (!AutoSelectAttackAnimation)
        {
            return AttackAnimation;
        }

        var availableAnimations = entity.Data.Get<List<string>>(UnitDataKeys.AvailableAnimations);
        if (availableAnimations.Count == 0)
        {
            return AttackAnimation;
        }

        if (!string.IsNullOrWhiteSpace(AttackAnimation) && availableAnimations.Contains(AttackAnimation))
        {
            return AttackAnimation;
        }

        for (var i = 0; i < availableAnimations.Count; i++)
        {
            if (availableAnimations[i].StartsWith("attack", StringComparison.Ordinal))
            {
                return availableAnimations[i];
            }
        }

        return AttackAnimation;
    }
}
