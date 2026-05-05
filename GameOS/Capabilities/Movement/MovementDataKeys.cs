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
        _ = InputDirection;
        _ = AIMoveDirection;
        _ = AIMoveSpeedMultiplier;
        _ = CanMoveInput;
        _ = LastMoveDirection;
    }
}
