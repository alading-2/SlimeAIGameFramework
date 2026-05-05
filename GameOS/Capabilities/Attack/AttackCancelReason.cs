namespace SkilmeAI.GameOS.Capabilities.Attack;

/// <summary>
/// 攻击流程取消原因。
/// </summary>
public enum AttackCancelReason
{
    /// <summary>目标已死亡。</summary>
    TargetDead,

    /// <summary>目标离开攻击范围。</summary>
    TargetOutOfRange,

    /// <summary>目标引用失效。</summary>
    TargetInvalid,

    /// <summary>自身死亡。</summary>
    SelfDead,

    /// <summary>自身被控或不可攻击。</summary>
    SelfDisabled,

    /// <summary>外部主动取消。</summary>
    ExternalCancel,

    /// <summary>组件注销清理。</summary>
    ComponentCleanup
}
