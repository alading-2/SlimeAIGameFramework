using SkilmeAI.GameOS.Runtime.Entity;

namespace SkilmeAI.GameOS.Capabilities.Effect;

/// <summary>
/// 效果生成结果。
/// </summary>
public readonly record struct EffectSpawnResult(IEntity Effect, bool Created);
