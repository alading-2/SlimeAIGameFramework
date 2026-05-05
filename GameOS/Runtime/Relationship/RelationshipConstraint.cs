namespace SkilmeAI.GameOS.Runtime.Relationship;

/// <summary>
/// 关系写入约束。
/// </summary>
public enum RelationshipConstraint
{
    /// <summary>不限制，同一关系类型下允许多父多子。</summary>
    None,

    /// <summary>同一关系类型下，子实体只能有一个父实体。</summary>
    OneToOne,

    /// <summary>同一关系类型下，父实体可有多个子实体，子实体只能有一个父实体。</summary>
    OneToMany
}
