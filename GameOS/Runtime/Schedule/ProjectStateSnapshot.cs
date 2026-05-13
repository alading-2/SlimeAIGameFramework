namespace SlimeAI.GameOS.Runtime.Schedule;

/// <summary>
/// 项目级状态快照，描述当前流程、覆盖层和模拟推进状态。
/// </summary>
public readonly record struct ProjectStateSnapshot(
    GameFlowState FlowState,
    OverlayFlags Overlays,
    SimulationState SimulationState);
