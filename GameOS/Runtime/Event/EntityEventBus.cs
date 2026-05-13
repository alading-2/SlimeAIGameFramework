using System;
using System.Collections.Generic;
using SlimeAI.GameOS.Observation;

namespace SlimeAI.GameOS.Runtime.Event;

/// <summary>
/// 实体级事件总线。支持 IEntityEvent、IBroadcastEvent；对 IBroadcastEvent 自动转发到 WorldEventBus。
/// </summary>
public sealed class EntityEventBus : IEventBus
{
    private static readonly GameOSContextLog Log = GameOSLog.For("EventBus");

    private readonly Dictionary<Type, List<Subscription>> subscriptions = new();
    private readonly HashSet<Type> dispatchingTypes = new();
    private readonly EventBusObservation observation = new();
    private readonly IWorldEventBusRouter? worldRouter;
    private int nextRegistrationOrder;

    /// <summary>
    /// 创建实体级事件总线。
    /// </summary>
    /// <param name="busName">总线标签（通常是 "entity:&lt;id&gt;"）。</param>
    /// <param name="worldRouter">Broadcast 事件转发到 world bus 的路由器，可为 null（未接入 world 时）。</param>
    public EntityEventBus(string busName, IWorldEventBusRouter? worldRouter = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(busName);
        BusName = busName;
        this.worldRouter = worldRouter;
    }

    /// <inheritdoc />
    public string BusName { get; }

    /// <inheritdoc />
    public EventBusObservation Observation => observation;

    /// <inheritdoc />
    public void Publish<T>(in T @event) where T : struct, IEvent
    {
        var eventType = typeof(T);

        if (!typeof(IEntityEvent).IsAssignableFrom(eventType))
        {
            Log.Error(
                $"EntityEventBus {BusName} rejected {eventType.Name}: payload MUST implement IEntityEvent or IBroadcastEvent.",
                new Dictionary<string, object?>
                {
                    ["busName"] = BusName,
                    ["eventType"] = eventType.FullName,
                });
            return;
        }

        observation.RecordPublish(eventType);

        if (!dispatchingTypes.Add(eventType))
        {
            var callChain = $"reentry on {eventType.FullName} within bus {BusName}";
            observation.RecordSameTypeReentry(eventType, callChain);
            Log.Error(
                $"EventBus {BusName} blocked same-type reentry for {eventType.Name}.",
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
            DispatchLocal(eventType, in @event);

            if (@event is IGlobalEvent && worldRouter != null)
            {
                worldRouter.RouteBroadcast(in @event);
            }
        }
        finally
        {
            dispatchingTypes.Remove(eventType);
        }
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

    private void DispatchLocal<T>(Type eventType, in T @event) where T : struct, IEvent
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
                    $"EventBus {BusName} handler threw for {eventType.Name}: {ex.GetType().Name} {ex.Message}",
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
        private readonly EntityEventBus bus;
        private readonly Subscription subscription;
        private readonly string handlerLabel;

        public SubscriptionToken(EntityEventBus bus, Subscription subscription, string handlerLabel)
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
/// 把 IBroadcastEvent 从实体 bus 转发到 world bus 的路由器。
/// </summary>
public interface IWorldEventBusRouter
{
    /// <summary>
    /// 把广播事件派发到 world bus。
    /// </summary>
    /// <typeparam name="T">事件类型，必须同时实现 IGlobalEvent。</typeparam>
    /// <param name="event">事件数据。</param>
    void RouteBroadcast<T>(in T @event) where T : struct, IEvent;
}
