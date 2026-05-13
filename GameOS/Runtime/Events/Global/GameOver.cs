using SlimeAI.GameOS.Runtime.Event;

namespace SlimeAI.GameOS.Runtime.Events.Global;

/// <summary>游戏结束。</summary>
public readonly record struct GameOver(bool IsVictory) : IGlobalEvent;
