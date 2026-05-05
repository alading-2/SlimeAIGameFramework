using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;

namespace SkilmeAI.GameOS.Runtime.Data;

/// <summary>
/// Runtime dynamic data container.
/// </summary>
public sealed class Data
{
    private static readonly Dictionary<Type, (PropertyInfo Prop, string Key)[]> ConfigPropertyCache = new();

    private readonly IDataChangeSink? changeSink;
    private readonly Dictionary<string, object?> values = new();
    private readonly Dictionary<string, List<DataModifier>> modifiers = new();
    private readonly Dictionary<string, object?> cachedValues = new();
    private readonly HashSet<string> dirtyKeys = new();

    /// <summary>
    /// Creates a runtime Data container.
    /// </summary>
    /// <param name="changeSink">Optional bridge for data change notifications.</param>
    public Data(IDataChangeSink? changeSink = null)
    {
        this.changeSink = changeSink;
    }

    /// <summary>
    /// Sets a base value.
    /// </summary>
    /// <param name="key">Runtime Data key.</param>
    /// <param name="value">New base value.</param>
    public bool Set<T>(string key, T value)
    {
        var meta = DataRegistry.GetMeta(key);
        object? finalValue = value;
        if (meta != null)
        {
            if (meta.HasOptions && !meta.IsValidOption(value))
            {
                return false;
            }
            finalValue = meta.Clamp(value);
        }

        values.TryGetValue(key, out var oldValue);
        if (values.ContainsKey(key) && Equals(oldValue, finalValue))
        {
            return false;
        }

        values[key] = finalValue;
        MarkDirty(key);
        NotifyChanged(key, oldValue, finalValue);
        return true;
    }

    /// <summary>
    /// Gets an effective value.
    /// </summary>
    /// <param name="key">Runtime Data key.</param>
    /// <param name="defaultValue">Optional fallback value.</param>
    public T Get<T>(string key, object? defaultValue = null)
    {
        var meta = DataRegistry.GetMeta(key);
        if (meta == null)
        {
            var fallback = defaultValue ?? DataMeta.GetTypeDefaultValue(typeof(T));
            return values.TryGetValue(key, out var rawValue) && rawValue != null
                ? ConvertValue<T>(rawValue, fallback)
                : ConvertValue<T>(fallback, default);
        }

        if (meta.IsComputed)
        {
            var fallback = defaultValue ?? meta.GetDefaultValue();
            return ConvertValue<T>(GetComputedValueBoxed(key, meta, fallback, typeof(T)), fallback);
        }

        var effectiveDefault = defaultValue ?? meta.GetDefaultValue();
        if (!values.TryGetValue(key, out var baseValue) || baseValue == null)
        {
            return ConvertValue<T>(effectiveDefault, default);
        }

        if (meta.SupportModifiers == true && modifiers.ContainsKey(key))
        {
            return ConvertValue<T>(GetModifiedValueBoxed(key, baseValue, effectiveDefault, typeof(T)), effectiveDefault);
        }

        return ConvertValue<T>(baseValue, effectiveDefault);
    }

    /// <summary>
    /// Gets a base value without modifiers.
    /// </summary>
    /// <param name="key">Runtime Data key.</param>
    /// <param name="defaultValue">Fallback value.</param>
    public T GetBase<T>(string key, T defaultValue = default!)
    {
        return values.TryGetValue(key, out var value) && value != null
            ? ConvertValue<T>(value, defaultValue)
            : defaultValue;
    }

    /// <summary>
    /// Returns true when the key exists or is computed.
    /// </summary>
    /// <param name="key">Runtime Data key.</param>
    public bool Has(string key)
    {
        return values.ContainsKey(key) || DataRegistry.IsComputed(key);
    }

    /// <summary>
    /// Removes a base value and its modifiers.
    /// </summary>
    /// <param name="key">Runtime Data key.</param>
    public bool Remove(string key)
    {
        if (!values.TryGetValue(key, out var oldValue))
        {
            return false;
        }

        values.Remove(key);
        modifiers.Remove(key);
        cachedValues.Remove(key);
        dirtyKeys.Remove(key);
        NotifyChanged(key, oldValue, null);
        return true;
    }

    /// <summary>
    /// Adds a numeric value to an existing base value.
    /// </summary>
    /// <param name="key">Runtime Data key.</param>
    /// <param name="delta">Numeric delta.</param>
    public void Add<T>(string key, T delta) where T : INumber<T>
    {
        Set(key, GetBase(key, T.Zero) + delta);
    }

    /// <summary>
    /// Multiplies an existing base value.
    /// </summary>
    /// <param name="key">Runtime Data key.</param>
    /// <param name="factor">Numeric multiplier.</param>
    public void Multiply<T>(string key, T factor) where T : INumber<T>
    {
        Set(key, GetBase(key, T.Zero) * factor);
    }

    /// <summary>
    /// Adds a modifier to a supported key.
    /// </summary>
    /// <param name="key">Runtime Data key.</param>
    /// <param name="modifier">Runtime modifier.</param>
    public bool AddModifier(string key, DataModifier modifier)
    {
        if (!DataRegistry.SupportModifiers(key))
        {
            return false;
        }

        if (!modifiers.TryGetValue(key, out var list))
        {
            list = new List<DataModifier>();
            modifiers[key] = list;
        }

        for (var i = 0; i < list.Count; i++)
        {
            if (list[i].Id == modifier.Id)
            {
                return false;
            }
        }

        var insertIndex = list.BinarySearch(modifier, ModifierPriorityComparer.Instance);
        if (insertIndex < 0) insertIndex = ~insertIndex;
        list.Insert(insertIndex, modifier);
        MarkDirty(key);
        NotifyChanged(key, null, Get<float>(key));
        return true;
    }

    /// <summary>
    /// Removes a modifier by id.
    /// </summary>
    /// <param name="key">Runtime Data key.</param>
    /// <param name="modifierId">Modifier id.</param>
    public bool RemoveModifier(string key, string modifierId)
    {
        if (!modifiers.TryGetValue(key, out var list))
        {
            return false;
        }

        var removed = list.RemoveAll(modifier => modifier.Id == modifierId);
        if (removed <= 0)
        {
            return false;
        }

        MarkDirty(key);
        NotifyChanged(key, null, Get<float>(key));
        return true;
    }

    /// <summary>
    /// Removes every modifier whose source matches the supplied source object.
    /// </summary>
    /// <param name="source">Modifier source object.</param>
    public int RemoveModifiersBySource(object source)
    {
        ArgumentNullException.ThrowIfNull(source);
        var removedTotal = 0;
        foreach (var pair in modifiers)
        {
            var removed = pair.Value.RemoveAll(modifier => ReferenceEquals(modifier.Source, source));
            if (removed <= 0)
            {
                continue;
            }

            removedTotal += removed;
            MarkDirty(pair.Key);
            NotifyChanged(pair.Key, null, Get<float>(pair.Key));
        }

        return removedTotal;
    }

    /// <summary>
    /// Gets a copy of all modifiers for a key.
    /// </summary>
    /// <param name="key">Runtime Data key.</param>
    public List<DataModifier> GetModifiers(string key)
    {
        return modifiers.TryGetValue(key, out var list) ? new List<DataModifier>(list) : new List<DataModifier>();
    }

    /// <summary>
    /// Loads public readable config properties into Data.
    /// </summary>
    /// <param name="config">Authoring config object.</param>
    public void LoadFromConfig(object config)
    {
        var type = config.GetType();
        if (!ConfigPropertyCache.TryGetValue(type, out var cached))
        {
            cached = BuildPropertyCache(type);
            ConfigPropertyCache[type] = cached;
        }

        for (var i = 0; i < cached.Length; i++)
        {
            var (prop, key) = cached[i];
            var value = prop.GetValue(config);
            if (value != null)
            {
                Set(key, value);
            }
        }
    }

    /// <summary>
    /// Resets existing values in a category to metadata defaults.
    /// </summary>
    /// <param name="category">Data category enum.</param>
    public void ResetByCategory(Enum category)
    {
        var metas = DataRegistry.GetCachedMetaByCategory(category);
        for (var i = 0; i < metas.Length; i++)
        {
            var meta = metas[i];
            if (values.ContainsKey(meta.Key))
            {
                Set(meta.Key, meta.GetDefaultValue());
            }
        }
    }

    /// <summary>
    /// Clears runtime values, modifiers and caches.
    /// </summary>
    public void Reset()
    {
        values.Clear();
        modifiers.Clear();
        cachedValues.Clear();
        dirtyKeys.Clear();
    }

    /// <summary>
    /// Gets a copy of all base values.
    /// </summary>
    public Dictionary<string, object?> GetAll()
    {
        return new Dictionary<string, object?>(values);
    }

    private static (PropertyInfo, string)[] BuildPropertyCache(Type type)
    {
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var result = new List<(PropertyInfo, string)>(properties.Length);
        for (var i = 0; i < properties.Length; i++)
        {
            var prop = properties[i];
            if (!prop.CanRead) continue;

            var attr = prop.GetCustomAttribute<DataKeyAttribute>();
            result.Add((prop, attr?.Key ?? prop.Name));
        }
        return result.ToArray();
    }

    private object? GetComputedValueBoxed(string key, DataMeta meta, object? defaultValue, Type targetType)
    {
        if (!dirtyKeys.Contains(key) && cachedValues.TryGetValue(key, out var cached))
        {
            return cached == null ? defaultValue : ConvertValueBoxed(cached, targetType, defaultValue);
        }

        var result = meta.Compute!(this);
        cachedValues[key] = result;
        dirtyKeys.Remove(key);
        return result == null ? defaultValue : ConvertValueBoxed(result, targetType, defaultValue);
    }

    private object? GetModifiedValueBoxed(string key, object baseValue, object? defaultValue, Type targetType)
    {
        if (!dirtyKeys.Contains(key) && cachedValues.TryGetValue(key, out var cached))
        {
            return cached == null ? defaultValue : ConvertValueBoxed(cached, targetType, defaultValue);
        }

        var finalValue = CalculateFinalValue(key, Convert.ToSingle(baseValue));
        cachedValues[key] = finalValue;
        dirtyKeys.Remove(key);
        return ConvertValueBoxed(finalValue, targetType, defaultValue);
    }

    private float CalculateFinalValue(string key, float baseValue)
    {
        if (!modifiers.TryGetValue(key, out var list) || list.Count == 0)
        {
            return baseValue;
        }

        var additive = 0f;
        var multiplicative = 1f;
        var finalAdditive = 0f;
        float? overrideValue = null;
        float? cap = null;

        for (var i = 0; i < list.Count; i++)
        {
            var modifier = list[i];
            switch (modifier.Type)
            {
                case ModifierType.Additive:
                    additive += modifier.Value;
                    break;
                case ModifierType.Multiplicative:
                    multiplicative *= modifier.Value;
                    break;
                case ModifierType.FinalAdditive:
                    finalAdditive += modifier.Value;
                    break;
                case ModifierType.Override:
                    overrideValue ??= modifier.Value;
                    break;
                case ModifierType.Cap:
                    cap = cap.HasValue ? Math.Min(cap.Value, modifier.Value) : modifier.Value;
                    break;
            }
        }

        var result = overrideValue ?? (baseValue + additive) * multiplicative + finalAdditive;
        if (cap.HasValue) result = Math.Min(result, cap.Value);

        var meta = DataRegistry.GetMeta(key);
        return meta != null ? Convert.ToSingle(meta.Clamp(result)) : result;
    }

    private void MarkDirty(string key)
    {
        dirtyKeys.Add(key);
        cachedValues.Remove(key);

        foreach (var dependentKey in DataRegistry.GetDependentComputedKeys(key))
        {
            dirtyKeys.Add(dependentKey);
            cachedValues.Remove(dependentKey);
        }
    }

    private void NotifyChanged(string key, object? oldValue, object? newValue)
    {
        changeSink?.OnDataChanged(new DataChangedEventData(key, oldValue, newValue));
    }

    private static object? ConvertValueBoxed(object value, Type targetType, object? defaultValue)
    {
        if (targetType.IsInstanceOfType(value))
        {
            return value;
        }

        try
        {
            var valueType = value.GetType();
            if (targetType.IsEnum)
            {
                if (value is string enumText)
                {
                    return Enum.Parse(targetType, enumText, ignoreCase: false);
                }

                return Enum.ToObject(targetType, Convert.ToInt64(value));
            }

            if (valueType.IsEnum)
            {
                if (targetType == typeof(string))
                {
                    return value.ToString() ?? defaultValue;
                }

                return Convert.ChangeType(Convert.ToInt64(value), targetType);
            }

            return Convert.ChangeType(value, targetType);
        }
        catch
        {
            return defaultValue;
        }
    }

    private static T ConvertValue<T>(object? value, object? defaultValue)
    {
        var fallback = defaultValue ?? DataMeta.GetTypeDefaultValue(typeof(T));
        if (value == null)
        {
            return fallback is T fallbackValue ? fallbackValue : default!;
        }

        var converted = ConvertValueBoxed(value, typeof(T), fallback);
        return converted is T typedValue ? typedValue : default!;
    }

    private sealed class ModifierPriorityComparer : IComparer<DataModifier>
    {
        public static readonly ModifierPriorityComparer Instance = new();

        public int Compare(DataModifier? x, DataModifier? y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            return x.Priority.CompareTo(y.Priority);
        }
    }
}
