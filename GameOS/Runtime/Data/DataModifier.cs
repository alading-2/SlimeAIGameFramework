using System;

namespace SkilmeAI.GameOS.Runtime.Data;

/// <summary>
/// Numeric modifier operation.
/// </summary>
public enum ModifierType
{
    /// <summary>
    /// Adds to the base value before multiplication.
    /// </summary>
    Additive,

    /// <summary>
    /// Multiplies the value after additive modifiers.
    /// </summary>
    Multiplicative,

    /// <summary>
    /// Adds to the value after multiplication.
    /// </summary>
    FinalAdditive,

    /// <summary>
    /// Replaces the value with the highest-priority override.
    /// </summary>
    Override,

    /// <summary>
    /// Limits the final value to the lowest cap.
    /// </summary>
    Cap,
}

/// <summary>
/// Runtime numeric modifier for Data values.
/// </summary>
public sealed class DataModifier
{
    /// <summary>
    /// Unique modifier id.
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    /// Modifier operation.
    /// </summary>
    public ModifierType Type { get; init; }

    /// <summary>
    /// Numeric modifier value.
    /// </summary>
    public float Value { get; init; }

    /// <summary>
    /// Lower values run earlier.
    /// </summary>
    public int Priority { get; init; }

    /// <summary>
    /// Optional source object used for bulk removal.
    /// </summary>
    public object? Source { get; init; }

    /// <summary>
    /// Creates a runtime Data modifier.
    /// </summary>
    /// <param name="type">Modifier operation.</param>
    /// <param name="value">Numeric modifier value.</param>
    /// <param name="priority">Sort priority. Lower values run earlier.</param>
    /// <param name="id">Stable modifier id. Generated when omitted.</param>
    /// <param name="source">Optional source object.</param>
    public DataModifier(ModifierType type, float value, int priority = 0, string? id = null, object? source = null)
    {
        Id = id ?? Guid.NewGuid().ToString();
        Type = type;
        Value = value;
        Priority = priority;
        Source = source;
    }
}
