using SkilmeAI.GameOS.Capabilities.Movement;
using SkilmeAI.GameOS.Runtime.Data;
using SkilmeAI.GameOS.Runtime.Entity;

namespace SkilmeAI.GameOS.Capabilities.Effect;

/// <summary>
/// Effect Capability 使用的运行时 DataKey。
/// </summary>
public static class EffectDataKeys
{
    /// <summary>效果资源路径。</summary>
    public static readonly DataMeta ScenePath = DataRegistry.Register(new DataMeta
    {
        Key = "Effect.ScenePath",
        DisplayName = "Effect Scene Path",
        Type = typeof(string),
        Category = EffectCategory.Basic,
        DefaultValue = string.Empty,
        Description = "效果 Godot 场景或资源路径。"
    });

    /// <summary>效果名称。</summary>
    public static readonly DataMeta Name = DataRegistry.Register(new DataMeta
    {
        Key = "Effect.Name",
        DisplayName = "Effect Name",
        Type = typeof(string),
        Category = EffectCategory.Basic,
        DefaultValue = string.Empty,
        Description = "效果显示或调试名称。"
    });

    /// <summary>效果播放动画名；为空时由 Godot 视觉节点使用当前动画或第一个可用动画。</summary>
    public static readonly DataMeta AnimationName = DataRegistry.Register(new DataMeta
    {
        Key = "Effect.AnimationName",
        DisplayName = "Effect Animation Name",
        Type = typeof(string),
        Category = EffectCategory.Basic,
        DefaultValue = string.Empty,
        Description = "效果视觉节点播放的动画名；为空时由桥接层自动选择。"
    });

    /// <summary>来源实体。</summary>
    public static readonly DataMeta SourceEntity = DataRegistry.Register(new DataMeta
    {
        Key = "Effect.SourceEntity",
        DisplayName = "Source Entity",
        Type = typeof(IEntity),
        Category = EffectCategory.Runtime,
        Description = "生成该效果的实体。"
    });

    /// <summary>关联技能实体。</summary>
    public static readonly DataMeta AbilityEntity = DataRegistry.Register(new DataMeta
    {
        Key = "Effect.AbilityEntity",
        DisplayName = "Ability Entity",
        Type = typeof(IEntity),
        Category = EffectCategory.Runtime,
        Description = "生成该效果的技能实体。"
    });

    /// <summary>目标实体。</summary>
    public static readonly DataMeta TargetEntity = DataRegistry.Register(new DataMeta
    {
        Key = "Effect.TargetEntity",
        DisplayName = "Target Entity",
        Type = typeof(IEntity),
        Category = EffectCategory.Runtime,
        Description = "效果附着或锁定的目标实体。"
    });

    /// <summary>效果位置。</summary>
    public static readonly DataMeta Position = DataRegistry.Register(new DataMeta
    {
        Key = "Effect.Position",
        DisplayName = "Effect Position",
        Type = typeof(Vector2Value),
        Category = EffectCategory.Runtime,
        DefaultValue = Vector2Value.Zero,
        Description = "效果生成位置。"
    });

    /// <summary>持续时间，-1 表示不自动结束。</summary>
    public static readonly DataMeta Duration = DataRegistry.Register(new DataMeta
    {
        Key = "Effect.Duration",
        DisplayName = "Effect Duration",
        Type = typeof(float),
        Category = EffectCategory.Basic,
        DefaultValue = -1f,
        MinValue = -1f,
        Description = "效果持续时间；-1 表示不自动结束。"
    });

    /// <summary>
    /// 显式触发静态 DataKey 注册。
    /// </summary>
    public static void RegisterAll()
    {
        _ = ScenePath;
        _ = Name;
        _ = AnimationName;
        _ = SourceEntity;
        _ = AbilityEntity;
        _ = TargetEntity;
        _ = Position;
        _ = Duration;
    }
}
