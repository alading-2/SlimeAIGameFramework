using System.Collections.Generic;

namespace SlimeAI.GameOS.Runtime.Relationship;

/// <summary>
/// 归属关系生命周期数据工具。
/// </summary>
public static class RelationshipLifecycle
{
    /// <summary>
    /// 创建 PARENT 关系上的生命周期数据。
    /// </summary>
    /// <param name="parentDestroyPolicy">父实体销毁时对子实体的处理策略。</param>
    public static Dictionary<string, object> CreateParentRelationshipData(ParentDestroyPolicy parentDestroyPolicy)
    {
        return new Dictionary<string, object>(1)
        {
            [RelationshipType.ParentDestroyPolicyDataKey] = (int)parentDestroyPolicy
        };
    }

    /// <summary>
    /// 读取 PARENT 关系上的父销毁策略，缺失时默认递归销毁。
    /// </summary>
    /// <param name="parentId">父实体 Id。</param>
    /// <param name="childId">子实体 Id。</param>
    public static ParentDestroyPolicy ReadParentDestroyPolicy(string parentId, string childId)
    {
        var relationData = RelationshipManager.GetRelationshipData(parentId, childId, RelationshipType.Parent);
        if (relationData == null)
        {
            return ParentDestroyPolicy.DestroyRecursively;
        }

        if (relationData.TryGetValue(RelationshipType.ParentDestroyPolicyDataKey, out var rawValue) &&
            rawValue is int intValue &&
            intValue >= (int)ParentDestroyPolicy.DestroyRecursively &&
            intValue <= (int)ParentDestroyPolicy.Detach)
        {
            return (ParentDestroyPolicy)intValue;
        }

        return ParentDestroyPolicy.DestroyRecursively;
    }
}
