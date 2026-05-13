using SlimeAI.GameOS.Runtime.Entity;

namespace SlimeAI.GameOS.Capabilities.Effect;

/// <summary>
/// 效果生成结果。
/// </summary>
public readonly record struct EffectSpawnResult(IEntity Effect, bool Created);
