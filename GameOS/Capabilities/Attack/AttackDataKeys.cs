using SkilmeAI.GameOS.Runtime.Data;

namespace SkilmeAI.GameOS.Capabilities.Attack;

/// <summary>
/// Attack Capability 使用的运行时 DataKey。
/// </summary>
public static class AttackDataKeys
{
    /// <summary>普通攻击伤害。</summary>
    public static readonly DataMeta Damage = DataRegistry.Register(new DataMeta
    {
        Key = "Attack.Damage",
        DisplayName = "Attack Damage",
        Type = typeof(float),
        Category = AttackCategory.Basic,
        DefaultValue = 0f,
        MinValue = 0f,
        SupportModifiers = true,
        Description = "普通攻击造成的基础伤害。"
    });

    /// <summary>普通攻击距离。</summary>
    public static readonly DataMeta Range = DataRegistry.Register(new DataMeta
    {
        Key = "Attack.Range",
        DisplayName = "Attack Range",
        Type = typeof(float),
        Category = AttackCategory.Basic,
        DefaultValue = 100f,
        MinValue = -1f,
        SupportModifiers = true,
        Description = "普通攻击允许命中的距离。"
    });

    /// <summary>攻击间隔，单位秒。</summary>
    public static readonly DataMeta Interval = DataRegistry.Register(new DataMeta
    {
        Key = "Attack.Interval",
        DisplayName = "Attack Interval",
        Type = typeof(float),
        Category = AttackCategory.Basic,
        DefaultValue = 1f,
        MinValue = 0f,
        SupportModifiers = true,
        Description = "两次普通攻击之间的最小间隔。"
    });

    /// <summary>攻击前摇时间，单位秒。</summary>
    public static readonly DataMeta WindUpTime = DataRegistry.Register(new DataMeta
    {
        Key = "Attack.WindUpTime",
        DisplayName = "Attack Wind Up Time",
        Type = typeof(float),
        Category = AttackCategory.Basic,
        DefaultValue = 0f,
        MinValue = 0f,
        SupportModifiers = true,
        Description = "普通攻击从开始到伤害判定的延迟。"
    });

    /// <summary>攻击后摇时间，单位秒。</summary>
    public static readonly DataMeta RecoveryTime = DataRegistry.Register(new DataMeta
    {
        Key = "Attack.RecoveryTime",
        DisplayName = "Attack Recovery Time",
        Type = typeof(float),
        Category = AttackCategory.Basic,
        DefaultValue = 0f,
        MinValue = 0f,
        SupportModifiers = true,
        Description = "普通攻击命中后的动作占用时间。"
    });

    /// <summary>当前是否允许发起普通攻击。</summary>
    public static readonly DataMeta CanAttack = DataRegistry.Register(new DataMeta
    {
        Key = "Attack.CanAttack",
        DisplayName = "Can Attack",
        Type = typeof(bool),
        Category = AttackCategory.Runtime,
        DefaultValue = true,
        Description = "状态系统或外部逻辑写入的普通攻击门禁。"
    });

    /// <summary>普通攻击是否正在执行。</summary>
    public static readonly DataMeta IsAttacking = DataRegistry.Register(new DataMeta
    {
        Key = "Attack.IsAttacking",
        DisplayName = "Is Attacking",
        Type = typeof(bool),
        Category = AttackCategory.Runtime,
        DefaultValue = false,
        Description = "普通攻击前摇或后摇是否正在占用实体。"
    });

    /// <summary>普通攻击当前状态。</summary>
    public static readonly DataMeta State = DataRegistry.Register(new DataMeta
    {
        Key = "Attack.State",
        DisplayName = "Attack State",
        Type = typeof(AttackState),
        Category = AttackCategory.Runtime,
        DefaultValue = AttackState.Idle,
        Description = "普通攻击状态机运行态。"
    });

    /// <summary>攻击冷却剩余时间，单位秒。</summary>
    public static readonly DataMeta CooldownRemaining = DataRegistry.Register(new DataMeta
    {
        Key = "Attack.CooldownRemaining",
        DisplayName = "Attack Cooldown Remaining",
        Type = typeof(float),
        Category = AttackCategory.Runtime,
        DefaultValue = 0f,
        MinValue = 0f,
        Description = "普通攻击再次可用前的剩余时间。"
    });

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
