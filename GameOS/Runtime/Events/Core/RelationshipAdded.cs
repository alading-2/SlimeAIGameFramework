using SkilmeAI.GameOS.Runtime.Event;

namespace SkilmeAI.GameOS.Runtime.Events.Core;

/// <summary>关系已添加。唯一 producer 是 RelationshipManager。</summary>
public readonly record struct RelationshipAdded(
    string ParentEntityId,
    string ChildEntityId,
    string RelationType) : IGlobalEvent;
