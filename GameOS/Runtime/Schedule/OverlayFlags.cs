using System;

namespace SkilmeAI.GameOS.Runtime.Schedule;

/// <summary>
/// 项目级覆盖层标记，可多选。
/// </summary>
[Flags]
public enum OverlayFlags : byte
{
    /// <summary>没有覆盖层。</summary>
    None = 0,

    /// <summary>暂停菜单覆盖层。</summary>
    PauseMenu = 1 << 0,

    /// <summary>模态 UI 覆盖层。</summary>
    ModalUi = 1 << 1,

    /// <summary>过场覆盖层。</summary>
    Cutscene = 1 << 2,

    /// <summary>交互 UI 覆盖层。</summary>
    InteractiveUi = PauseMenu | ModalUi,

    /// <summary>会阻塞局内主玩法的覆盖层。</summary>
    Blocking = PauseMenu | ModalUi | Cutscene
}
