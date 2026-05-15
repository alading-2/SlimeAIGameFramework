using System;
using System.Collections.Generic;

namespace SlimeAI.GameOS.Runtime.Entity;

/// <summary>
/// 业务 owner cleanup hook 注册表，framework 在 <c>EntityManager.Destroy</c>
/// 销毁完 lifecycle children、发布 <c>EntityDestroyed</c> 事件之前回调每个注册项，
/// 自动维持 owner 侧 <see cref="EntityIdList"/> 与 child 侧 <see cref="EntityId"/>? 的一致性。
/// </summary>
/// <remarks>
/// <para>
/// Capability 启动时 MUST 调用 <see cref="Register(OwnedReferenceDescriptor)"/> 注册 typed descriptor；
/// 复杂自定义清理可注册 <see cref="IOwnedReferenceCleaner"/>。
/// </para>
/// </remarks>
public static class RuntimeOwnedReferenceRegistry
{
    private static readonly List<OwnedReferenceDescriptor> Descriptors = new();
    private static readonly List<IOwnedReferenceCleaner> Cleaners = new();

    /// <summary>注册 typed owner ↔ owned-list descriptor。重复注册同一 descriptor 会被忽略。</summary>
    public static void Register(OwnedReferenceDescriptor descriptor)
    {
        if (descriptor.ChildToOwnerKey == null || descriptor.OwnerListKey == null)
        {
            return;
        }

        if (Descriptors.Contains(descriptor))
        {
            return;
        }

        Descriptors.Add(descriptor);
    }

    /// <summary>注册自定义 cleanup 实例。重复注册同一引用会被忽略。</summary>
    public static void Register(IOwnedReferenceCleaner cleaner)
    {
        ArgumentNullException.ThrowIfNull(cleaner);
        if (Cleaners.Contains(cleaner))
        {
            return;
        }

        Cleaners.Add(cleaner);
    }

    /// <summary>清空所有 descriptor 与 cleaner，用于 test cleanup 或 RuntimeWorld 重置。</summary>
    public static void Clear()
    {
        Descriptors.Clear();
        Cleaners.Clear();
    }

    /// <summary>
    /// 在 entity 销毁前被 <see cref="EntityManager.Destroy(EntityId)"/> 调用：
    /// 遍历每个 descriptor，从 owner 的 owned-id-list 中移除被销毁 entity；
    /// 然后调用每个 cleaner。任何一个 descriptor 失败不影响其它。
    /// </summary>
    internal static void NotifyDestroying(IEntity destroyed)
    {
        NotifyDestroying(destroyed, EntityManager.Get);
    }

    /// <summary>
    /// 在指定 world registry 的查找函数中执行 owner cleanup。
    /// </summary>
    internal static void NotifyDestroying(IEntity destroyed, Func<EntityId, IEntity?> resolveEntity)
    {
        if (destroyed == null)
        {
            return;
        }

        ArgumentNullException.ThrowIfNull(resolveEntity);

        for (var i = 0; i < Descriptors.Count; i++)
        {
            ApplyDescriptor(Descriptors[i], destroyed, resolveEntity);
        }

        for (var i = 0; i < Cleaners.Count; i++)
        {
            Cleaners[i].OnEntityDestroying(destroyed);
        }
    }

    private static void ApplyDescriptor(
        OwnedReferenceDescriptor descriptor,
        IEntity destroyed,
        Func<EntityId, IEntity?> resolveEntity)
    {
        var ownerSlot = destroyed.Data.Get(descriptor.ChildToOwnerKey);
        if (!ownerSlot.HasValue || ownerSlot.Value.IsEmpty)
        {
            return;
        }

        var owner = resolveEntity(ownerSlot.Value);
        if (owner == null)
        {
            return;
        }

        var list = owner.Data.Get(descriptor.OwnerListKey);
        var next = list.Remove(destroyed.EntityId);
        if (!next.Equals(list))
        {
            owner.Data.Set(descriptor.OwnerListKey, next);
        }
    }
}
