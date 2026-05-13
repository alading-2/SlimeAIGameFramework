using System;
using System.Collections.Generic;
using Godot;
using SkilmeAI.GameOS.Capabilities.Effect;
using SkilmeAI.GameOS.Capabilities.Effect.Events;
using SkilmeAI.GameOS.Capabilities.Movement;
using SkilmeAI.GameOS.Capabilities.Projectile;
using ProjectileSpawned = SkilmeAI.GameOS.Capabilities.Projectile.Events.Spawned;
using EffectSpawned = SkilmeAI.GameOS.Capabilities.Effect.Events.Spawned;
using SkilmeAI.GameOS.Runtime.Entity;
using SkilmeAI.GameOS.Runtime.Event;
using SkilmeAI.GameOS.Runtime.Events.Core;
using SkilmeAI.GameOS.Runtime.Resource;

namespace SkilmeAI.GameOS.GodotBridge;

/// <summary>
/// 监听 Projectile / Effect Runtime 生成事件，并在 Godot SceneTree 中实例化对应视觉节点。
/// </summary>
public partial class GodotProjectileEffectSpawner : Node
{
    private IDisposable? projectileSpawnedToken;
    private IDisposable? effectSpawnedToken;
    private IDisposable? entityDestroyedToken;
    private readonly HashSet<string> spawnedVisualEntityIds = new(StringComparer.Ordinal);

    /// <summary>
    /// 是否进入 SceneTree 后自动订阅 Runtime 事件。
    /// </summary>
    [Export]
    public bool AutoSubscribe { get; set; } = true;

    /// <summary>
    /// 投射物节点挂载父节点路径；为空时挂到本节点。
    /// </summary>
    [Export]
    public NodePath? ProjectileParentPath { get; set; }

    /// <summary>
    /// 效果节点挂载父节点路径；为空时挂到本节点。
    /// </summary>
    [Export]
    public NodePath? EffectParentPath { get; set; }

    /// <inheritdoc />
    public override void _EnterTree()
    {
        if (AutoSubscribe)
        {
            Subscribe();
        }
    }

    /// <inheritdoc />
    public override void _ExitTree()
    {
        Unsubscribe();
    }

    /// <summary>
    /// 手动订阅 Projectile / Effect 运行时事件。
    /// </summary>
    public void Subscribe()
    {
        if (projectileSpawnedToken != null)
        {
            return;
        }

        projectileSpawnedToken = WorldEvents.World.Subscribe<ProjectileSpawned>(OnProjectileSpawned);
        effectSpawnedToken = WorldEvents.World.Subscribe<EffectSpawned>(OnEffectSpawned);
        entityDestroyedToken = WorldEvents.World.Subscribe<EntityDestroyed>(OnEntityDestroyed);
    }

    /// <summary>
    /// 手动取消订阅 Projectile / Effect 运行时事件。
    /// </summary>
    public void Unsubscribe()
    {
        projectileSpawnedToken?.Dispose();
        effectSpawnedToken?.Dispose();
        entityDestroyedToken?.Dispose();
        projectileSpawnedToken = null;
        effectSpawnedToken = null;
        entityDestroyedToken = null;
    }

    private void OnProjectileSpawned(ProjectileSpawned data)
    {
        var scenePath = data.Projectile.Data.Get(ProjectileDataKeys.ScenePath, string.Empty);
        var position = data.Projectile.Data.Get<Vector2Value>(MovementDataKeys.Position, Vector2Value.Zero);
        InstantiateVisual(data.Projectile, scenePath, position, ResolveParent(ProjectileParentPath), "Projectile");
    }

    private void OnEffectSpawned(EffectSpawned data)
    {
        var scenePath = data.Effect.Data.Get(EffectDataKeys.ScenePath, string.Empty);
        var position = data.Effect.Data.Get<Vector2Value>(MovementDataKeys.Position, Vector2Value.Zero);
        var node = InstantiateVisual(data.Effect, scenePath, position, ResolveParent(EffectParentPath), "Effect");
        if (node != null)
        {
            PlayEffectAnimation(data.Effect, node);
        }
    }

    private void OnEntityDestroyed(EntityDestroyed data)
    {
        if (!spawnedVisualEntityIds.Remove(data.Entity.EntityId))
        {
            return;
        }

        var node = GodotNodeRegistry.GetNodeById(data.Entity.EntityId);
        if (node == null)
        {
            return;
        }

        GodotNodeRegistry.Unregister(node, data.Entity.EntityId);
        if (node.IsInsideTree() && !node.IsQueuedForDeletion())
        {
            node.QueueFree();
        }
    }

    private Node? InstantiateVisual(
        IEntity entity,
        string scenePath,
        Vector2Value position,
        Node parent,
        string kind)
    {
        if (string.IsNullOrWhiteSpace(scenePath))
        {
            return null;
        }

        var scene = ResourceManagement.LoadPath<PackedScene>(scenePath);
        if (scene == null)
        {
            GD.PushWarning($"GameOS {kind} scene load failed: {scenePath}");
            return null;
        }

        var node = scene.Instantiate();
        node.Name = $"{kind}_{entity.EntityId}";
        ApplyInitialPosition(node, position);
        parent.AddChild(node);
        if (GodotNodeRegistry.Register(node, entity.EntityId))
        {
            spawnedVisualEntityIds.Add(entity.EntityId);
            return node;
        }

        if (node.IsInsideTree() && !node.IsQueuedForDeletion())
        {
            node.QueueFree();
        }

        return null;
    }

    private Node ResolveParent(NodePath? path)
    {
        if (path == null || path.IsEmpty)
        {
            return this;
        }

        return GetNodeOrNull<Node>(path) ?? this;
    }

    private static void ApplyInitialPosition(Node node, Vector2Value position)
    {
        if (node is Node2D node2D)
        {
            node2D.Position = new Vector2(position.X, position.Y);
        }
    }

    private static void PlayEffectAnimation(IEntity effect, Node node)
    {
        var sprite = ResolveAnimatedSprite(node);
        if (sprite?.SpriteFrames == null)
        {
            return;
        }

        var animationName = ResolveEffectAnimationName(effect, sprite);
        if (string.IsNullOrWhiteSpace(animationName))
        {
            return;
        }

        var duration = effect.Data.Get<float>(EffectDataKeys.Duration, -1f);
        sprite.SpeedScale = ResolveSpeedScale(sprite, animationName, duration);
        sprite.Play(animationName);
    }

    private static string ResolveEffectAnimationName(IEntity effect, AnimatedSprite2D sprite)
    {
        var animationName = effect.Data.Get(EffectDataKeys.AnimationName, string.Empty);
        if (!string.IsNullOrWhiteSpace(animationName) && sprite.SpriteFrames?.HasAnimation(animationName) == true)
        {
            return animationName;
        }

        animationName = sprite.Animation.ToString();
        if (!string.IsNullOrWhiteSpace(animationName) && sprite.SpriteFrames?.HasAnimation(animationName) == true)
        {
            return animationName;
        }

        var names = sprite.SpriteFrames?.GetAnimationNames();
        if (names == null || names.Length == 0)
        {
            return string.Empty;
        }

        return names[0].ToString();
    }

    private static float ResolveSpeedScale(AnimatedSprite2D sprite, string animationName, float duration)
    {
        if (sprite.SpriteFrames == null || duration <= 0f)
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

    private static AnimatedSprite2D? ResolveAnimatedSprite(Node node)
    {
        if (node is AnimatedSprite2D sprite)
        {
            return sprite;
        }

        if (node.GetNodeOrNull("VisualRoot") is AnimatedSprite2D directSprite)
        {
            return directSprite;
        }

        var descendants = node.FindChildren("*", nameof(AnimatedSprite2D), recursive: true, owned: false);
        return descendants.Count > 0 ? descendants[0] as AnimatedSprite2D : null;
    }
}
