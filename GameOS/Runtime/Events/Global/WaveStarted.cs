using SkilmeAI.GameOS.Runtime.Event;

namespace SkilmeAI.GameOS.Runtime.Events.Global;

/// <summary>波次开始。</summary>
public readonly record struct WaveStarted(int WaveIndex) : IGlobalEvent;
