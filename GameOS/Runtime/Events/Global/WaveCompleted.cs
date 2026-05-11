using SkilmeAI.GameOS.Runtime.Event;

namespace SkilmeAI.GameOS.Runtime.Events.Global;

/// <summary>波次完成。</summary>
public readonly record struct WaveCompleted(int WaveIndex) : IGlobalEvent;
