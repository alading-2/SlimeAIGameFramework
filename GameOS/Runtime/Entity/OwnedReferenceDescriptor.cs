using SlimeAI.GameOS.Runtime.Data;

namespace SlimeAI.GameOS.Runtime.Entity;

/// <summary>
/// 描述一对 typed owner-child DataKey 配对：child 侧单引用 + owner 侧 EntityIdList。
/// </summary>
/// <param name="ChildToOwnerKey">child 实体上指向 owner 的 typed DataKey。</param>
/// <param name="OwnerListKey">owner 实体上的 owned-id-list typed DataKey。</param>
/// <remarks>
/// Capability 启动时 MUST 通过 <see cref="RuntimeOwnedReferenceRegistry.Register(OwnedReferenceDescriptor)"/>
/// 注册一次，framework 会在 <c>EntityManager.Destroy</c> 路径自动维持双向一致性。
/// </remarks>
public readonly record struct OwnedReferenceDescriptor(
    DataKey<EntityId?> ChildToOwnerKey,
    DataKey<EntityIdList> OwnerListKey);
