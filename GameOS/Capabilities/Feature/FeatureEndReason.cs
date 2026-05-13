namespace SlimeAI.GameOS.Capabilities.Feature;

/// <summary>
/// Feature 单次运行结束原因。
/// </summary>
public enum FeatureEndReason
{
    /// <summary>正常完成。</summary>
    Completed = 0,

    /// <summary>被取消。</summary>
    Cancelled = 1,

    /// <summary>失败。</summary>
    Failed = 2
}
