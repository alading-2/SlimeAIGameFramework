using System;

namespace SkilmeAI.GameOS.Runtime.Schedule;

/// <summary>
/// 项目状态服务，统一维护流程、覆盖层和模拟状态。
/// </summary>
public sealed class ProjectStateService
{
    private const ushort KnownFlowStateBits =
        (ushort)(GameFlowState.Boot
            | GameFlowState.FrontEnd
            | GameFlowState.SessionPreparing
            | GameFlowState.SessionPlaying
            | GameFlowState.SessionResolving
            | GameFlowState.SessionEnded
            | GameFlowState.ShuttingDown);

    private const byte KnownSimulationStateBits =
        (byte)(SimulationState.Running | SimulationState.Suspended);

    private ProjectStateSnapshot snapshot = new(
        GameFlowState.Boot,
        OverlayFlags.None,
        SimulationState.Running);

    /// <summary>当前流程状态。</summary>
    public GameFlowState FlowState => snapshot.FlowState;

    /// <summary>当前覆盖层。</summary>
    public OverlayFlags Overlays => snapshot.Overlays;

    /// <summary>当前模拟状态。</summary>
    public SimulationState SimulationState => snapshot.SimulationState;

    /// <summary>当前完整快照。</summary>
    public ProjectStateSnapshot Snapshot => snapshot;

    /// <summary>状态切换前事件。</summary>
    public event EventHandler<ProjectStateChangedEventArgs>? BeforeStateChanged;

    /// <summary>状态切换事件。</summary>
    public event EventHandler<ProjectStateChangedEventArgs>? StateChanged;

    /// <summary>状态切换后事件。</summary>
    public event EventHandler<ProjectStateChangedEventArgs>? AfterStateChanged;

    /// <summary>
    /// 用新快照替换当前状态。
    /// </summary>
    /// <param name="next">新的项目状态。</param>
    public void Apply(ProjectStateSnapshot next)
    {
        ValidateSnapshot(next);
        if (snapshot.Equals(next))
        {
            return;
        }

        var previous = snapshot;
        var args = new ProjectStateChangedEventArgs(previous, next);

        BeforeStateChanged?.Invoke(this, args);
        snapshot = next;
        StateChanged?.Invoke(this, args);
        AfterStateChanged?.Invoke(this, args);
    }

    /// <summary>
    /// 单独切换流程状态。
    /// </summary>
    /// <param name="state">目标流程状态。</param>
    public void SetFlowState(GameFlowState state)
    {
        Apply(snapshot with { FlowState = state });
    }

    /// <summary>
    /// 单独切换覆盖层。
    /// </summary>
    /// <param name="overlays">目标覆盖层。</param>
    public void SetOverlays(OverlayFlags overlays)
    {
        Apply(snapshot with { Overlays = overlays });
    }

    /// <summary>
    /// 单独切换模拟状态。
    /// </summary>
    /// <param name="state">目标模拟状态。</param>
    public void SetSimulationState(SimulationState state)
    {
        Apply(snapshot with { SimulationState = state });
    }

    /// <summary>
    /// 进入前台流程。
    /// </summary>
    public void EnterFrontEnd()
    {
        Apply(new ProjectStateSnapshot(GameFlowState.FrontEnd, OverlayFlags.None, SimulationState.Running));
    }

    /// <summary>
    /// 进入局内主流程。
    /// </summary>
    public void BeginGameplaySession()
    {
        Apply(new ProjectStateSnapshot(GameFlowState.SessionPlaying, OverlayFlags.None, SimulationState.Running));
    }

    /// <summary>
    /// 打开暂停菜单并暂停模拟。
    /// </summary>
    public void OpenPauseMenu()
    {
        Apply(snapshot with
        {
            Overlays = snapshot.Overlays | OverlayFlags.PauseMenu,
            SimulationState = SimulationState.Suspended
        });
    }

    /// <summary>
    /// 关闭暂停菜单并恢复模拟。
    /// </summary>
    public void ClosePauseMenu()
    {
        Apply(snapshot with
        {
            Overlays = snapshot.Overlays & ~OverlayFlags.PauseMenu,
            SimulationState = SimulationState.Running
        });
    }

    private static void ValidateSnapshot(ProjectStateSnapshot value)
    {
        if (!IsSingleFlowState(value.FlowState))
        {
            throw new ArgumentException($"ProjectStateSnapshot.FlowState 必须是单一流程状态: {value.FlowState}");
        }

        if (!IsSingleSimulationState(value.SimulationState))
        {
            throw new ArgumentException($"ProjectStateSnapshot.SimulationState 必须是单一模拟状态: {value.SimulationState}");
        }
    }

    private static bool IsSingleFlowState(GameFlowState state)
    {
        var value = (ushort)state;
        return value != 0 && (value & ~KnownFlowStateBits) == 0 && (value & (value - 1)) == 0;
    }

    private static bool IsSingleSimulationState(SimulationState state)
    {
        var value = (byte)state;
        return value != 0 && (value & ~KnownSimulationStateBits) == 0 && (value & (value - 1)) == 0;
    }
}
