using Godot;
using SlimeAI.GameOS.Runtime.Timer;

namespace SlimeAI.GameOS.GodotBridge;

/// <summary>
/// 用 Godot `_Process` 驱动 GameOS 全局 TimerManager。
/// </summary>
public partial class GameOSTimerDriver : Node
{
    /// <summary>
    /// 是否启用 Tick。
    /// </summary>
    [Export]
    public bool TickEnabled { get; set; } = true;

    /// <summary>
    /// 当前驱动的计时器管理器。
    /// </summary>
    public TimerManager TimerManager { get; set; } = TimerManager.Instance;

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        if (!TickEnabled)
        {
            return;
        }

        var seconds = (float)delta;
        TimerManager.Tick(seconds, seconds);
    }
}
