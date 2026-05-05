using System.Collections.Generic;

namespace SkilmeAI.GameOS.Runtime.Relationship;

/// <summary>
/// Entity 之间的一条有向关系记录。
/// </summary>
public sealed class RelationshipRecord
{
    /// <summary>父级或来源实体 Id。</summary>
    public required string ParentEntityId { get; init; }

    /// <summary>子级或目标实体 Id。</summary>
    public required string ChildEntityId { get; init; }

    /// <summary>关系类型。</summary>
    public required string RelationType { get; init; }

    /// <summary>关系附加数据，低频使用，不进入热路径。</summary>
    public Dictionary<string, object> Data { get; } = new();

    /// <summary>优先级，数值越小越靠前。</summary>
    public int Priority { get; init; }
}
