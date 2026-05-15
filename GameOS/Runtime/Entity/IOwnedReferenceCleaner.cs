namespace SlimeAI.GameOS.Runtime.Entity;

/// <summary>
/// 在 entity 销毁前被 framework 调用，让 capability 完成 typed owner cleanup。
/// </summary>
/// <remarks>
/// <para>
/// Capability MUST 通过 <see cref="RuntimeOwnedReferenceRegistry"/> 注册实例，
/// 不应直接订阅 <c>EntityDestroyed</c> 事件做 cleanup。
/// </para>
/// </remarks>
public interface IOwnedReferenceCleaner
{
    /// <summary>
    /// 当 <paramref name="destroyed"/> 即将被销毁，且 framework typed descriptor 已处理完时调用。
    /// </summary>
    /// <param name="destroyed">即将销毁的实体。</param>
    void OnEntityDestroying(IEntity destroyed);
}
