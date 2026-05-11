using SkilmeAI.GameOS.Runtime.Event;

namespace SkilmeAI.GameOS.Capabilities.Movement.Events;

/// <summary>实体移动已停止。MovementSystem 是唯一 producer。</summary>
public readonly record struct Stopped(MovementStopContext Context) : IEntityEvent;
