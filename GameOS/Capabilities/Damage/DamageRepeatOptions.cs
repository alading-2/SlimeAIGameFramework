namespace SkilmeAI.GameOS.Capabilities.Damage;

/// <summary>
/// DamageTool 周期伤害参数。
/// </summary>
public readonly record struct DamageRepeatOptions
{
    /// <summary>两次伤害间隔秒数。</summary>
    public float Interval { get; init; } = 1f;

    /// <summary>总触发次数，-1 表示无限循环。</summary>
    public int RepeatCount { get; init; } = -1;

    /// <summary>是否立即结算第一次伤害。</summary>
    public bool ApplyImmediately { get; init; } = true;

    /// <summary>计时器标签。</summary>
    public string TimerTag { get; init; } = string.Empty;

    /// <summary>是否使用非缩放时间。</summary>
    public bool UseUnscaledTime { get; init; } = false;

    /// <summary>
    /// 创建周期伤害参数。
    /// </summary>
    /// <param name="interval">两次伤害间隔秒数。</param>
    public DamageRepeatOptions(float interval)
    {
        Interval = interval;
    }
}
