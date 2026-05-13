namespace SlimeAI.GameOS.Capabilities.Movement;

/// <summary>
/// Movement 碰撞目标匹配模式。
/// </summary>
public enum MovementCollisionTargetMatchMode
{
    /// <summary>接受所有通过 Collision 过滤的目标。</summary>
    Any = 0,

    /// <summary>只接受当前 MovementParams.TargetEntityId 指向的目标。</summary>
    TrackedTargetOnly = 1,

    /// <summary>只接受 MovementCollisionParams.SpecificTargetEntityId 指向的目标。</summary>
    SpecificEntity = 2
}
