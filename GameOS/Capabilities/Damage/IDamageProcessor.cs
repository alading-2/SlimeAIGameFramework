namespace SlimeAI.GameOS.Capabilities.Damage;

/// <summary>
/// 伤害处理器接口。
/// </summary>
public interface IDamageProcessor
{
    /// <summary>执行优先级，数值越小越早执行。</summary>
    int Priority { get; }

    /// <summary>
    /// 处理伤害上下文。
    /// </summary>
    /// <param name="info">伤害上下文。</param>
    void Process(DamageInfo info);
}
