using System;
using System.Collections.Generic;

namespace SkilmeAI.GameOS.Runtime.Data;

/// <summary>
/// DataKey 全局索引，只负责稳定键名和 key id 分配，不承载业务访问入口。
/// </summary>
internal static class DataKeyRegistry
{
    private static readonly Dictionary<string, IDataKey> KeysByStableKey = new(StringComparer.Ordinal);
    private static readonly List<IDataKey> KeysById = new();

    /// <summary>
    /// 注册 key 并返回稳定 id。
    /// </summary>
    public static int Register(IDataKey key)
    {
        if (KeysByStableKey.TryGetValue(key.StableKey, out var existing))
        {
            return existing.Id;
        }

        var id = KeysById.Count;
        KeysByStableKey.Add(key.StableKey, key);
        KeysById.Add(key);
        return id;
    }

    /// <summary>
    /// 按稳定键名解析 key。
    /// </summary>
    public static bool TryResolve(string stableKey, out IDataKey key)
    {
        return KeysByStableKey.TryGetValue(stableKey, out key!);
    }

    /// <summary>
    /// 返回当前全部 key 的快照。
    /// </summary>
    public static IReadOnlyList<IDataKey> GetAll()
    {
        return KeysById.ToArray();
    }
}
