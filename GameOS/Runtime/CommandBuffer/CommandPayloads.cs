using SlimeAI.GameOS.Runtime.Entity;

namespace SlimeAI.GameOS.Runtime.CommandBuffer;

/// <summary>延迟生成 Runtime Entity 的 typed payload。</summary>
public readonly record struct SpawnCommandPayload(EntitySpawnConfig Config);

/// <summary>延迟销毁 Runtime Entity 的 typed payload。</summary>
public readonly record struct DestroyCommandPayload(EntityId Target);

/// <summary>延迟挂接 lifecycle parent-child 的 typed payload。</summary>
public readonly record struct AttachCommandPayload(EntityId Parent, EntityId Child, ParentDestroyPolicy Policy, int Priority);

/// <summary>延迟断开 lifecycle parent-child 的 typed payload。</summary>
public readonly record struct DetachCommandPayload(EntityId Parent, EntityId Child);

/// <summary>延迟重放 framework-known global event 的 typed payload。</summary>
public readonly record struct QueuedEventCommandPayload(string EventTypeName, byte[] PayloadBytes);

/// <summary>延迟注册资源映射的 typed payload。</summary>
public readonly record struct ResourceRequestCommandPayload(string ResourceKey, string ResourcePath);

/// <summary>延迟请求 Godot 节点实例化的 typed payload。</summary>
public readonly record struct GodotNodeInstantiatePayload(string ScenePath, EntityId? AssociatedEntity);

/// <summary>延迟请求 Godot 节点释放的 typed payload。</summary>
public readonly record struct GodotNodeFreeCommandPayload(string GodotNodePath);
