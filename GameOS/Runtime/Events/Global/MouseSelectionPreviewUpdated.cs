using Godot;
using SkilmeAI.GameOS.Runtime.Event;

namespace SkilmeAI.GameOS.Runtime.Events.Global;

/// <summary>鼠标框选预览更新。</summary>
public readonly record struct MouseSelectionPreviewUpdated(
    Vector2 StartScreenPosition,
    Vector2 CurrentScreenPosition,
    Rect2 ScreenRect) : IGlobalEvent;
