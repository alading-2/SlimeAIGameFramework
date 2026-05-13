using System;
using SlimeAI.GameOS.Runtime.Pool;

namespace SlimeAI.GameOS.Runtime.Timer;

/// <summary>
/// 纯 C# 计时器管理器；Godot 项目后续由 Node _Process 桥接驱动。
/// </summary>
public sealed class TimerManager
{
    private readonly ObjectPool<GameTimer> timerPool;

    /// <summary>
    /// 进程级默认计时器管理器。
    /// </summary>
    public static TimerManager Instance { get; } = new("GameOS.TimerManager.Global");

    /// <summary>
    /// 创建计时器管理器。
    /// </summary>
    /// <param name="poolName">内部计时器对象池名称。</param>
    public TimerManager(string poolName = "GameOS.TimerManager")
    {
        timerPool = new ObjectPool<GameTimer>(
            static () => new GameTimer(),
            new ObjectPoolConfig
            {
                Name = poolName,
                InitialSize = 8,
                MaxSize = -1
            });
    }

    /// <summary>
    /// 创建一次性延迟计时器。
    /// </summary>
    /// <param name="duration">持续时间，单位秒。</param>
    /// <param name="useUnscaledTime">是否消费 unscaled delta。</param>
    public GameTimer Delay(float duration, bool useUnscaledTime = false)
    {
        return ConfigureTimer(duration, isLoop: false, useUnscaledTime);
    }

    /// <summary>
    /// Creates an unlimited loop timer.
    /// </summary>
    /// <param name="interval">Loop interval in seconds.</param>
    /// <param name="useUnscaledTime">Whether to consume unscaled delta.</param>
    public GameTimer Loop(float interval, bool useUnscaledTime = false)
    {
        return ConfigureTimer(interval, isLoop: true, useUnscaledTime, repeatCount: -1);
    }

    /// <summary>
    /// Creates a finite repeat timer.
    /// </summary>
    /// <param name="interval">Repeat interval in seconds.</param>
    /// <param name="count">Repeat count.</param>
    /// <param name="immediate">Whether first callback runs on the next tick.</param>
    /// <param name="useUnscaledTime">Whether to consume unscaled delta.</param>
    public GameTimer Repeat(float interval, int count, bool immediate = false, bool useUnscaledTime = false)
    {
        return ConfigureTimer(interval, isLoop: true, useUnscaledTime, repeatCount: Math.Max(0, count), immediate: immediate);
    }

    /// <summary>
    /// Creates a loop timer bounded by total duration.
    /// </summary>
    /// <param name="duration">Total duration in seconds.</param>
    /// <param name="interval">Countdown tick interval in seconds.</param>
    /// <param name="immediate">Whether first callback runs on the next tick.</param>
    /// <param name="useUnscaledTime">Whether to consume unscaled delta.</param>
    public GameTimer Countdown(float duration, float interval, bool immediate = false, bool useUnscaledTime = false)
    {
        return ConfigureTimer(interval, isLoop: true, useUnscaledTime, repeatCount: -1, totalDuration: duration, immediate: immediate);
    }

    /// <summary>
    /// Advances all active timers.
    /// </summary>
    /// <param name="delta">Scaled delta in seconds.</param>
    /// <param name="unscaledDelta">Unscaled delta in seconds; defaults to scaled delta when omitted.</param>
    public void Tick(float delta, float? unscaledDelta = null)
    {
        var rawUnscaledDelta = unscaledDelta ?? delta;
        timerPool.ForEachActive(timer =>
        {
            if (timer.IsDone)
            {
                timerPool.Release(timer);
                return;
            }

            timer.Update(timer.UseUnscaledTime ? rawUnscaledDelta : delta);
        });
    }

    /// <summary>
    /// Cancels a timer by id.
    /// </summary>
    /// <param name="id">Timer id.</param>
    public void Cancel(string id)
    {
        timerPool.ForEachActive(timer =>
        {
            if (timer.Id == id)
            {
                timer.Cancel();
            }
        });
    }

    /// <summary>
    /// Cancels timers by tag.
    /// </summary>
    /// <param name="tag">Timer tag.</param>
    public void CancelByTag(string tag)
    {
        timerPool.ForEachActive(timer =>
        {
            if (timer.Tag == tag)
            {
                timer.Cancel();
            }
        });
    }

    /// <summary>
    /// Pauses or resumes every active timer.
    /// </summary>
    /// <param name="paused">Pause state.</param>
    public void SetAllTimerPaused(bool paused)
    {
        timerPool.ForEachActive(timer => timer.SystemPaused = paused);
    }

    /// <summary>
    /// Number of active timers.
    /// </summary>
    public int ActiveCount => timerPool.ActiveCount;

    /// <summary>
    /// Releases every active timer.
    /// </summary>
    public void Clear()
    {
        timerPool.ReleaseAll();
    }

    private GameTimer ConfigureTimer(float duration, bool isLoop, bool useUnscaledTime, int repeatCount = -1, float totalDuration = 0f, bool immediate = false)
    {
        var timer = timerPool.Get();
        timer.Configure(duration, isLoop, useUnscaledTime, repeatCount, totalDuration, immediate);
        timer.Id = Guid.NewGuid().ToString("N");
        return timer;
    }
}
