using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Event;

namespace SlimeAI.GameOS.Runtime.Events.Core;

/// <summary>关系已添加。唯一 producer 是 RelationshipManager。</summary>
public readonly record struct RelationshipAdded(
    EntityId ParentEntityId,
    EntityId ChildEntityId,
    string RelationType) : IGlobalEvent;
