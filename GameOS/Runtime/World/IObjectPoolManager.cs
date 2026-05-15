using System.Collections.Generic;
using SlimeAI.GameOS.Runtime.Pool;

namespace SlimeAI.GameOS.Runtime.World;

/// <summary>
/// RuntimeWorld 内部的对象池管理句柄。
/// </summary>
public interface IObjectPoolManager
{
    /// <summary>注册泛型对象池。</summary>
    void Register<T>(ObjectPool<T> pool) where T : class;

    /// <summary>注册非泛型对象池句柄。</summary>
    void RegisterPool(IObjectPool pool);

    /// <summary>注销泛型对象池。</summary>
    void Unregister<T>(ObjectPool<T> pool) where T : class;

    /// <summary>注销非泛型对象池句柄。</summary>
    void UnregisterPool(IObjectPool pool);

    /// <summary>记录对象到池名的归属。</summary>
    void MapObject(object instance, string poolName);

    /// <summary>移除对象到池名的归属。</summary>
    void UnmapObject(object instance);

    /// <summary>把对象归还到来源池。</summary>
    bool ReturnToPool(object instance);

    /// <summary>按名称获取泛型对象池。</summary>
    ObjectPool<T>? GetPool<T>(string name) where T : class;

    /// <summary>按名称获取非泛型对象池。</summary>
    IObjectPool? GetNamedPool(string name);

    /// <summary>返回所有对象池统计快照。</summary>
    Dictionary<string, PoolStats> GetAllStats();

    /// <summary>销毁全部对象池和映射。</summary>
    void Clear();
}
