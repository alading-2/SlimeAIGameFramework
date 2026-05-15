using System;
using System.Collections.Generic;
using SlimeAI.GameOS.Runtime.Resource;

namespace SlimeAI.GameOS.Runtime.World;

/// <summary>
/// RuntimeWorld 持有的资源目录状态。
/// </summary>
public sealed class ResourceCatalogState : IResourceCatalog
{
    private readonly Dictionary<ResourceCategory, Dictionary<string, ResourceData>> resources = new();

    /// <inheritdoc />
    public void Register(string key, ResourceCategory category, string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (!resources.TryGetValue(category, out var byKey))
        {
            byKey = new Dictionary<string, ResourceData>(StringComparer.Ordinal);
            resources[category] = byKey;
        }

        byKey[key] = new ResourceData(category, path);
    }

    /// <inheritdoc />
    public bool TryGet(string key, ResourceCategory category, out ResourceData data)
    {
        if (resources.TryGetValue(category, out var byKey) && byKey.TryGetValue(key, out data))
        {
            return true;
        }

        data = default;
        return false;
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetKeys(ResourceCategory category)
    {
        return resources.TryGetValue(category, out var byKey)
            ? new List<string>(byKey.Keys)
            : Array.Empty<string>();
    }

    /// <inheritdoc />
    public void Clear()
    {
        resources.Clear();
    }
}
