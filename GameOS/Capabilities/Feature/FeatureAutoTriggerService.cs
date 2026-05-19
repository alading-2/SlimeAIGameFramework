using System;
using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Event;
using SlimeAI.GameOS.Runtime.Timer;

namespace SlimeAI.GameOS.Capabilities.Feature;

/// <summary>
/// Feature 自动触发服务。调用方负责注册和 Dispose 生命周期。
/// </summary>
public sealed class FeatureAutoTriggerService
{
    private readonly FeatureService featureService;
    private readonly TimerManager timerManager;
    private readonly Func<float> rollPercent;

    /// <summary>
    /// 创建 Feature 自动触发服务。
    /// </summary>
    /// <param name="featureService">Feature 生命周期服务。</param>
    /// <param name="timerManager">计时器管理器；为 null 时使用进程默认入口。</param>
    /// <param name="rollPercent">返回 0-100 的随机数函数；测试可注入确定性实现。</param>
    public FeatureAutoTriggerService(
        FeatureService featureService,
        TimerManager? timerManager = null,
        Func<float>? rollPercent = null)
    {
        this.featureService = featureService ?? throw new ArgumentNullException(nameof(featureService));
        this.timerManager = timerManager ?? TimerManager.Instance;
        this.rollPercent = rollPercent ?? (() => Random.Shared.NextSingle() * 100f);
        FeatureDataKeys.RegisterAll();
    }

    /// <summary>
    /// 注册 Periodic Feature。返回值 Dispose 后会停止后续触发。
    /// </summary>
    public IDisposable RegisterPeriodic(IEntity owner, IEntity feature, FeatureDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentNullException.ThrowIfNull(feature);
        ArgumentNullException.ThrowIfNull(definition);

        var interval = MathF.Max(0.01f, feature.Data.Get<float>(FeatureDataKeys.Cooldown, 1f));
        var timer = timerManager
            .Loop(interval)
            .OnLoop(() =>
            {
                if (feature.Data.Get<bool>(FeatureDataKeys.IsEnabled, true))
                {
                    featureService.Activate(CreateContext(owner, feature, definition));
                }
            });

        return new TimerRegistration(timer);
    }

    /// <summary>
    /// 注册 OnEvent Feature。事件由调用方提供的 bus 派发。
    /// </summary>
    public IDisposable RegisterOnEvent<TEvent>(
        IEntity owner,
        IEntity feature,
        FeatureDefinition definition,
        IEventBus bus)
        where TEvent : struct, IEntityEvent
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentNullException.ThrowIfNull(feature);
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(bus);

        return bus.Subscribe<TEvent>(data =>
        {
            if (!feature.Data.Get<bool>(FeatureDataKeys.IsEnabled, true) || !PassesChance(feature))
            {
                return;
            }

            var payload = new FeatureEventActivationPayload<TEvent>(data);
            featureService.Activate(CreateContext(owner, feature, definition, payload));
        });
    }

    private bool PassesChance(IEntity feature)
    {
        var chance = feature.Data.Get<float>(FeatureDataKeys.TriggerChance, 100f);
        if (chance <= 0f)
        {
            return false;
        }

        return chance >= 100f || rollPercent() < chance;
    }

    private static FeatureContext CreateContext(
        IEntity owner,
        IEntity feature,
        FeatureDefinition definition,
        IFeatureActivationPayload? sourceEventPayload = null)
    {
        return new FeatureContext
        {
            Owner = owner,
            Feature = feature,
            Definition = definition,
            ActivationPayload = sourceEventPayload,
            SourceEventPayload = sourceEventPayload
        };
    }

    private sealed class TimerRegistration : IDisposable
    {
        private readonly GameTimer timer;

        public TimerRegistration(GameTimer timer)
        {
            this.timer = timer;
        }

        public void Dispose()
        {
            timer.Cancel();
        }
    }
}
