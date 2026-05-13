using System;

namespace SlimeAI.GameOS.Runtime.Schedule;

/// <summary>
/// 游戏流程状态。配置侧可按位组合，运行时快照只允许单一流程状态。
/// </summary>
[Flags]
public enum GameFlowState : ushort
{
    /// <summary>运行条件中表示不限制流程状态。</summary>
    None = 0,

    /// <summary>框架启动中。</summary>
    Boot = 1 << 0,

    /// <summary>主菜单、配置页等非局内界面。</summary>
    FrontEnd = 1 << 1,

    /// <summary>单局准备中。</summary>
    SessionPreparing = 1 << 2,

    /// <summary>单局玩法进行中。</summary>
    SessionPlaying = 1 << 3,

    /// <summary>单局结算中。</summary>
    SessionResolving = 1 << 4,

    /// <summary>单局已结束。</summary>
    SessionEnded = 1 << 5,

    /// <summary>应用关闭流程。</summary>
    ShuttingDown = 1 << 6,

    /// <summary>前台流程。</summary>
    FrontEndFlow = Boot | FrontEnd,

    /// <summary>完整单局流程。</summary>
    Session = SessionPreparing | SessionPlaying | SessionResolving | SessionEnded,

    /// <summary>活跃单局流程。</summary>
    ActiveSession = SessionPreparing | SessionPlaying | SessionResolving,

    /// <summary>局内主玩法流程。</summary>
    Gameplay = SessionPlaying,

    /// <summary>常规运行流程。</summary>
    Runtime = FrontEnd | Session,

    /// <summary>所有已知流程状态。</summary>
    All = Boot | Runtime | ShuttingDown
}
