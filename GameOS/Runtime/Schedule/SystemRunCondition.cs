namespace SkilmeAI.GameOS.Runtime.Schedule;

/// <summary>
/// 系统运行条件，用于把业务状态判断前置到调度层。
/// </summary>
public sealed class SystemRunCondition
{
    /// <summary>无条件运行。</summary>
    public static SystemRunCondition Always { get; } = new();

    /// <summary>
    /// 局内主玩法运行条件。
    /// </summary>
    public static SystemRunCondition GameplayRunning()
    {
        return new SystemRunCondition
        {
            AllowedFlowStates = GameFlowState.Gameplay,
            BlockedOverlays = OverlayFlags.Blocking,
            AllowedSimulationStates = SimulationState.Running
        };
    }

    /// <summary>
    /// 覆盖层系统运行条件。
    /// </summary>
    /// <param name="requiredOverlays">要求存在的覆盖层；None 时使用默认阻塞覆盖层集合。</param>
    public static SystemRunCondition OverlayActive(OverlayFlags requiredOverlays = OverlayFlags.None)
    {
        return new SystemRunCondition
        {
            RequiredOverlays = requiredOverlays == OverlayFlags.None
                ? OverlayFlags.PauseMenu | OverlayFlags.ModalUi | OverlayFlags.Cutscene
                : requiredOverlays
        };
    }

    /// <summary>允许的流程状态，None 表示不限制。</summary>
    public GameFlowState AllowedFlowStates { get; init; } = GameFlowState.None;

    /// <summary>要求存在的覆盖层，None 表示不要求。</summary>
    public OverlayFlags RequiredOverlays { get; init; } = OverlayFlags.None;

    /// <summary>禁止存在的覆盖层，None 表示不屏蔽。</summary>
    public OverlayFlags BlockedOverlays { get; init; } = OverlayFlags.None;

    /// <summary>允许的模拟状态，None 表示不限制。</summary>
    public SimulationState AllowedSimulationStates { get; init; } = SimulationState.None;

    /// <summary>
    /// 判断快照是否满足运行条件。
    /// </summary>
    /// <param name="snapshot">当前项目状态快照。</param>
    public bool Evaluate(ProjectStateSnapshot snapshot)
    {
        return !GetBlockedReason(snapshot).IsBlocked;
    }

    /// <summary>
    /// 返回运行条件未通过的原因。
    /// </summary>
    /// <param name="snapshot">当前项目状态快照。</param>
    public (bool IsBlocked, string Reason) GetBlockedReason(ProjectStateSnapshot snapshot)
    {
        if (!MatchesFlowState(AllowedFlowStates, snapshot.FlowState))
        {
            return (true, $"FlowState={snapshot.FlowState} 不在允许范围 {AllowedFlowStates}");
        }

        if (!ContainsRequiredOverlays(RequiredOverlays, snapshot.Overlays))
        {
            return (true, $"Overlays={snapshot.Overlays} 缺少要求覆盖层 {RequiredOverlays}");
        }

        if (HasBlockedOverlay(BlockedOverlays, snapshot.Overlays))
        {
            return (true, $"Overlays={snapshot.Overlays} 命中禁止覆盖层 {BlockedOverlays}");
        }

        if (!MatchesSimulationState(AllowedSimulationStates, snapshot.SimulationState))
        {
            return (true, $"SimulationState={snapshot.SimulationState} 不在允许范围 {AllowedSimulationStates}");
        }

        return (false, string.Empty);
    }

    private static bool MatchesFlowState(GameFlowState allowedStates, GameFlowState currentState)
    {
        return allowedStates == GameFlowState.None || (allowedStates & currentState) != 0;
    }

    private static bool MatchesSimulationState(SimulationState allowedStates, SimulationState currentState)
    {
        return allowedStates == SimulationState.None || (allowedStates & currentState) != 0;
    }

    private static bool ContainsRequiredOverlays(OverlayFlags requiredOverlays, OverlayFlags currentOverlays)
    {
        return requiredOverlays == OverlayFlags.None || (currentOverlays & requiredOverlays) == requiredOverlays;
    }

    private static bool HasBlockedOverlay(OverlayFlags blockedOverlays, OverlayFlags currentOverlays)
    {
        return blockedOverlays != OverlayFlags.None && (currentOverlays & blockedOverlays) != 0;
    }
}
