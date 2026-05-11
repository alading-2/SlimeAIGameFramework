using SkilmeAI.GameOS.Runtime.Event;

namespace SkilmeAI.GameOS.Runtime.Events.Global;

/// <summary>游戏暂停。</summary>
public readonly record struct GamePause() : IGlobalEvent;
