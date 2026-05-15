using System;

namespace SlimeAI.GameOS.Runtime.CommandBuffer;

/// <summary>
/// 结构变更受保护区域守卫。
/// </summary>
public interface IStructuralChangeGuard : IDisposable
{
    /// <summary>进入 guard 的原因标签。</summary>
    string Reason { get; }
}
