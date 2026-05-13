namespace SlimeAI.GameOS.Capabilities.Movement;

/// <summary>
/// Movement Strategy 的单帧更新结果。
/// </summary>
public readonly record struct MovementUpdateResult(
    bool IsCompleted,
    float Distance,
    bool HasFacingDirection,
    Vector2Value FacingDirection)
{
    /// <summary>继续移动。</summary>
    public static MovementUpdateResult Continue(float distance = 0f)
    {
        return new MovementUpdateResult(false, distance, false, Vector2Value.Zero);
    }

    /// <summary>继续移动，并显式提供朝向。</summary>
    public static MovementUpdateResult Continue(float distance, Vector2Value facingDirection)
    {
        return new MovementUpdateResult(false, distance, true, facingDirection);
    }

    /// <summary>本次移动完成。</summary>
    public static MovementUpdateResult Complete()
    {
        return new MovementUpdateResult(true, 0f, false, Vector2Value.Zero);
    }
}
