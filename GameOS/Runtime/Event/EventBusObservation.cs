using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace SkilmeAI.GameOS.Runtime.Event;

/// <summary>
/// 每总线实例的事件可观测数据。记录订阅、发布、同类型嵌套阻断和 handler 异常。
/// </summary>
public sealed class EventBusObservation
{
    private const int MaxHandlerExceptions = 64;
    private const string SchemaVersion = "1.0";

    private readonly object sync = new();
    private readonly Dictionary<Type, SubscriptionStats> subscriptions = new();
    private readonly Dictionary<Type, long> publishCounts = new();
    private readonly Dictionary<Type, long> reentryBlockedCounts = new();
    private readonly LinkedList<HandlerExceptionRecord> exceptions = new();

    internal void RecordSubscribe(Type eventType, string handlerLabel, int registrationOrder)
    {
        lock (sync)
        {
            if (!subscriptions.TryGetValue(eventType, out var stats))
            {
                stats = new SubscriptionStats();
                subscriptions[eventType] = stats;
            }

            stats.Handlers.Add(new SubscriptionEntry(handlerLabel, registrationOrder));
        }
    }

    internal void RecordUnsubscribe(Type eventType, int registrationOrder)
    {
        lock (sync)
        {
            if (!subscriptions.TryGetValue(eventType, out var stats))
            {
                return;
            }

            stats.Handlers.RemoveAll(entry => entry.RegistrationOrder == registrationOrder);
            if (stats.Handlers.Count == 0)
            {
                subscriptions.Remove(eventType);
            }
        }
    }

    internal void RecordPublish(Type eventType)
    {
        lock (sync)
        {
            publishCounts.TryGetValue(eventType, out var count);
            publishCounts[eventType] = count + 1;
        }
    }

    internal void RecordSameTypeReentry(Type eventType, string callChain)
    {
        lock (sync)
        {
            reentryBlockedCounts.TryGetValue(eventType, out var count);
            reentryBlockedCounts[eventType] = count + 1;
            AppendException(new HandlerExceptionRecord(
                eventType.FullName ?? eventType.Name,
                HandlerLabel: "(reentry guard)",
                ExceptionType: "SameTypeReentryBlocked",
                Message: callChain));
        }
    }

    internal void RecordHandlerException(Type eventType, string handlerLabel, Exception ex)
    {
        lock (sync)
        {
            AppendException(new HandlerExceptionRecord(
                eventType.FullName ?? eventType.Name,
                handlerLabel,
                ex.GetType().FullName ?? ex.GetType().Name,
                ex.Message));
        }
    }

    private void AppendException(HandlerExceptionRecord record)
    {
        exceptions.AddLast(record);
        while (exceptions.Count > MaxHandlerExceptions)
        {
            exceptions.RemoveFirst();
        }
    }

    /// <summary>
    /// 按当前状态生成 eventbus-dump.json 文件。
    /// </summary>
    /// <param name="busName">来源 bus 的名称。</param>
    /// <param name="path">输出文件绝对路径。</param>
    public void ExportTo(string busName, string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var dump = Snapshot(busName);
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };
        File.WriteAllText(path, JsonSerializer.Serialize(dump, options), Encoding.UTF8);
    }

    private EventBusObservationDump Snapshot(string busName)
    {
        lock (sync)
        {
            var subscriptionDump = new Dictionary<string, SubscriptionDumpEntry>();
            foreach (var (eventType, stats) in subscriptions)
            {
                var handlers = new List<SubscriptionDumpHandler>(stats.Handlers.Count);
                foreach (var entry in stats.Handlers)
                {
                    handlers.Add(new SubscriptionDumpHandler(entry.HandlerLabel, entry.RegistrationOrder));
                }

                subscriptionDump[eventType.FullName ?? eventType.Name] = new SubscriptionDumpEntry(handlers.Count, handlers);
            }

            var emittedCounts = new Dictionary<string, long>();
            foreach (var (eventType, count) in publishCounts)
            {
                emittedCounts[eventType.FullName ?? eventType.Name] = count;
            }

            var sameTypeReentry = new Dictionary<string, long>();
            foreach (var (eventType, count) in reentryBlockedCounts)
            {
                sameTypeReentry[eventType.FullName ?? eventType.Name] = count;
            }

            var handlerExceptions = new List<HandlerExceptionRecord>(exceptions);
            var registrationOrder = new List<string>();
            foreach (var (_, stats) in subscriptions)
            {
                foreach (var entry in stats.Handlers)
                {
                    registrationOrder.Add($"{entry.RegistrationOrder}:{entry.HandlerLabel}");
                }
            }

            return new EventBusObservationDump(
                SchemaVersion,
                busName,
                DateTime.UtcNow,
                subscriptionDump,
                emittedCounts,
                sameTypeReentry,
                handlerExceptions,
                registrationOrder);
        }
    }

    private sealed class SubscriptionStats
    {
        public List<SubscriptionEntry> Handlers { get; } = new();
    }

    private readonly record struct SubscriptionEntry(string HandlerLabel, int RegistrationOrder);

    /// <summary>
    /// Handler 异常或 reentry block 记录。
    /// </summary>
    /// <param name="EventType">事件类型全名。</param>
    /// <param name="HandlerLabel">handler 标签。</param>
    /// <param name="ExceptionType">异常类型全名。</param>
    /// <param name="Message">异常或上下文消息。</param>
    public readonly record struct HandlerExceptionRecord(
        string EventType,
        string HandlerLabel,
        string ExceptionType,
        string Message);

    private readonly record struct SubscriptionDumpEntry(
        int HandlerCount,
        List<SubscriptionDumpHandler> Handlers);

    private readonly record struct SubscriptionDumpHandler(
        string HandlerLabel,
        int RegistrationOrder);

    private readonly record struct EventBusObservationDump(
        string SchemaVersion,
        string BusName,
        DateTime GeneratedAtUtc,
        Dictionary<string, SubscriptionDumpEntry> Subscriptions,
        Dictionary<string, long> EmittedCounts,
        Dictionary<string, long> SameTypeReentryBlockedCounts,
        List<HandlerExceptionRecord> HandlerExceptions,
        List<string> HandlerRegistrationOrder);
}
