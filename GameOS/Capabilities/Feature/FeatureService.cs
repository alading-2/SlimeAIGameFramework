using System;
using System.Collections.Generic;
using SlimeAI.GameOS.Capabilities.Damage;
using SlimeAI.GameOS.Capabilities.Feature.Events;
using SlimeAI.GameOS.Runtime.Data;
using SlimeAI.GameOS.Runtime.Entity;

namespace SlimeAI.GameOS.Capabilities.Feature;

/// <summary>
/// Feature Capability 最小生命周期服务。
/// </summary>
public sealed class FeatureService
{
    /// <summary>进程级默认 FeatureService。</summary>
    public static FeatureService Default { get; } = new();

    /// <summary>进程级默认 FeatureService；向后兼容别名。</summary>
    public static FeatureService Instance => Default;

    /// <summary>
    /// 创建 FeatureService，并确保 Feature DataKey 已注册。
    /// </summary>
    public FeatureService()
    {
        FeatureDataKeys.RegisterAll();
    }

    /// <summary>
    /// 授予 Feature。
    /// </summary>
    /// <param name="owner">拥有者。</param>
    /// <param name="feature">Feature 实体。</param>
    /// <param name="definition">Feature 定义。</param>
    public FeatureContext Grant(IEntity owner, IEntity feature, FeatureDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentNullException.ThrowIfNull(feature);
        ArgumentNullException.ThrowIfNull(definition);

        feature.Data.Set(FeatureDataKeys.FeatureId, definition.FeatureId);
        feature.Data.Set(FeatureDataKeys.HandlerId, definition.HandlerId);
        feature.Data.Set(FeatureDataKeys.IsEnabled, true);
        feature.Data.Set(FeatureDataKeys.IsActive, false);

        ApplyModifiers(owner, feature, definition);
        var context = new FeatureContext { Owner = owner, Feature = feature, Definition = definition };
        ExecuteActions(definition.Actions, context);
        FeatureHandlerRegistry.Get(definition.HandlerId)?.OnGranted(context);

        owner.Events.Publish(new Granted(owner, feature, definition));
        return context;
    }

    /// <summary>
    /// 移除 Feature。
    /// </summary>
    /// <param name="owner">拥有者。</param>
    /// <param name="feature">Feature 实体。</param>
    public void Remove(IEntity owner, IEntity feature)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentNullException.ThrowIfNull(feature);

        var definition = ReadDefinition(feature);
        var context = new FeatureContext { Owner = owner, Feature = feature, Definition = definition };
        FeatureHandlerRegistry.Get(definition.HandlerId)?.OnRemoved(context);
        owner.Data.RemoveModifiersBySource(feature);

        owner.Events.Publish(new Removed(owner, feature, definition));
    }

    /// <summary>
    /// 激活 Feature 并执行处理器。
    /// </summary>
    /// <param name="context">Feature 上下文。</param>
    public FeatureContext Activate(FeatureContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (context.Owner == null || context.Feature == null)
        {
            return context;
        }

        context.Feature.Data.Set(FeatureDataKeys.IsActive, true);
        var definition = context.Definition ?? ReadDefinition(context.Feature);
        var handler = FeatureHandlerRegistry.Get(definition.HandlerId);
        handler?.OnActivated(context);

        context.Feature.Events.Publish(new Activated(context));

        context.ExecutionResult = handler?.OnExecute(context);
        context.Feature.Data.Add(FeatureDataKeys.ActivationCount, 1);

        context.Feature.Events.Publish(new Executed(context));
        return context;
    }

    /// <summary>
    /// 结束 Feature 单次运行。
    /// </summary>
    /// <param name="context">Feature 上下文。</param>
    /// <param name="reason">结束原因。</param>
    public void End(FeatureContext context, FeatureEndReason reason = FeatureEndReason.Completed)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (context.Feature == null)
        {
            return;
        }

        context.Feature.Data.Set(FeatureDataKeys.IsActive, false);
        var definition = context.Definition ?? ReadDefinition(context.Feature);
        FeatureHandlerRegistry.Get(definition.HandlerId)?.OnEnded(context, reason);

        context.Feature.Events.Publish(new Ended(context, reason));
    }

    /// <summary>
    /// 启用 Feature。
    /// </summary>
    /// <param name="owner">拥有者。</param>
    /// <param name="feature">Feature 实体。</param>
    public void Enable(IEntity owner, IEntity feature)
    {
        SetEnabled(owner, feature, true);
    }

    /// <summary>
    /// 禁用 Feature。
    /// </summary>
    /// <param name="owner">拥有者。</param>
    /// <param name="feature">Feature 实体。</param>
    public void Disable(IEntity owner, IEntity feature)
    {
        SetEnabled(owner, feature, false);
    }

    /// <summary>
    /// 批量执行 Feature action。null action 会被跳过。
    /// </summary>
    /// <param name="actions">待执行 action 集合。</param>
    /// <param name="context">Feature 上下文。</param>
    public void ExecuteActions(IEnumerable<IFeatureAction>? actions, FeatureContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (actions == null)
        {
            return;
        }

        foreach (var action in actions)
        {
            action?.Execute(context);
        }
    }

    private static void SetEnabled(IEntity owner, IEntity feature, bool enabled)
    {
        feature.Data.Set(FeatureDataKeys.IsEnabled, enabled);
        var definition = ReadDefinition(feature);
        var context = new FeatureContext { Owner = owner, Feature = feature, Definition = definition };
        if (enabled)
        {
            FeatureHandlerRegistry.Get(definition.HandlerId)?.OnEnabled(context);
            owner.Events.Publish(new Enabled(owner, feature, definition));
        }
        else
        {
            FeatureHandlerRegistry.Get(definition.HandlerId)?.OnDisabled(context);
            owner.Events.Publish(new Disabled(owner, feature, definition));
        }
    }

    private static void ApplyModifiers(IEntity owner, IEntity feature, FeatureDefinition definition)
    {
        for (var i = 0; i < definition.Modifiers.Length; i++)
        {
            var entry = definition.Modifiers[i];
            if (string.IsNullOrEmpty(entry.DataKey.StableKey))
            {
                continue;
            }

            owner.Data.AddModifier(
                entry.DataKey,
                new DataModifier(
                    entry.ModifierType,
                    entry.Value,
                    entry.Priority,
                    id: $"{feature.EntityId}:{entry.DataKey.StableKey}:{i}",
                    source: feature));
        }
    }

    private static FeatureDefinition ReadDefinition(IEntity feature)
    {
        return new FeatureDefinition
        {
            FeatureId = feature.Data.Get(FeatureDataKeys.FeatureId, string.Empty),
            HandlerId = feature.Data.Get(FeatureDataKeys.HandlerId, string.Empty)
        };
    }
}
