using System.Collections.Generic;
using Godot;

namespace SlimeAI.GameOS.Runtime.Resource;

/// <summary>
/// 资源加载门面；运行时代码应使用这里而不是直接调用 GD.Load。
/// </summary>
public static class ResourceManagement
{
    /// <summary>
    /// 加载已映射的 Godot 资源。
    /// </summary>
    /// <param name="key">稳定资源键。</param>
    /// <param name="category">资源分类。</param>
    public static T? Load<T>(string key, ResourceCategory category) where T : class
    {
        return ResourceCatalog.TryGet(key, category, out var data)
            ? GD.Load<T>(data.Path)
            : null;
    }

    /// <summary>
    /// 直接加载 Godot 资源路径。
    /// </summary>
    /// <param name="path">Godot 资源路径，例如 res://Scenes/Main.tscn。</param>
    public static T? LoadPath<T>(string path) where T : class
    {
        return string.IsNullOrWhiteSpace(path) ? null : GD.Load<T>(path);
    }

    /// <summary>
    /// 获取已映射的 Godot 资源路径。
    /// </summary>
    /// <param name="key">稳定资源键。</param>
    /// <param name="category">资源分类。</param>
    public static string? GetPath(string key, ResourceCategory category)
    {
        return ResourceCatalog.TryGet(key, category, out var data) ? data.Path : null;
    }

    /// <summary>
    /// 加载某个分类下已注册的全部资源。
    /// </summary>
    /// <param name="category">资源分类。</param>
    public static IReadOnlyList<T> LoadAll<T>(ResourceCategory category) where T : class
    {
        var result = new List<T>();
        var keys = ResourceCatalog.GetKeys(category);
        for (var i = 0; i < keys.Count; i++)
        {
            var resource = Load<T>(keys[i], category);
            if (resource != null)
            {
                result.Add(resource);
            }
        }

        return result;
    }
}
