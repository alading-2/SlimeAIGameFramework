using SkilmeAI.GameOS.Runtime.Event;

namespace SkilmeAI.GameOS.Runtime.Events.Global;

/// <summary>游戏结束。</summary>
public readonly record struct GameOver(bool IsVictory) : IGlobalEvent;
