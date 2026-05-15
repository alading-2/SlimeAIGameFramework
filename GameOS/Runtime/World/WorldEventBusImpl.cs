using System;
using SlimeAI.GameOS.Runtime.CommandBuffer;
using SlimeAI.GameOS.Runtime.Event;

namespace SlimeAI.GameOS.Runtime.World;

/// <summary>
/// RuntimeWorld 默认持有的 world event bus 实例。
/// </summary>
public sealed class WorldEventBusImpl : WorldEventBus
{
    private RuntimeCommandBuffer? commands;

    internal void SetCommandBuffer(RuntimeCommandBuffer commandBuffer)
    {
        commands = commandBuffer ?? throw new ArgumentNullException(nameof(commandBuffer));
    }

    protected override IDisposable EnterHandlerDispatchGuard(Type eventType)
    {
        return commands?.EnterGuard("event-dispatch:" + eventType.Name)
            ?? base.EnterHandlerDispatchGuard(eventType);
    }
}
