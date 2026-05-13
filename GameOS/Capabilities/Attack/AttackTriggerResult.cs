namespace SlimeAI.GameOS.Capabilities.Attack;

/// <summary>
/// 普通攻击请求结果。
/// </summary>
public enum AttackTriggerResult
{
    /// <summary>攻击请求已接受。</summary>
    Success = 0,

    /// <summary>没有有效目标。</summary>
    FailNoTarget = 1,

    /// <summary>攻击者已死亡。</summary>
    FailSelfDead = 2,

    /// <summary>目标已死亡。</summary>
    FailTargetDead = 3,

    /// <summary>攻击者当前不可攻击。</summary>
    FailDisabled = 4,

    /// <summary>攻击仍在冷却。</summary>
    FailCooldown = 5,

    /// <summary>攻击状态正在占用中。</summary>
    FailActive = 6,

    /// <summary>目标超出攻击距离。</summary>
    FailOutOfRange = 7
}
