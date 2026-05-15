using System;
using SlimeAI.GameOS.Runtime.CommandBuffer;
using SlimeAI.GameOS.Runtime.Schedule;

namespace SlimeAI.GameOS.Runtime.World;

/// <summary>
/// GameOS Runtime 的世界容器 facade。
/// </summary>
public sealed class RuntimeWorld : IDisposable
{
    private readonly EntityRegistry entities;
    private readonly LifecycleTreeImpl lifecycle;
    private readonly WorldEventBusImpl events;
    private readonly ResourceCatalogState resources;
    private readonly ObjectPoolManagerState pools;
    private readonly RuntimeCommandBuffer commands;
    private readonly RuntimeSchedule schedule;
    private readonly Action<string>? disposeStepObserver;

    private RuntimeWorld(bool isDefault, Action<string>? disposeStepObserver = null)
    {
        IsDefault = isDefault;
        events = new WorldEventBusImpl();
        commands = new RuntimeCommandBuffer(this);
        events.SetCommandBuffer(commands);
        schedule = new RuntimeSchedule(commands);
        lifecycle = new LifecycleTreeImpl(events, commands);
        entities = new EntityRegistry(lifecycle, events, commands);
        resources = new ResourceCatalogState();
        pools = new ObjectPoolManagerState();
        this.disposeStepObserver = disposeStepObserver;
    }

    /// <summary>进程级默认 world；现有 static facade 全部转发到这里。</summary>
    public static RuntimeWorld Default { get; } = new(isDefault: true);

    /// <summary>创建独立 scoped world，用于测试和沙箱运行。</summary>
    public static RuntimeWorld CreateScoped()
    {
        return new RuntimeWorld(isDefault: false);
    }

    /// <summary>创建带 dispose 观察钩子的 scoped world，供框架测试验证顺序。</summary>
    internal static RuntimeWorld CreateScoped(Action<string> disposeStepObserver)
    {
        ArgumentNullException.ThrowIfNull(disposeStepObserver);
        return new RuntimeWorld(isDefault: false, disposeStepObserver);
    }

    /// <summary>实体注册表句柄。</summary>
    public IEntityRegistry Entities => EnsureAlive(entities);

    /// <summary>Lifecycle 树句柄。</summary>
    public ILifecycleTree Lifecycle => EnsureAlive(lifecycle);

    /// <summary>World 事件总线句柄。</summary>
    public IWorldEventBus Events => EnsureAlive(events);

    /// <summary>资源目录句柄。</summary>
    public IResourceCatalog Resources => EnsureAlive(resources);

    /// <summary>对象池管理句柄。</summary>
    public IObjectPoolManager Pools => EnsureAlive(pools);

    /// <summary>Runtime 调度句柄。</summary>
    public IRuntimeSchedule Schedule => EnsureAlive(schedule);

    /// <summary>Runtime CommandBuffer 句柄。</summary>
    public IRuntimeCommandBuffer Commands => EnsureAlive(commands);

    internal EntityRegistry EntityRegistry => entities;

    internal LifecycleTreeImpl LifecycleTree => lifecycle;

    internal WorldEventBusImpl EventBus => events;

    internal ResourceCatalogState ResourceCatalog => resources;

    /// <summary>是否为进程级默认 world。</summary>
    public bool IsDefault { get; }

    /// <summary>是否已经开始 dispose。</summary>
    public bool IsDisposed { get; private set; }

    /// <inheritdoc />
    public void Dispose()
    {
        if (IsDefault)
        {
            throw new InvalidOperationException("RuntimeWorld.Default cannot be disposed");
        }

        if (IsDisposed)
        {
            return;
        }

        IsDisposed = true;
        ClearSubsystem("Schedule", schedule.Clear);
        ClearSubsystem("Commands", () => commands.Clear());
        ClearSubsystem("Pools", pools.Clear);
        ClearSubsystem("Resources", resources.Clear);
        ClearSubsystem("Lifecycle", lifecycle.Clear);
        ClearSubsystem("Entities", entities.Clear);
        ClearSubsystem("Events", events.Clear);
    }

    private void ClearSubsystem(string name, Action clear)
    {
        disposeStepObserver?.Invoke(name);
        clear();
    }

    private T EnsureAlive<T>(T subsystem)
    {
        if (IsDisposed)
        {
            throw new ObjectDisposedException(nameof(RuntimeWorld));
        }

        return subsystem;
    }
}
