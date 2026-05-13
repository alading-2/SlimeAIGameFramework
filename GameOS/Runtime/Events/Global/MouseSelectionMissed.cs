using Godot;
using SlimeAI.GameOS.Runtime.Event;

namespace SlimeAI.GameOS.Runtime.Events.Global;

/// <summary>鼠标选择未命中。</summary>
public readonly record struct MouseSelectionMissed(
    Vector2 ScreenPosition,
    Vector2 WorldPosition,
    Rect2 ScreenRect,
    MouseSelectionInteractionKind InteractionKind) : IGlobalEvent;
