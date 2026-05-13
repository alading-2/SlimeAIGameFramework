using SkilmeAI.GameOS.Capabilities.AI;
using SkilmeAI.GameOS.Capabilities.Ability;
using SkilmeAI.GameOS.Capabilities.Attack;
using SkilmeAI.GameOS.Capabilities.Collision;
using SkilmeAI.GameOS.Capabilities.Damage;
using SkilmeAI.GameOS.Capabilities.Effect;
using SkilmeAI.GameOS.Capabilities.Feature;
using SkilmeAI.GameOS.Capabilities.Movement;
using SkilmeAI.GameOS.Capabilities.Projectile;
using SkilmeAI.GameOS.Capabilities.Unit;
using SkilmeAI.GameOS.Runtime.Schedule;

namespace SkilmeAI.GameOS.Runtime.Data;

/// <summary>
/// 框架内置 DataKey 注册入口。
/// </summary>
public static class FrameworkDataKeys
{
    /// <summary>
    /// 显式触发框架首批 Runtime / Capability DataKey 静态初始化。
    /// </summary>
    public static void RegisterAll()
    {
        ScheduleDataKeys.RegisterAll();
        UnitDataKeys.RegisterAll();
        MovementDataKeys.RegisterAll();
        CollisionDataKeys.RegisterAll();
        DamageDataKeys.RegisterAll();
        AbilityDataKeys.RegisterAll();
        FeatureDataKeys.RegisterAll();
        AttackDataKeys.RegisterAll();
        AIDataKeys.RegisterAll();
        ProjectileDataKeys.RegisterAll();
        EffectDataKeys.RegisterAll();
    }
}
