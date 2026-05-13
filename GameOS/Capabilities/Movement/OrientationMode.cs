namespace SlimeAI.GameOS.Capabilities.Movement;

/// <summary>
/// Movement 朝向输出模式。
/// </summary>
public enum OrientationMode
{
    /// <summary>跟随 MovementDataKeys.FacingDirection。</summary>
    FollowMovement = 0,

    /// <summary>忽略移动朝向，仅按角速度自转。</summary>
    SpinOnly = 1,

    /// <summary>跟随移动朝向，并叠加自转偏移。</summary>
    FollowMovementAndSpin = 2
}
