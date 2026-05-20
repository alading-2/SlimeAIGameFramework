using Godot;
using SlimeAI.GameOS.Capabilities.AI;
using SlimeAI.GameOS.Capabilities.Attack;
using SlimeAI.GameOS.Capabilities.Collision;
using SlimeAI.GameOS.Capabilities.Damage;
using SlimeAI.GameOS.Capabilities.Movement;
using SlimeAI.GameOS.Capabilities.Unit;
using SlimeAI.GameOS.Runtime.Resource;

namespace SlimeAI.GameOS.GodotBridge;

/// <summary>
/// 框架级 Godot 单位组合器，根据 profile 挂载通用 GodotBridge Adapter。
/// </summary>
public static class GodotUnitComposer
{
    /// <summary>
    /// 按 profile 组合一个 GodotEntity2D。若实体已在树内，会立即注册新增 Adapter。
    /// </summary>
    public static GodotUnitCompositionResult Compose(GodotEntity2D entity, GodotUnitCompositionProfile profile)
    {
        var result = new GodotUnitCompositionResult();
        if (entity == null)
        {
            return Fail(result, "Entity is required.");
        }

        if (profile == null)
        {
            return Fail(result, "Profile is required.");
        }

        RegisterDataKeys();

        if (profile.IncludeVisual)
        {
            var visual = TryLoadVisual(entity, out var failureReason);
            if (!string.IsNullOrEmpty(failureReason))
            {
                return Fail(result, failureReason);
            }

            if (visual != null)
            {
                result.VisualRoot = visual;
            }
        }

        if (profile.IncludeAnimation)
        {
            AddAdapter(entity, result, new GodotUnitAnimationComponent
            {
                Name = profile.AnimationName,
                AutoDriveLocomotion = true,
                PlayDamagedOnDamage = true,
                PlayDeathOnKilled = true
            });
        }

        if (profile.IncludeOrientation)
        {
            AddAdapter(entity, result, new GodotOrientationComponent
            {
                Name = profile.OrientationName,
                Sink = OrientationSink.VisualFlipX
            });
        }

        if (profile.IncludeAI)
        {
            AddAdapter(entity, result, BuildAI(entity, profile));
        }

        if (profile.IncludeAttack)
        {
            AddAdapter(entity, result, BuildAttack(entity, profile));
        }

        if (profile.IncludeHurtbox)
        {
            AddHurtbox(entity, profile, result);
        }

        if (profile.IncludeContactDamageReceiver)
        {
            AddAdapter(entity, result, new GodotContactDamageComponent { Name = profile.ContactDamageName });
        }

        if (entity.IsInsideTree())
        {
            result.RegisteredAdapterCount = entity.BridgeContext.RegisterComponents(entity, entity);
        }

        return result;
    }

    private static void RegisterDataKeys()
    {
        UnitDataKeys.RegisterAll();
        CollisionDataKeys.RegisterAll();
        DamageDataKeys.RegisterAll();
        AttackDataKeys.RegisterAll();
        AIDataKeys.RegisterAll();
    }

    private static Node? TryLoadVisual(GodotEntity2D entity, out string failureReason)
    {
        failureReason = string.Empty;
        if (entity.GetNodeOrNull("VisualRoot") != null)
        {
            return null;
        }

        var visualPath = entity.Data.Get(UnitDataKeys.VisualScenePath, string.Empty);
        if (string.IsNullOrWhiteSpace(visualPath))
        {
            return null;
        }

        var visualScene = ResourceManagement.LoadPath<PackedScene>(visualPath);
        if (visualScene == null)
        {
            failureReason = $"Unit visual scene load failed: {visualPath}";
            return null;
        }

        var visual = visualScene.Instantiate();
        visual.Name = "VisualRoot";
        entity.AddChild(visual);
        return visual;
    }

    private static GodotAIComponent BuildAI(GodotEntity2D entity, GodotUnitCompositionProfile profile)
    {
        return new GodotAIComponent
        {
            Name = profile.AIName,
            IsEnabled = entity.Data.Get(AIDataKeys.IsEnabled, true),
            AttackRange = entity.Data.Get(AttackDataKeys.Range, entity.Data.Get(AIDataKeys.AttackRange, 100f)),
            TargetSearchRange = entity.Data.Get(UnitDataKeys.DetectionRange, -1f),
            ChaseSpeedMultiplier = entity.Data.Get(MovementDataKeys.AIMoveSpeedMultiplier, 1f)
        };
    }

    private static GodotAttackComponent BuildAttack(GodotEntity2D entity, GodotUnitCompositionProfile profile)
    {
        return new GodotAttackComponent
        {
            Name = profile.AttackName,
            PreferExistingDataOnRegister = true,
            Damage = entity.Data.Get(AttackDataKeys.Damage, 0f),
            Range = entity.Data.Get(AttackDataKeys.Range, 100f),
            Interval = entity.Data.Get(AttackDataKeys.Interval, 1f),
            WindUpTime = entity.Data.Get(AttackDataKeys.WindUpTime, 0f),
            RecoveryTime = entity.Data.Get(AttackDataKeys.RecoveryTime, 0f),
            CanAttack = entity.Data.Get(AttackDataKeys.CanAttack, true)
        };
    }

    private static void AddHurtbox(
        GodotEntity2D entity,
        GodotUnitCompositionProfile profile,
        GodotUnitCompositionResult result)
    {
        var hurtbox = new GodotHurtboxComponent
        {
            Name = profile.HurtboxName
        };
        var radius = entity.Data.Get(CollisionDataKeys.CollisionRadius, 0f);
        if (radius <= 0f)
        {
            radius = profile.FallbackHurtboxRadius;
        }

        var shape = new CollisionShape2D
        {
            Name = "CollisionShape2D",
            Shape = new CircleShape2D { Radius = radius }
        };
        hurtbox.AddChild(shape);
        entity.AddChild(hurtbox);
        result.HurtboxShape = shape;
        result.AddAdapter(hurtbox);
    }

    private static void AddAdapter(GodotEntity2D entity, GodotUnitCompositionResult result, Node adapter)
    {
        entity.AddChild(adapter);
        result.AddAdapter(adapter);
    }

    private static GodotUnitCompositionResult Fail(GodotUnitCompositionResult result, string reason)
    {
        result.Success = false;
        result.FailureReason = reason;
        return result;
    }
}
