using SlimeAI.GameOS.GodotBridge;
using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.World;

internal static class GodotBridgeContextTests
{
    public static void ContextRegistriesAreIsolated()
    {
        GodotNodeRegistry.Clear();
        using var worldA = RuntimeWorld.CreateScoped();
        using var worldB = RuntimeWorld.CreateScoped();
        var contextA = new GodotBridgeContext(worldA);
        var contextB = new GodotBridgeContext(worldB);
        var entityA = new EntityId("bridge-context-entity-a");
        var entityB = new EntityId("bridge-context-entity-b");

        var registeredA = contextA.Registry.RegisterAdapter(entityA, "shared-adapter-id");
        var registeredB = contextB.Registry.RegisterAdapter(entityB, "shared-adapter-id");

        TestAssert.AssertEqual("context a registered", true, registeredA);
        TestAssert.AssertEqual("context b registered", true, registeredB);
        TestAssert.AssertEqual("context a mapping", true, contextA.Registry.IsAdapterRegistered(entityA, "shared-adapter-id"));
        TestAssert.AssertEqual("context b mapping", true, contextB.Registry.IsAdapterRegistered(entityB, "shared-adapter-id"));
        TestAssert.AssertEqual("default registry isolated", 0, GodotNodeRegistry.GetAdaptersByEntity(entityA).Count);
    }
}
