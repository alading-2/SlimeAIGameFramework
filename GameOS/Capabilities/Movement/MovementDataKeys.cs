using SkilmeAI.GameOS.Runtime.Data;

namespace SkilmeAI.GameOS.Capabilities.Movement;

/// <summary>
/// Movement Capability 使用的运行时 DataKey。
/// </summary>
public static class MovementDataKeys
{
    /// <summary>实体当前位置。</summary>
    public static readonly DataMeta Position = DataRegistry.Register(new DataMeta
    {
        Key = "Movement.Position",
        DisplayName = "Position",
        Type = typeof(Vector2Value),
        Category = MovementCategory.Runtime,
        DefaultValue = Vector2Value.Zero,
        Description = "Movement Capability 维护的运行时位置。"
    });

    /// <summary>实体当前速度，单位为像素/秒。</summary>
    public static readonly DataMeta Velocity = DataRegistry.Register(new DataMeta
    {
        Key = "Movement.Velocity",
        DisplayName = "Velocity",
        Type = typeof(Vector2Value),
        Category = MovementCategory.Runtime,
        DefaultValue = Vector2Value.Zero,
        Description = "Movement Strategy 输出的本帧速度。"
    });

    /// <summary>实体当前面向方向。</summary>
    public static readonly DataMeta FacingDirection = DataRegistry.Register(new DataMeta
    {
        Key = "Movement.FacingDirection",
        DisplayName = "Facing Direction",
        Type = typeof(Vector2Value),
        Category = MovementCategory.Runtime,
        DefaultValue = Vector2Value.Zero,
        Description = "Movement 输出给视觉或朝向系统的方向意图。"
    });

    /// <summary>实体是否正在被 MovementSystem 驱动。</summary>
    public static readonly DataMeta IsMoving = DataRegistry.Register(new DataMeta
    {
        Key = "Movement.IsMoving",
        DisplayName = "Is Moving",
        Type = typeof(bool),
        Category = MovementCategory.Runtime,
        DefaultValue = false,
        Description = "MovementSystem 当前是否持有该实体的活动移动状态。"
    });

    /// <summary>速度插值系数，越大响应越快；0 或不存在表示瞬时速度（无平滑）。</summary>
    public static readonly DataMeta Acceleration = DataRegistry.Register(new DataMeta
    {
        Key = "Movement.Acceleration",
        DisplayName = "Acceleration",
        Type = typeof(float),
        Category = MovementCategory.Config,
        DefaultValue = 0f,
        MinValue = 0f,
        Description = "PlayerInput 策略的速度插值系数；0 表示无平滑直接到达目标速度。"
    });

    /// <summary>默认移动速度，单位为像素/秒。</summary>
    public static readonly DataMeta MoveSpeed = DataRegistry.Register(new DataMeta
    {
        Key = "Movement.MoveSpeed",
        DisplayName = "Move Speed",
        Type = typeof(float),
        Category = MovementCategory.Config,
        DefaultValue = 0f,
        MinValue = 0f,
        SupportModifiers = true,
        Description = "未在 MovementParams 指定速度时使用的默认移动速度。"
    });

    /// <summary>玩家输入层写入的移动方向。</summary>
    public static readonly DataMeta InputDirection = DataRegistry.Register(new DataMeta
    {
        Key = "Movement.InputDirection",
        DisplayName = "Input Direction",
        Type = typeof(Vector2Value),
        Category = MovementCategory.Runtime,
        DefaultValue = Vector2Value.Zero,
        Description = "PlayerInput 策略消费的外部输入方向。"
    });

    /// <summary>AI 层写入的移动方向。</summary>
    public static readonly DataMeta AIMoveDirection = DataRegistry.Register(new DataMeta
    {
        Key = "Movement.AIMoveDirection",
        DisplayName = "AI Move Direction",
        Type = typeof(Vector2Value),
        Category = MovementCategory.Runtime,
        DefaultValue = Vector2Value.Zero,
        Description = "AIControlled 策略消费的 AI 移动方向。"
    });

    /// <summary>AI 层写入的移动速度倍率。</summary>
    public static readonly DataMeta AIMoveSpeedMultiplier = DataRegistry.Register(new DataMeta
    {
        Key = "Movement.AIMoveSpeedMultiplier",
        DisplayName = "AI Move Speed Multiplier",
        Type = typeof(float),
        Category = MovementCategory.Runtime,
        DefaultValue = 1f,
        MinValue = 0f,
        Description = "AIControlled 策略消费的速度倍率。"
    });

    /// <summary>是否允许输入/AI 常驻移动。</summary>
    public static readonly DataMeta CanMoveInput = DataRegistry.Register(new DataMeta
    {
        Key = "Movement.CanMoveInput",
        DisplayName = "Can Move Input",
        Type = typeof(bool),
        Category = MovementCategory.Runtime,
        DefaultValue = true,
        Description = "PlayerInput 和 AIControlled 策略的移动门控。"
    });

    /// <summary>最近一次非零主动移动方向。</summary>
    public static readonly DataMeta LastMoveDirection = DataRegistry.Register(new DataMeta
    {
        Key = "Movement.LastMoveDirection",
        DisplayName = "Last Move Direction",
        Type = typeof(Vector2Value),
        Category = MovementCategory.Runtime,
        DefaultValue = new Vector2Value(1f, 0f),
        Description = "输入驱动移动记录的最后一次非零方向。"
    });

    /// <summary>技能或投射物 authoring 指定的移动模式。</summary>
    public static readonly DataMeta HandlerMoveMode = DataRegistry.Register(new DataMeta
    {
        Key = "Movement.Handler.MoveMode",
        DisplayName = "Handler Move Mode",
        Type = typeof(MoveMode),
        Category = MovementCategory.Config,
        DefaultValue = MoveMode.None,
        Description = "Ability / Feature handler 从 DataOS 读取后映射到 MovementParams.Mode 的移动模式。"
    });

    /// <summary>handler authoring 指定的最大移动距离，-1 表示不限制。</summary>
    public static readonly DataMeta HandlerMaxDistance = DataRegistry.Register(new DataMeta
    {
        Key = "Movement.Handler.MaxDistance",
        DisplayName = "Handler Max Distance",
        Type = typeof(float),
        Category = MovementCategory.Config,
        DefaultValue = -1f,
        MinValue = -1f,
        Description = "Ability / Feature handler 映射到 MovementParams.MaxDistance 的最大移动距离，-1 表示不限制。"
    });

    /// <summary>handler authoring 指定的最小飞行时长。</summary>
    public static readonly DataMeta HandlerMinTravelDuration = DataRegistry.Register(new DataMeta
    {
        Key = "Movement.Handler.MinTravelDuration",
        DisplayName = "Handler Min Travel Duration",
        Type = typeof(float),
        Category = MovementCategory.Config,
        DefaultValue = 0f,
        MinValue = 0f,
        Description = "Ability / Feature handler 使用速度推导飞行时长时的最小时长。"
    });

    /// <summary>handler authoring 指定的最大飞行时长，-1 表示不限制。</summary>
    public static readonly DataMeta HandlerMaxTravelDuration = DataRegistry.Register(new DataMeta
    {
        Key = "Movement.Handler.MaxTravelDuration",
        DisplayName = "Handler Max Travel Duration",
        Type = typeof(float),
        Category = MovementCategory.Config,
        DefaultValue = -1f,
        MinValue = -1f,
        Description = "Ability / Feature handler 使用速度推导飞行时长时的最大时长，-1 表示不限制。"
    });

    /// <summary>handler authoring 指定的生成数量。</summary>
    public static readonly DataMeta HandlerProjectileCount = DataRegistry.Register(new DataMeta
    {
        Key = "Movement.Handler.ProjectileCount",
        DisplayName = "Handler Projectile Count",
        Type = typeof(int),
        Category = MovementCategory.Config,
        DefaultValue = 1,
        MinValue = 0f,
        Description = "Ability / Feature handler 需要生成多发移动实体时使用的数量。"
    });

    /// <summary>SineWave 横向振幅，单位像素。</summary>
    public static readonly DataMeta WaveAmplitude = DataRegistry.Register(new DataMeta
    {
        Key = "Movement.WaveAmplitude",
        DisplayName = "Wave Amplitude",
        Type = typeof(float),
        Category = MovementCategory.Config,
        DefaultValue = 50f,
        MinValue = 0f,
        Description = "SineWave 策略横向振幅，映射到 MovementParams.WaveAmplitude。"
    });

    /// <summary>SineWave 频率，单位周期/秒。</summary>
    public static readonly DataMeta WaveFrequency = DataRegistry.Register(new DataMeta
    {
        Key = "Movement.WaveFrequency",
        DisplayName = "Wave Frequency",
        Type = typeof(float),
        Category = MovementCategory.Config,
        DefaultValue = 2f,
        MinValue = 0f,
        Description = "SineWave 策略频率，映射到 MovementParams.WaveFrequency。"
    });

    /// <summary>SineWave 初始相位，单位度。</summary>
    public static readonly DataMeta WavePhase = DataRegistry.Register(new DataMeta
    {
        Key = "Movement.WavePhase",
        DisplayName = "Wave Phase",
        Type = typeof(float),
        Category = MovementCategory.Config,
        DefaultValue = 0f,
        Description = "SineWave 策略初始相位，映射到 MovementParams.WavePhase。"
    });

    /// <summary>Orbit 半径，单位像素。</summary>
    public static readonly DataMeta OrbitRadius = DataRegistry.Register(new DataMeta
    {
        Key = "Movement.OrbitRadius",
        DisplayName = "Orbit Radius",
        Type = typeof(float),
        Category = MovementCategory.Config,
        DefaultValue = 0f,
        MinValue = 0f,
        Description = "Orbit 策略半径，映射到 MovementParams.OrbitRadius。"
    });

    /// <summary>Orbit 角速度，单位度/秒。</summary>
    public static readonly DataMeta OrbitAngularSpeed = DataRegistry.Register(new DataMeta
    {
        Key = "Movement.OrbitAngularSpeed",
        DisplayName = "Orbit Angular Speed",
        Type = typeof(float),
        Category = MovementCategory.Config,
        DefaultValue = 0f,
        MinValue = 0f,
        Description = "Orbit 策略角速度，映射到 MovementParams.OrbitAngularSpeed。"
    });

    /// <summary>Orbit 角加速度，单位度/秒^2。</summary>
    public static readonly DataMeta OrbitAngularAcceleration = DataRegistry.Register(new DataMeta
    {
        Key = "Movement.OrbitAngularAcceleration",
        DisplayName = "Orbit Angular Acceleration",
        Type = typeof(float),
        Category = MovementCategory.Config,
        DefaultValue = 0f,
        Description = "Orbit 策略角加速度，映射到 MovementParams.OrbitAngularAcceleration。"
    });

    /// <summary>Orbit 总角度，单位度，-1 表示不限制。</summary>
    public static readonly DataMeta OrbitTotalAngle = DataRegistry.Register(new DataMeta
    {
        Key = "Movement.OrbitTotalAngle",
        DisplayName = "Orbit Total Angle",
        Type = typeof(float),
        Category = MovementCategory.Config,
        DefaultValue = -1f,
        MinValue = -1f,
        Description = "Orbit 策略总角度，映射到 MovementParams.OrbitTotalAngle，-1 表示不限制。"
    });

    /// <summary>Orbit 是否顺时针。</summary>
    public static readonly DataMeta IsOrbitClockwise = DataRegistry.Register(new DataMeta
    {
        Key = "Movement.IsOrbitClockwise",
        DisplayName = "Is Orbit Clockwise",
        Type = typeof(bool),
        Category = MovementCategory.Config,
        DefaultValue = true,
        Description = "Orbit 策略顺时针开关，Godot 2D 坐标下正角度为顺时针。"
    });

    /// <summary>Boomerang 到达去程终点后的停顿时间。</summary>
    public static readonly DataMeta BoomerangPauseTime = DataRegistry.Register(new DataMeta
    {
        Key = "Movement.BoomerangPauseTime",
        DisplayName = "Boomerang Pause Time",
        Type = typeof(float),
        Category = MovementCategory.Config,
        DefaultValue = 0f,
        MinValue = 0f,
        Description = "Boomerang 策略顶点停顿时间，映射到 MovementParams.BoomerangPauseTime。"
    });

    /// <summary>Boomerang 返程速度倍率。</summary>
    public static readonly DataMeta BoomerangReturnSpeedMultiplier = DataRegistry.Register(new DataMeta
    {
        Key = "Movement.BoomerangReturnSpeedMultiplier",
        DisplayName = "Boomerang Return Speed Multiplier",
        Type = typeof(float),
        Category = MovementCategory.Config,
        DefaultValue = 1f,
        MinValue = 0f,
        Description = "Boomerang 策略返程速度倍率，映射到 MovementParams.BoomerangReturnSpeedMultiplier。"
    });

    /// <summary>Boomerang 弧高，单位像素；0 表示策略自动估算。</summary>
    public static readonly DataMeta BoomerangArcHeight = DataRegistry.Register(new DataMeta
    {
        Key = "Movement.BoomerangArcHeight",
        DisplayName = "Boomerang Arc Height",
        Type = typeof(float),
        Category = MovementCategory.Config,
        DefaultValue = 0f,
        MinValue = 0f,
        Description = "Boomerang 策略弧高，映射到 MovementParams.BoomerangArcHeight；0 表示自动估算。"
    });

    /// <summary>Boomerang 是否按顺时针方向弯曲。</summary>
    public static readonly DataMeta BoomerangIsClockwise = DataRegistry.Register(new DataMeta
    {
        Key = "Movement.BoomerangIsClockwise",
        DisplayName = "Boomerang Is Clockwise",
        Type = typeof(bool),
        Category = MovementCategory.Config,
        DefaultValue = false,
        Description = "Boomerang 策略弯曲方向，映射到 MovementParams.BoomerangIsClockwise。"
    });

    /// <summary>Bezier 模板阶数。</summary>
    public static readonly DataMeta BezierDegree = DataRegistry.Register(new DataMeta
    {
        Key = "Movement.BezierDegree",
        DisplayName = "Bezier Degree",
        Type = typeof(int),
        Category = MovementCategory.Config,
        DefaultValue = 2,
        MinValue = 1f,
        Description = "Bezier handler 模板阶数；具体 handler 可据此生成 MovementParams.BezierPoints。"
    });

    /// <summary>Bezier 模板模式名称。</summary>
    public static readonly DataMeta BezierPattern = DataRegistry.Register(new DataMeta
    {
        Key = "Movement.BezierPattern",
        DisplayName = "Bezier Pattern",
        Type = typeof(string),
        Category = MovementCategory.Config,
        DefaultValue = string.Empty,
        Description = "Bezier handler 模板模式名称；具体 handler 可据此生成 MovementParams.BezierPoints。"
    });

    /// <summary>Parabola 顶点高度偏移，单位像素。</summary>
    public static readonly DataMeta ParabolaApexHeight = DataRegistry.Register(new DataMeta
    {
        Key = "Movement.ParabolaApexHeight",
        DisplayName = "Parabola Apex Height",
        Type = typeof(float),
        Category = MovementCategory.Config,
        DefaultValue = 0f,
        Description = "Parabola 策略顶点高度偏移，映射到 MovementParams.ParabolaApexHeight。"
    });

    /// <summary>CircularArc 半径倍率，用于按飞行距离推导半径。</summary>
    public static readonly DataMeta CircularArcRadiusScale = DataRegistry.Register(new DataMeta
    {
        Key = "Movement.CircularArcRadiusScale",
        DisplayName = "Circular Arc Radius Scale",
        Type = typeof(float),
        Category = MovementCategory.Config,
        DefaultValue = 0f,
        MinValue = 0f,
        Description = "CircularArc handler 按 travelDistance * scale 推导 MovementParams.CircularArcRadius 时使用的倍率。"
    });

    /// <summary>CircularArc 半径下限偏移，用于保证半径大于半弦长。</summary>
    public static readonly DataMeta CircularArcRadiusMinOffset = DataRegistry.Register(new DataMeta
    {
        Key = "Movement.CircularArcRadiusMinOffset",
        DisplayName = "Circular Arc Radius Min Offset",
        Type = typeof(float),
        Category = MovementCategory.Config,
        DefaultValue = 0f,
        MinValue = 0f,
        Description = "CircularArc handler 按 travelDistance * 0.5 + offset 推导半径下限时使用的偏移。"
    });

    /// <summary>CircularArc 是否按顺时针方向采样。</summary>
    public static readonly DataMeta CircularArcClockwise = DataRegistry.Register(new DataMeta
    {
        Key = "Movement.CircularArcClockwise",
        DisplayName = "Circular Arc Clockwise",
        Type = typeof(bool),
        Category = MovementCategory.Config,
        DefaultValue = false,
        Description = "CircularArc 策略采样方向，映射到 MovementParams.CircularArcClockwise。"
    });

    /// <summary>曲线是否固定朝世界上方弓起。</summary>
    public static readonly DataMeta BowWorldUp = DataRegistry.Register(new DataMeta
    {
        Key = "Movement.BowWorldUp",
        DisplayName = "Bow World Up",
        Type = typeof(bool),
        Category = MovementCategory.Config,
        DefaultValue = false,
        Description = "曲线策略是否固定朝世界上方弓起，映射到 MovementParams.BowWorldUp。"
    });

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
