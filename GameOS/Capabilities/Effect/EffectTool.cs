using System;
using SlimeAI.GameOS.Capabilities.Effect.Events;
using SlimeAI.GameOS.Capabilities.Movement;
using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Relationship;

namespace SlimeAI.GameOS.Capabilities.Effect;

/// <summary>
/// Effect Capability 最小 Runtime 入口，负责生成效果实体并写入运行时 Data。
/// </summary>
public static class EffectTool
{
    /// <summary>
    /// 生成纯 Runtime 效果实体。
    /// </summary>
    /// <param name="options">效果生成参数。</param>
    public static EffectSpawnResult Spawn(EffectSpawnOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        EffectDataKeys.RegisterAll();

        var effect = EntityManager.Spawn(new EntitySpawnConfig
        {
            EntityId = options.EntityId,
            ParentEntityId = options.Source.EntityId,
            AutoAddParentRelation = true,
            ParentRelationTypes = [RelationshipType.EntityToEffect, RelationshipType.Source]
        });
        var position = options.Target?.Data.Get<Vector2Value>(MovementDataKeys.Position, options.Position)
            ?? options.Position;

        effect.Data.Set(EffectDataKeys.SourceEntity, options.Source);
        if (options.Ability != null)
        {
            effect.Data.Set(EffectDataKeys.AbilityEntity, options.Ability);
        }

        if (options.Target != null)
        {
            effect.Data.Set(EffectDataKeys.TargetEntity, options.Target);
            RelationshipManager.AddRelationship(
                effect.EntityId,
                options.Target.EntityId,
                RelationshipType.Target);
        }

        effect.Data.Set(EffectDataKeys.ScenePath, options.ScenePath ?? string.Empty);
        effect.Data.Set(EffectDataKeys.Name, options.Name ?? string.Empty);
        effect.Data.Set(EffectDataKeys.AnimationName, options.AnimationName ?? string.Empty);
        effect.Data.Set(EffectDataKeys.Position, position);
        effect.Data.Set(EffectDataKeys.Duration, options.Duration);
        effect.Data.Set(MovementDataKeys.Position, position);

        effect.Events.Publish(new Spawned(effect, options.Source, options.Ability, options.Target));
        return new EffectSpawnResult(effect, true);
    }
}
