namespace SkilmeAI.GameOS.GodotBridge;

/// <summary>
/// 旧项目 `AttackComponent` 场景名兼容入口。
/// </summary>
public partial class AttackComponent : GodotAttackComponent
{
    /// <summary>
    /// 创建旧 AttackComponent 兼容包装，默认保留注册前已写入的 Runtime Data。
    /// </summary>
    public AttackComponent()
    {
        PreferExistingDataOnRegister = true;
    }
}
