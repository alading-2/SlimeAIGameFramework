using System;
using System.Collections.Generic;

namespace SkilmeAI.GameOS.Runtime.Event;

/// <summary>
/// Runtime event bus for local entity events and low-frequency global events.
/// </summary>
public sealed class EventBus
{
    private sealed class Subscription
    {
        public required string EventName { get; init; }
        public required Delegate Handler { get; init; }
        public required int Priority { get; init; }
        public required bool Once { get; init; }
        public bool IsPendingRemoval { get; set; }
    }

    private readonly Dictionary<string, List<Subscription>> subscriptions = new(StringComparer.Ordinal);
    private readonly List<Subscription> pendingRemovals = new();
    private readonly HashSet<string> emittingEvents = new(StringComparer.Ordinal);
    private int emittingCount;

    /// <summary>
    /// Raised when a subscriber throws. If unset, exceptions are swallowed to keep event dispatch isolated.
    /// </summary>
    public event Action<string, Exception>? HandlerException;

    /// <summary>
    /// Subscribes a typed handler.
    /// </summary>
    /// <param name="eventName">Stable event name.</param>
    /// <param name="handler">Typed event handler.</param>
    /// <param name="priority">Dispatch priority; larger values run earlier.</param>
    public void On<T>(string eventName, Action<T> handler, int priority = (int)EventPriority.Normal)
    {
        Subscribe(eventName, handler, priority, once: false);
    }

    /// <summary>
    /// Subscribes a parameterless handler.
    /// </summary>
    /// <param name="eventName">Stable event name.</param>
    /// <param name="handler">Event handler.</param>
    /// <param name="priority">Dispatch priority; larger values run earlier.</param>
    public void On(string eventName, Action handler, int priority = (int)EventPriority.Normal)
    {
        Subscribe(eventName, handler, priority, once: false);
    }

    /// <summary>
    /// Subscribes a typed one-shot handler.
    /// </summary>
    /// <param name="eventName">Stable event name.</param>
    /// <param name="handler">Typed event handler.</param>
    /// <param name="priority">Dispatch priority; larger values run earlier.</param>
    public void Once<T>(string eventName, Action<T> handler, int priority = (int)EventPriority.Normal)
    {
        Subscribe(eventName, handler, priority, once: true);
    }

    /// <summary>
    /// Subscribes a parameterless one-shot handler.
    /// </summary>
    /// <param name="eventName">Stable event name.</param>
    /// <param name="handler">Event handler.</param>
    /// <param name="priority">Dispatch priority; larger values run earlier.</param>
    public void Once(string eventName, Action handler, int priority = (int)EventPriority.Normal)
    {
        Subscribe(eventName, handler, priority, once: true);
    }

    /// <summary>
    /// Unsubscribes a typed handler.
    /// </summary>
    /// <param name="eventName">Stable event name.</param>
    /// <param name="handler">Handler instance used during subscription.</param>
    public void Off<T>(string eventName, Action<T> handler)
    {
        Unsubscribe(eventName, handler);
    }

    /// <summary>
    /// Unsubscribes a parameterless handler.
    /// </summary>
    /// <param name="eventName">Stable event name.</param>
    /// <param name="handler">Handler instance used during subscription.</param>
    public void Off(string eventName, Action handler)
    {
        Unsubscribe(eventName, handler);
    }

    /// <summary>
    /// Emits a typed event.
    /// </summary>
    /// <param name="eventName">Stable event name.</param>
    /// <param name="data">Event payload.</param>
    public void Emit<T>(string eventName, T data)
    {
        Trigger(eventName, data);
    }

    /// <summary>
    /// Emits a parameterless event.
    /// </summary>
    /// <param name="eventName">Stable event name.</param>
    public void Emit(string eventName)
    {
        Trigger<object?>(eventName, null);
    }

    /// <summary>
    /// Clears one event's subscriptions.
    /// </summary>
    /// <param name="eventName">Stable event name.</param>
    public void ClearEvent(string eventName)
    {
        subscriptions.Remove(eventName);
    }

    /// <summary>
    /// Clears all subscriptions and dispatch state.
    /// </summary>
    public void Clear()
    {
        subscriptions.Clear();
        pendingRemovals.Clear();
        emittingEvents.Clear();
        emittingCount = 0;
    }

    private void Subscribe(string eventName, Delegate handler, int priority, bool once)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(eventName);
        ArgumentNullException.ThrowIfNull(handler);

        if (!subscriptions.TryGetValue(eventName, out var list))
        {
            list = new List<Subscription>();
            subscriptions[eventName] = list;
        }

        var subscription = new Subscription
        {
            EventName = eventName,
            Handler = handler,
            Priority = priority,
            Once = once
        };

        var insertAt = list.Count;
        for (var i = 0; i < list.Count; i++)
        {
            if (priority > list[i].Priority)
            {
                insertAt = i;
                break;
            }
        }

        list.Insert(insertAt, subscription);
    }

    private void Unsubscribe(string eventName, Delegate handler)
    {
        if (!subscriptions.TryGetValue(eventName, out var list))
        {
            return;
        }

        for (var i = 0; i < list.Count; i++)
        {
            if (list[i].Handler == handler)
            {
                RemoveSubscription(list[i], list);
                return;
            }
        }
    }

    private void Trigger<T>(string eventName, T data)
    {
        if (emittingEvents.Contains(eventName))
        {
            return;
        }

        if (!subscriptions.TryGetValue(eventName, out var list) || list.Count == 0)
        {
            return;
        }

        emittingEvents.Add(eventName);
        emittingCount++;
        try
        {
            var count = list.Count;
            for (var i = 0; i < count; i++)
            {
                var subscription = list[i];
                if (subscription.IsPendingRemoval)
                {
                    continue;
                }

                if (data is EventContext context && context.IsPropagationStopped)
                {
                    break;
                }

                try
                {
                    InvokeSubscription(subscription, data);
                    if (subscription.Once)
                    {
                        RemoveSubscription(subscription, list);
                    }
                }
                catch (Exception ex)
                {
                    HandlerException?.Invoke(eventName, ex);
                }
            }
        }
        finally
        {
            emittingCount--;
            emittingEvents.Remove(eventName);
            if (emittingCount <= 0)
            {
                ProcessPendingRemovals();
            }
        }
    }

    private static void InvokeSubscription<T>(Subscription subscription, T data)
    {
        if (subscription.Handler is Action action)
        {
            action.Invoke();
            return;
        }

        if (subscription.Handler is Action<T> typedHandler)
        {
            typedHandler(data);
        }
    }

    private void RemoveSubscription(Subscription subscription, List<Subscription> list)
    {
        if (emittingCount > 0)
        {
            subscription.IsPendingRemoval = true;
            pendingRemovals.Add(subscription);
            return;
        }

        list.Remove(subscription);
        if (list.Count == 0)
        {
            subscriptions.Remove(subscription.EventName);
        }
    }

    private void ProcessPendingRemovals()
    {
        for (var i = 0; i < pendingRemovals.Count; i++)
        {
            var subscription = pendingRemovals[i];
            if (subscriptions.TryGetValue(subscription.EventName, out var list))
            {
                list.Remove(subscription);
                if (list.Count == 0)
                {
                    subscriptions.Remove(subscription.EventName);
                }
            }
        }

        pendingRemovals.Clear();
    }
}
