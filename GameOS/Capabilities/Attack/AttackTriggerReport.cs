using SkilmeAI.GameOS.Capabilities.Damage;

namespace SkilmeAI.GameOS.Capabilities.Attack;

/// <summary>
/// 普通攻击请求报告。
/// </summary>
/// <param name="Result">请求结果。</param>
/// <param name="DamageResult">本次伤害结果；未进入伤害结算时为空。</param>
/// <param name="Message">调试信息。</param>
public readonly record struct AttackTriggerReport(
    AttackTriggerResult Result,
    DamageResult? DamageResult,
    string Message);
