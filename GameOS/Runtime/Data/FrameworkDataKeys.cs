using SlimeAI.GameOS.Capabilities.AI;
using SlimeAI.GameOS.Capabilities.Ability;
using SlimeAI.GameOS.Capabilities.Attack;
using SlimeAI.GameOS.Capabilities.Collision;
using SlimeAI.GameOS.Capabilities.Damage;
using SlimeAI.GameOS.Capabilities.Effect;
using SlimeAI.GameOS.Capabilities.Feature;
using SlimeAI.GameOS.Capabilities.Movement;
using SlimeAI.GameOS.Capabilities.Projectile;
using SlimeAI.GameOS.Capabilities.Unit;
using SlimeAI.GameOS.Runtime.Schedule;

namespace SlimeAI.GameOS.Runtime.Data;

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
