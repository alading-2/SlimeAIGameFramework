using System;
using System.Collections.Generic;
using Godot;
using SkilmeAI.GameOS.Capabilities.Unit;
using SkilmeAI.GameOS.Capabilities.Unit.Events;
using SkilmeAI.GameOS.Runtime.Entity;

namespace SkilmeAI.GameOS.GodotBridge;

/// <summary>
/// Godot 单位动画桥：消费 Unit 动画事件并驱动 AnimatedSprite2D。
/// </summary>
public partial class GodotUnitAnimationComponent : Node, IGodotComponent
{
    private IEntity? entity;
    private AnimatedSprite2D? sprite;
    private IDisposable? playToken;
    private IDisposable? stopToken;
    private bool isOneShotAnimation;

    /// <summary>默认待机动画名。</summary>
    [Export]
    public string IdleAnimation { get; set; } = "idle";

    /// <summary>是否注册时立即播放待机动画。</summary>
    [Export]
    public bool PlayIdleOnRegister { get; set; } = true;

    /// <summary>当前动画名。</summary>
    public string CurrentAnimation { get; private set; } = string.Empty;

    /// <inheritdoc />
    public void OnComponentRegistered(IEntity entity, Node entityNode)
    {
        this.entity = entity;
        UnitDataKeys.RegisterAll();
        sprite = ResolveSprite(entityNode);
        playToken = entity.Events.Subscribe<PlayAnimationRequested>(OnPlayAnimationRequested);
        stopToken = entity.Events.Subscribe<StopAnimationRequested>(OnStopAnimationRequested);

        if (sprite != null)
        {
            sprite.AnimationFinished += OnAnimationFinished;
            CacheAvailableAnimations(entity, sprite);
            if (PlayIdleOnRegister)
            {
                Play(IdleAnimation, forceRestart: false);
            }
        }
    }

    /// <inheritdoc />
    public void OnComponentUnregistered(IEntity? entity, Node? entityNode)
    {
        playToken?.Dispose();
        stopToken?.Dispose();
        playToken = null;
        stopToken = null;

        if (sprite != null && GodotObject.IsInstanceValid(sprite))
        {
            sprite.AnimationFinished -= OnAnimationFinished;
        }

        sprite = null;
        this.entity = null;
        CurrentAnimation = string.Empty;
        isOneShotAnimation = false;
    }

    private void OnPlayAnimationRequested(PlayAnimationRequested data)
    {
        if (entity == null || data.Entity.EntityId != entity.EntityId)
        {
            return;
        }

        Play(data.AnimationName, data.ForceRestart, data.Duration);
    }

    private void OnStopAnimationRequested(StopAnimationRequested data)
    {
        if (entity == null || data.Entity.EntityId != entity.EntityId || sprite == null)
        {
            return;
        }

        sprite.Stop();
        sprite.SpeedScale = 1f;
        isOneShotAnimation = false;
        Play(IdleAnimation, forceRestart: true);
    }

    private void Play(string animationName, bool forceRestart = false, float duration = -1f)
    {
        if (sprite == null || sprite.SpriteFrames == null || string.IsNullOrWhiteSpace(animationName))
        {
            return;
        }

        if (!sprite.SpriteFrames.HasAnimation(animationName))
        {
            if (animationName != IdleAnimation)
            {
                Play(IdleAnimation, forceRestart);
            }

            return;
        }

        if (!forceRestart && CurrentAnimation == animationName && sprite.IsPlaying())
        {
            return;
        }

        CurrentAnimation = animationName;
        isOneShotAnimation = !sprite.SpriteFrames.GetAnimationLoop(animationName);
        sprite.SpeedScale = ResolveSpeedScale(animationName, duration);
        sprite.Play(animationName);
    }

    private float ResolveSpeedScale(string animationName, float duration)
    {
        if (sprite?.SpriteFrames == null || duration <= 0f)
        {
            return 1f;
        }

        var frameCount = sprite.SpriteFrames.GetFrameCount(animationName);
        var fps = sprite.SpriteFrames.GetAnimationSpeed(animationName);
        if (frameCount <= 0 || fps <= 0f)
        {
            return 1f;
        }

        return (float)(frameCount / fps / duration);
    }

    private void OnAnimationFinished()
    {
        if (entity == null || sprite == null || !isOneShotAnimation)
        {
            return;
        }

        var finishedAnimation = CurrentAnimation;
        isOneShotAnimation = false;
        entity.Events.Publish(new AnimationFinished(entity, finishedAnimation));

        if (finishedAnimation != IdleAnimation)
        {
            Play(IdleAnimation, forceRestart: true);
        }
    }

    private static void CacheAvailableAnimations(IEntity entity, AnimatedSprite2D sprite)
    {
        if (sprite.SpriteFrames == null)
        {
            return;
        }

        var names = new List<string>();
        foreach (var animationName in sprite.SpriteFrames.GetAnimationNames())
        {
            names.Add(animationName);
        }

        entity.Data.Set(UnitDataKeys.AvailableAnimations, names);
    }

    private static AnimatedSprite2D? ResolveSprite(Node entityNode)
    {
        if (entityNode.GetNodeOrNull("VisualRoot") is AnimatedSprite2D directSprite)
        {
            return directSprite;
        }

        var descendants = entityNode.FindChildren("*", nameof(AnimatedSprite2D), recursive: true, owned: false);
        return descendants.Count > 0 ? descendants[0] as AnimatedSprite2D : null;
    }
}
