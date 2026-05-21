using SlimeAI.GameOS.Capabilities.Collision;
using SlimeAI.GameOS.Capabilities.Damage;
using SlimeAI.GameOS.Capabilities.Movement;
using SlimeAI.GameOS.Observation;
using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Timer;
using SlimeAI.GameOS.Runtime.World;
using CollisionEvents = SlimeAI.GameOS.Capabilities.Collision.Events;
using DamageEvents = SlimeAI.GameOS.Capabilities.Damage.Events;
using static TestAssert;

internal partial class Program
{
    static void TestCollisionLayerMaskEvent()
    {
        using var world = RuntimeWorld.CreateScoped();
        var source = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("collision-source") });
        source.Data.Set(CollisionDataKeys.CollisionLayer, CollisionLayers.Projectile);
        source.Data.Set(CollisionDataKeys.CollisionMask, CollisionLayers.EnemyHurtbox);
        source.Data.Set(CollisionDataKeys.Team, 1);

        var target = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("collision-target") });
        target.Data.Set(CollisionDataKeys.CollisionLayer, CollisionLayers.EnemyHurtbox);
        target.Data.Set(CollisionDataKeys.Team, 2);

        var sameTeam = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("collision-same-team") });
        sameTeam.Data.Set(CollisionDataKeys.CollisionLayer, CollisionLayers.EnemyHurtbox);
        sameTeam.Data.Set(CollisionDataKeys.Team, 1);

        var entered = 0;
        var exited = 0;
        CollisionContact? received = null;
        source.Events.Subscribe<CollisionEvents.Entered>(data =>
        {
            entered++;
            received = data.Contact;
        });
        source.Events.Subscribe<CollisionEvents.Exited>(_ => exited++);

        var collision = new CollisionSystem();
        AssertEqual("collision can collide", true, collision.CanCollide(source, target));
        AssertEqual("collision same team blocked", false, collision.CanCollide(source, sameTeam, new CollisionFilterPolicy(IgnoreSameTeam: true)));
        AssertEqual("collision entered emitted", true, collision.EmitEntered(source, target));
        AssertEqual("collision exited emitted", true, collision.EmitExited(source, target));
        AssertEqual("collision entered count", 1, entered);
        AssertEqual("collision exited count", 1, exited);
        AssertEqual("collision payload source", source.EntityId, received?.Source.EntityId);
        AssertEqual("collision payload target", target.EntityId, received?.Target.EntityId);
        AssertEqual("collision payload layer", CollisionLayers.EnemyHurtbox, received?.TargetLayer);

    }

    static void TestDamageServiceAppliesHealth()
    {
        using var world = RuntimeWorld.CreateScoped();
        var attacker = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("damage-attacker") });
        var victim = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("damage-victim") });
        victim.Data.Set(DamageDataKeys.MaxHp, 20f);
        victim.Data.Set(DamageDataKeys.CurrentHp, 20f);

        var damagedEvents = 0;
        var healthEvents = 0;
        victim.Events.Subscribe<DamageEvents.Damaged>(data =>
        {
            damagedEvents++;
            AssertEqual("damaged attacker", attacker.EntityId, data.Info.Attacker?.EntityId);
        });
        victim.Events.Subscribe<DamageEvents.HealthChanged>(data =>
        {
            healthEvents++;
            AssertNear("health old", 20f, data.OldHp);
            AssertNear("health new", 13f, data.NewHp);
        });

        var result = new DamageService().Process(new DamageInfo
        {
            Attacker = attacker,
            Victim = victim,
            Damage = 7f,
            Type = DamageType.Physical,
            Tags = DamageTags.Attack
        });

        AssertEqual("damage applied", true, result.Applied);
        AssertNear("damage final", 7f, result.Info.FinalDamage);
        AssertNear("damage hp", 13f, victim.Data.Get<float>(DamageDataKeys.CurrentHp));
        AssertNear("damage dealt stat", 7f, attacker.Data.Get<float>(DamageDataKeys.TotalDamageDealt));
        AssertNear("damage taken stat", 7f, victim.Data.Get<float>(DamageDataKeys.TotalDamageTaken));
        AssertEqual("damage hit stat", 1, attacker.Data.Get<int>(DamageDataKeys.TotalHits));
        AssertEqual("damaged events", 1, damagedEvents);
        AssertEqual("health events", 1, healthEvents);

    }

    static void TestDamageServiceKilledEvent()
    {
        using var world = RuntimeWorld.CreateScoped();
        var attacker = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("damage-killer") });
        var victim = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("damage-killed-victim") });
        victim.Data.Set(DamageDataKeys.CurrentHp, 5f);

        var killedEvents = 0;
        victim.Events.Subscribe<DamageEvents.Killed>(data =>
        {
            killedEvents++;
            AssertEqual("killed killer", attacker.EntityId, data.Killer?.EntityId);
        });

        var result = new DamageService().Process(new DamageInfo
        {
            Attacker = attacker,
            Victim = victim,
            Damage = 10f,
            Tags = DamageTags.Contact
        });

        AssertEqual("kill applied", true, result.Applied);
        AssertEqual("kill fatal", true, result.Info.IsFatal);
        AssertEqual("kill is dead", true, victim.Data.Get<bool>(DamageDataKeys.IsDead));
        AssertNear("kill hp", 0f, victim.Data.Get<float>(DamageDataKeys.CurrentHp));
        AssertEqual("kill stat", 1, attacker.Data.Get<int>(DamageDataKeys.TotalKills));
        AssertEqual("killed events", 1, killedEvents);

    }

    static void TestDamagePipelineDodge()
    {
        using var world = RuntimeWorld.CreateScoped();
        var attacker = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("damage-dodge-attacker") });
        var victim = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("damage-dodge-victim") });
        victim.Data.Set(DamageDataKeys.CurrentHp, 20f);
        victim.Data.Set(DamageDataKeys.DodgeChance, 100f);

        var damagedEvents = 0;
        var dodgedEvents = 0;
        victim.Events.Subscribe<DamageEvents.Damaged>(_ => damagedEvents++);
        victim.Events.Subscribe<DamageEvents.Dodged>(data =>
        {
            dodgedEvents++;
            AssertEqual("dodged attacker", attacker.EntityId, data.Attacker?.EntityId);
        });

        var result = new DamageService().Process(new DamageInfo
        {
            Attacker = attacker,
            Victim = victim,
            Damage = 10f,
            Type = DamageType.Physical
        });

        AssertEqual("dodge not applied", false, result.Applied);
        AssertEqual("dodge flag", true, result.Info.IsDodged);
        AssertNear("dodge final", 0f, result.Info.FinalDamage);
        AssertNear("dodge hp unchanged", 20f, victim.Data.Get<float>(DamageDataKeys.CurrentHp));
        AssertEqual("dodge damaged events", 0, damagedEvents);
        AssertEqual("dodge event", 1, dodgedEvents);

    }

    static void TestDamagePipelineCriticalArmorAndStats()
    {
        using var world = RuntimeWorld.CreateScoped();
        var attacker = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("damage-crit-attacker") });
        attacker.Data.Set(DamageDataKeys.CritRate, 100f);
        attacker.Data.Set(DamageDataKeys.CritDamage, 200f);

        var victim = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("damage-crit-victim") });
        victim.Data.Set(DamageDataKeys.CurrentHp, 100f);
        victim.Data.Set(DamageDataKeys.Armor, 15f);

        var result = new DamageService().Process(new DamageInfo
        {
            Attacker = attacker,
            Victim = victim,
            Damage = 20f,
            Type = DamageType.Physical
        });

        AssertEqual("crit armor applied", true, result.Applied);
        AssertEqual("crit flag", true, result.Info.IsCritical);
        AssertNear("armor multiplier", 0.5f, result.Info.ArmorMultiplier);
        AssertNear("crit armor final", 20f, result.Info.FinalDamage);
        AssertNear("crit armor hp", 80f, victim.Data.Get<float>(DamageDataKeys.CurrentHp));
        AssertNear("crit highest", 20f, attacker.Data.Get<float>(DamageDataKeys.HighestSingleDamage));
        AssertEqual("crit count", 1, attacker.Data.Get<int>(DamageDataKeys.TotalCriticalHits));
        AssertEqual("crit encounter count", 1, attacker.Data.Get<int>(DamageDataKeys.EncounterCriticalHits));
        AssertEqual("hit count", 1, attacker.Data.Get<int>(DamageDataKeys.TotalHits));
        AssertNear("encounter damage dealt", 20f, attacker.Data.Get<float>(DamageDataKeys.EncounterDamageDealt));
        AssertNear("encounter damage taken", 20f, victim.Data.Get<float>(DamageDataKeys.EncounterDamageTaken));

    }

    static void TestDamagePipelineTrueDamageBypassesDodgeAndArmor()
    {
        using var world = RuntimeWorld.CreateScoped();
        var attacker = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("damage-true-attacker") });
        var victim = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("damage-true-victim") });
        victim.Data.Set(DamageDataKeys.CurrentHp, 50f);
        victim.Data.Set(DamageDataKeys.DodgeChance, 100f);
        victim.Data.Set(DamageDataKeys.Armor, 100f);

        var result = new DamageService().Process(new DamageInfo
        {
            Attacker = attacker,
            Victim = victim,
            Damage = 12f,
            Type = DamageType.True
        });

        AssertEqual("true damage applied", true, result.Applied);
        AssertEqual("true damage not dodged", false, result.Info.IsDodged);
        AssertNear("true damage armor multiplier unchanged", 1f, result.Info.ArmorMultiplier);
        AssertNear("true damage final", 12f, result.Info.FinalDamage);
        AssertNear("true damage hp", 38f, victim.Data.Get<float>(DamageDataKeys.CurrentHp));

    }

    static void TestDamagePipelineLifesteal()
    {
        using var world = RuntimeWorld.CreateScoped();
        var attacker = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("damage-lifesteal-attacker") });
        attacker.Data.Set(DamageDataKeys.MaxHp, 30f);
        attacker.Data.Set(DamageDataKeys.CurrentHp, 10f);
        attacker.Data.Set(DamageDataKeys.LifeSteal, 100f);

        var victim = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("damage-lifesteal-victim") });
        victim.Data.Set(DamageDataKeys.CurrentHp, 40f);

        var healedEvents = 0;
        attacker.Events.Subscribe<DamageEvents.Healed>(data =>
        {
            healedEvents++;
            AssertNear("lifesteal event amount", 10f, data.Amount);
        });

        var result = new DamageService().Process(new DamageInfo
        {
            Attacker = attacker,
            Victim = victim,
            Damage = 10f,
            Type = DamageType.Physical
        });

        AssertEqual("lifesteal applied", true, result.Applied);
        AssertNear("lifesteal victim hp", 30f, victim.Data.Get<float>(DamageDataKeys.CurrentHp));
        AssertNear("lifesteal attacker hp", 20f, attacker.Data.Get<float>(DamageDataKeys.CurrentHp));
        AssertNear("lifesteal amount", 10f, result.Info.LifestealAmount);
        AssertNear("lifesteal stat", 10f, attacker.Data.Get<float>(DamageDataKeys.TotalHealingDone));
        AssertEqual("lifesteal event", 1, healedEvents);

    }

    static void TestDamagePipelineShield()
    {
        using var world = RuntimeWorld.CreateScoped();
        var attacker = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("damage-shield-attacker") });
        var victim = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("damage-shield-victim") });
        victim.Data.Set(DamageDataKeys.CurrentHp, 30f);
        victim.Data.Set(DamageDataKeys.Shield, 8f);

        var result = new DamageService().Process(new DamageInfo
        {
            Attacker = attacker,
            Victim = victim,
            Damage = 10f,
            Type = DamageType.Physical
        });

        AssertEqual("shield partial applied", true, result.Applied);
        AssertNear("shield partial final", 2f, result.Info.FinalDamage);
        AssertNear("shield absorbed", 8f, result.Info.ShieldDamageAbsorbed);
        AssertNear("shield removed", 0f, victim.Data.Get<float>(DamageDataKeys.Shield));
        AssertNear("shield hp reduced by remaining", 28f, victim.Data.Get<float>(DamageDataKeys.CurrentHp));
        AssertNear("shield stat", 8f, victim.Data.Get<float>(DamageDataKeys.TotalShieldAbsorbed));

        victim.Data.Set(DamageDataKeys.CurrentHp, 30f);
        victim.Data.Set(DamageDataKeys.Shield, 20f);
        var blocked = new DamageService().Process(new DamageInfo
        {
            Attacker = attacker,
            Victim = victim,
            Damage = 5f,
            Type = DamageType.Physical
        });

        AssertEqual("shield full blocked", false, blocked.Applied);
        AssertEqual("shield full blocked flag", true, blocked.Info.IsBlocked);
        AssertNear("shield full final", 0f, blocked.Info.FinalDamage);
        AssertNear("shield full hp unchanged", 30f, victim.Data.Get<float>(DamageDataKeys.CurrentHp));
        AssertNear("shield full remaining", 15f, victim.Data.Get<float>(DamageDataKeys.Shield));

    }

    static void TestHealServiceAppliesAndClamps()
    {
        GameOSLog.Reset(new GameOSLogOptions { EnableStdout = false, EnableJsonl = false });
        var memory = new GameOSMemoryLogSink();
        GameOSLog.AddSink(memory);

        using var world = RuntimeWorld.CreateScoped();
        var healer = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("heal-service-healer") });
        var target = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("heal-service-target") });
        target.Data.Set(DamageDataKeys.MaxHp, 20f);
        target.Data.Set(DamageDataKeys.CurrentHp, 10f);

        var healedEvents = 0;
        target.Events.Subscribe<DamageEvents.Healed>(data =>
        {
            healedEvents++;
            AssertEqual("heal event source", HealSource.Direct, data.Info.Source);
            AssertNear("heal event amount", 10f, data.Amount);
        });

        var result = new HealService().Process(new HealInfo
        {
            Healer = healer,
            Target = target,
            Amount = 15f,
            Source = HealSource.Direct
        });

        AssertEqual("heal applied", true, result.Applied);
        AssertNear("heal final", 10f, result.Info.FinalAmount);
        AssertNear("heal hp clamped", 20f, target.Data.Get<float>(DamageDataKeys.CurrentHp));
        AssertNear("heal done stat", 10f, healer.Data.Get<float>(DamageDataKeys.TotalHealingDone));
        AssertNear("heal received stat", 10f, target.Data.Get<float>(DamageDataKeys.TotalHealingReceived));
        AssertEqual("heal events", 1, healedEvents);
        AssertEqual("heal log", true, memory.Entries.Any(entry =>
            entry.Context == "HealService" &&
            entry.Level == GameOSLogLevel.Info &&
            entry.Message == "Heal applied: heal-service-healer->heal-service-target, amount=10, hp=10->20" &&
            Equals(entry.Values["healer"], healer.EntityId) &&
            Equals(entry.Values["target"], target.EntityId) &&
            Equals(entry.Values["amount"], 10f) &&
            Equals(entry.Values["oldHp"], 10f) &&
            Equals(entry.Values["newHp"], 20f)));

        GameOSLog.Reset(new GameOSLogOptions { EnableStdout = false, EnableJsonl = false });
    }

    static void TestDamageToolMultiTargetAndPeriodic()
    {
        using var world = RuntimeWorld.CreateScoped();
        var attacker = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("damage-tool-attacker") });
        var first = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("damage-tool-first") });
        var second = world.Entities.Spawn(new EntitySpawnConfig { EntityId = new EntityId("damage-tool-second") });
        first.Data.Set(DamageDataKeys.CurrentHp, 20f);
        second.Data.Set(DamageDataKeys.CurrentHp, 20f);

        var targets = new[] { first, second };
        var once = DamageTool.Apply(targets, new DamageApplyOptions(3f)
        {
            Attacker = attacker,
            Type = DamageType.Physical,
            Tags = DamageTags.Ability
        });

        AssertEqual("tool once target count", 2, once.TargetCount);
        AssertEqual("tool once applied count", 2, once.AppliedCount);
        AssertNear("tool once first hp", 17f, first.Data.Get<float>(DamageDataKeys.CurrentHp));
        AssertNear("tool once second hp", 17f, second.Data.Get<float>(DamageDataKeys.CurrentHp));

        var timerManager = new TimerManager("damage-tool-test-timers");
        var periodic = DamageTool.ApplyPeriodic(
            targets,
            new DamageApplyOptions(2f)
            {
                Attacker = attacker,
                Type = DamageType.Magical,
                Tags = DamageTags.Persistent
            },
            new DamageRepeatOptions(0.5f)
            {
                RepeatCount = 3,
                ApplyImmediately = true,
                TimerTag = "damage-tool-periodic"
            },
            timerManager);

        AssertEqual("tool periodic initial applied", 2, periodic.AppliedCount);
        AssertEqual("tool periodic has timer", true, periodic.Timer != null);
        AssertNear("tool periodic first initial", 15f, first.Data.Get<float>(DamageDataKeys.CurrentHp));

        timerManager.Tick(0.5f);
        timerManager.Tick(0.5f);

        AssertNear("tool periodic first final", 11f, first.Data.Get<float>(DamageDataKeys.CurrentHp));
        AssertNear("tool periodic second final", 11f, second.Data.Get<float>(DamageDataKeys.CurrentHp));
        AssertNear("tool periodic dealt stat", 18f, attacker.Data.Get<float>(DamageDataKeys.TotalDamageDealt));
        timerManager.Clear();
    }
}
