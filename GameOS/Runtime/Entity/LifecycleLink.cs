namespace SlimeAI.GameOS.Runtime.Entity;

/// <summary>
/// 父实体销毁时对子实体的处理策略，作为 <see cref="LifecycleLink.DestroyPolicy"/> 字段。
/// </summary>
public enum ParentDestroyPolicy
{
    /// <summary>父实体销毁时递归销毁子实体。</summary>
    DestroyRecursively = 0,

    /// <summary>父实体销毁时只断开 lifecycle link，子实体继续存活。</summary>
    Detach = 1
}

/// <summary>
/// LifecycleTree 中的一条生命周期父子链路。
/// </summary>
/// <param name="ParentEntityId">父实体 Id。</param>
/// <param name="ChildEntityId">子实体 Id。</param>
/// <param name="DestroyPolicy">父实体销毁时的处理策略。</param>
/// <param name="Priority">优先级，数值越小越靠前。</param>
/// <remarks>
/// <para>
/// link 是不可变 typed record；任何需要新增的 per-link 元数据 MUST 以 typed 字段
/// 形式扩展，而不是依赖 <c>Dictionary&lt;string, object&gt;</c>。
/// </para>
/// </remarks>
public readonly record struct LifecycleLink(
    EntityId ParentEntityId,
    EntityId ChildEntityId,
    ParentDestroyPolicy DestroyPolicy,
    int Priority);
