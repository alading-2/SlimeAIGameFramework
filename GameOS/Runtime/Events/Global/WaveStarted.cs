using SlimeAI.GameOS.Runtime.Event;

namespace SlimeAI.GameOS.Runtime.Events.Global;

/// <summary>波次开始。</summary>
public readonly record struct WaveStarted(int WaveIndex) : IGlobalEvent;
