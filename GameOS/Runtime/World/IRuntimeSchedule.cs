using System.Collections.Generic;
using SlimeAI.GameOS.Runtime.CommandBuffer;
using SlimeAI.GameOS.Runtime.Schedule;

namespace SlimeAI.GameOS.Runtime.World;

/// <summary>
/// RuntimeWorld 持有的 Schedule 句柄。
/// </summary>
public interface IRuntimeSchedule
{
    ProjectStateService ProjectState { get; }
    bool Register(SystemDescriptor descriptor, SystemConfig config);
    void Bootstrap();
    bool EnsureSystem(string systemId);
    bool SetSystemEnabled(string systemId, bool enabled);
    bool RemoveSystem(string systemId);
    T? Resolve<T>() where T : class;
    bool CanExecute(string systemId, out string message);
    SystemExecuteResult<TResult> Execute<TSystem, TRequest, TResult>(TRequest request)
        where TSystem : class, IRuntimeCommandHandler<TRequest, TResult>;
    IReadOnlyList<SystemRuntimeInfo> GetRuntimeInfo();
    CommandPlaybackReport RunPhase(SchedulePhase phase);
    void PrintStatus();
    void Clear();
}
