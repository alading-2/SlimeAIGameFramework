namespace SlimeAI.GameOS.Capabilities.Movement;

/// <summary>
/// 通用朝向控制参数；只描述视觉朝向，不参与位移和碰撞。
/// </summary>
public readonly record struct OrientationParams
{
    /// <summary>
    /// 带默认值的 struct 需要显式无参构造函数。
    /// </summary>
    public OrientationParams()
    {
    }

    /// <summary>朝向模式。</summary>
    public OrientationMode Mode { get; init; } = OrientationMode.FollowMovement;

    /// <summary>自转角速度，单位度/秒。</summary>
    public float AngularSpeed { get; init; } = 0f;

    /// <summary>自转角加速度，单位度/秒^2。</summary>
    public float AngularAcceleration { get; init; } = 0f;

    /// <summary>总自转角度，-1 表示不限制。</summary>
    public float TotalAngle { get; init; } = -1f;

    /// <summary>初始角度偏移，单位度。</summary>
    public float InitialAngle { get; init; } = 0f;

    /// <summary>是否按 Godot 2D 顺时针方向自转。</summary>
    public bool IsClockwise { get; init; } = true;
}
