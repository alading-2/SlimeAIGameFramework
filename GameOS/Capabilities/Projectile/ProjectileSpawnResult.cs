using SkilmeAI.GameOS.Runtime.Entity;

namespace SkilmeAI.GameOS.Capabilities.Projectile;

/// <summary>
/// 投射物生成结果。
/// </summary>
public readonly record struct ProjectileSpawnResult(IEntity Projectile, bool Created);
