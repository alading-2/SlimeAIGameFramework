namespace SkilmeAI.GameOS.Capabilities.Feature;

/// <summary>
/// Feature 处理器接口。
/// </summary>
public interface IFeatureHandler
{
    /// <summary>完整唯一 Handler Id。</summary>
    string FeatureId { get; }

    /// <summary>Feature 被授予。</summary>
    /// <param name="context">Feature 上下文。</param>
    void OnGranted(FeatureContext context) { }

    /// <summary>Feature 被移除。</summary>
    /// <param name="context">Feature 上下文。</param>
    void OnRemoved(FeatureContext context) { }

    /// <summary>Feature 被启用。</summary>
    /// <param name="context">Feature 上下文。</param>
    void OnEnabled(FeatureContext context) { }

    /// <summary>Feature 被禁用。</summary>
    /// <param name="context">Feature 上下文。</param>
    void OnDisabled(FeatureContext context) { }

    /// <summary>Feature 单次运行开始。</summary>
    /// <param name="context">Feature 上下文。</param>
    void OnActivated(FeatureContext context) { }

    /// <summary>Feature 执行。</summary>
    /// <param name="context">Feature 上下文。</param>
    object? OnExecute(FeatureContext context) => null;

    /// <summary>Feature 单次运行结束。</summary>
    /// <param name="context">Feature 上下文。</param>
    /// <param name="reason">结束原因。</param>
    void OnEnded(FeatureContext context, FeatureEndReason reason) { }
}
