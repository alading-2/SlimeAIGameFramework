namespace SkilmeAI.GameOS.Capabilities.Movement;

/// <summary>
/// Movement 朝向输出目标。
/// </summary>
public enum OrientationSink
{
    /// <summary>直接写 Godot Entity root 的 RotationDegrees。</summary>
    RootRotation = 0,

    /// <summary>通过 VisualRoot / AnimatedSprite2D 的 FlipH 表示左右朝向。</summary>
    VisualFlipX = 1
}
