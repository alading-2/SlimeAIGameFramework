using SlimeAI.GameOS.Runtime.Entity;

namespace SlimeAI.GameOS.Capabilities.Projectile;

/// <summary>
/// 投射物生成结果。
/// </summary>
public readonly record struct ProjectileSpawnResult(IEntity Projectile, bool Created);
