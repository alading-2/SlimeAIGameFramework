using System.Collections.Generic;
using SlimeAI.GameOS.Runtime.Resource;

namespace SlimeAI.GameOS.Runtime.World;

/// <summary>
/// RuntimeWorld 内部的资源目录句柄。
/// </summary>
public interface IResourceCatalog
{
    /// <summary>注册或替换资源映射。</summary>
    void Register(string key, ResourceCategory category, string path);

    /// <summary>尝试查找资源映射。</summary>
    bool TryGet(string key, ResourceCategory category, out ResourceData data);

    /// <summary>返回指定分类下的资源键快照。</summary>
    IReadOnlyList<string> GetKeys(ResourceCategory category);

    /// <summary>清空全部资源映射。</summary>
    void Clear();
}
