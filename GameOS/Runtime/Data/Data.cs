using System;
using System.Collections.Generic;
using System.Numerics;

namespace SlimeAI.GameOS.Runtime.Data;

/// <summary>
/// Runtime typed data container.
/// </summary>
public sealed class Data
{
    private readonly DataCatalog catalog;
    private readonly IDataChangeSink? changeSink;
    private readonly Dictionary<int, IDataSlot> slots = new();

    /// <summary>
    /// 创建带 frozen catalog 的运行时 Data 容器。
    /// </summary>
    public Data(DataCatalog catalog, IDataChangeSink? changeSink = null)
    {
        this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        this.changeSink = changeSink;
    }

    /// <summary>
    /// 使用当前已注册 key 创建默认框架 catalog。
    /// </summary>
    public Data(IDataChangeSink? changeSink = null)
        : this(DataCatalog.Framework, changeSink)
    {
    }

    /// <summary>当前 Data 绑定的 catalog。</summary>
    public DataCatalog Catalog => catalog;

    /// <summary>
    /// Sets a typed base value.
    /// </summary>
    public bool Set<T>(DataKey<T> key, T value)
    {
        var slot = GetOrCreateSlot(key);
        if (!slot.TrySet(value, out var oldValue, out var newValue))
        {
            return false;
        }

        MarkDirty(key);
        NotifyChanged(key, oldValue, newValue);
        return true;
    }

    /// <summary>
    /// Gets an effective typed value.
    /// </summary>
    public T Get<T>(DataKey<T> key)
    {
        var slot = GetOrCreateSlot(key);
        return slot.Get(this);
    }

    /// <summary>
    /// Gets an effective typed value with a caller fallback for unset base keys.
    /// </summary>
    public T Get<T>(DataKey<T> key, T defaultValue)
    {
        var slot = GetOrCreateSlot(key);
        return slot.HasBaseValue || key.IsComputed ? slot.Get(this) : defaultValue;
    }

    /// <summary>
    /// Attempts to read an explicit base value. Computed keys are treated as present.
    /// </summary>
    public bool TryGet<T>(DataKey<T> key, out T value)
    {
        var slot = GetOrCreateSlot(key);
        if (key.IsComputed)
        {
            value = slot.Get(this);
            return true;
        }

        return slot.TryGetBase(out value);
    }

    /// <summary>
    /// Gets a base value without modifiers.
    /// </summary>
    public T GetBase<T>(DataKey<T> key)
    {
        var slot = GetOrCreateSlot(key);
        return slot.GetBase();
    }

    /// <summary>
    /// Gets a base value without modifiers.
    /// </summary>
    public T GetBase<T>(DataKey<T> key, T defaultValue)
    {
        var slot = GetOrCreateSlot(key);
        return slot.TryGetBase(out var value) ? value : defaultValue;
    }

    /// <summary>
    /// Returns true when the typed key has an explicit base value or is computed.
    /// </summary>
    public bool Has<T>(DataKey<T> key)
    {
        var slot = GetOrCreateSlot(key);
        return key.IsComputed || slot.HasBaseValue;
    }

    /// <summary>
    /// Removes a typed base value and its modifiers.
    /// </summary>
    public bool Remove<T>(DataKey<T> key)
    {
        if (!slots.TryGetValue(key.Id, out var slot) || !slot.Remove(out var oldValue))
        {
            return false;
        }

        MarkDirty(key);
        NotifyChanged(key, oldValue, null);
        return true;
    }

    /// <summary>
    /// Adds a numeric value to an existing base value.
    /// </summary>
    public void Add<T>(DataKey<T> key, T delta) where T : INumber<T>
    {
        Set(key, GetBase(key, key.DefaultValue) + delta);
    }

    /// <summary>
    /// Multiplies an existing base value.
    /// </summary>
    public void Multiply<T>(DataKey<T> key, T factor) where T : INumber<T>
    {
        Set(key, GetBase(key, key.DefaultValue) * factor);
    }

    /// <summary>
    /// Adds a modifier to a typed numeric key.
    /// </summary>
    public bool AddModifier<T>(DataKey<T> key, DataModifier modifier)
    {
        return AddModifier((IDataKey)key, modifier);
    }

    /// <summary>
    /// Adds a modifier to a catalog-resolved numeric key.
    /// </summary>
    public bool AddModifier(IDataKey key, DataModifier modifier)
    {
        if (!key.SupportsModifiers || !key.IsNumeric)
        {
            return false;
        }

        var slot = GetOrCreateSlot(key);
        if (!slot.AddModifier(modifier))
        {
            return false;
        }

        MarkDirty(key);
        NotifyChanged(key, null, slot.GetBoxed(this));
        return true;
    }

    /// <summary>
    /// Removes a modifier by id.
    /// </summary>
    public bool RemoveModifier<T>(DataKey<T> key, string modifierId)
    {
        return RemoveModifier((IDataKey)key, modifierId);
    }

    /// <summary>
    /// Removes a modifier by id.
    /// </summary>
    public bool RemoveModifier(IDataKey key, string modifierId)
    {
        if (!slots.TryGetValue(key.Id, out var slot) || !slot.RemoveModifier(modifierId))
        {
            return false;
        }

        MarkDirty(key);
        NotifyChanged(key, null, slot.GetBoxed(this));
        return true;
    }

    /// <summary>
    /// Removes every modifier whose source matches the supplied source object.
    /// </summary>
    public int RemoveModifiersBySource(object source)
    {
        ArgumentNullException.ThrowIfNull(source);
        var removedTotal = 0;
        foreach (var pair in slots)
        {
            var removed = pair.Value.RemoveModifiersBySource(source);
            if (removed <= 0)
            {
                continue;
            }

            removedTotal += removed;
            MarkDirty(pair.Value.Key);
            NotifyChanged(pair.Value.Key, null, pair.Value.GetBoxed(this));
        }

        return removedTotal;
    }

    /// <summary>
    /// Gets a copy of all modifiers for a key.
    /// </summary>
    public IReadOnlyList<DataModifier> GetModifiers<T>(DataKey<T> key)
    {
        return slots.TryGetValue(key.Id, out var slot) ? slot.GetModifiers() : Array.Empty<DataModifier>();
    }

    /// <summary>
    /// Resets existing values in a category to runtime defaults.
    /// </summary>
    public void ResetByCategory(Enum category)
    {
        var keys = catalog.GetByCategory(category);
        for (var i = 0; i < keys.Count; i++)
        {
            var key = keys[i];
            if (!slots.TryGetValue(key.Id, out var slot) || !slot.HasBaseValue)
            {
                continue;
            }

            TryApplyUntyped(key, key.DefaultValueBoxed, out var oldValue, out var newValue);
            NotifyChanged(key, oldValue, newValue);
        }
    }

    /// <summary>
    /// Clears runtime values, modifiers and caches.
    /// </summary>
    public void Reset()
    {
        slots.Clear();
    }

    /// <summary>
    /// Gets a copy of all explicit base values keyed by stable DataKey.
    /// </summary>
    public Dictionary<string, object?> GetAll()
    {
        var result = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var pair in slots)
        {
            var slot = pair.Value;
            if (slot.HasBaseValue)
            {
                result[slot.Key.StableKey] = slot.BaseValueBoxed;
            }
        }

        return result;
    }

    /// <summary>
    /// Catalog/snapshot 边界使用的 typed apply 入口。
    /// </summary>
    internal bool TryApplyUntyped(IDataKey key, object? value, out object? oldValue, out object? newValue)
    {
        var slot = GetOrCreateSlot(key);
        if (!slot.TrySetBoxed(value, out oldValue, out newValue))
        {
            return false;
        }

        MarkDirty(key);
        NotifyChanged(key, oldValue, newValue);
        return true;
    }

    private DataSlot<T> GetOrCreateSlot<T>(DataKey<T> key)
    {
        if (!slots.TryGetValue(key.Id, out var slot))
        {
            slot = key.CreateSlot();
            slots.Add(key.Id, slot);
        }

        return (DataSlot<T>)slot;
    }

    private IDataSlot GetOrCreateSlot(IDataKey key)
    {
        if (!slots.TryGetValue(key.Id, out var slot))
        {
            slot = key.CreateSlot();
            slots.Add(key.Id, slot);
        }

        return slot;
    }

    private void MarkDirty(IDataKey key)
    {
        if (slots.TryGetValue(key.Id, out var slot))
        {
            slot.MarkDirty();
        }

        var dependents = catalog.GetDependentKeyIds(key.Id);
        for (var i = 0; i < dependents.Count; i++)
        {
            if (slots.TryGetValue(dependents[i], out var dependentSlot))
            {
                dependentSlot.MarkDirty();
            }
        }
    }

    private void NotifyChanged(IDataKey key, object? oldValue, object? newValue)
    {
        changeSink?.OnDataChanged(new DataChangedEventData(key, oldValue, newValue));
    }
}
