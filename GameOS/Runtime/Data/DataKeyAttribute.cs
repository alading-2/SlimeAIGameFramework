using System;

namespace SkilmeAI.GameOS.Runtime.Data;

/// <summary>
/// Maps an authoring config property to a runtime Data key.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class DataKeyAttribute : Attribute
{
    /// <summary>
    /// Runtime Data key.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Creates a Data key mapping attribute.
    /// </summary>
    /// <param name="key">Runtime Data key.</param>
    public DataKeyAttribute(string key)
    {
        Key = key;
    }
}

