namespace SkilmeAI.GameOS.Runtime.Resource;

/// <summary>
/// Coarse resource categories used by generated or hand-authored resource maps.
/// </summary>
public enum ResourceCategory
{
    /// <summary>Entity scenes or runtime entity resources.</summary>
    Entity,
    /// <summary>Component scenes or resources.</summary>
    Component,
    /// <summary>System scenes or resources.</summary>
    System,
    /// <summary>Tool scenes or resources.</summary>
    Tools,
    /// <summary>UI scenes or resources.</summary>
    UI,
    /// <summary>General asset resources.</summary>
    Asset,
    /// <summary>Runtime or authoring data resources.</summary>
    Data,
    /// <summary>Configuration resources.</summary>
    Config,
    /// <summary>Test scenes or resources.</summary>
    Test,
    /// <summary>Fallback category.</summary>
    Other
}
