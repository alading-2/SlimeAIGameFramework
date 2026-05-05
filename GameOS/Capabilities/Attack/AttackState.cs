namespace SkilmeAI.GameOS.Capabilities.Attack;

/// <summary>
/// 普通攻击运行时状态。
/// </summary>
public enum AttackState
{
    /// <summary>空闲，可以接受新攻击请求。</summary>
    Idle = 0,

    /// <summary>攻击前摇阶段。</summary>
    WindUp = 1,

    /// <summary>攻击后摇阶段。</summary>
    Recovery = 2
}
