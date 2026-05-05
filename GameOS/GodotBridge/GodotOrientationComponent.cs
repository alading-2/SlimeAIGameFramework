using System;
using Godot;
using SkilmeAI.GameOS.Capabilities.Movement;
using SkilmeAI.GameOS.Runtime.Entity;
using SkilmeAI.GameOS.Runtime.Event;

namespace SkilmeAI.GameOS.GodotBridge;

/// <summary>
/// Godot 2D 朝向输出组件：消费 Movement FacingDirection 并写入 RotationDegrees 或 FlipH。
/// </summary>
public partial class GodotOrientationComponent : Node, IGodotComponent
{
    private IEntity? entity;
    private Node2D? ownerNode;
    private AnimatedSprite2D? visualSprite;
    private Action<GameEventType.Movement.StartedEventData>? movementStartedHandler;
    private Action<GameEventType.Movement.StoppedEventData>? movementStoppedHandler;
    private OrientationMode activeMode = OrientationMode.FollowMovement;
    private float baseAngle;
    private float currentAngularSpeed;
    private float angularAcceleration;
    private float totalAngle = -1f;
    private float initialAngle;
    private float accumulatedAngle;
    private bool isClockwise = true;
    private bool isActive = true;

    /// <summary>朝向输出目标。</summary>
    [Export]
    public OrientationSink Sink { get; set; } = OrientationSink.RootRotation;

    /// <summary>默认朝向模式。</summary>
    [Export]
    public OrientationMode Mode { get; set; } = OrientationMode.FollowMovement;

    /// <summary>默认自转角速度，单位度/秒。</summary>
    [Export]
    public float AngularSpeed { get; set; } = 0f;

    /// <summary>默认自转角加速度，单位度/秒^2。</summary>
    [Export]
    public float AngularAcceleration { get; set; } = 0f;

    /// <summary>默认总自转角度，-1 表示不限制。</summary>
    [Export]
    public float TotalAngle { get; set; } = -1f;

    /// <summary>默认初始角度偏移，单位度。</summary>
    [Export]
    public float InitialAngle { get; set; } = 0f;

    /// <summary>默认是否顺时针自转。</summary>
    [Export]
    public bool IsClockwise { get; set; } = true;

    /// <inheritdoc />
    public void OnComponentRegistered(IEntity entity, Node entityNode)
    {
        if (entityNode is not Node2D node)
        {
            return;
        }

        this.entity = entity;
        ownerNode = node;
        visualSprite = ResolveVisualSprite(node);
        movementStartedHandler = OnMovementStarted;
        movementStoppedHandler = OnMovementStopped;
        entity.Events.On(GameEventType.Movement.Started, movementStartedHandler);
        entity.Events.On(GameEventType.Movement.Stopped, movementStoppedHandler);
        ApplyOrientation(ReadExportedParams());
    }

    /// <inheritdoc />
    public void OnComponentUnregistered(IEntity? entity, Node? entityNode)
    {
        if (this.entity != null && movementStartedHandler != null)
        {
            this.entity.Events.Off(GameEventType.Movement.Started, movementStartedHandler);
        }

        if (this.entity != null && movementStoppedHandler != null)
        {
            this.entity.Events.Off(GameEventType.Movement.Stopped, movementStoppedHandler);
        }

        movementStartedHandler = null;
        movementStoppedHandler = null;
        visualSprite = null;
        ownerNode = null;
        this.entity = null;
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        if (!isActive || entity == null || ownerNode == null)
        {
            return;
        }

        var finalAngle = ResolveBaseAngle() + ResolveSpinOffset((float)delta);
        ApplyOutput(finalAngle);
    }

    private void OnMovementStarted(GameEventType.Movement.StartedEventData data)
    {
        ApplyOrientation(data.Params.Orientation ?? ReadExportedParams());
    }

    private void OnMovementStopped(GameEventType.Movement.StoppedEventData data)
    {
        if (activeMode == OrientationMode.SpinOnly || totalAngle >= 0f)
        {
            isActive = false;
        }
    }

    private OrientationParams ReadExportedParams()
    {
        return new OrientationParams
        {
            Mode = Mode,
            AngularSpeed = AngularSpeed,
            AngularAcceleration = AngularAcceleration,
            TotalAngle = TotalAngle,
            InitialAngle = InitialAngle,
            IsClockwise = IsClockwise
        };
    }

    private void ApplyOrientation(OrientationParams orientation)
    {
        activeMode = orientation.Mode;
        currentAngularSpeed = MathF.Max(0f, orientation.AngularSpeed);
        angularAcceleration = orientation.AngularAcceleration;
        totalAngle = orientation.TotalAngle;
        initialAngle = orientation.InitialAngle;
        accumulatedAngle = 0f;
        isClockwise = orientation.IsClockwise;
        baseAngle = ResolveInitialBaseAngle(activeMode);
        isActive = true;
    }

    private float ResolveInitialBaseAngle(OrientationMode mode)
    {
        if (ownerNode == null || entity == null)
        {
            return 0f;
        }

        if (mode == OrientationMode.SpinOnly)
        {
            return GetCurrentPresentedAngle();
        }

        var facing = entity.Data.Get<Vector2Value>(MovementDataKeys.FacingDirection, Vector2Value.Zero);
        return facing.Length > 0.001f ? AngleFromDirection(facing) : GetCurrentPresentedAngle();
    }

    private float ResolveBaseAngle()
    {
        if (entity == null || activeMode == OrientationMode.SpinOnly)
        {
            return baseAngle;
        }

        var facing = entity.Data.Get<Vector2Value>(MovementDataKeys.FacingDirection, Vector2Value.Zero);
        if (facing.Length <= 0.001f)
        {
            return baseAngle;
        }

        baseAngle = AngleFromDirection(facing);
        return baseAngle;
    }

    private float ResolveSpinOffset(float delta)
    {
        if (activeMode == OrientationMode.FollowMovement || Sink == OrientationSink.VisualFlipX)
        {
            return initialAngle;
        }

        if (MathF.Abs(angularAcceleration) > 0.001f)
        {
            currentAngularSpeed = MathF.Max(0f, currentAngularSpeed + (angularAcceleration * delta));
        }

        var deltaAngle = currentAngularSpeed * delta;
        if (totalAngle >= 0f)
        {
            var remainingAngle = MathF.Max(0f, totalAngle - accumulatedAngle);
            deltaAngle = MathF.Min(deltaAngle, remainingAngle);
        }

        accumulatedAngle += deltaAngle;
        if (totalAngle >= 0f && accumulatedAngle >= totalAngle - 0.001f)
        {
            isActive = false;
        }

        return initialAngle + (isClockwise ? accumulatedAngle : -accumulatedAngle);
    }

    private void ApplyOutput(float angle)
    {
        if (ownerNode == null)
        {
            return;
        }

        if (Sink == OrientationSink.VisualFlipX)
        {
            ApplyVisualFlip(angle);
            return;
        }

        ownerNode.RotationDegrees = angle;
    }

    private void ApplyVisualFlip(float angle)
    {
        if (visualSprite == null)
        {
            return;
        }

        var radians = angle * (MathF.PI / 180f);
        var x = MathF.Cos(radians);
        if (MathF.Abs(x) < 0.1f)
        {
            return;
        }

        visualSprite.FlipH = x < 0f;
    }

    private float GetCurrentPresentedAngle()
    {
        if (ownerNode == null)
        {
            return baseAngle;
        }

        if (Sink == OrientationSink.VisualFlipX)
        {
            return visualSprite?.FlipH == true ? 180f : 0f;
        }

        return ownerNode.RotationDegrees;
    }

    private static float AngleFromDirection(Vector2Value direction)
    {
        return MathF.Atan2(direction.Y, direction.X) * (180f / MathF.PI);
    }

    private static AnimatedSprite2D? ResolveVisualSprite(Node2D node)
    {
        if (node.GetNodeOrNull("VisualRoot") is AnimatedSprite2D directSprite)
        {
            return directSprite;
        }

        var descendants = node.FindChildren("*", nameof(AnimatedSprite2D), recursive: true, owned: false);
        return descendants.Count > 0 ? descendants[0] as AnimatedSprite2D : null;
    }
}
