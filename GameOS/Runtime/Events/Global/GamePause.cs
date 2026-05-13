using SlimeAI.GameOS.Runtime.Event;

namespace SlimeAI.GameOS.Runtime.Events.Global;

/// <summary>游戏暂停。</summary>
public readonly record struct GamePause() : IGlobalEvent;
