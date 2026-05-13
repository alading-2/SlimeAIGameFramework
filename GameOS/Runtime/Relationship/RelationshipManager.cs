using System;
using System.Collections.Generic;
using SlimeAI.GameOS.Runtime.Event;
using SlimeAI.GameOS.Runtime.Events.Core;

namespace SlimeAI.GameOS.Runtime.Relationship;

/// <summary>
/// Runtime Entity 关系图，基于父索引、子索引和类型索引做低成本查询。
/// </summary>
public static class RelationshipManager
{
    private static readonly Dictionary<string, RelationshipRecord> Relationships = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, HashSet<string>> ParentIndex = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, HashSet<string>> ChildIndex = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, HashSet<string>> TypeIndex = new(StringComparer.Ordinal);

    /// <summary>
    /// 添加一条有向关系。
    /// </summary>
    /// <param name="parentId">父级或来源实体 Id。</param>
    /// <param name="childId">子级或目标实体 Id。</param>
    /// <param name="relationType">关系类型。</param>
    /// <param name="data">关系附加数据。</param>
    /// <param name="constraint">关系写入约束。</param>
    /// <param name="priority">优先级，数值越小越靠前。</param>
    public static bool AddRelationship(
        string parentId,
        string childId,
        string relationType,
        Dictionary<string, object>? data = null,
        RelationshipConstraint constraint = RelationshipConstraint.None,
        int priority = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(parentId);
        ArgumentException.ThrowIfNullOrWhiteSpace(childId);
        ArgumentException.ThrowIfNullOrWhiteSpace(relationType);

        var relationshipId = GenerateRelationshipId(parentId, childId, relationType);
        if (Relationships.ContainsKey(relationshipId))
        {
            return false;
        }

        if ((constraint == RelationshipConstraint.OneToOne || constraint == RelationshipConstraint.OneToMany) &&
            GetParentEntitiesByChildAndType(childId, relationType).Count > 0)
        {
            return false;
        }

        var record = new RelationshipRecord
        {
            ParentEntityId = parentId,
            ChildEntityId = childId,
            RelationType = relationType,
            Priority = priority
        };

        if (data != null)
        {
            foreach (var kvp in data)
            {
                record.Data[kvp.Key] = kvp.Value;
            }
        }

        Relationships[relationshipId] = record;
        GetOrCreateSet(ParentIndex, parentId).Add(relationshipId);
        GetOrCreateSet(ChildIndex, childId).Add(relationshipId);
        GetOrCreateSet(TypeIndex, relationType).Add(relationshipId);

        WorldEvents.World.Publish(new RelationshipAdded(parentId, childId, relationType));
        return true;
    }

    /// <summary>
    /// 移除一条有向关系。
    /// </summary>
    /// <param name="parentId">父级或来源实体 Id。</param>
    /// <param name="childId">子级或目标实体 Id。</param>
    /// <param name="relationType">关系类型。</param>
    public static bool RemoveRelationship(string parentId, string childId, string relationType)
    {
        var relationshipId = GenerateRelationshipId(parentId, childId, relationType);
        if (!Relationships.Remove(relationshipId))
        {
            return false;
        }

        ParentIndex.GetValueOrDefault(parentId)?.Remove(relationshipId);
        ChildIndex.GetValueOrDefault(childId)?.Remove(relationshipId);
        TypeIndex.GetValueOrDefault(relationType)?.Remove(relationshipId);

        CleanupEmptySet(ParentIndex, parentId);
        CleanupEmptySet(ChildIndex, childId);
        CleanupEmptySet(TypeIndex, relationType);

        WorldEvents.World.Publish(new RelationshipRemoved(parentId, childId, relationType));
        return true;
    }

    /// <summary>
    /// 绑定父子归属关系，并可同步绑定额外业务关系。
    /// </summary>
    /// <param name="childId">子实体 Id。</param>
    /// <param name="parentId">父实体 Id。</param>
    /// <param name="autoAddParentRelation">是否自动添加 PARENT 关系。</param>
    /// <param name="parentDestroyPolicy">PARENT 关系上的父销毁策略。</param>
    /// <param name="relationTypes">额外业务关系类型。</param>
    public static bool BindParentRelationships(
        string childId,
        string parentId,
        bool autoAddParentRelation = true,
        ParentDestroyPolicy parentDestroyPolicy = ParentDestroyPolicy.DestroyRecursively,
        params string[] relationTypes)
    {
        var normalized = NormalizeOwnedRelationTypes(autoAddParentRelation, relationTypes);
        if (normalized.Count == 0 || WouldCreateParentCycle(childId, parentId))
        {
            return false;
        }

        foreach (var relationType in normalized)
        {
            if (GetParentEntitiesByChildAndType(childId, relationType).Count > 0)
            {
                return false;
            }
        }

        foreach (var relationType in normalized)
        {
            var data = relationType == RelationshipType.Parent
                ? RelationshipLifecycle.CreateParentRelationshipData(parentDestroyPolicy)
                : null;

            if (!AddRelationship(parentId, childId, relationType, data, RelationshipConstraint.OneToMany))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 合并写入关系附加数据。
    /// </summary>
    public static bool SetRelationshipData(string parentId, string childId, string relationType, Dictionary<string, object> data)
    {
        var relationshipId = GenerateRelationshipId(parentId, childId, relationType);
        if (!Relationships.TryGetValue(relationshipId, out var record))
        {
            return false;
        }

        foreach (var kvp in data)
        {
            record.Data[kvp.Key] = kvp.Value;
        }

        return true;
    }

    /// <summary>
    /// 获取关系附加数据。
    /// </summary>
    public static IReadOnlyDictionary<string, object>? GetRelationshipData(string parentId, string childId, string relationType)
    {
        var relationshipId = GenerateRelationshipId(parentId, childId, relationType);
        return Relationships.GetValueOrDefault(relationshipId)?.Data;
    }

    /// <summary>
    /// 判断关系是否存在。
    /// </summary>
    public static bool HasRelationship(string parentId, string childId, string relationType)
    {
        return Relationships.ContainsKey(GenerateRelationshipId(parentId, childId, relationType));
    }

    /// <summary>
    /// 获取父实体在指定关系类型下的子实体 Id 快照。
    /// </summary>
    public static IReadOnlyList<string> GetChildEntitiesByParentAndType(string parentId, string relationType)
    {
        var records = GetChildRelationshipsByParentAndType(parentId, relationType);
        var result = new List<string>(records.Count);
        foreach (var record in records)
        {
            result.Add(record.ChildEntityId);
        }

        return result;
    }

    /// <summary>
    /// 获取子实体在指定关系类型下的父实体 Id 快照。
    /// </summary>
    public static IReadOnlyList<string> GetParentEntitiesByChildAndType(string childId, string relationType)
    {
        var result = new List<string>();
        if (!ChildIndex.TryGetValue(childId, out var relationshipIds))
        {
            return result;
        }

        foreach (var relationshipId in relationshipIds)
        {
            if (Relationships.TryGetValue(relationshipId, out var record) && record.RelationType == relationType)
            {
                result.Add(record.ParentEntityId);
            }
        }

        return result;
    }

    /// <summary>
    /// 获取父实体在指定关系类型下的关系记录快照。
    /// </summary>
    public static IReadOnlyList<RelationshipRecord> GetChildRelationshipsByParentAndType(
        string parentId,
        string relationType,
        bool sortByPriority = false)
    {
        var result = new List<RelationshipRecord>();
        if (!ParentIndex.TryGetValue(parentId, out var relationshipIds))
        {
            return result;
        }

        foreach (var relationshipId in relationshipIds)
        {
            if (Relationships.TryGetValue(relationshipId, out var record) && record.RelationType == relationType)
            {
                result.Add(record);
            }
        }

        if (sortByPriority)
        {
            result.Sort(static (left, right) => left.Priority.CompareTo(right.Priority));
        }

        return result;
    }

    /// <summary>
    /// 获取父实体关联的全部关系记录快照。
    /// </summary>
    public static IReadOnlyList<RelationshipRecord> GetRelationshipsByParent(string parentId)
    {
        return GetRelationshipsByIndex(ParentIndex, parentId);
    }

    /// <summary>
    /// 获取子实体关联的全部关系记录快照。
    /// </summary>
    public static IReadOnlyList<RelationshipRecord> GetRelationshipsByChild(string childId)
    {
        return GetRelationshipsByIndex(ChildIndex, childId);
    }

    /// <summary>
    /// 获取某类关系的全部记录快照。
    /// </summary>
    public static IReadOnlyList<RelationshipRecord> GetRelationshipsByType(string relationType)
    {
        return GetRelationshipsByIndex(TypeIndex, relationType);
    }

    /// <summary>
    /// 移除某个实体参与的全部关系。
    /// </summary>
    /// <param name="entityId">实体 Id。</param>
    public static void RemoveAllForEntity(string entityId)
    {
        var toRemove = new List<RelationshipRecord>();
        toRemove.AddRange(GetRelationshipsByParent(entityId));
        toRemove.AddRange(GetRelationshipsByChild(entityId));

        foreach (var record in toRemove)
        {
            RemoveRelationship(record.ParentEntityId, record.ChildEntityId, record.RelationType);
        }
    }

    /// <summary>
    /// 清空关系图。
    /// </summary>
    public static void Clear()
    {
        Relationships.Clear();
        ParentIndex.Clear();
        ChildIndex.Clear();
        TypeIndex.Clear();
    }

    private static string GenerateRelationshipId(string parentId, string childId, string relationType)
    {
        return $"{parentId}:{childId}:{relationType}";
    }

    private static HashSet<string> GetOrCreateSet(Dictionary<string, HashSet<string>> index, string key)
    {
        if (!index.TryGetValue(key, out var set))
        {
            set = new HashSet<string>(StringComparer.Ordinal);
            index[key] = set;
        }

        return set;
    }

    private static void CleanupEmptySet(Dictionary<string, HashSet<string>> index, string key)
    {
        if (index.TryGetValue(key, out var set) && set.Count == 0)
        {
            index.Remove(key);
        }
    }

    private static IReadOnlyList<RelationshipRecord> GetRelationshipsByIndex(
        Dictionary<string, HashSet<string>> index,
        string key)
    {
        var result = new List<RelationshipRecord>();
        if (!index.TryGetValue(key, out var relationshipIds))
        {
            return result;
        }

        foreach (var relationshipId in relationshipIds)
        {
            if (Relationships.TryGetValue(relationshipId, out var record))
            {
                result.Add(record);
            }
        }

        return result;
    }

    private static List<string> NormalizeOwnedRelationTypes(bool autoAddParentRelation, IEnumerable<string>? relationTypes)
    {
        var result = new List<string>(4);
        if (autoAddParentRelation)
        {
            result.Add(RelationshipType.Parent);
        }

        if (relationTypes == null)
        {
            return result;
        }

        foreach (var relationType in relationTypes)
        {
            if (string.IsNullOrWhiteSpace(relationType) || result.Contains(relationType))
            {
                continue;
            }

            result.Add(relationType);
        }

        return result;
    }

    private static bool WouldCreateParentCycle(string childId, string parentId)
    {
        if (childId == parentId)
        {
            return true;
        }

        var currentId = parentId;
        for (var depth = 0; depth < 128; depth++)
        {
            var parents = GetParentEntitiesByChildAndType(currentId, RelationshipType.Parent);
            if (parents.Count == 0)
            {
                return false;
            }

            currentId = parents[0];
            if (currentId == childId)
            {
                return true;
            }
        }

        return true;
    }
}
