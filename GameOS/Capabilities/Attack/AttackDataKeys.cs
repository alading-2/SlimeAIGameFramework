using SlimeAI.GameOS.Runtime.Data;

namespace SlimeAI.GameOS.Capabilities.Attack;

/// <summary>
/// Attack Capability 使用的运行时 DataKey。
/// </summary>
public static class AttackDataKeys
{
    /// <summary>普通攻击伤害。</summary>
    public static readonly DataKey<float> Damage = DataKey.Create<float>("Attack.Damage",
        defaultValue: 0f,
        category: AttackCategory.Basic,
        minValue: 0f,
        supportsModifiers: true);

    /// <summary>普通攻击距离。</summary>
    public static readonly DataKey<float> Range = DataKey.Create<float>("Attack.Range",
        defaultValue: 100f,
        category: AttackCategory.Basic,
        minValue: -1f,
        supportsModifiers: true);

    /// <summary>攻击间隔，单位秒。</summary>
    public static readonly DataKey<float> Interval = DataKey.Create<float>("Attack.Interval",
        defaultValue: 1f,
        category: AttackCategory.Basic,
        minValue: 0f,
        supportsModifiers: true);

    /// <summary>攻击前摇时间，单位秒。</summary>
    public static readonly DataKey<float> WindUpTime = DataKey.Create<float>("Attack.WindUpTime",
        defaultValue: 0f,
        category: AttackCategory.Basic,
        minValue: 0f,
        supportsModifiers: true);

    /// <summary>攻击后摇时间，单位秒。</summary>
    public static readonly DataKey<float> RecoveryTime = DataKey.Create<float>("Attack.RecoveryTime",
        defaultValue: 0f,
        category: AttackCategory.Basic,
        minValue: 0f,
        supportsModifiers: true);

    /// <summary>当前是否允许发起普通攻击。</summary>
    public static readonly DataKey<bool> CanAttack = DataKey.Create<bool>("Attack.CanAttack",
        defaultValue: true,
        category: AttackCategory.Runtime);

    /// <summary>普通攻击是否正在执行。</summary>
    public static readonly DataKey<bool> IsAttacking = DataKey.Create<bool>("Attack.IsAttacking",
        defaultValue: false,
        category: AttackCategory.Runtime);

    /// <summary>普通攻击当前状态。</summary>
    public static readonly DataKey<AttackState> State = DataKey.Create<AttackState>("Attack.State",
        defaultValue: AttackState.Idle,
        category: AttackCategory.Runtime);

    /// <summary>攻击冷却剩余时间，单位秒。</summary>
    public static readonly DataKey<float> CooldownRemaining = DataKey.Create<float>("Attack.CooldownRemaining",
        defaultValue: 0f,
        category: AttackCategory.Runtime,
        minValue: 0f);

    /// <summary>
    /// 显式触发静态 DataKey 注册。
    /// </summary>
    public static void RegisterAll()
    {
        _ = Damage;
        _ = Range;
        _ = Interval;
        _ = WindUpTime;
        _ = RecoveryTime;
        _ = CanAttack;
        _ = IsAttacking;
        _ = State;
        _ = CooldownRemaining;
    }
}
