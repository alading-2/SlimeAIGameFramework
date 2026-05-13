namespace SlimeAI.GameOS.Runtime.Schedule;

/// <summary>
/// 系统挂载分组。纯 Runtime 阶段只用于排序、查询和 Debug。
/// </summary>
public enum SystemGroup : byte
{
    /// <summary>基础运行时系统。</summary>
    Base = 0,

    /// <summary>主玩法系统。</summary>
    Gameplay = 1,

    /// <summary>战斗系统。</summary>
    Combat = 2,

    /// <summary>UI 系统。</summary>
    UI = 3,

    /// <summary>调试系统。</summary>
    Debug = 4,

    /// <summary>测试系统。</summary>
    Test = 5,

    /// <summary>未分类系统。</summary>
    Else = 6
}
