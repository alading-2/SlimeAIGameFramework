namespace SkilmeAI.GameOS.Capabilities.Ability;

/// <summary>
/// 技能触发结果。
/// </summary>
public enum AbilityTriggerResult
{
    /// <summary>触发成功。</summary>
    Success = 0,

    /// <summary>通用失败。</summary>
    Failed = 1,

    /// <summary>施法者已死亡。</summary>
    FailCasterDead = 2,

    /// <summary>技能未启用。</summary>
    FailDisabled = 3,

    /// <summary>技能正在执行。</summary>
    FailActive = 4,

    /// <summary>技能冷却中。</summary>
    FailCooldown = 5,

    /// <summary>技能没有可用充能。</summary>
    FailNoCharge = 6,

    /// <summary>缺少目标。</summary>
    FailNoTarget = 7
}
