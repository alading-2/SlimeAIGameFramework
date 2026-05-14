using System.Collections.Generic;
using SlimeAI.GameOS.Runtime.Entity;

namespace SlimeAI.GameOS.Capabilities.Movement;

/// <summary>
/// 一次移动的输入参数和运行时统计。
/// </summary>
public record struct MovementParams
{
    /// <summary>移动模式。</summary>
    public MoveMode Mode { get; init; } = MoveMode.None;

    /// <summary>输入方向。未指定目标点时按该方向移动。</summary>
    public Vector2Value Direction { get; init; } = Vector2Value.Zero;

    /// <summary>输入角度，单位度，0=右、90=下、正值顺时针。</summary>
    public float Angle { get; init; } = 0f;

    /// <summary>可选目标点。存在时 Charge 会实时朝目标点移动。</summary>
    public Vector2Value? TargetPosition { get; init; } = null;

    /// <summary>可选目标实体 Id，用于 AttachToHost 等运行时跟随语义。</summary>
    public EntityId? TargetEntityId { get; init; } = null;

    /// <summary>本次移动速度，0 表示回退到 DataKey MoveSpeed。</summary>
    public float Speed { get; init; } = 0f;

    /// <summary>最大持续时间，-1 表示不限制。</summary>
    public float MaxDuration { get; init; } = -1f;

    /// <summary>最大移动距离，-1 表示不限制。</summary>
    public float MaxDistance { get; init; } = -1f;

    /// <summary>到达目标点的停止阈值。</summary>
    public float ReachDistance { get; init; } = 1f;

    /// <summary>到达目标点时是否停止。</summary>
    public bool StopAtTarget { get; init; } = true;

    /// <summary>移动碰撞策略；null 表示本次移动不启用碰撞扫描。</summary>
    public MovementCollisionParams? CollisionParams { get; init; } = null;

    /// <summary>移动期间可选的视觉朝向参数；null 表示使用朝向组件默认设置。</summary>
    public OrientationParams? Orientation { get; init; } = null;

    /// <summary>是否把非零速度方向同步到 FacingDirection。</summary>
    public bool FaceVelocity { get; init; } = true;

    /// <summary>附着宿主时叠加的位置偏移。</summary>
    public Vector2Value AttachOffset { get; init; } = Vector2Value.Zero;

    /// <summary>Bezier 控制点；不含起点，最后一个点通常是终点。</summary>
    public IReadOnlyList<Vector2Value>? BezierPoints { get; init; } = null;

    /// <summary>Boomerang 到达去程终点后的停顿时间，单位秒。</summary>
    public float BoomerangPauseTime { get; init; } = 0f;

    /// <summary>Boomerang 返程速度倍率。</summary>
    public float BoomerangReturnSpeedMultiplier { get; init; } = 1f;

    /// <summary>Boomerang 弧高，单位像素；小于等于 0 时自动估算。</summary>
    public float BoomerangArcHeight { get; init; } = 0f;

    /// <summary>Boomerang 是否按顺时针方向弯曲。</summary>
    public bool BoomerangIsClockwise { get; init; } = false;

    /// <summary>Orbit 圆心。</summary>
    public Vector2Value OrbitCenter { get; init; } = Vector2Value.Zero;

    /// <summary>Orbit 初始角度，单位度；null 表示从当前位置推导。</summary>
    public float? OrbitInitAngle { get; init; } = null;

    /// <summary>Orbit 半径，单位像素。</summary>
    public float OrbitRadius { get; init; } = 0f;

    /// <summary>Orbit 角速度，单位度/秒。</summary>
    public float OrbitAngularSpeed { get; init; } = 0f;

    /// <summary>Orbit 角加速度，单位度/秒^2。</summary>
    public float OrbitAngularAcceleration { get; init; } = 0f;

    /// <summary>Orbit 总角度，单位度，-1 表示不限制。</summary>
    public float OrbitTotalAngle { get; init; } = -1f;

    /// <summary>Orbit 是否顺时针，Godot 2D 坐标下正角度为顺时针。</summary>
    public bool IsOrbitClockwise { get; init; } = true;

    /// <summary>SineWave 横向振幅，单位像素。</summary>
    public float WaveAmplitude { get; init; } = 50f;

    /// <summary>SineWave 频率，单位周期/秒。</summary>
    public float WaveFrequency { get; init; } = 2f;

    /// <summary>SineWave 初始相位，单位度。</summary>
    public float WavePhase { get; init; } = 0f;

    /// <summary>曲线是否自动朝屏幕上方弓起。</summary>
    public bool BowWorldUp { get; init; } = false;

    /// <summary>Parabola 顶点高度偏移，单位像素。</summary>
    public float ParabolaApexHeight { get; init; } = 0f;

    /// <summary>CircularArc 半径，单位像素。</summary>
    public float CircularArcRadius { get; init; } = 0f;

    /// <summary>CircularArc 是否按顺时针方向采样。</summary>
    public bool CircularArcClockwise { get; init; } = false;

    /// <summary>已持续时间，由 MovementSystem 写入。</summary>
    public float ElapsedTime { get; set; } = 0f;

    /// <summary>已移动距离，由 MovementSystem 写入。</summary>
    public float TraveledDistance { get; set; } = 0f;

    /// <summary>
    /// 带默认值的 struct 需要显式无参构造函数。
    /// </summary>
    public MovementParams()
    {
    }
}
