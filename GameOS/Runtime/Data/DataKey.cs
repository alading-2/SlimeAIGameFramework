using System;
using System.Collections.Generic;

namespace SlimeAI.GameOS.Runtime.Data;

/// <summary>
/// DataKey 的非泛型只读边界，只供 catalog、snapshot、debug 和内部 slot 使用。
/// </summary>
public interface IDataKey
{
    /// <summary>稳定键名，用于 DataOS、snapshot 和 observation。</summary>
    string StableKey { get; }

    /// <summary>Catalog 本地 key id。</summary>
    int Id { get; }

    /// <summary>运行时值类型。</summary>
    Type ValueType { get; }

    /// <summary>运行时默认值。</summary>
    object? DefaultValueBoxed { get; }

    /// <summary>可选分类。</summary>
    Enum? Category { get; }

    /// <summary>可选数值下限。</summary>
    float? MinValue { get; }

    /// <summary>可选数值上限。</summary>
    float? MaxValue { get; }

    /// <summary>是否以百分比语义展示或校验。</summary>
    bool IsPercentage { get; }

    /// <summary>是否允许数值修改器。</summary>
    bool SupportsModifiers { get; }

    /// <summary>是否为计算键。</summary>
    bool IsComputed { get; }

    /// <summary>是否为数值类型。</summary>
    bool IsNumeric { get; }

    /// <summary>选项集合。</summary>
    IReadOnlyList<string> Options { get; }

    /// <summary>计算键依赖。</summary>
    IReadOnlyList<IDataKey> Dependencies { get; }

    /// <summary>创建对应 typed slot。</summary>
    IDataSlot CreateSlot();

    /// <summary>把外部值严格转换为该 key 的运行时类型。</summary>
    bool TryConvert(object? value, out object? typedValue);
}

/// <summary>
/// Runtime Data 的业务访问入口。
/// </summary>
/// <typeparam name="T">运行时值类型。</typeparam>
public sealed class DataKey<T> : IDataKey
{
    internal DataKey(
        string stableKey,
        T defaultValue,
        Enum? category,
        float? minValue,
        float? maxValue,
        bool isPercentage,
        bool supportsModifiers,
        IReadOnlyList<string>? options,
        IReadOnlyList<IDataKey>? dependencies,
        Func<Data, T>? compute)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stableKey);
        StableKey = stableKey;
        DefaultValue = defaultValue;
        Category = category;
        MinValue = minValue;
        MaxValue = maxValue;
        IsPercentage = isPercentage;
        SupportsModifiers = supportsModifiers;
        Options = options == null ? Array.Empty<string>() : new List<string>(options);
        Dependencies = dependencies == null ? Array.Empty<IDataKey>() : new List<IDataKey>(dependencies);
        Compute = compute;
        Id = DataKeyRegistry.Register(this);
    }

    /// <inheritdoc />
    public string StableKey { get; }

    /// <inheritdoc />
    public int Id { get; }

    /// <summary>运行时默认值。</summary>
    public T DefaultValue { get; }

    /// <inheritdoc />
    public Type ValueType => typeof(T);

    /// <inheritdoc />
    public object? DefaultValueBoxed => DefaultValue;

    /// <inheritdoc />
    public Enum? Category { get; }

    /// <inheritdoc />
    public float? MinValue { get; }

    /// <inheritdoc />
    public float? MaxValue { get; }

    /// <inheritdoc />
    public bool IsPercentage { get; }

    /// <inheritdoc />
    public bool SupportsModifiers { get; }

    /// <inheritdoc />
    public bool IsComputed => Compute != null;

    /// <inheritdoc />
    public bool IsNumeric => DataKey.IsNumericType(typeof(T));

    /// <inheritdoc />
    public IReadOnlyList<string> Options { get; }

    /// <inheritdoc />
    public IReadOnlyList<IDataKey> Dependencies { get; }

    /// <summary>计算函数。</summary>
    public Func<Data, T>? Compute { get; }

    /// <inheritdoc />
    public IDataSlot CreateSlot()
    {
        return new DataSlot<T>(this);
    }

    /// <inheritdoc />
    public bool TryConvert(object? value, out object? typedValue)
    {
        if (DataKey.TryConvert(value, typeof(T), out typedValue))
        {
            typedValue = ClampBoxed(typedValue);
            return true;
        }

        return false;
    }

    internal T Clamp(T value)
    {
        if (!IsNumeric || value == null)
        {
            return value;
        }

        var numeric = Convert.ToSingle(value);
        if (MinValue.HasValue) numeric = Math.Max(numeric, MinValue.Value);
        if (MaxValue.HasValue) numeric = Math.Min(numeric, MaxValue.Value);
        return (T)Convert.ChangeType(numeric, Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T));
    }

    internal bool IsValidOption(T value)
    {
        if (Options.Count == 0)
        {
            return true;
        }

        return value is int index && index >= 0 && index < Options.Count;
    }

    private object? ClampBoxed(object? value)
    {
        return value is T typed ? Clamp(typed) : value;
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return StableKey;
    }
}

/// <summary>
/// DataKey 构造辅助入口。
/// </summary>
public static class DataKey
{
    /// <summary>
    /// 创建并注册一个 typed DataKey。
    /// </summary>
    public static DataKey<T> Create<T>(
        string stableKey,
        T defaultValue = default!,
        Enum? category = null,
        float? minValue = null,
        float? maxValue = null,
        bool isPercentage = false,
        bool supportsModifiers = false,
        IReadOnlyList<string>? options = null,
        IReadOnlyList<IDataKey>? dependencies = null,
        Func<Data, T>? compute = null)
    {
        return new DataKey<T>(
            stableKey,
            defaultValue,
            category,
            minValue,
            maxValue,
            isPercentage,
            supportsModifiers,
            options,
            dependencies,
            compute);
    }

    internal static bool IsNumericType(Type type)
    {
        var actual = Nullable.GetUnderlyingType(type) ?? type;
        return actual == typeof(byte)
            || actual == typeof(sbyte)
            || actual == typeof(short)
            || actual == typeof(ushort)
            || actual == typeof(int)
            || actual == typeof(uint)
            || actual == typeof(long)
            || actual == typeof(ulong)
            || actual == typeof(float)
            || actual == typeof(double)
            || actual == typeof(decimal);
    }

    internal static object? GetTypeDefaultValue(Type type)
    {
        var actual = Nullable.GetUnderlyingType(type) ?? type;
        if (actual == typeof(string)) return string.Empty;
        if (actual.IsEnum) return Enum.GetValues(actual).GetValue(0);
        if (type.IsValueType) return Activator.CreateInstance(type);
        return null;
    }

    internal static bool TryConvert(object? value, Type targetType, out object? typedValue)
    {
        if (value == null)
        {
            typedValue = targetType.IsValueType && Nullable.GetUnderlyingType(targetType) == null
                ? GetTypeDefaultValue(targetType)
                : null;
            return !targetType.IsValueType || Nullable.GetUnderlyingType(targetType) != null;
        }

        var actualTarget = Nullable.GetUnderlyingType(targetType) ?? targetType;
        if (actualTarget.IsInstanceOfType(value))
        {
            typedValue = value;
            return true;
        }

        try
        {
            if (actualTarget.IsEnum)
            {
                typedValue = value is string enumText
                    ? Enum.Parse(actualTarget, enumText, ignoreCase: false)
                    : Enum.ToObject(actualTarget, Convert.ToInt64(value));
                return true;
            }

            var valueType = value.GetType();
            if (valueType.IsEnum)
            {
                typedValue = actualTarget == typeof(string)
                    ? value.ToString()
                    : Convert.ChangeType(Convert.ToInt64(value), actualTarget);
                return true;
            }

            typedValue = Convert.ChangeType(value, actualTarget);
            return true;
        }
        catch
        {
            typedValue = null;
            return false;
        }
    }
}
