using System;
using System.Collections.Generic;

namespace SkilmeAI.GameOS.Runtime.Data;

/// <summary>
/// DataSlot 的非泛型内部桥。
/// </summary>
public interface IDataSlot
{
    /// <summary>对应 DataKey。</summary>
    IDataKey Key { get; }

    /// <summary>是否存在显式基础值。</summary>
    bool HasBaseValue { get; }

    /// <summary>基础值。</summary>
    object? BaseValueBoxed { get; }

    /// <summary>读取最终值。</summary>
    object? GetBoxed(Data owner);

    /// <summary>读取基础值或默认值。</summary>
    object? GetBaseBoxed();

    /// <summary>尝试读取显式基础值。</summary>
    bool TryGetBaseBoxed(out object? value);

    /// <summary>写入基础值。</summary>
    bool TrySetBoxed(object? value, out object? oldValue, out object? newValue);

    /// <summary>移除基础值、修改器和缓存。</summary>
    bool Remove(out object? oldValue);

    /// <summary>增加修改器。</summary>
    bool AddModifier(DataModifier modifier);

    /// <summary>按 id 移除修改器。</summary>
    bool RemoveModifier(string modifierId);

    /// <summary>按来源移除修改器。</summary>
    int RemoveModifiersBySource(object source);

    /// <summary>复制修改器列表。</summary>
    IReadOnlyList<DataModifier> GetModifiers();

    /// <summary>标记缓存脏。</summary>
    void MarkDirty();
}

/// <summary>
/// Typed Data 存储单元。
/// </summary>
public sealed class DataSlot<T> : IDataSlot
{
    private readonly DataKey<T> key;
    private readonly List<DataModifier> modifiers = new();
    private bool hasBaseValue;
    private T baseValue = default!;
    private bool dirty = true;
    private bool hasCachedValue;
    private T cachedValue = default!;

    /// <summary>
    /// 创建 typed slot。
    /// </summary>
    public DataSlot(DataKey<T> key)
    {
        this.key = key;
    }

    /// <inheritdoc />
    public IDataKey Key => key;

    /// <inheritdoc />
    public bool HasBaseValue => hasBaseValue;

    /// <inheritdoc />
    public object? BaseValueBoxed => hasBaseValue ? baseValue : null;

    /// <inheritdoc />
    public object? GetBoxed(Data owner)
    {
        return Get(owner);
    }

    /// <summary>
    /// 读取最终值。
    /// </summary>
    public T Get(Data owner)
    {
        if (key.Compute != null)
        {
            if (!dirty && hasCachedValue)
            {
                return cachedValue;
            }

            cachedValue = key.Clamp(key.Compute(owner));
            hasCachedValue = true;
            dirty = false;
            return cachedValue;
        }

        if (!hasBaseValue)
        {
            return key.DefaultValue;
        }

        if (!key.SupportsModifiers || modifiers.Count == 0)
        {
            return baseValue;
        }

        if (!dirty && hasCachedValue)
        {
            return cachedValue;
        }

        cachedValue = ApplyModifiers(baseValue);
        hasCachedValue = true;
        dirty = false;
        return cachedValue;
    }

    /// <inheritdoc />
    public object? GetBaseBoxed()
    {
        return hasBaseValue ? baseValue : key.DefaultValue;
    }

    /// <summary>
    /// 读取基础值或默认值。
    /// </summary>
    public T GetBase()
    {
        return hasBaseValue ? baseValue : key.DefaultValue;
    }

    /// <inheritdoc />
    public bool TryGetBaseBoxed(out object? value)
    {
        value = hasBaseValue ? baseValue : key.DefaultValue;
        return hasBaseValue;
    }

    /// <summary>
    /// 尝试读取显式基础值。
    /// </summary>
    public bool TryGetBase(out T value)
    {
        value = hasBaseValue ? baseValue : key.DefaultValue;
        return hasBaseValue;
    }

    /// <inheritdoc />
    public bool TrySetBoxed(object? value, out object? oldValue, out object? newValue)
    {
        oldValue = hasBaseValue ? baseValue : null;
        newValue = null;
        if (!key.TryConvert(value, out var typed))
        {
            return false;
        }

        var typedValue = (T)typed!;
        return TrySet(typedValue, out oldValue, out newValue);
    }

    /// <summary>
    /// 写入基础值。
    /// </summary>
    public bool TrySet(T value, out object? oldValue, out object? newValue)
    {
        oldValue = hasBaseValue ? baseValue : null;
        if (!key.IsValidOption(value))
        {
            newValue = null;
            return false;
        }

        var finalValue = key.Clamp(value);
        if (hasBaseValue && EqualityComparer<T>.Default.Equals(baseValue, finalValue))
        {
            newValue = finalValue;
            return false;
        }

        baseValue = finalValue;
        hasBaseValue = true;
        MarkDirty();
        newValue = finalValue;
        return true;
    }

    /// <inheritdoc />
    public bool Remove(out object? oldValue)
    {
        if (!hasBaseValue)
        {
            oldValue = null;
            return false;
        }

        oldValue = baseValue;
        hasBaseValue = false;
        baseValue = default!;
        modifiers.Clear();
        MarkDirty();
        return true;
    }

    /// <inheritdoc />
    public bool AddModifier(DataModifier modifier)
    {
        if (!key.SupportsModifiers || !key.IsNumeric)
        {
            return false;
        }

        for (var i = 0; i < modifiers.Count; i++)
        {
            if (modifiers[i].Id == modifier.Id)
            {
                return false;
            }
        }

        var insertIndex = modifiers.BinarySearch(modifier, ModifierPriorityComparer.Instance);
        if (insertIndex < 0) insertIndex = ~insertIndex;
        modifiers.Insert(insertIndex, modifier);
        MarkDirty();
        return true;
    }

    /// <inheritdoc />
    public bool RemoveModifier(string modifierId)
    {
        var removed = modifiers.RemoveAll(modifier => modifier.Id == modifierId);
        if (removed <= 0)
        {
            return false;
        }

        MarkDirty();
        return true;
    }

    /// <inheritdoc />
    public int RemoveModifiersBySource(object source)
    {
        var removed = modifiers.RemoveAll(modifier => ReferenceEquals(modifier.Source, source));
        if (removed > 0)
        {
            MarkDirty();
        }

        return removed;
    }

    /// <inheritdoc />
    public IReadOnlyList<DataModifier> GetModifiers()
    {
        return modifiers.ToArray();
    }

    /// <inheritdoc />
    public void MarkDirty()
    {
        dirty = true;
        hasCachedValue = false;
        cachedValue = default!;
    }

    private T ApplyModifiers(T value)
    {
        if (modifiers.Count == 0)
        {
            return value;
        }

        var baseNumeric = Convert.ToSingle(value);
        var additive = 0f;
        var multiplicative = 1f;
        var finalAdditive = 0f;
        float? overrideValue = null;
        float? cap = null;

        for (var i = 0; i < modifiers.Count; i++)
        {
            var modifier = modifiers[i];
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

        var result = overrideValue ?? (baseNumeric + additive) * multiplicative + finalAdditive;
        if (cap.HasValue) result = Math.Min(result, cap.Value);
        return key.Clamp((T)Convert.ChangeType(result, Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T)));
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
