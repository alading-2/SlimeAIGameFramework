namespace SkilmeAI.GameOS.Capabilities.Movement;

/// <summary>
/// Movement 停止原因。
/// </summary>
public enum MovementStopReason
{
    /// <summary>策略自然完成。</summary>
    Completed = 0,

    /// <summary>达到最大持续时间。</summary>
    MaxDuration = 1,

    /// <summary>达到最大移动距离。</summary>
    MaxDistance = 2,

    /// <summary>被新移动替换。</summary>
    Replaced = 3,

    /// <summary>外部主动停止。</summary>
    External = 4,

    /// <summary>缺少策略或参数非法。</summary>
    Invalid = 5,

    /// <summary>运动碰撞策略请求停止。</summary>
    Collision = 6
}
