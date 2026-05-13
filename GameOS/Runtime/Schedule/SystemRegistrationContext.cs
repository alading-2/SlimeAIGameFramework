namespace SlimeAI.GameOS.Runtime.Schedule;

/// <summary>
/// 系统注册上下文。
/// </summary>
public sealed class SystemRegistrationContext
{
    /// <summary>
    /// 创建系统注册上下文。
    /// </summary>
    /// <param name="descriptor">当前系统描述符。</param>
    /// <param name="projectState">项目状态服务。</param>
    public SystemRegistrationContext(SystemDescriptor descriptor, ProjectStateService projectState)
    {
        Descriptor = descriptor;
        ProjectState = projectState;
    }

    /// <summary>当前系统描述符。</summary>
    public SystemDescriptor Descriptor { get; }

    /// <summary>项目状态服务。</summary>
    public ProjectStateService ProjectState { get; }
}
