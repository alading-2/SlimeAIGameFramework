using System;
using System.Collections.Generic;

namespace SkilmeAI.GameOS.Runtime.Resource;

/// <summary>
/// Runtime 资源键注册表。
/// </summary>
public static class ResourceCatalog
{
    private static readonly Dictionary<ResourceCategory, Dictionary<string, ResourceData>> Resources = new();

    /// <summary>
    /// 注册或替换资源映射。
    /// </summary>
    /// <param name="key">稳定资源键。</param>
    /// <param name="category">资源分类。</param>
    /// <param name="path">Godot 资源路径。</param>
    public static void Register(string key, ResourceCategory category, string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (!Resources.TryGetValue(category, out var byKey))
        {
            byKey = new Dictionary<string, ResourceData>(StringComparer.Ordinal);
            Resources[category] = byKey;
        }

        byKey[key] = new ResourceData(category, path);
    }

    /// <summary>
    /// 尝试查找资源映射。
    /// </summary>
    /// <param name="key">稳定资源键。</param>
    /// <param name="category">资源分类。</param>
    /// <param name="data">解析出的资源数据。</param>
    public static bool TryGet(string key, ResourceCategory category, out ResourceData data)
    {
        if (Resources.TryGetValue(category, out var byKey) && byKey.TryGetValue(key, out data))
        {
            return true;
        }

        data = default;
        return false;
    }

    /// <summary>
    /// 返回指定分类下的资源键快照。
    /// </summary>
    /// <param name="category">资源分类。</param>
    public static IReadOnlyList<string> GetKeys(ResourceCategory category)
    {
        return Resources.TryGetValue(category, out var byKey)
            ? new List<string>(byKey.Keys)
            : Array.Empty<string>();
    }

    /// <summary>
    /// 清空全部映射。
    /// </summary>
    public static void Clear()
    {
        Resources.Clear();
    }
}
