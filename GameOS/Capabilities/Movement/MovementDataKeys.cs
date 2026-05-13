using SkilmeAI.GameOS.Runtime.Data;

namespace SkilmeAI.GameOS.Capabilities.Movement;

/// <summary>
/// Movement Capability 使用的运行时 DataKey。
/// </summary>
public static class MovementDataKeys
{
    /// <summary>实体当前位置。</summary>
    public static readonly DataKey<Vector2Value> Position = DataKey.Create<Vector2Value>("Movement.Position",
        defaultValue: Vector2Value.Zero,
        category: MovementCategory.Runtime);

    /// <summary>实体当前速度，单位为像素/秒。</summary>
    public static readonly DataKey<Vector2Value> Velocity = DataKey.Create<Vector2Value>("Movement.Velocity",
        defaultValue: Vector2Value.Zero,
        category: MovementCategory.Runtime);

    /// <summary>实体当前面向方向。</summary>
    public static readonly DataKey<Vector2Value> FacingDirection = DataKey.Create<Vector2Value>("Movement.FacingDirection",
        defaultValue: Vector2Value.Zero,
        category: MovementCategory.Runtime);

    /// <summary>实体是否正在被 MovementSystem 驱动。</summary>
    public static readonly DataKey<bool> IsMoving = DataKey.Create<bool>("Movement.IsMoving",
        defaultValue: false,
        category: MovementCategory.Runtime);

    /// <summary>速度插值系数，越大响应越快；0 或不存在表示瞬时速度（无平滑）。</summary>
    public static readonly DataKey<float> Acceleration = DataKey.Create<float>("Movement.Acceleration",
        defaultValue: 0f,
        category: MovementCategory.Config,
        minValue: 0f);

    /// <summary>默认移动速度，单位为像素/秒。</summary>
    public static readonly DataKey<float> MoveSpeed = DataKey.Create<float>("Movement.MoveSpeed",
        defaultValue: 0f,
        category: MovementCategory.Config,
        minValue: 0f,
        supportsModifiers: true);

    /// <summary>玩家输入层写入的移动方向。</summary>
    public static readonly DataKey<Vector2Value> InputDirection = DataKey.Create<Vector2Value>("Movement.InputDirection",
        defaultValue: Vector2Value.Zero,
        category: MovementCategory.Runtime);

    /// <summary>AI 层写入的移动方向。</summary>
    public static readonly DataKey<Vector2Value> AIMoveDirection = DataKey.Create<Vector2Value>("Movement.AIMoveDirection",
        defaultValue: Vector2Value.Zero,
        category: MovementCategory.Runtime);

    /// <summary>AI 层写入的移动速度倍率。</summary>
    public static readonly DataKey<float> AIMoveSpeedMultiplier = DataKey.Create<float>("Movement.AIMoveSpeedMultiplier",
        defaultValue: 1f,
        category: MovementCategory.Runtime,
        minValue: 0f);

    /// <summary>是否允许输入/AI 常驻移动。</summary>
    public static readonly DataKey<bool> CanMoveInput = DataKey.Create<bool>("Movement.CanMoveInput",
        defaultValue: true,
        category: MovementCategory.Runtime);

    /// <summary>最近一次非零主动移动方向。</summary>
    public static readonly DataKey<Vector2Value> LastMoveDirection = DataKey.Create<Vector2Value>("Movement.LastMoveDirection",
        defaultValue: new Vector2Value(1f, 0f),
        category: MovementCategory.Runtime);

    /// <summary>技能或投射物 authoring 指定的移动模式。</summary>
    public static readonly DataKey<MoveMode> HandlerMoveMode = DataKey.Create<MoveMode>("Movement.Handler.MoveMode",
        defaultValue: MoveMode.None,
        category: MovementCategory.Config);

    /// <summary>handler authoring 指定的最大移动距离，-1 表示不限制。</summary>
    public static readonly DataKey<float> HandlerMaxDistance = DataKey.Create<float>("Movement.Handler.MaxDistance",
        defaultValue: -1f,
        category: MovementCategory.Config,
        minValue: -1f);

    /// <summary>handler authoring 指定的最小飞行时长。</summary>
    public static readonly DataKey<float> HandlerMinTravelDuration = DataKey.Create<float>("Movement.Handler.MinTravelDuration",
        defaultValue: 0f,
        category: MovementCategory.Config,
        minValue: 0f);

    /// <summary>handler authoring 指定的最大飞行时长，-1 表示不限制。</summary>
    public static readonly DataKey<float> HandlerMaxTravelDuration = DataKey.Create<float>("Movement.Handler.MaxTravelDuration",
        defaultValue: -1f,
        category: MovementCategory.Config,
        minValue: -1f);

    /// <summary>handler authoring 指定的生成数量。</summary>
    public static readonly DataKey<int> HandlerProjectileCount = DataKey.Create<int>("Movement.Handler.ProjectileCount",
        defaultValue: 1,
        category: MovementCategory.Config,
        minValue: 0f);

    /// <summary>SineWave 横向振幅，单位像素。</summary>
    public static readonly DataKey<float> WaveAmplitude = DataKey.Create<float>("Movement.WaveAmplitude",
        defaultValue: 50f,
        category: MovementCategory.Config,
        minValue: 0f);

    /// <summary>SineWave 频率，单位周期/秒。</summary>
    public static readonly DataKey<float> WaveFrequency = DataKey.Create<float>("Movement.WaveFrequency",
        defaultValue: 2f,
        category: MovementCategory.Config,
        minValue: 0f);

    /// <summary>SineWave 初始相位，单位度。</summary>
    public static readonly DataKey<float> WavePhase = DataKey.Create<float>("Movement.WavePhase",
        defaultValue: 0f,
        category: MovementCategory.Config);

    /// <summary>Orbit 半径，单位像素。</summary>
    public static readonly DataKey<float> OrbitRadius = DataKey.Create<float>("Movement.OrbitRadius",
        defaultValue: 0f,
        category: MovementCategory.Config,
        minValue: 0f);

    /// <summary>Orbit 角速度，单位度/秒。</summary>
    public static readonly DataKey<float> OrbitAngularSpeed = DataKey.Create<float>("Movement.OrbitAngularSpeed",
        defaultValue: 0f,
        category: MovementCategory.Config,
        minValue: 0f);

    /// <summary>Orbit 角加速度，单位度/秒^2。</summary>
    public static readonly DataKey<float> OrbitAngularAcceleration = DataKey.Create<float>("Movement.OrbitAngularAcceleration",
        defaultValue: 0f,
        category: MovementCategory.Config);

    /// <summary>Orbit 总角度，单位度，-1 表示不限制。</summary>
    public static readonly DataKey<float> OrbitTotalAngle = DataKey.Create<float>("Movement.OrbitTotalAngle",
        defaultValue: -1f,
        category: MovementCategory.Config,
        minValue: -1f);

    /// <summary>Orbit 是否顺时针。</summary>
    public static readonly DataKey<bool> IsOrbitClockwise = DataKey.Create<bool>("Movement.IsOrbitClockwise",
        defaultValue: true,
        category: MovementCategory.Config);

    /// <summary>Boomerang 到达去程终点后的停顿时间。</summary>
    public static readonly DataKey<float> BoomerangPauseTime = DataKey.Create<float>("Movement.BoomerangPauseTime",
        defaultValue: 0f,
        category: MovementCategory.Config,
        minValue: 0f);

    /// <summary>Boomerang 返程速度倍率。</summary>
    public static readonly DataKey<float> BoomerangReturnSpeedMultiplier = DataKey.Create<float>("Movement.BoomerangReturnSpeedMultiplier",
        defaultValue: 1f,
        category: MovementCategory.Config,
        minValue: 0f);

    /// <summary>Boomerang 弧高，单位像素；0 表示策略自动估算。</summary>
    public static readonly DataKey<float> BoomerangArcHeight = DataKey.Create<float>("Movement.BoomerangArcHeight",
        defaultValue: 0f,
        category: MovementCategory.Config,
        minValue: 0f);

    /// <summary>Boomerang 是否按顺时针方向弯曲。</summary>
    public static readonly DataKey<bool> BoomerangIsClockwise = DataKey.Create<bool>("Movement.BoomerangIsClockwise",
        defaultValue: false,
        category: MovementCategory.Config);

    /// <summary>Bezier 模板阶数。</summary>
    public static readonly DataKey<int> BezierDegree = DataKey.Create<int>("Movement.BezierDegree",
        defaultValue: 2,
        category: MovementCategory.Config,
        minValue: 1f);

    /// <summary>Bezier 模板模式名称。</summary>
    public static readonly DataKey<string> BezierPattern = DataKey.Create<string>("Movement.BezierPattern",
        defaultValue: string.Empty,
        category: MovementCategory.Config);

    /// <summary>Parabola 顶点高度偏移，单位像素。</summary>
    public static readonly DataKey<float> ParabolaApexHeight = DataKey.Create<float>("Movement.ParabolaApexHeight",
        defaultValue: 0f,
        category: MovementCategory.Config);

    /// <summary>CircularArc 半径倍率，用于按飞行距离推导半径。</summary>
    public static readonly DataKey<float> CircularArcRadiusScale = DataKey.Create<float>("Movement.CircularArcRadiusScale",
        defaultValue: 0f,
        category: MovementCategory.Config,
        minValue: 0f);

    /// <summary>CircularArc 半径下限偏移，用于保证半径大于半弦长。</summary>
    public static readonly DataKey<float> CircularArcRadiusMinOffset = DataKey.Create<float>("Movement.CircularArcRadiusMinOffset",
        defaultValue: 0f,
        category: MovementCategory.Config,
        minValue: 0f);

    /// <summary>CircularArc 是否按顺时针方向采样。</summary>
    public static readonly DataKey<bool> CircularArcClockwise = DataKey.Create<bool>("Movement.CircularArcClockwise",
        defaultValue: false,
        category: MovementCategory.Config);

    /// <summary>曲线是否固定朝世界上方弓起。</summary>
    public static readonly DataKey<bool> BowWorldUp = DataKey.Create<bool>("Movement.BowWorldUp",
        defaultValue: false,
        category: MovementCategory.Config);

    /// <summary>
    /// 显式触发静态 DataKey 注册。
    /// </summary>
    public static void RegisterAll()
    {
        _ = Position;
        _ = Velocity;
        _ = FacingDirection;
        _ = IsMoving;
        _ = MoveSpeed;
        _ = Acceleration;
        _ = InputDirection;
        _ = AIMoveDirection;
        _ = AIMoveSpeedMultiplier;
        _ = CanMoveInput;
        _ = LastMoveDirection;
        _ = HandlerMoveMode;
        _ = HandlerMaxDistance;
        _ = HandlerMinTravelDuration;
        _ = HandlerMaxTravelDuration;
        _ = HandlerProjectileCount;
        _ = WaveAmplitude;
        _ = WaveFrequency;
        _ = WavePhase;
        _ = OrbitRadius;
        _ = OrbitAngularSpeed;
        _ = OrbitAngularAcceleration;
        _ = OrbitTotalAngle;
        _ = IsOrbitClockwise;
        _ = BoomerangPauseTime;
        _ = BoomerangReturnSpeedMultiplier;
        _ = BoomerangArcHeight;
        _ = BoomerangIsClockwise;
        _ = BezierDegree;
        _ = BezierPattern;
        _ = ParabolaApexHeight;
        _ = CircularArcRadiusScale;
        _ = CircularArcRadiusMinOffset;
        _ = CircularArcClockwise;
        _ = BowWorldUp;
    }
}
