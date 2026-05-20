using System;
using System.Collections.Generic;
using Godot;
using SlimeAI.GameOS.Capabilities.Attack;
using SlimeAI.GameOS.Capabilities.Collision;
using SlimeAI.GameOS.Capabilities.Damage;
using SlimeAI.GameOS.Capabilities.Damage.Events;
using SlimeAI.GameOS.Capabilities.Movement;
using SlimeAI.GameOS.Capabilities.Unit;
using SlimeAI.GameOS.GodotBridge;
using SlimeAI.GameOS.Observation;
using SlimeAI.SceneTests.GameOS.Capabilities;

namespace SlimeAI.SceneTests.GameOS.GodotBridge.UnitComposition;

public partial class UnitCompositionValidationScene : Node
{
    private const string ScenePath = "res://SlimeAI/Src/Validation/GameOS/GodotBridge/UnitComposition/UnitCompositionValidation.tscn";
    private const string PassMarker = "GameOS GodotBridge UnitComposition validation PASS";
    private const string FailMarker = "GameOS GodotBridge UnitComposition validation FAIL";
    private const string VisualPath = "res://assets/Unit/Enemy/yuren/AnimatedSprite2D/yuren.tscn";

    public override void _Ready()
    {
        CapabilityValidationSupport.Run(
            this,
            ScenePath,
            "UnitCompositionValidation",
            "GameOS/GodotBridge/UnitComposition",
            "unit-composition-validation.json",
            PassMarker,
            FailMarker,
            new[] { "SlimeAI.GameOS.GodotBridge", "SlimeAI.GameOS.Capabilities.Unit" },
            new[] { "Visual asset path is read from Unit.VisualScenePath; no BrotatoLike namespace is referenced by framework adapters." },
            new[] { "GodotEntity2D", "GodotUnitCompositionProfile", "Unit.VisualScenePath", "Collision.Radius", "Movement.Velocity", "Damage events" },
            new[]
            {
                "VisualRoot is loaded from Unit.VisualScenePath",
                "Profile adapters are added and registered for an in-tree entity",
                "Hurtbox has CircleShape2D with configured radius",
                "Animation component switches idle/run and plays damaged/death",
                "Composed framework adapters do not use BrotatoLike namespaces"
            },
            new[] { $"stdout contains {PassMarker}", "artifact status is pass", "failureReasons is empty" },
            new[] { $"stdout contains {FailMarker}", "any composition check fails", "artifact standard-answer fields are missing" },
            validation =>
            {
                validation.Check("visual_load_and_hurtbox_shape", "Composer", ValidateVisualLoadAndHurtboxShape);
                validation.Check("registered_after_enter_tree", "Composer", ValidateRegisteredAfterEnterTree);
                validation.Check("animation_locomotion_damage_death", "Animation", ValidateAnimationLocomotionDamageDeath);
                validation.Check("no_game_namespace_adapter", "Boundary", ValidateNoGameNamespaceAdapter);
            });
    }

    private CheckResult ValidateVisualLoadAndHurtboxShape()
    {
        var entity = BuildEntity("composition-visual", VisualPath, 21f);
        var result = GodotUnitComposer.Compose(entity, new GodotUnitCompositionProfile
        {
            IncludeAI = true,
            IncludeContactDamageReceiver = true,
            FallbackHurtboxRadius = 12f
        });

        AddChild(entity);
        var shape = result.HurtboxShape?.Shape as CircleShape2D;
        var success = result.Success
            && result.VisualRoot != null
            && entity.GetNodeOrNull("VisualRoot") != null
            && shape != null
            && MathF.Abs(shape.Radius - 21f) <= 0.001f
            && entity.GetNodeOrNull<GodotAIComponent>("AI") != null
            && entity.GetNodeOrNull<GodotContactDamageComponent>("ContactDamage") != null;

        return CheckResult.From(success, success ? "visual and hurtbox composition passed" : "visual or hurtbox composition failed", CapabilityValidationSupport.Details(
            ("success", result.Success),
            ("failureReason", result.FailureReason),
            ("visualRoot", result.VisualRoot?.Name),
            ("hurtboxRadius", shape?.Radius),
            ("adapterCount", result.Adapters.Count)));
    }

    private CheckResult ValidateRegisteredAfterEnterTree()
    {
        var entity = BuildEntity("composition-registered", string.Empty, 18f);
        AddChild(entity);
        var result = GodotUnitComposer.Compose(entity, new GodotUnitCompositionProfile
        {
            IncludeVisual = false,
            IncludeAI = false,
            IncludeContactDamageReceiver = true
        });

        var stats = entity.BridgeContext.GetStats();
        var success = result.Success
            && result.RegisteredAdapterCount >= 5
            && stats.ComponentNodes >= result.RegisteredAdapterCount;

        return CheckResult.From(success, success ? "in-tree adapter registration passed" : "in-tree adapter registration failed", CapabilityValidationSupport.Details(
            ("registeredAdapterCount", result.RegisteredAdapterCount),
            ("bridgeComponents", stats.ComponentNodes),
            ("adapterCount", result.Adapters.Count)));
    }

    private CheckResult ValidateAnimationLocomotionDamageDeath()
    {
        var entity = BuildEntity("composition-animation", string.Empty, 10f);
        var sprite = new AnimatedSprite2D
        {
            Name = "VisualRoot",
            SpriteFrames = BuildSpriteFrames()
        };
        entity.AddChild(sprite);
        var result = GodotUnitComposer.Compose(entity, new GodotUnitCompositionProfile
        {
            IncludeVisual = false,
            IncludeOrientation = false,
            IncludeAI = false,
            IncludeAttack = false,
            IncludeHurtbox = false,
            IncludeContactDamageReceiver = false
        });
        AddChild(entity);

        var animation = entity.GetNodeOrNull<GodotUnitAnimationComponent>("UnitAnimation");
        if (animation == null)
        {
            return CheckResult.Fail("animation component missing", CapabilityValidationSupport.Details(("adapterCount", result.Adapters.Count)));
        }

        entity.Data.Set(MovementDataKeys.Velocity, new Vector2Value(60f, 0f));
        animation._Process(0.016d);
        var runAnimation = animation.CurrentAnimation;

        entity.Data.Set(MovementDataKeys.Velocity, Vector2Value.Zero);
        animation._Process(0.016d);
        var idleAnimation = animation.CurrentAnimation;

        entity.Events.Publish(new Damaged(new DamageInfo { Victim = entity, Damage = 1f }));
        var damagedAnimation = animation.CurrentAnimation;

        entity.Events.Publish(new Killed(entity, null, new DamageInfo { Victim = entity, Damage = 999f }));
        var deathAnimation = animation.CurrentAnimation;

        var success = result.Success
            && runAnimation == "run"
            && idleAnimation == "idle"
            && damagedAnimation == "beattacked"
            && deathAnimation == "dead";

        return CheckResult.From(success, success ? "animation locomotion and damage/death passed" : "animation state mismatch", CapabilityValidationSupport.Details(
            ("run", runAnimation),
            ("idle", idleAnimation),
            ("damaged", damagedAnimation),
            ("death", deathAnimation)));
    }

    private CheckResult ValidateNoGameNamespaceAdapter()
    {
        var entity = BuildEntity("composition-boundary", string.Empty, 12f);
        var result = GodotUnitComposer.Compose(entity, new GodotUnitCompositionProfile
        {
            IncludeVisual = false,
            IncludeAI = true,
            IncludeContactDamageReceiver = true
        });

        var offending = new List<string>();
        foreach (var adapter in result.Adapters)
        {
            var ns = adapter.GetType().Namespace ?? string.Empty;
            if (ns.StartsWith("BrotatoLike", StringComparison.Ordinal))
            {
                offending.Add(adapter.GetType().FullName ?? adapter.GetType().Name);
            }
        }

        return CheckResult.From(result.Success && offending.Count == 0, offending.Count == 0 ? "no game namespace adapter" : "game namespace adapter found", CapabilityValidationSupport.Details(
            ("offending", string.Join(",", offending)),
            ("adapterCount", result.Adapters.Count)));
    }

    private static GodotEntity2D BuildEntity(string id, string visualPath, float radius)
    {
        var entity = new GodotEntity2D
        {
            Name = id,
            EntityIdOverride = id
        };
        entity.Data.Set(UnitDataKeys.VisualScenePath, visualPath);
        entity.Data.Set(CollisionDataKeys.CollisionRadius, radius);
        entity.Data.Set(CollisionDataKeys.CollisionLayer, 1u);
        entity.Data.Set(CollisionDataKeys.CollisionMask, 2u);
        entity.Data.Set(AttackDataKeys.Damage, 3f);
        entity.Data.Set(AttackDataKeys.Range, 32f);
        entity.Data.Set(DamageDataKeys.CurrentHp, 10f);
        entity.Data.Set(DamageDataKeys.MaxHp, 10f);
        entity.Data.Set(MovementDataKeys.Position, Vector2Value.Zero);
        return entity;
    }

    private static SpriteFrames BuildSpriteFrames()
    {
        var frames = new SpriteFrames();
        AddAnimation(frames, "idle", loop: true);
        AddAnimation(frames, "run", loop: true);
        AddAnimation(frames, "beattacked", loop: false);
        AddAnimation(frames, "dead", loop: false);
        return frames;
    }

    private static void AddAnimation(SpriteFrames frames, string name, bool loop)
    {
        if (!frames.HasAnimation(name))
        {
            frames.AddAnimation(name);
        }

        frames.SetAnimationLoop(name, loop);
    }
}
