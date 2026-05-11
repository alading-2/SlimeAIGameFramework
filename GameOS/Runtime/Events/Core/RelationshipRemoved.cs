using SkilmeAI.GameOS.Runtime.Event;

namespace SkilmeAI.GameOS.Runtime.Events.Core;

/// <summary>关系已移除。唯一 producer 是 RelationshipManager。</summary>
public readonly record struct RelationshipRemoved(
    string ParentEntityId,
    string ChildEntityId,
    string RelationType) : IGlobalEvent;
