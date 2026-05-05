using System;
using System.Collections.Generic;

namespace SkilmeAI.GameOS.Runtime.Data;

/// <summary>
/// Runtime metadata for a Data key.
/// </summary>
public sealed class DataMeta
{
    /// <summary>
    /// Runtime key name.
    /// </summary>
    public required string Key { get; init; }

    /// <summary>
    /// Display name used by tools.
    /// </summary>
    public string DisplayName { get; init; } = "";

    /// <summary>
    /// Runtime value type.
    /// </summary>
    public required Type Type { get; init; }

    /// <summary>
    /// Human-readable description.
    /// </summary>
    public string Description { get; init; } = "";

    /// <summary>
    /// Optional category enum for grouped reset and authoring UI.
    /// </summary>
    public Enum? Category { get; init; }

    /// <summary>
    /// Optional default value. When omitted, the type default is used.
    /// </summary>
    public object? DefaultValue { get; init; }

    /// <summary>
    /// Optional minimum numeric value.
    /// </summary>
    public float? MinValue { get; init; }

    /// <summary>
    /// Optional maximum numeric value.
    /// </summary>
    public float? MaxValue { get; init; }

    /// <summary>
    /// True when the value should be displayed as a percentage.
    /// </summary>
    public bool IsPercentage { get; init; }

    /// <summary>
    /// True when modifiers can be applied to this key.
    /// </summary>
    public bool? SupportModifiers { get; init; }

    /// <summary>
    /// True when Entity migration is allowed to copy this key.
    /// </summary>
    public bool CanMigrate { get; init; } = true;

    /// <summary>
    /// Optional icon path for authoring tools.
    /// </summary>
    public string IconPath { get; init; } = "";

    /// <summary>
    /// Optional indexed option names.
    /// </summary>
    public IReadOnlyList<string>? Options { get; init; }

    /// <summary>
    /// Data keys this computed key depends on.
    /// </summary>
    public string[]? Dependencies { get; init; }

    /// <summary>
    /// Computes a runtime value from a Data container.
    /// </summary>
    public Func<Data, object?>? Compute { get; init; }

    /// <summary>
    /// True when this metadata describes a numeric type.
    /// </summary>
    public bool IsNumeric => Type == typeof(int) || Type == typeof(float) || Type == typeof(double);

    /// <summary>
    /// True when this metadata describes a computed key.
    /// </summary>
    public bool IsComputed => Compute != null;

    /// <summary>
    /// True when this metadata has indexed options.
    /// </summary>
    public bool HasOptions => Options != null && Options.Count > 0;

    /// <summary>
    /// Gets the explicit default value or the type default.
    /// </summary>
    public object? GetDefaultValue()
    {
        return DefaultValue ?? GetTypeDefaultValue(Type);
    }

    /// <summary>
    /// Gets a default value for the supplied type.
    /// </summary>
    /// <param name="type">Runtime value type.</param>
    public static object? GetTypeDefaultValue(Type type)
    {
        if (type == typeof(int)) return 0;
        if (type == typeof(float)) return 0f;
        if (type == typeof(double)) return 0.0;
        if (type == typeof(bool)) return false;
        if (type == typeof(string)) return "";
        if (type.IsEnum) return Enum.GetValues(type).GetValue(0);
        if (type.IsValueType) return Activator.CreateInstance(type);
        return null;
    }

    /// <summary>
    /// Clamps numeric values to metadata bounds.
    /// </summary>
    /// <param name="value">Raw runtime value.</param>
    public object? Clamp(object? value)
    {
        if (value == null || !IsNumeric) return value;

        var numericValue = Convert.ToSingle(value);
        if (MinValue.HasValue) numericValue = Math.Max(numericValue, MinValue.Value);
        if (MaxValue.HasValue) numericValue = Math.Min(numericValue, MaxValue.Value);

        if (Type == typeof(int)) return (int)numericValue;
        if (Type == typeof(float)) return numericValue;
        if (Type == typeof(double)) return (double)numericValue;
        return value;
    }

    /// <summary>
    /// Returns true when the supplied option index is valid.
    /// </summary>
    /// <param name="value">Runtime option index.</param>
    public bool IsValidOption(object? value)
    {
        if (!HasOptions) return true;
        return value is int index && index >= 0 && index < Options!.Count;
    }

    /// <summary>
    /// Converts metadata to its key for ergonomic Data access.
    /// </summary>
    /// <param name="meta">Data key metadata.</param>
    public static implicit operator string(DataMeta meta) => meta.Key;
}

