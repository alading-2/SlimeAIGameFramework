namespace SlimeAI.GameOS.Runtime.Schedule;

/// <summary>
/// Schedule 托管系统的最小生命周期协议。
/// </summary>
public interface IRuntimeSystem
{
    /// <summary>
    /// 系统实例已创建并纳入调度。
    /// </summary>
    /// <param name="context">系统注册上下文。</param>
    void OnRegistered(SystemRegistrationContext context)
    {
    }

    /// <summary>
    /// 系统即将从调度中移除。
    /// </summary>
    void OnUnregistered()
    {
    }

    /// <summary>
    /// 系统进入运行态。
    /// </summary>
    /// <param name="snapshot">当前项目状态快照。</param>
    void OnStarted(ProjectStateSnapshot snapshot)
    {
    }

    /// <summary>
    /// 系统离开运行态。
    /// </summary>
    /// <param name="snapshot">当前项目状态快照。</param>
    void OnStopped(ProjectStateSnapshot snapshot)
    {
    }

    /// <summary>
    /// 项目状态发生切换。
    /// </summary>
    /// <param name="args">状态切换参数。</param>
    void OnProjectStateChanged(ProjectStateChangedEventArgs args)
    {
    }
}
