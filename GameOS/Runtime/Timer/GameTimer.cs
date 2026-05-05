using System;
using SkilmeAI.GameOS.Runtime.Pool;

namespace SkilmeAI.GameOS.Runtime.Timer;

/// <summary>
/// Pooled runtime timer driven by TimerManager.Tick.
/// </summary>
public sealed class GameTimer : IPoolable
{
    private Action? onComplete;
    private Action? onLoop;
    private Action<int>? onRepeat;
    private Action<float, float>? onCountdown;
    private Action<float>? onUpdate;
    private bool shouldTriggerImmediately;
    private bool manualPaused;

    /// <summary>
    /// Unique timer id.
    /// </summary>
    public string Id { get; internal set; } = string.Empty;

    /// <summary>
    /// Current cycle duration in seconds.
    /// </summary>
    public float Duration { get; private set; }

    /// <summary>
    /// Current cycle elapsed time in seconds.
    /// </summary>
    public float Elapsed { get; private set; }

    /// <summary>
    /// Current cycle progress from 0 to 1.
    /// </summary>
    public float Progress => Duration > 0f ? Math.Clamp(Elapsed / Duration, 0f, 1f) : 1f;

    /// <summary>
    /// True for repeating timers.
    /// </summary>
    public bool IsLoop { get; private set; }

    /// <summary>
    /// True when the timer should use unscaled delta.
    /// </summary>
    public bool UseUnscaledTime { get; private set; }

    /// <summary>
    /// Manual or system pause state.
    /// </summary>
    public bool IsPaused
    {
        get => manualPaused || SystemPaused;
        set => manualPaused = value;
    }

    /// <summary>
    /// Pause flag controlled by systems.
    /// </summary>
    public bool SystemPaused { get; set; }

    /// <summary>
    /// True when the timer is complete or cancelled.
    /// </summary>
    public bool IsDone { get; private set; }

    /// <summary>
    /// True when Cancel ended the timer.
    /// </summary>
    public bool IsCancelled { get; private set; }

    /// <summary>
    /// Optional batch-management tag.
    /// </summary>
    public string? Tag { get; private set; }

    /// <summary>
    /// Remaining repeat count. -1 means unlimited.
    /// </summary>
    public int RepeatCount { get; private set; }

    /// <summary>
    /// Total countdown duration in seconds. 0 means disabled.
    /// </summary>
    public float TotalDuration { get; private set; }

    /// <summary>
    /// Elapsed time across loop cycles.
    /// </summary>
    public float TotalElapsed { get; private set; }

    /// <summary>
    /// Total countdown progress from 0 to 1.
    /// </summary>
    public float TotalProgress => TotalDuration > 0f ? Math.Clamp(TotalElapsed / TotalDuration, 0f, 1f) : 0f;

    /// <summary>
    /// Sets completion callback.
    /// </summary>
    /// <param name="callback">Callback invoked once when timer completes.</param>
    public GameTimer OnComplete(Action callback)
    {
        onComplete = callback;
        return this;
    }

    /// <summary>
    /// Sets loop callback.
    /// </summary>
    /// <param name="callback">Callback invoked every loop cycle.</param>
    public GameTimer OnLoop(Action callback)
    {
        onLoop = callback;
        return this;
    }

    /// <summary>
    /// Sets repeat callback.
    /// </summary>
    /// <param name="callback">Callback receives remaining repeat count.</param>
    public GameTimer OnRepeat(Action<int> callback)
    {
        onRepeat = callback;
        return this;
    }

    /// <summary>
    /// Sets countdown callback.
    /// </summary>
    /// <param name="callback">Callback receives total elapsed seconds and total progress.</param>
    public GameTimer OnCountdown(Action<float, float> callback)
    {
        onCountdown = callback;
        return this;
    }

    /// <summary>
    /// Sets progress callback.
    /// </summary>
    /// <param name="callback">Callback receives current cycle progress.</param>
    public GameTimer OnUpdate(Action<float> callback)
    {
        onUpdate = callback;
        return this;
    }

    /// <summary>
    /// Assigns a batch-management tag.
    /// </summary>
    /// <param name="tag">Timer tag.</param>
    public GameTimer WithTag(string tag)
    {
        Tag = tag;
        return this;
    }

    /// <summary>
    /// Triggers the first loop/repeat callback on the next tick.
    /// </summary>
    public GameTimer Immediate()
    {
        shouldTriggerImmediately = true;
        return this;
    }

    /// <inheritdoc />
    public void OnPoolRelease()
    {
        onComplete = null;
        onLoop = null;
        onUpdate = null;
        onRepeat = null;
        onCountdown = null;
    }

    /// <inheritdoc />
    public void OnPoolReset()
    {
        Reset();
    }

    /// <summary>
    /// Pauses the timer.
    /// </summary>
    public void Pause()
    {
        IsPaused = true;
    }

    /// <summary>
    /// Resumes the timer.
    /// </summary>
    public void Resume()
    {
        IsPaused = false;
    }

    /// <summary>
    /// Cancels the timer without completion callback.
    /// </summary>
    public void Cancel()
    {
        IsCancelled = true;
        IsDone = true;
    }

    /// <summary>
    /// Forces completion.
    /// </summary>
    /// <param name="triggerCallback">Whether OnComplete should run.</param>
    public void Complete(bool triggerCallback = true)
    {
        if (IsDone)
        {
            return;
        }

        Elapsed = Duration;
        IsDone = true;
        if (triggerCallback)
        {
            onComplete?.Invoke();
        }
    }

    internal void Configure(float duration, bool isLoop, bool useUnscaledTime, int repeatCount = -1, float totalDuration = 0f, bool immediate = false)
    {
        Duration = Math.Max(0f, duration);
        Elapsed = 0f;
        IsLoop = isLoop;
        UseUnscaledTime = useUnscaledTime;
        manualPaused = false;
        SystemPaused = false;
        IsDone = false;
        IsCancelled = false;
        RepeatCount = isLoop ? repeatCount : 0;
        TotalDuration = Math.Max(0f, totalDuration);
        TotalElapsed = 0f;
        shouldTriggerImmediately = immediate;
    }

    internal void Update(float delta)
    {
        if (IsDone || IsPaused)
        {
            return;
        }

        if (shouldTriggerImmediately)
        {
            shouldTriggerImmediately = false;
            onLoop?.Invoke();
            if (RepeatCount > 0)
            {
                RepeatCount--;
                onRepeat?.Invoke(RepeatCount);
                if (RepeatCount <= 0)
                {
                    Complete();
                    return;
                }
            }
        }

        Elapsed += Math.Max(0f, delta);
        onUpdate?.Invoke(Progress);

        if (Elapsed < Duration)
        {
            return;
        }

        if (!IsLoop)
        {
            Complete();
            return;
        }

        Elapsed -= Duration;
        TotalElapsed += Duration;
        onLoop?.Invoke();

        if (RepeatCount > 0)
        {
            RepeatCount--;
            onRepeat?.Invoke(RepeatCount);
            if (RepeatCount <= 0)
            {
                Complete();
                return;
            }
        }

        if (TotalDuration > 0f)
        {
            onCountdown?.Invoke(TotalElapsed, TotalProgress);
            if (TotalElapsed >= TotalDuration)
            {
                Complete();
                return;
            }
        }

        if (Elapsed >= Duration)
        {
            Elapsed = 0f;
        }
    }

    private void Reset()
    {
        Id = string.Empty;
        Duration = 0f;
        Elapsed = 0f;
        IsLoop = false;
        UseUnscaledTime = false;
        manualPaused = false;
        SystemPaused = false;
        IsDone = false;
        IsCancelled = false;
        Tag = null;
        RepeatCount = 0;
        TotalDuration = 0f;
        TotalElapsed = 0f;
        shouldTriggerImmediately = false;
    }
}
