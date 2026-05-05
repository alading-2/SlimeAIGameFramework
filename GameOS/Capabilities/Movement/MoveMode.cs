namespace SkilmeAI.GameOS.Capabilities.Movement;

/// <summary>
/// Movement Capability 当前迁入的移动模式。
/// </summary>
public enum MoveMode
{
    /// <summary>无运动。</summary>
    None = 0,

    /// <summary>直线冲锋，可按方向、目标点、时长或距离约束停止。</summary>
    Charge = 1,

    /// <summary>围绕固定点做圆周运动。</summary>
    Orbit = 2,

    /// <summary>沿基础方向前进并叠加横向正弦偏移。</summary>
    SineWave = 3,

    /// <summary>从起点到终点的贝塞尔曲线轨迹。</summary>
    BezierCurve = 4,

    /// <summary>飞出、停顿、返回的回旋镖轨迹。</summary>
    Boomerang = 5,

    /// <summary>附着并跟随宿主实体。</summary>
    AttachToHost = 6,

    /// <summary>由外部玩家输入方向驱动的常驻移动。</summary>
    PlayerInput = 7,

    /// <summary>由 AI 写入方向和倍率驱动的常驻移动。</summary>
    AIControlled = 8,

    /// <summary>从起点到终点的抛物线轨迹。</summary>
    Parabola = 9,

    /// <summary>从起点到终点的圆弧轨迹。</summary>
    CircularArc = 10
}
