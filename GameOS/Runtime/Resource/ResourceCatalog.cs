using System;
using System.Collections.Generic;
using SlimeAI.GameOS.Runtime.World;

namespace SlimeAI.GameOS.Runtime.Resource;

/// <summary>
/// Runtime 资源键注册表。
/// </summary>
public static class ResourceCatalog
{
    /// <summary>
    /// 注册或替换资源映射。
    /// </summary>
    /// <param name="key">稳定资源键。</param>
    /// <param name="category">资源分类。</param>
    /// <param name="path">Godot 资源路径。</param>
    public static void Register(string key, ResourceCategory category, string path)
    {
        RuntimeWorld.Default.Resources.Register(key, category, path);
    }

    /// <summary>
    /// 尝试查找资源映射。
    /// </summary>
    /// <param name="key">稳定资源键。</param>
    /// <param name="category">资源分类。</param>
    /// <param name="data">解析出的资源数据。</param>
    public static bool TryGet(string key, ResourceCategory category, out ResourceData data)
    {
        return RuntimeWorld.Default.Resources.TryGet(key, category, out data);
    }

    /// <summary>
    /// 返回指定分类下的资源键快照。
    /// </summary>
    /// <param name="category">资源分类。</param>
    public static IReadOnlyList<string> GetKeys(ResourceCategory category)
    {
        return RuntimeWorld.Default.Resources.GetKeys(category);
    }

    /// <summary>
    /// 清空全部映射。
    /// </summary>
    public static void Clear()
    {
        RuntimeWorld.Default.Resources.Clear();
    }
}
