using System;
using System.Collections.Generic;
using System.Linq;

namespace SkilmeAI.GameOS.Runtime.Data;

/// <summary>
/// Registry for runtime Data metadata.
/// </summary>
public static class DataRegistry
{
    private static readonly Dictionary<string, DataMeta> MetaRegistry = new();
    private static readonly Dictionary<Enum, DataMeta[]> CategoryCache = new();

    /// <summary>
    /// Registers metadata and returns the same instance for static field initialization.
    /// </summary>
    /// <param name="meta">Data metadata.</param>
    public static DataMeta Register(DataMeta meta)
    {
        MetaRegistry[meta.Key] = meta;
        if (meta.Category != null) CategoryCache.Remove(meta.Category);
        return meta;
    }

    /// <summary>
    /// Gets metadata by key.
    /// </summary>
    /// <param name="key">Runtime Data key.</param>
    public static DataMeta? GetMeta(string key)
    {
        return MetaRegistry.GetValueOrDefault(key);
    }

    /// <summary>
    /// Returns true when the key is computed.
    /// </summary>
    /// <param name="key">Runtime Data key.</param>
    public static bool IsComputed(string key)
    {
        return GetMeta(key)?.IsComputed ?? false;
    }

    /// <summary>
    /// Returns true when the key supports modifiers.
    /// </summary>
    /// <param name="key">Runtime Data key.</param>
    public static bool SupportModifiers(string key)
    {
        return GetMeta(key)?.SupportModifiers ?? false;
    }

    /// <summary>
    /// Gets all computed keys depending on the supplied base key.
    /// </summary>
    /// <param name="baseKey">Base runtime Data key.</param>
    public static IEnumerable<string> GetDependentComputedKeys(string baseKey)
    {
        return MetaRegistry.Values
            .Where(meta => meta.IsComputed && meta.Dependencies != null && meta.Dependencies.Contains(baseKey))
            .Select(meta => meta.Key);
    }

    /// <summary>
    /// Gets cached metadata for a category.
    /// </summary>
    /// <param name="category">Data category enum.</param>
    public static DataMeta[] GetCachedMetaByCategory(Enum category)
    {
        if (!CategoryCache.TryGetValue(category, out var cached))
        {
            cached = MetaRegistry.Values.Where(meta => Equals(meta.Category, category)).ToArray();
            CategoryCache[category] = cached;
        }
        return cached;
    }

    /// <summary>
    /// Gets all registered keys.
    /// </summary>
    public static IEnumerable<string> GetAllKeys()
    {
        return MetaRegistry.Keys;
    }

    /// <summary>
    /// Clears runtime metadata. Intended for isolated tests and generators.
    /// </summary>
    public static void Clear()
    {
        MetaRegistry.Clear();
        CategoryCache.Clear();
    }
}

