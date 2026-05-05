namespace SkilmeAI.GameOS.Runtime.Entity;

using SkilmeAI.GameOS.Runtime.Relationship;

/// <summary>
/// 纯 Runtime Entity 生成参数。
/// </summary>
public readonly record struct EntitySpawnConfig
{
    /// <summary>
    /// 调用方传入的实体 Id；为空时自动生成 GUID 字符串。
    /// </summary>
    public string? EntityId { get; init; }

    /// <summary>
    /// 可选 Authoring 配置对象，生成后会加载到实体 Data。
    /// </summary>
    public object? Config { get; init; }

    /// <summary>
    /// 可选父实体 Id；填入后生成阶段会自动绑定归属关系。
    /// </summary>
    public string? ParentEntityId { get; init; }

    /// <summary>
    /// 是否自动补充 PARENT 关系。
    /// </summary>
    public bool AutoAddParentRelation { get; init; }

    /// <summary>
    /// 父实体销毁时对子实体的处理策略，只写入 PARENT 关系。
    /// </summary>
    public ParentDestroyPolicy ParentDestroyPolicy { get; init; }

    /// <summary>
    /// 额外业务关系类型，例如 EntityToProjectile。
    /// </summary>
    public string[]? ParentRelationTypes { get; init; }
}
