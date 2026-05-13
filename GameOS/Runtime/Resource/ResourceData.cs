namespace SlimeAI.GameOS.Runtime.Resource;

/// <summary>
/// Resource map entry.
/// </summary>
/// <param name="Category">Resource category.</param>
/// <param name="Path">Godot resource path.</param>
public readonly record struct ResourceData(ResourceCategory Category, string Path);
