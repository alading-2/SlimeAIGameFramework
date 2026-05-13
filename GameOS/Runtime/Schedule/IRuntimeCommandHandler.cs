namespace SlimeAI.GameOS.Runtime.Schedule;

/// <summary>
/// Runtime 系统命令处理器协议。
/// </summary>
public interface IRuntimeCommandHandler<in TRequest, out TResult>
{
    /// <summary>
    /// 执行系统命令。
    /// </summary>
    /// <param name="request">命令请求。</param>
    TResult Execute(TRequest request);
}
