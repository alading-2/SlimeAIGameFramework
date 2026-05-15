using System;
using System.Collections.Generic;
using SlimeAI.GameOS.Observation;
using SlimeAI.GameOS.Runtime.World;

namespace SlimeAI.GameOS.Runtime.Event;

/// <summary>
/// 进程级事件总线。接受 IGlobalEvent / IBroadcastEvent；拒绝 IEntityEvent-only。
/// </summary>
public class WorldEventBus : IWorldEventBus
{
    private static readonly GameOSContextLog Log = GameOSLog.For("EventBus");

    private readonly Dictionary<Type, List<Subscription>> subscriptions = new();
    private readonly HashSet<Type> dispatchingTypes = new();
    private readonly EventBusObservation observation = new();
    private int nextRegistrationOrder;

    /// <inheritdoc />
    public string BusName => "world";

    /// <summary>
    /// 内部 observation 句柄，供测试与 dump 导出使用。
    /// </summary>
    public EventBusObservation Observation => observation;

    /// <inheritdoc />
    public void Publish<T>(in T @event) where T : struct, IEvent
    {
        var eventType = typeof(T);

        if (!typeof(IGlobalEvent).IsAssignableFrom(eventType))
        {
            Log.Error(
                $"WorldEventBus rejected {eventType.Name}: payload MUST implement IGlobalEvent or IBroadcastEvent.",
                new Dictionary<string, object?>
                {
                    ["busName"] = BusName,
                    ["eventType"] = eventType.FullName,
                });
            return;
        }

        DispatchLocal(eventType, in @event);
    }

    /// <inheritdoc />
    public IDisposable Subscribe<T>(Action<T> handler) where T : struct, IEvent
    {
        ArgumentNullException.ThrowIfNull(handler);

        var eventType = typeof(T);
        if (!subscriptions.TryGetValue(eventType, out var list))
        {
            list = new List<Subscription>();
            subscriptions[eventType] = list;
        }

        var order = ++nextRegistrationOrder;
        var subscription = new Subscription(eventType, handler, order);
        list.Add(subscription);

        var handlerLabel = HandlerLabel(handler);
        observation.RecordSubscribe(eventType, handlerLabel, order);

        return new SubscriptionToken(this, subscription, handlerLabel);
    }

    /// <inheritdoc />
    public void ExportObservation(string path)
    {
        observation.ExportTo(BusName, path);
    }

    /// <inheritdoc />
    public void RouteBroadcast<T>(in T @event) where T : struct, IEvent
    {
        if (@event is not IGlobalEvent)
        {
            return;
        }

        DispatchLocal(typeof(T), in @event);
    }

    private void DispatchLocal<T>(Type eventType, in T @event) where T : struct, IEvent
    {
        observation.RecordPublish(eventType);

        if (!dispatchingTypes.Add(eventType))
        {
            var callChain = $"reentry on {eventType.FullName} within bus {BusName}";
            observation.RecordSameTypeReentry(eventType, callChain);
            Log.Error(
                $"WorldEventBus blocked same-type reentry for {eventType.Name}.",
                new Dictionary<string, object?>
                {
                    ["busName"] = BusName,
                    ["eventType"] = eventType.FullName,
                    ["callChain"] = callChain,
                });
            return;
        }

        try
        {
            if (!subscriptions.TryGetValue(eventType, out var list) || list.Count == 0)
            {
                return;
            }

            var snapshot = list.ToArray();
            foreach (var subscription in snapshot)
            {
                if (subscription.IsDisposed)
                {
                    continue;
                }

                try
                {
                    ((Action<T>)subscription.Handler)(@event);
                }
                catch (Exception ex)
                {
                    observation.RecordHandlerException(eventType, HandlerLabel(subscription.Handler), ex);
                    Log.Error(
                        $"WorldEventBus handler threw for {eventType.Name}: {ex.GetType().Name} {ex.Message}",
                        new Dictionary<string, object?>
                        {
                            ["busName"] = BusName,
                            ["eventType"] = eventType.FullName,
                            ["handler"] = HandlerLabel(subscription.Handler),
                            ["exceptionType"] = ex.GetType().FullName,
                        });
                }
            }
        }
        finally
        {
            dispatchingTypes.Remove(eventType);
        }
    }

    internal void Unsubscribe(Subscription subscription, string handlerLabel)
    {
        if (subscription.IsDisposed)
        {
            return;
        }

        subscription.IsDisposed = true;

        if (subscriptions.TryGetValue(subscription.EventType, out var list))
        {
            list.Remove(subscription);
            if (list.Count == 0)
            {
                subscriptions.Remove(subscription.EventType);
            }
        }

        observation.RecordUnsubscribe(subscription.EventType, subscription.RegistrationOrder);
    }

    /// <summary>
    /// 清空所有订阅，供测试或 runtime 关闭时使用。
    /// </summary>
    public void Clear()
    {
        subscriptions.Clear();
        dispatchingTypes.Clear();
    }

    private static string HandlerLabel(Delegate handler)
    {
        var target = handler.Target?.GetType().Name ?? "static";
        return $"{target}.{handler.Method.Name}";
    }

    internal sealed class Subscription
    {
        public Subscription(Type eventType, Delegate handler, int registrationOrder)
        {
            EventType = eventType;
            Handler = handler;
            RegistrationOrder = registrationOrder;
        }

        public Type EventType { get; }

        public Delegate Handler { get; }

        public int RegistrationOrder { get; }

        public bool IsDisposed { get; set; }
    }

    private sealed class SubscriptionToken : IDisposable
    {
        private readonly WorldEventBus bus;
        private readonly Subscription subscription;
        private readonly string handlerLabel;

        public SubscriptionToken(WorldEventBus bus, Subscription subscription, string handlerLabel)
        {
            this.bus = bus;
            this.subscription = subscription;
            this.handlerLabel = handlerLabel;
        }

        public void Dispose()
        {
            bus.Unsubscribe(subscription, handlerLabel);
        }
    }
}

/// <summary>
/// 进程级事件总线的静态访问点。替换旧 GlobalEventBus.Global。
/// </summary>
public static class WorldEvents
{
    /// <summary>
    /// 全局单例 world bus，供跨实体/系统级事件派发。
    /// </summary>
    public static IWorldEventBus World => RuntimeWorld.Default.Events;
}
