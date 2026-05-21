using System;
using System.Collections.Generic;
using Godot;
using SlimeAI.GameOS.Capabilities.Effect;
using SlimeAI.GameOS.Capabilities.Effect.Events;
using SlimeAI.GameOS.Capabilities.Movement;
using SlimeAI.GameOS.Capabilities.Projectile;
using ProjectileSpawned = SlimeAI.GameOS.Capabilities.Projectile.Events.Spawned;
using EffectSpawned = SlimeAI.GameOS.Capabilities.Effect.Events.Spawned;
using SlimeAI.GameOS.Runtime.Entity;
using SlimeAI.GameOS.Runtime.Event;
using SlimeAI.GameOS.Runtime.Events.Core;
using SlimeAI.GameOS.Runtime.Resource;
using SlimeAI.GameOS.Runtime.Timer;

namespace SlimeAI.GameOS.GodotBridge;

/// <summary>
/// 监听 Projectile / Effect Runtime 生成事件，并在 Godot SceneTree 中实例化对应视觉节点。
/// </summary>
public partial class GodotProjectileEffectSpawner : Node
{
    private IDisposable? projectileSpawnedToken;
    private IDisposable? effectSpawnedToken;
    private IDisposable? entityDestroyedToken;
    private readonly HashSet<EntityId> spawnedVisualEntityIds = new();
    private readonly Dictionary<string, GodotNodePool<Node>> visualPools = new(StringComparer.Ordinal);

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
            ScheduleFiniteEffectCleanup(data.Effect, node);
        }
    }

    private void OnEntityDestroyed(EntityDestroyed data)
    {
        if (!spawnedVisualEntityIds.Remove(data.Entity.EntityId))
        {
            return;
        }

        var node = GodotNodeRegistry.GetNodeById(data.Entity.EntityId.Value);
        if (node == null)
        {
            return;
        }

        GodotNodeRegistry.Unregister(node, data.Entity.EntityId.Value);
        if (GodotNodePoolManager.ReturnToPool(node))
        {
            return;
        }

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

        var pool = GetOrCreateVisualPool(scenePath, parent, kind);
        if (pool == null)
        {
            return null;
        }

        var node = pool.Get(activateNode: false);
        node.Name = $"{kind}_{entity.EntityId.Value}";
        ResetVisualNodeForAcquire(node);
        ApplyInitialPosition(node, position);
        node.SetMeta("GameOSVisualKind", kind);
        node.SetMeta("GameOSVisualScenePath", scenePath);
        node.SetMeta("GameOSVisualPoolName", pool.PoolName);
        if (GodotNodeRegistry.Register(node, entity.EntityId.Value))
        {
            spawnedVisualEntityIds.Add(entity.EntityId);
            pool.Activate(node);
            return node;
        }

        if (!GodotNodePoolManager.ReturnToPool(node) && node.IsInsideTree() && !node.IsQueuedForDeletion())
        {
            node.QueueFree();
        }

        return null;
    }

    private GodotNodePool<Node>? GetOrCreateVisualPool(string scenePath, Node parent, string kind)
    {
        var key = $"{kind}|{scenePath}";
        if (visualPools.TryGetValue(key, out var existing))
        {
            return existing;
        }

        var scene = ResourceManagement.LoadPath<PackedScene>(scenePath);
        if (scene == null)
        {
            GD.PushWarning($"GameOS {kind} scene load failed: {scenePath}");
            return null;
        }

        var pool = new GodotNodePool<Node>(
            () => scene.Instantiate(),
            new GodotNodePoolConfig
            {
                Name = BuildPoolName(kind, scenePath),
                InitialSize = 0,
                MaxSize = -1,
                ActiveParent = parent
            },
            ResetVisualNodeForRelease);
        visualPools[key] = pool;
        return pool;
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

    private static void ScheduleFiniteEffectCleanup(IEntity effect, Node node)
    {
        var lifetime = ResolveEffectLifetime(effect, node);
        if (lifetime <= 0f)
        {
            return;
        }

        var effectId = effect.EntityId;
        TimerManager.Instance.Delay(lifetime).OnComplete(() =>
        {
            if (EntityManager.Get(effectId) != null)
            {
                EntityManager.Destroy(effectId);
            }
        });
    }

    private static float ResolveEffectLifetime(IEntity effect, Node node)
    {
        var duration = effect.Data.Get<float>(EffectDataKeys.Duration, -1f);
        if (duration > 0f)
        {
            return duration;
        }

        var sprite = ResolveAnimatedSprite(node);
        if (sprite?.SpriteFrames == null)
        {
            return -1f;
        }

        var animationName = ResolveEffectAnimationName(effect, sprite);
        if (string.IsNullOrWhiteSpace(animationName))
        {
            return -1f;
        }

        return ResolveAnimationDuration(sprite, animationName);
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

    private static float ResolveAnimationDuration(AnimatedSprite2D sprite, string animationName)
    {
        if (sprite.SpriteFrames == null)
        {
            return -1f;
        }

        if (sprite.SpriteFrames.GetAnimationLoop(animationName))
        {
            return -1f;
        }

        var frameCount = sprite.SpriteFrames.GetFrameCount(animationName);
        var fps = sprite.SpriteFrames.GetAnimationSpeed(animationName);
        if (frameCount <= 0 || fps <= 0f)
        {
            return -1f;
        }

        return (float)(frameCount / fps);
    }

    private static void ResetVisualNodeForAcquire(Node node)
    {
        ResetVisualNodeForRelease(node);
        if (node is CanvasItem item)
        {
            item.Visible = true;
        }
    }

    private static void ResetVisualNodeForRelease(Node node)
    {
        if (node is Node2D node2D)
        {
            node2D.Position = Vector2.Zero;
            node2D.Rotation = 0f;
            node2D.Scale = Vector2.One;
        }

        if (node is CanvasItem item)
        {
            item.Modulate = new Color(1f, 1f, 1f, 1f);
            item.SelfModulate = new Color(1f, 1f, 1f, 1f);
        }

        var sprite = ResolveAnimatedSprite(node);
        if (sprite != null)
        {
            sprite.Stop();
            sprite.Frame = 0;
            sprite.FrameProgress = 0f;
            sprite.SpeedScale = 1f;
        }

        var players = node.FindChildren("*", nameof(AnimationPlayer), recursive: true, owned: false);
        for (var i = 0; i < players.Count; i++)
        {
            if (players[i] is AnimationPlayer player)
            {
                player.Stop();
            }
        }
    }

    private string BuildPoolName(string kind, string scenePath)
    {
        var suffix = SanitizePoolNameSegment(scenePath);
        return $"GameOS{kind}VisualPool_{GetInstanceId()}_{suffix}";
    }

    private static string SanitizePoolNameSegment(string value)
    {
        Span<char> buffer = stackalloc char[Math.Min(96, Math.Max(1, value.Length))];
        var index = 0;
        for (var i = 0; i < value.Length && index < buffer.Length; i++)
        {
            var c = value[i];
            buffer[index++] = (c >= 'A' && c <= 'Z')
                || (c >= 'a' && c <= 'z')
                || (c >= '0' && c <= '9')
                ? c
                : '_';
        }

        return index > 0 ? new string(buffer[..index]) : "visual";
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
