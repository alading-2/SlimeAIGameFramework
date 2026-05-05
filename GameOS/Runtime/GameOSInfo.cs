namespace SkilmeAI.GameOS.Runtime;

/// <summary>
/// 暴露当前 GameOS 包身份和迁移阶段。
/// </summary>
public static class GameOSInfo
{
    /// <summary>
    /// 游戏仓库和生成文档使用的稳定包 Id。
    /// </summary>
    public const string PackageId = "SkilmeAI.GameOS";

    /// <summary>
    /// 迁移阶段包版本；包契约变化时更新。
    /// </summary>
    public const string Version = "0.1.0-alpha.0";

    /// <summary>
    /// 供 AI 任务路由读取的阶段标记。
    /// </summary>
    public const string Stage = "GodotBridge";
}
