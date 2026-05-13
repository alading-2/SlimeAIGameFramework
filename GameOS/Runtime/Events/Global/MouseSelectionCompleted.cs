using System.Collections.Generic;
using Godot;
using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Event;

namespace SlimeAI.GameOS.Runtime.Events.Global;

/// <summary>鼠标选择交互类型。</summary>
public enum MouseSelectionInteractionKind
{
    /// <summary>单点点击选择。</summary>
    Click = 0,

    /// <summary>框选区域选择。</summary>
    Box = 1,
}

/// <summary>鼠标选择命中来源。</summary>
public enum MouseSelectionHitKind
{
    /// <summary>没有命中。</summary>
    None = 0,

    /// <summary>物理点选命中。</summary>
    PhysicsPoint = 1,

    /// <summary>距离回退命中。</summary>
    DistanceFallback = 2,

    /// <summary>框选矩形命中。</summary>
    BoxRect = 3,
}

/// <summary>鼠标选择完成。唯一 producer 是 GameOS MouseSelection 模块。</summary>
public readonly record struct MouseSelectionCompleted(
    IReadOnlyList<IEntity> Entities,
    IEntity? PrimaryEntity,
    Vector2 ScreenPosition,
    Vector2 WorldPosition,
    Rect2 ScreenRect,
    MouseSelectionHitKind HitKind,
    MouseSelectionInteractionKind InteractionKind) : IGlobalEvent;
