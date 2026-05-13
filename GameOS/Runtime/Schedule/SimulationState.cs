using System;

namespace SlimeAI.GameOS.Runtime.Schedule;

/// <summary>
/// 项目级模拟状态。配置侧可按位组合，运行时快照只允许单一模拟状态。
/// </summary>
[Flags]
public enum SimulationState : byte
{
    /// <summary>运行条件中表示不限制模拟状态。</summary>
    None = 0,

    /// <summary>模拟正常推进。</summary>
    Running = 1 << 0,

    /// <summary>模拟暂停推进。</summary>
    Suspended = 1 << 1,

    /// <summary>所有已知模拟状态。</summary>
    Any = Running | Suspended
}
