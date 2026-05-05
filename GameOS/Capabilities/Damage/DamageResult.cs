namespace SkilmeAI.GameOS.Capabilities.Damage;

/// <summary>
/// 单次伤害处理结果。
/// </summary>
public readonly record struct DamageResult(
    bool Applied,
    DamageInfo Info,
    float OldHp,
    float NewHp,
    string Message);
