using System;

namespace SkilmeAI.GameOS.Runtime.Schedule;

/// <summary>
/// 系统描述符，只保存 SystemId 和实例工厂。
/// </summary>
public sealed class SystemDescriptor
{
    /// <summary>
    /// 创建系统描述符。
    /// </summary>
    /// <param name="systemId">系统唯一 Id。</param>
    /// <param name="factory">系统实例工厂。</param>
    public SystemDescriptor(string systemId, Func<object> factory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(systemId);
        ArgumentNullException.ThrowIfNull(factory);

        SystemId = systemId;
        Factory = factory;
    }

    /// <summary>系统唯一 Id。</summary>
    public string SystemId { get; }

    /// <summary>系统实例工厂。</summary>
    public Func<object> Factory { get; }
}
