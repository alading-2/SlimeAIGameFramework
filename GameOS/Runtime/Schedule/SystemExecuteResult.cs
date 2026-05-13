namespace SlimeAI.GameOS.Runtime.Schedule;

/// <summary>
/// 系统命令执行结果。
/// </summary>
public readonly record struct SystemExecuteResult<TResult>(
    bool Success,
    TResult? Value,
    string Message)
{
    /// <summary>
    /// 创建成功结果。
    /// </summary>
    /// <param name="value">领域结果。</param>
    public static SystemExecuteResult<TResult> Ok(TResult value)
    {
        return new SystemExecuteResult<TResult>(true, value, string.Empty);
    }

    /// <summary>
    /// 创建阻断结果。
    /// </summary>
    /// <param name="message">阻断原因。</param>
    public static SystemExecuteResult<TResult> Blocked(string message)
    {
        return new SystemExecuteResult<TResult>(false, default, message);
    }
}
