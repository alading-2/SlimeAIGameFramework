using System;
using System.Collections.Generic;
using SlimeAI.GameOS.Capabilities.Effect.Events;
using SlimeAI.GameOS.Capabilities.Movement;
using SlimeAI.GameOS.Observation;
using SlimeAI.GameOS.Runtime.Entity;

namespace SlimeAI.GameOS.Capabilities.Effect;

/// <summary>
/// Effect Capability 最小 Runtime 入口，负责生成效果实体并写入运行时 Data。
/// </summary>
public static class EffectTool
{
    private static readonly GameOSContextLog Log = GameOSLog.For("EffectTool");
    private static bool ownerCleanupRegistered;

    /// <summary>
    /// 在 capability 启动入口调用，向 framework 注册 source -> SpawnedEffectIds owner cleanup hook。
    /// </summary>
    public static void Initialize()
    {
        EffectDataKeys.RegisterAll();
        if (ownerCleanupRegistered)
        {
            return;
        }

        RuntimeOwnedReferenceRegistry.Register(new OwnedReferenceDescriptor(
            EffectDataKeys.SourceEntity,
            EffectDataKeys.SpawnedEffectIds));
        ownerCleanupRegistered = true;
    }

    /// <summary>
    /// 生成纯 Runtime 效果实体。
    /// </summary>
    /// <param name="options">效果生成参数。</param>
    public static EffectSpawnResult Spawn(EffectSpawnOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        Initialize();

        var effect = EntityManager.Spawn(new EntitySpawnConfig
        {
            EntityId = options.EntityId,
            ParentEntityId = options.Source.EntityId,
        });
        var position = options.Target?.Data.Get<Vector2Value>(MovementDataKeys.Position, options.Position)
            ?? options.Position;

        effect.Data.Set(EffectDataKeys.SourceEntity, options.Source.EntityId);
        if (options.Ability != null)
        {
            effect.Data.Set(EffectDataKeys.AbilityEntity, options.Ability.EntityId);
        }

        if (options.Target != null)
        {
            effect.Data.Set(EffectDataKeys.TargetEntity, options.Target.EntityId);
        }

        // 同步 spawner owner-list：把当前 effect id 写回 source 的 typed list。
        var spawnedList = options.Source.Data.Get(EffectDataKeys.SpawnedEffectIds);
        options.Source.Data.Set(EffectDataKeys.SpawnedEffectIds, spawnedList.Add(effect.EntityId));

        effect.Data.Set(EffectDataKeys.ScenePath, options.ScenePath ?? string.Empty);
        effect.Data.Set(EffectDataKeys.Name, options.Name ?? string.Empty);
        effect.Data.Set(EffectDataKeys.AnimationName, options.AnimationName ?? string.Empty);
        effect.Data.Set(EffectDataKeys.Position, position);
        effect.Data.Set(EffectDataKeys.Duration, options.Duration);
        effect.Data.Set(MovementDataKeys.Position, position);

        Log.Debug(
            $"Effect played: {effect.EntityId}",
            new Dictionary<string, object?>
            {
                ["effectId"] = effect.EntityId.Value,
                ["sourceId"] = options.Source.EntityId.Value,
                ["targetId"] = options.Target?.EntityId.Value,
                ["animationName"] = options.AnimationName,
            });

        effect.Events.Publish(new Spawned(effect, options.Source, options.Ability, options.Target));
        return new EffectSpawnResult(effect, true);
    }
}
