using System.Text.Json;
using SlimeAI.GameOS.Observation;
using SlimeAI.GameOS.Runtime.Event;
using static TestAssert;

internal static class EventBusTests
{
    public static void ReentrySameTypeIsBlocked()
    {
        WithMutedLog(() =>
        {
            var bus = new EntityEventBus("entity:reentry");
            var handled = new List<int>();

            bus.Subscribe<TestEntityEvent>(data =>
            {
                handled.Add(data.Value);
                bus.Publish(new TestEntityEvent(data.Value + 1));
            });
            bus.Subscribe<TestEntityEvent>(data => handled.Add(data.Value * 10));

            bus.Publish(new TestEntityEvent(1));

            AssertEqual("same-type reentry outer handlers continue", "1,10", string.Join(",", handled));
            using var dump = ExportDump(bus);
            var reentry = dump.RootElement
                .GetProperty("SameTypeReentryBlockedCounts")
                .GetProperty(typeof(TestEntityEvent).FullName!);
            AssertEqual("same-type reentry recorded", 1, reentry.GetInt32());
        });
    }

    public static void DifferentTypeCascadeAllowed()
    {
        var bus = new EntityEventBus("entity:cascade");
        var order = new List<string>();

        bus.Subscribe<TestEntityEvent>(_ =>
        {
            order.Add("entity");
            bus.Publish(new TestCascadeEvent(2));
        });
        bus.Subscribe<TestCascadeEvent>(data => order.Add($"cascade:{data.Value}"));

        bus.Publish(new TestEntityEvent(1));

        AssertEqual("different type cascade", "entity,cascade:2", string.Join(",", order));
    }

    public static void DifferentBusSameTypeAllowed()
    {
        var first = new EntityEventBus("entity:first");
        var second = new EntityEventBus("entity:second");
        var handled = new List<string>();

        first.Subscribe<TestEntityEvent>(data =>
        {
            handled.Add($"first:{data.Value}");
            second.Publish(new TestEntityEvent(data.Value + 1));
        });
        second.Subscribe<TestEntityEvent>(data => handled.Add($"second:{data.Value}"));

        first.Publish(new TestEntityEvent(1));

        AssertEqual("different bus same type", "first:1,second:2", string.Join(",", handled));
    }

    public static void HandlerExceptionIsIsolated()
    {
        WithMutedLog(() =>
        {
            var bus = new EntityEventBus("entity:exception");
            var handled = 0;

            bus.Subscribe<TestEntityEvent>(_ => throw new InvalidOperationException("expected-test-exception"));
            bus.Subscribe<TestEntityEvent>(_ => handled++);

            bus.Publish(new TestEntityEvent(1));

            AssertEqual("handler after exception runs", 1, handled);
            using var dump = ExportDump(bus);
            var exceptions = dump.RootElement.GetProperty("HandlerExceptions");
            AssertEqual("handler exception recorded", 1, exceptions.GetArrayLength());
            AssertEqual(
                "handler exception type",
                typeof(InvalidOperationException).FullName,
                exceptions[0].GetProperty("ExceptionType").GetString());
        });
    }

    public static void DisposeUnsubscribes()
    {
        var bus = new EntityEventBus("entity:dispose");
        var handled = 0;

        var sub = bus.Subscribe<TestEntityEvent>(_ => handled++);
        bus.Publish(new TestEntityEvent(1));
        sub.Dispose();
        bus.Publish(new TestEntityEvent(2));

        AssertEqual("dispose unsubscribes", 1, handled);
        using var dump = ExportDump(bus);
        AssertEqual("disposed handler removed from subscriptions", 0, dump.RootElement.GetProperty("Subscriptions").EnumerateObject().Count());
    }

    public static void RegistrationOrderPreserved()
    {
        var bus = new EntityEventBus("entity:order");
        var order = new List<int>();

        bus.Subscribe<TestEntityEvent>(_ => order.Add(1));
        bus.Subscribe<TestEntityEvent>(_ => order.Add(2));

        bus.Publish(new TestEntityEvent(1));

        AssertEqual("registration dispatch order", "1,2", string.Join(",", order));
        using var dump = ExportDump(bus);
        var registrationOrder = dump.RootElement.GetProperty("HandlerRegistrationOrder");
        AssertEqual("first registration order", true, registrationOrder[0].GetString()?.StartsWith("1:", StringComparison.Ordinal));
        AssertEqual("second registration order", true, registrationOrder[1].GetString()?.StartsWith("2:", StringComparison.Ordinal));
    }

    public static void BroadcastRoutesToBothBuses()
    {
        var world = new WorldEventBus();
        var entity = new EntityEventBus("entity:broadcast", world);
        var entityHandled = 0;
        var worldHandled = 0;

        entity.Subscribe<TestBroadcastEvent>(_ => entityHandled++);
        world.Subscribe<TestBroadcastEvent>(_ => worldHandled++);

        entity.Publish(new TestBroadcastEvent(1));

        AssertEqual("broadcast entity handler", 1, entityHandled);
        AssertEqual("broadcast world handler", 1, worldHandled);
    }

    public static void EntityEventOnWorldBusRejected()
    {
        WithMutedLog(() =>
        {
            var world = new WorldEventBus();
            var handled = 0;
            world.Subscribe<TestEntityEvent>(_ => handled++);

            world.Publish(new TestEntityEvent(1));

            AssertEqual("entity event rejected by world bus", 0, handled);
            using var dump = ExportDump(world);
            AssertEqual("rejected entity event not counted as emitted", 0, dump.RootElement.GetProperty("EmittedCounts").EnumerateObject().Count());
        });
    }

    public static void GlobalEventOnEntityBusRejected()
    {
        WithMutedLog(() =>
        {
            var entity = new EntityEventBus("entity:global-reject");
            var handled = 0;
            entity.Subscribe<TestGlobalEvent>(_ => handled++);

            entity.Publish(new TestGlobalEvent(1));

            AssertEqual("global event rejected by entity bus", 0, handled);
            using var dump = ExportDump(entity);
            AssertEqual("rejected global event not counted as emitted", 0, dump.RootElement.GetProperty("EmittedCounts").EnumerateObject().Count());
        });
    }

    private static void WithMutedLog(Action action)
    {
        GameOSLog.Reset(new GameOSLogOptions { EnableStdout = false, EnableJsonl = false });
        try
        {
            action();
        }
        finally
        {
            GameOSLog.Reset(new GameOSLogOptions { EnableStdout = false, EnableJsonl = false });
        }
    }

    private static JsonDocument ExportDump(IEventBus bus)
    {
        var path = Path.Combine(Path.GetTempPath(), $"eventbus-test-{Guid.NewGuid():N}.json");
        bus.ExportObservation(path);
        try
        {
            return JsonDocument.Parse(File.ReadAllText(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    private readonly record struct TestEntityEvent(int Value) : IEntityEvent;

    private readonly record struct TestCascadeEvent(int Value) : IEntityEvent;

    private readonly record struct TestGlobalEvent(int Value) : IGlobalEvent;

    private readonly record struct TestBroadcastEvent(int Value) : IBroadcastEvent;
}
