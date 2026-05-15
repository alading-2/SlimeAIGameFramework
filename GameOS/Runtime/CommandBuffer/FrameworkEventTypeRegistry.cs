using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using SlimeAI.GameOS.Runtime.Event;
using SlimeAI.GameOS.Runtime.World;

namespace SlimeAI.GameOS.Runtime.CommandBuffer;

internal static class FrameworkEventTypeRegistry
{
    private static readonly Lazy<Dictionary<string, Type>> KnownGlobalEvents = new(BuildKnownGlobalEvents);
    private static readonly MethodInfo PublishDeserializedMethod = typeof(FrameworkEventTypeRegistry)
        .GetMethod(nameof(PublishDeserialized), BindingFlags.Static | BindingFlags.NonPublic)!;

    public static DeferredCommandFailureReason Publish(IWorldEventBus bus, QueuedEventCommandPayload payload)
    {
        if (string.IsNullOrWhiteSpace(payload.EventTypeName) || payload.PayloadBytes.Length == 0)
        {
            return DeferredCommandFailureReason.InvalidPayload;
        }

        if (!KnownGlobalEvents.Value.TryGetValue(payload.EventTypeName, out var eventType))
        {
            return DeferredCommandFailureReason.UnsupportedKind;
        }

        try
        {
            PublishDeserializedMethod.MakeGenericMethod(eventType).Invoke(null, new object[] { bus, payload.PayloadBytes });
            return DeferredCommandFailureReason.None;
        }
        catch (TargetInvocationException)
        {
            return DeferredCommandFailureReason.HandlerException;
        }
        catch (ArgumentException)
        {
            return DeferredCommandFailureReason.InvalidPayload;
        }
        catch (JsonException)
        {
            return DeferredCommandFailureReason.InvalidPayload;
        }
    }

    private static Dictionary<string, Type> BuildKnownGlobalEvents()
    {
        var result = new Dictionary<string, Type>(StringComparer.Ordinal);
        var assembly = typeof(IEvent).Assembly;
        foreach (var type in assembly.GetTypes())
        {
            if (!type.IsValueType || !typeof(IGlobalEvent).IsAssignableFrom(type))
            {
                continue;
            }

            var ns = type.Namespace ?? string.Empty;
            if (!ns.StartsWith("SlimeAI.GameOS.Runtime.Events.", StringComparison.Ordinal)
                && !ns.StartsWith("SlimeAI.GameOS.Capabilities.", StringComparison.Ordinal))
            {
                continue;
            }

            result[type.FullName ?? type.Name] = type;
        }

        return result;
    }

    private static void PublishDeserialized<TEvent>(IWorldEventBus bus, byte[] payloadBytes)
        where TEvent : struct, IGlobalEvent
    {
        var @event = JsonSerializer.Deserialize<TEvent>(payloadBytes);
        bus.Publish(in @event);
    }
}
