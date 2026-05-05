namespace SkilmeAI.GameOS.Capabilities.Collision;

/// <summary>
/// CollisionSystem 的基础过滤策略。
/// </summary>
public readonly record struct CollisionFilterPolicy(
    bool RequireLayerMaskMatch = true,
    bool IgnoreSameTeam = false,
    bool IgnoreSelf = true)
{
    /// <summary>默认策略：需要 layer/mask 命中，忽略自身。</summary>
    public static readonly CollisionFilterPolicy Default = new();
}
