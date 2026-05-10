namespace SkilmeAI.GameOS.Runtime.Relationship;

/// <summary>
/// GameOS 内置关系类型。
/// </summary>
public static class RelationshipType
{
    /// <summary>Entity 与 Component 的组合关系。</summary>
    public const string EntityToComponent = "relationship.entity.component";

    /// <summary>通用父子归属关系，生命周期策略只挂在该关系上。</summary>
    public const string Parent = "relationship.parent";

    /// <summary>拥有关系，例如单位拥有技能、物品或 UI。</summary>
    public const string Owner = "relationship.owner";

    /// <summary>来源关系，例如投射物由某个实体生成。</summary>
    public const string Source = "relationship.source";

    /// <summary>目标关系，例如效果当前锁定某个实体。</summary>
    public const string Target = "relationship.target";

    /// <summary>装备关系。</summary>
    public const string EquippedBy = "relationship.equipped_by";

    /// <summary>生成者关系。</summary>
    public const string SpawnedBy = "relationship.spawned_by";

    /// <summary>影响关系，例如 Buff 或 Feature 正作用于某个实体。</summary>
    public const string AffectedBy = "relationship.affected_by";

    /// <summary>实体与技能关系，保留旧 BrotatoLike 能力迁移入口。</summary>
    public const string EntityToAbility = "relationship.entity.ability";

    /// <summary>实体与物品关系，保留旧 BrotatoLike 装备迁移入口。</summary>
    public const string EntityToItem = "relationship.entity.item";

    /// <summary>实体与投射物关系，保留旧 BrotatoLike 生成迁移入口。</summary>
    public const string EntityToProjectile = "relationship.entity.projectile";

    /// <summary>实体与效果关系，保留旧 BrotatoLike 生成迁移入口。</summary>
    public const string EntityToEffect = "relationship.entity.effect";

    /// <summary>实体与 UI 关系，保留旧 BrotatoLike HUD 迁移入口。</summary>
    public const string EntityToUi = "relationship.entity.ui";

    /// <summary>父关系生命周期数据键。</summary>
    public const string ParentDestroyPolicyDataKey = "parent_destroy_policy";
}
