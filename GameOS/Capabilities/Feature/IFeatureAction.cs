namespace SlimeAI.GameOS.Capabilities.Feature;

/// <summary>
/// Feature 原子执行单元。具体行为由框架通用代码或游戏侧 handler 提供。
/// </summary>
public interface IFeatureAction
{
    /// <summary>
    /// 执行一次 Feature action。
    /// </summary>
    /// <param name="context">Feature 生命周期上下文。</param>
    void Execute(FeatureContext context);
}
