using System;
using System.Collections.Generic;
using System.Linq;

namespace SlimeAI.GameOS.Runtime.Data;

/// <summary>
/// 冻结的 Runtime DataKey 合约。
/// </summary>
public sealed class DataCatalog
{
    private readonly Dictionary<string, IDataKey> keysByStableKey;
    private readonly Dictionary<int, IDataKey> keysById;
    private readonly Dictionary<int, List<int>> dependentsByKeyId;
    private readonly Dictionary<string, CapabilityCatalogEntry> capabilities;

    internal DataCatalog(
        string catalogId,
        IReadOnlyList<IDataKey> keys,
        IReadOnlyList<CapabilityCatalogEntry> capabilities)
    {
        CatalogId = catalogId;
        Keys = keys.ToArray();
        this.capabilities = capabilities.ToDictionary(item => item.CapabilityId, StringComparer.Ordinal);
        keysByStableKey = Keys.ToDictionary(key => key.StableKey, StringComparer.Ordinal);
        keysById = Keys.ToDictionary(key => key.Id);
        dependentsByKeyId = BuildDependents(Keys);
    }

    /// <summary>Catalog 稳定 Id。</summary>
    public string CatalogId { get; }

    /// <summary>Catalog 内的 key 集合。</summary>
    public IReadOnlyList<IDataKey> Keys { get; }

    /// <summary>
    /// 根据当前已注册 DataKey 创建框架默认 catalog。
    /// </summary>
    public static DataCatalog Framework
    {
        get
        {
            FrameworkDataKeys.RegisterAll();
            return CreateBuilder("framework").AddRegisteredKeys().Build();
        }
    }

    /// <summary>
    /// 创建 builder。
    /// </summary>
    public static DataCatalogBuilder CreateBuilder(string catalogId)
    {
        return new DataCatalogBuilder(catalogId);
    }

    /// <summary>
    /// 按稳定键名解析 key。
    /// </summary>
    public bool TryResolve(string stableKey, out IDataKey key)
    {
        return keysByStableKey.TryGetValue(stableKey, out key!)
            || DataKeyRegistry.TryResolve(stableKey, out key!);
    }

    /// <summary>
    /// 按稳定键名解析 typed key。
    /// </summary>
    public bool TryResolve<T>(string stableKey, out DataKey<T> key)
    {
        if (TryResolve(stableKey, out var untyped) && untyped is DataKey<T> typed)
        {
            key = typed;
            return true;
        }

        key = default!;
        return false;
    }

    /// <summary>
    /// 按 key id 解析 key。
    /// </summary>
    public bool TryGetById(int id, out IDataKey key)
    {
        return keysById.TryGetValue(id, out key!)
            || DataKeyRegistry.GetAll().FirstOrDefault(item => item.Id == id) is { } registryKey && Assign(registryKey, out key);

        static bool Assign(IDataKey source, out IDataKey key)
        {
            key = source;
            return true;
        }
    }

    /// <summary>
    /// 返回依赖指定 key 的 computed key。
    /// </summary>
    public IReadOnlyList<int> GetDependentKeyIds(int keyId)
    {
        return dependentsByKeyId.TryGetValue(keyId, out var ids) ? ids : Array.Empty<int>();
    }

    /// <summary>
    /// 返回指定分类中的 key。
    /// </summary>
    public IReadOnlyList<IDataKey> GetByCategory(Enum category)
    {
        return Keys.Where(key => Equals(key.Category, category)).ToArray();
    }

    /// <summary>
    /// 返回 capability 是否启用；未声明时按启用处理。
    /// </summary>
    public bool IsCapabilityEnabled(string capabilityId)
    {
        return !capabilities.TryGetValue(capabilityId, out var entry) || entry.Enabled;
    }

    private static Dictionary<int, List<int>> BuildDependents(IReadOnlyList<IDataKey> keys)
    {
        var result = new Dictionary<int, List<int>>();
        for (var i = 0; i < keys.Count; i++)
        {
            var computed = keys[i];
            for (var j = 0; j < computed.Dependencies.Count; j++)
            {
                var dependency = computed.Dependencies[j];
                if (!result.TryGetValue(dependency.Id, out var list))
                {
                    list = new List<int>();
                    result[dependency.Id] = list;
                }

                list.Add(computed.Id);
            }
        }

        return result;
    }
}

/// <summary>
/// Capability 在 catalog 中的启用状态。
/// </summary>
public readonly record struct CapabilityCatalogEntry(string CapabilityId, bool Enabled, string OwnerSkill = "");

/// <summary>
/// DataCatalog 构造器。
/// </summary>
public sealed class DataCatalogBuilder
{
    private readonly string catalogId;
    private readonly Dictionary<string, IDataKey> keys = new(StringComparer.Ordinal);
    private readonly List<CapabilityCatalogEntry> capabilities = new();

    internal DataCatalogBuilder(string catalogId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(catalogId);
        this.catalogId = catalogId;
    }

    /// <summary>
    /// 添加一个 key。
    /// </summary>
    public DataCatalogBuilder Add<T>(DataKey<T> key)
    {
        keys[key.StableKey] = key;
        return this;
    }

    /// <summary>
    /// 添加非泛型 key。
    /// </summary>
    public DataCatalogBuilder Add(IDataKey key)
    {
        keys[key.StableKey] = key;
        return this;
    }

    /// <summary>
    /// 添加当前进程已注册的所有 key。
    /// </summary>
    public DataCatalogBuilder AddRegisteredKeys()
    {
        var registered = DataKeyRegistry.GetAll();
        for (var i = 0; i < registered.Count; i++)
        {
            Add(registered[i]);
        }

        return this;
    }

    /// <summary>
    /// 声明 capability 启用状态。
    /// </summary>
    public DataCatalogBuilder AddCapability(string capabilityId, bool enabled, string ownerSkill = "")
    {
        capabilities.Add(new CapabilityCatalogEntry(capabilityId, enabled, ownerSkill));
        return this;
    }

    /// <summary>
    /// 冻结 catalog。
    /// </summary>
    public DataCatalog Build()
    {
        return new DataCatalog(catalogId, keys.Values.ToArray(), capabilities.ToArray());
    }
}
