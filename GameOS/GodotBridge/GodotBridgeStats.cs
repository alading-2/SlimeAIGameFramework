namespace SlimeAI.GameOS.GodotBridge;

/// <summary>
/// GodotBridge 当前注册状态快照。
/// </summary>
public readonly record struct GodotBridgeStats(int TotalNodes, int EntityNodes, int ComponentNodes);
