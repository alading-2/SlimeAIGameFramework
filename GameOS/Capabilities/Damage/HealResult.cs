namespace SkilmeAI.GameOS.Capabilities.Damage;

/// <summary>
/// 单次治疗处理结果。
/// </summary>
public readonly record struct HealResult(
    bool Applied,
    HealInfo Info,
    float OldHp,
    float NewHp,
    string Message);
