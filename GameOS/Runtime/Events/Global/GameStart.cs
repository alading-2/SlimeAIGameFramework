using SlimeAI.GameOS.Runtime.Event;

namespace SlimeAI.GameOS.Runtime.Events.Global;

/// <summary>游戏开始。唯一 producer 是游戏主循环宿主。</summary>
public readonly record struct GameStart() : IGlobalEvent;
