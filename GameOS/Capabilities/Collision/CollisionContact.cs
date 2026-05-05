using SkilmeAI.GameOS.Runtime.Entity;

namespace SkilmeAI.GameOS.Capabilities.Collision;

/// <summary>
/// 一次运行时碰撞接触。
/// </summary>
public readonly record struct CollisionContact(
    IEntity Source,
    IEntity Target,
    uint SourceLayer,
    uint TargetLayer);
