using System;

namespace SkilmeAI.GameOS.Runtime.Schedule;

/// <summary>
/// 项目状态切换事件参数。
/// </summary>
public sealed class ProjectStateChangedEventArgs : EventArgs
{
    /// <summary>
    /// 创建状态切换事件参数。
    /// </summary>
    /// <param name="previous">切换前状态。</param>
    /// <param name="current">切换后状态。</param>
    public ProjectStateChangedEventArgs(ProjectStateSnapshot previous, ProjectStateSnapshot current)
    {
        Previous = previous;
        Current = current;
    }

    /// <summary>切换前状态。</summary>
    public ProjectStateSnapshot Previous { get; }

    /// <summary>切换后状态。</summary>
    public ProjectStateSnapshot Current { get; }
}
