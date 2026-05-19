using System;
using System.Collections.Generic;
using SlimeAI.GameOS.Capabilities.Collision;
using SlimeAI.GameOS.Capabilities.Damage;
using SlimeAI.GameOS.Capabilities.Movement;
using SlimeAI.GameOS.Runtime.Entity;

namespace SlimeAI.GameOS.Capabilities.Ability;

/// <summary>
/// Ability 目标上下文辅助工具。调用方显式使用它准备 CastContext，AbilityService 不做统一索敌。
/// </summary>
public static class AbilityTargetingTool
{
    /// <summary>
    /// 按 Ability Data 构造自动索敌参数。
    /// </summary>
    /// <param name="ability">技能实体。</param>
    public static AbilityAutoTargetOptions CreateOptionsFromAbility(IEntity ability)
    {
        ArgumentNullException.ThrowIfNull(ability);
        AbilityDataKeys.RegisterAll();
        return new AbilityAutoTargetOptions
        {
            Range = ability.Data.Get<float>(AbilityDataKeys.AutoTargetRange, -1f),
            MaxTargets = ability.Data.Get<int>(AbilityDataKeys.AutoTargetMaxTargets, 1),
            IgnoreSameTeam = ability.Data.Get<bool>(AbilityDataKeys.AutoTargetIgnoreSameTeam, true),
            RequiresDamageable = ability.Data.Get<bool>(AbilityDataKeys.AutoTargetRequiresDamageable, true)
        };
    }

    /// <summary>
    /// 尝试为技能构造自动索敌后的施法上下文。
    /// </summary>
    /// <param name="caster">施法者。</param>
    /// <param name="ability">技能实体。</param>
    /// <param name="context">输出施法上下文。</param>
    /// <param name="options">可选覆盖参数；为空时从 Ability Data 读取。</param>
    /// <param name="damageType">上下文伤害类型。</param>
    /// <param name="damageTags">上下文额外伤害标签。</param>
    /// <param name="targetQuery">Ability 目标候选查询；null 时使用 RuntimeAbilityTargetQuery。</param>
    public static bool TryBuildContext(
        IEntity caster,
        IEntity ability,
        out AbilityCastContext? context,
        AbilityAutoTargetOptions? options = null,
        DamageType damageType = DamageType.Physical,
        DamageTags damageTags = DamageTags.None,
        IAbilityTargetQuery? targetQuery = null)
    {
        ArgumentNullException.ThrowIfNull(caster);
        ArgumentNullException.ThrowIfNull(ability);
        AbilityDataKeys.RegisterAll();
        options ??= CreateOptionsFromAbility(ability);
        context = null;

        var targetSelection = ability.Data.Get<AbilityTargetSelection>(
            AbilityDataKeys.TargetSelection,
            AbilityTargetSelection.None);
        if (targetSelection == AbilityTargetSelection.None)
        {
            context = new AbilityCastContext
            {
                Caster = caster,
                Ability = ability,
                DamageType = damageType,
                DamageTags = damageTags
            };
            return true;
        }

        if (targetSelection == AbilityTargetSelection.Point)
        {
            return false;
        }

        var targets = ResolveEntityTargets(caster, ability, options, targetQuery);
        if (targets.Count == 0)
        {
            return false;
        }

        var targetPosition = targets[0].Data.Get<Vector2Value>(MovementDataKeys.Position, Vector2Value.Zero);
        context = new AbilityCastContext
        {
            Caster = caster,
            Ability = ability,
            Targets = targets,
            TargetPosition = targetPosition,
            DamageType = damageType,
            DamageTags = damageTags
        };
        return true;
    }

    /// <summary>
    /// 解析自动索敌实体目标，按距离从近到远返回。
    /// </summary>
    /// <param name="caster">施法者。</param>
    /// <param name="ability">技能实体。</param>
    /// <param name="options">自动索敌参数。</param>
    /// <param name="targetQuery">Ability 目标候选查询；null 时使用 RuntimeAbilityTargetQuery。</param>
    public static IReadOnlyList<IEntity> ResolveEntityTargets(
        IEntity caster,
        IEntity ability,
        AbilityAutoTargetOptions? options = null,
        IAbilityTargetQuery? targetQuery = null)
    {
        ArgumentNullException.ThrowIfNull(caster);
        ArgumentNullException.ThrowIfNull(ability);
        options ??= CreateOptionsFromAbility(ability);

        var maxTargets = options.MaxTargets;
        var targets = new List<IEntity>();
        if (maxTargets == 0)
        {
            return targets;
        }

        var distances = new List<float>();
        var origin = caster.Data.Get<Vector2Value>(MovementDataKeys.Position, Vector2Value.Zero);
        var casterTeam = caster.Data.Get<int>(CollisionDataKeys.Team, 0);
        var entities = (targetQuery ?? new RuntimeAbilityTargetQuery()).GetCandidates(caster);

        for (var i = 0; i < entities.Count; i++)
        {
            var candidate = entities[i];
            if (!CanSelectCandidate(caster, ability, candidate, options, casterTeam))
            {
                continue;
            }

            var candidatePosition = candidate.Data.Get<Vector2Value>(MovementDataKeys.Position, Vector2Value.Zero);
            var distance = Vector2Value.Distance(origin, candidatePosition);
            if (options.Range >= 0f && distance > options.Range)
            {
                continue;
            }

            InsertByDistance(targets, distances, candidate, distance, maxTargets);
        }

        return targets;
    }

    private static bool CanSelectCandidate(
        IEntity caster,
        IEntity ability,
        IEntity candidate,
        AbilityAutoTargetOptions options,
        int casterTeam)
    {
        if (ReferenceEquals(candidate, caster) || ReferenceEquals(candidate, ability))
        {
            return false;
        }

        if (options.RequiresDamageable && !candidate.Data.Has(DamageDataKeys.CurrentHp))
        {
            return false;
        }

        if (!options.IncludeDeadTargets && candidate.Data.Get<bool>(DamageDataKeys.IsDead, false))
        {
            return false;
        }

        if (options.IgnoreSameTeam
            && casterTeam != 0
            && candidate.Data.Get<int>(CollisionDataKeys.Team, 0) == casterTeam)
        {
            return false;
        }

        return true;
    }

    private static void InsertByDistance(
        List<IEntity> targets,
        List<float> distances,
        IEntity candidate,
        float distance,
        int maxTargets)
    {
        var insertIndex = targets.Count;
        for (var i = 0; i < distances.Count; i++)
        {
            if (distance < distances[i])
            {
                insertIndex = i;
                break;
            }
        }

        targets.Insert(insertIndex, candidate);
        distances.Insert(insertIndex, distance);
        if (maxTargets >= 0 && targets.Count > maxTargets)
        {
            var lastIndex = targets.Count - 1;
            targets.RemoveAt(lastIndex);
            distances.RemoveAt(lastIndex);
        }
    }
}
