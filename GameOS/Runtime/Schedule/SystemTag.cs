using System;

namespace SlimeAI.GameOS.Runtime.Schedule;

/// <summary>
/// 系统逻辑标签，用于预设装载、查询和调试展示。
/// </summary>
[Flags]
public enum SystemTag : ulong
{
    /// <summary>无标签。</summary>
    None = 0,

    /// <summary>核心系统。</summary>
    Core = 1UL << 0,

    /// <summary>主玩法系统。</summary>
    Gameplay = 1UL << 1,

    /// <summary>战斗系统。</summary>
    Combat = 1UL << 2,

    /// <summary>UI 系统。</summary>
    UI = 1UL << 3,

    /// <summary>调试系统。</summary>
    Debug = 1UL << 4,

    /// <summary>测试系统。</summary>
    Test = 1UL << 5,

    /// <summary>Roguelike 玩法系统。</summary>
    Roguelike = 1UL << 6,

    /// <summary>运行时系统。</summary>
    Runtime = 1UL << 7
}
