namespace SlimeAI.GameOS.GodotBridge;

/// <summary>
/// Godot 单位组合配置，描述一个 Runtime Entity 需要挂载哪些通用 Adapter。
/// </summary>
public sealed class GodotUnitCompositionProfile
{
    /// <summary>是否从 `Unit.VisualScenePath` 加载视觉节点。</summary>
    public bool IncludeVisual { get; init; } = true;

    /// <summary>是否挂载单位动画 Adapter。</summary>
    public bool IncludeAnimation { get; init; } = true;

    /// <summary>是否挂载朝向 Adapter。</summary>
    public bool IncludeOrientation { get; init; } = true;

    /// <summary>是否挂载 AI Adapter。</summary>
    public bool IncludeAI { get; init; }

    /// <summary>是否挂载攻击 Adapter。</summary>
    public bool IncludeAttack { get; init; } = true;

    /// <summary>是否挂载 Hurtbox Adapter。</summary>
    public bool IncludeHurtbox { get; init; } = true;

    /// <summary>是否挂载接触伤害接收 Adapter。</summary>
    public bool IncludeContactDamageReceiver { get; init; }

    /// <summary>缺少 `Collision.Radius` 时使用的 Hurtbox 半径。</summary>
    public float FallbackHurtboxRadius { get; init; } = 16f;

    /// <summary>Hurtbox 节点名。</summary>
    public string HurtboxName { get; init; } = "Hurtbox";

    /// <summary>动画节点名。</summary>
    public string AnimationName { get; init; } = "UnitAnimation";

    /// <summary>朝向节点名。</summary>
    public string OrientationName { get; init; } = "Orientation";

    /// <summary>AI 节点名。</summary>
    public string AIName { get; init; } = "AI";

    /// <summary>攻击节点名。</summary>
    public string AttackName { get; init; } = "Attack";

    /// <summary>接触伤害节点名。</summary>
    public string ContactDamageName { get; init; } = "ContactDamage";
}
