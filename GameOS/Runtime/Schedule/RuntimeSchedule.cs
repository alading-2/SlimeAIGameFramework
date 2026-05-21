using System;
using System.Collections.Generic;
using System.Text;
using SlimeAI.GameOS.Observation;
using SlimeAI.GameOS.Runtime.CommandBuffer;
using SlimeAI.GameOS.Runtime.World;

namespace SlimeAI.GameOS.Runtime.Schedule;

/// <summary>
/// 纯 C# Runtime 调度器，负责系统实例、依赖、运行条件和生命周期。
/// </summary>
public sealed class RuntimeSchedule : IRuntimeSchedule
{
    private static readonly GameOSContextLog Log = GameOSLog.For("RuntimeSchedule");

    private sealed class Entry
    {
        public required SystemDescriptor Descriptor { get; init; }
        public required SystemConfig Config { get; init; }
        public required object Instance { get; init; }
        public required IRuntimeSystem? System { get; init; }
        public bool IsEnabled { get; set; }
        public bool IsStateAllowed { get; set; }
        public bool IsRunning { get; set; }
        public string BlockedReason { get; set; } = string.Empty;
    }

    private readonly Dictionary<string, SystemDescriptor> descriptors = new(StringComparer.Ordinal);
    private readonly Dictionary<string, SystemConfig> configs = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Entry> entries = new(StringComparer.Ordinal);
    private readonly IRuntimeCommandBuffer? commandBuffer;
    private bool isCleared;

    /// <summary>项目状态服务。</summary>
    public ProjectStateService ProjectState { get; } = new();

    /// <summary>
    /// 创建调度器并订阅项目状态切换。
    /// </summary>
    public RuntimeSchedule()
        : this(null)
    {
    }

    /// <summary>
    /// 创建带 CommandBuffer playback 入口的调度器。
    /// </summary>
    public RuntimeSchedule(IRuntimeCommandBuffer? commandBuffer)
    {
        this.commandBuffer = commandBuffer;
        ProjectState.StateChanged += OnProjectStateChanged;
    }

    /// <summary>
    /// 注册系统描述符和配置。
    /// </summary>
    /// <param name="descriptor">系统描述符。</param>
    /// <param name="config">系统调度配置。</param>
    public bool Register(SystemDescriptor descriptor, SystemConfig config)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(config);

        if (!string.Equals(descriptor.SystemId, config.SystemId, StringComparison.Ordinal))
        {
            Log.Warn(
                $"System registration id mismatch: descriptor={descriptor.SystemId}, config={config.SystemId}",
                new Dictionary<string, object?>
                {
                    ["descriptorSystemId"] = descriptor.SystemId,
                    ["configSystemId"] = config.SystemId
                });
            return false;
        }

        if (descriptors.ContainsKey(descriptor.SystemId))
        {
            Log.Debug(
                $"System registration skipped: {descriptor.SystemId} already registered",
                new Dictionary<string, object?>
                {
                    ["systemId"] = descriptor.SystemId
                });
            return false;
        }

        descriptors[descriptor.SystemId] = descriptor;
        configs[config.SystemId] = config;
        Log.Info(
            $"System registered: {descriptor.SystemId}, phase={config.Group}",
            new Dictionary<string, object?>
            {
                ["systemId"] = descriptor.SystemId,
                ["group"] = config.Group,
                ["phase"] = config.Group,
                ["priority"] = config.Priority,
                ["startEnabled"] = config.StartEnabled
            });
        return true;
    }

    /// <summary>
    /// 按配置优先级启动全部已注册系统。
    /// </summary>
    public void Bootstrap()
    {
        var ordered = new List<SystemConfig>(configs.Values);
        ordered.Sort(static (left, right) => left.Priority.CompareTo(right.Priority));

        Log.Info(
            $"RuntimeSchedule bootstrap started: {ordered.Count} systems",
            new Dictionary<string, object?>
            {
                ["systemCount"] = ordered.Count
            });

        foreach (var config in ordered)
        {
            EnsureSystem(config.SystemId);
        }

        Log.Info(
            $"RuntimeSchedule bootstrap completed: {entries.Count} systems loaded",
            new Dictionary<string, object?>
            {
                ["systemCount"] = entries.Count
            });
        PrintStatus();
    }

    /// <summary>
    /// 确保指定系统已加载。
    /// </summary>
    /// <param name="systemId">系统 Id。</param>
    public bool EnsureSystem(string systemId)
    {
        if (entries.ContainsKey(systemId))
        {
            return true;
        }

        if (!descriptors.TryGetValue(systemId, out var descriptor) || !configs.TryGetValue(systemId, out var config))
        {
            return false;
        }

        foreach (var dependency in config.Dependencies)
        {
            if (!EnsureSystem(dependency))
            {
                return false;
            }
        }

        var instance = descriptor.Factory.Invoke();
        var runtimeSystem = instance as IRuntimeSystem;
        runtimeSystem?.OnRegistered(new SystemRegistrationContext(descriptor, ProjectState));

        var (isBlocked, blockedReason) = config.RunCondition.GetBlockedReason(ProjectState.Snapshot);
        var entry = new Entry
        {
            Descriptor = descriptor,
            Config = config,
            Instance = instance,
            System = runtimeSystem,
            IsEnabled = config.StartEnabled,
            IsStateAllowed = !isBlocked,
            BlockedReason = blockedReason
        };

        entries[systemId] = entry;
        ApplyEntryState(entry, notifyTransition: true);
        Log.Info(
            $"System loaded: {systemId}, enabled={entry.IsEnabled}, stateAllowed={entry.IsStateAllowed}, running={entry.IsRunning}",
            new Dictionary<string, object?>
            {
                ["systemId"] = systemId,
                ["isEnabled"] = entry.IsEnabled,
                ["isStateAllowed"] = entry.IsStateAllowed,
                ["isRunning"] = entry.IsRunning,
                ["blockedReason"] = entry.BlockedReason
            });
        return true;
    }

    /// <summary>
    /// 设置系统人工启用状态。
    /// </summary>
    /// <param name="systemId">系统 Id。</param>
    /// <param name="enabled">目标启用状态。</param>
    public bool SetSystemEnabled(string systemId, bool enabled)
    {
        if (!entries.TryGetValue(systemId, out var entry) || (!enabled && entry.Config.Required))
        {
            return false;
        }

        if (entry.IsEnabled == enabled)
        {
            return true;
        }

        entry.IsEnabled = enabled;
        ApplyEntryState(entry, notifyTransition: true);
        return true;
    }

    /// <summary>
    /// 移除已加载系统。
    /// </summary>
    /// <param name="systemId">系统 Id。</param>
    public bool RemoveSystem(string systemId)
    {
        if (!entries.TryGetValue(systemId, out var entry) || entry.Config.Required)
        {
            return false;
        }

        if (FindLoadedDependentSystem(systemId).Length > 0)
        {
            return false;
        }

        if (entry.IsRunning)
        {
            entry.System?.OnStopped(ProjectState.Snapshot);
        }

        entry.System?.OnUnregistered();
        entries.Remove(systemId);
        return true;
    }

    /// <summary>
    /// 按类型解析已加载系统。
    /// </summary>
    public T? Resolve<T>() where T : class
    {
        foreach (var entry in entries.Values)
        {
            if (entry.Instance is T typed)
            {
                return typed;
            }
        }

        return null;
    }

    /// <summary>
    /// 判断系统当前是否允许执行外部命令。
    /// </summary>
    public bool CanExecute(string systemId, out string message)
    {
        if (!entries.TryGetValue(systemId, out var entry))
        {
            message = $"系统 {systemId} 未加载";
            return false;
        }

        if (!entry.IsEnabled)
        {
            message = $"系统 {systemId} 已禁用";
            return false;
        }

        if (!entry.IsStateAllowed)
        {
            message = string.IsNullOrEmpty(entry.BlockedReason)
                ? $"系统 {systemId} 当前项目状态不允许运行"
                : entry.BlockedReason;
            return false;
        }

        if (!entry.IsRunning)
        {
            message = $"系统 {systemId} 尚未进入运行态";
            return false;
        }

        message = string.Empty;
        return true;
    }

    /// <summary>
    /// 通过调度门禁执行系统命令。
    /// </summary>
    public SystemExecuteResult<TResult> Execute<TSystem, TRequest, TResult>(TRequest request)
        where TSystem : class, IRuntimeCommandHandler<TRequest, TResult>
    {
        foreach (var entry in entries.Values)
        {
            if (entry.Instance is not TSystem system)
            {
                continue;
            }

            if (!CanExecute(entry.Descriptor.SystemId, out var message))
            {
                return SystemExecuteResult<TResult>.Blocked(message);
            }

            return SystemExecuteResult<TResult>.Ok(system.Execute(request));
        }

        return SystemExecuteResult<TResult>.Blocked($"系统 {typeof(TSystem).Name} 未加载");
    }

    /// <summary>
    /// 获取全部系统状态快照。
    /// </summary>
    public IReadOnlyList<SystemRuntimeInfo> GetRuntimeInfo()
    {
        var result = new List<SystemRuntimeInfo>(entries.Count);
        foreach (var entry in entries.Values)
        {
            result.Add(ToRuntimeInfo(entry));
        }

        return result;
    }

    /// <summary>
    /// 播放指定 phase 的 RuntimeCommandBuffer；不 tick capability service。
    /// </summary>
    public CommandPlaybackReport RunPhase(SchedulePhase phase)
    {
        if (isCleared)
        {
            throw new ObjectDisposedException(nameof(RuntimeSchedule));
        }

        var report = commandBuffer?.Playback(phase) ?? CommandPlaybackReport.Empty(phase);
        Log.Debug(
            $"Phase {phase}: {report.PlayedCount} systems executed",
            new Dictionary<string, object?>
            {
                ["phase"] = phase,
                ["count"] = report.PlayedCount,
                ["queuedCount"] = report.QueuedCount,
                ["playedCount"] = report.PlayedCount,
                ["failedCount"] = report.FailedCount,
                ["skippedCount"] = report.SkippedCount
            });
        return report;
    }

    /// <summary>
    /// 输出已加载系统状态摘要。
    /// </summary>
    public void PrintStatus()
    {
        var snapshot = ProjectState.Snapshot;
        var builder = new StringBuilder();
        builder.Append("RuntimeSchedule status: ")
            .Append(entries.Count)
            .Append(" systems, flowState=")
            .Append(snapshot.FlowState)
            .Append(", overlays=")
            .Append(snapshot.Overlays)
            .Append(", simulation=")
            .Append(snapshot.SimulationState);

        foreach (var entry in entries.Values)
        {
            builder.AppendLine()
                .Append("- ")
                .Append(entry.Descriptor.SystemId)
                .Append(" group=")
                .Append(entry.Config.Group)
                .Append(" enabled=")
                .Append(entry.IsEnabled)
                .Append(" stateAllowed=")
                .Append(entry.IsStateAllowed)
                .Append(" running=")
                .Append(entry.IsRunning);

            if (!string.IsNullOrWhiteSpace(entry.BlockedReason))
            {
                builder.Append(" blockedReason=\"")
                    .Append(entry.BlockedReason)
                    .Append('"');
            }
        }

        Log.Info(
            builder.ToString(),
            new Dictionary<string, object?>
            {
                ["systemCount"] = entries.Count,
                ["flowState"] = snapshot.FlowState,
                ["overlays"] = snapshot.Overlays,
                ["simulationState"] = snapshot.SimulationState
            });
    }

    /// <summary>
    /// 清空全部系统。
    /// </summary>
    public void Clear()
    {
        foreach (var entry in entries.Values)
        {
            if (entry.IsRunning)
            {
                entry.System?.OnStopped(ProjectState.Snapshot);
            }

            entry.System?.OnUnregistered();
        }

        entries.Clear();
        descriptors.Clear();
        configs.Clear();
        isCleared = true;
    }

    private void OnProjectStateChanged(object? sender, ProjectStateChangedEventArgs args)
    {
        Log.Info(
            $"FlowState: {args.Previous.FlowState}->{args.Current.FlowState}, overlays={args.Current.Overlays}",
            new Dictionary<string, object?>
            {
                ["previousFlowState"] = args.Previous.FlowState,
                ["currentFlowState"] = args.Current.FlowState,
                ["overlays"] = args.Current.Overlays,
                ["previousOverlays"] = args.Previous.Overlays,
                ["simulationState"] = args.Current.SimulationState
            });

        foreach (var entry in entries.Values)
        {
            var (isBlocked, blockedReason) = entry.Config.RunCondition.GetBlockedReason(args.Current);
            entry.IsStateAllowed = !isBlocked;
            entry.BlockedReason = blockedReason;
            entry.System?.OnProjectStateChanged(args);
            ApplyEntryState(entry, notifyTransition: true);
        }
    }

    private void ApplyEntryState(Entry entry, bool notifyTransition)
    {
        var shouldRun = entry.IsEnabled && entry.IsStateAllowed;
        if (!notifyTransition)
        {
            entry.IsRunning = shouldRun;
            return;
        }

        if (shouldRun == entry.IsRunning)
        {
            return;
        }

        if (shouldRun)
        {
            entry.System?.OnStarted(ProjectState.Snapshot);
            entry.IsRunning = true;
            return;
        }

        entry.System?.OnStopped(ProjectState.Snapshot);
        entry.IsRunning = false;
    }

    private string FindLoadedDependentSystem(string targetSystemId)
    {
        foreach (var entry in entries.Values)
        {
            if (string.Equals(entry.Descriptor.SystemId, targetSystemId, StringComparison.Ordinal))
            {
                continue;
            }

            foreach (var dependency in entry.Config.Dependencies)
            {
                if (string.Equals(dependency, targetSystemId, StringComparison.Ordinal))
                {
                    return entry.Descriptor.SystemId;
                }
            }
        }

        return string.Empty;
    }

    private static SystemRuntimeInfo ToRuntimeInfo(Entry entry)
    {
        return new SystemRuntimeInfo
        {
            SystemId = entry.Descriptor.SystemId,
            IsAdded = true,
            IsEnabled = entry.IsEnabled,
            IsRunning = entry.IsRunning,
            IsStateAllowed = entry.IsStateAllowed,
            BlockedReason = entry.BlockedReason,
            Group = entry.Config.Group,
            Tags = entry.Config.Tags
        };
    }
}
