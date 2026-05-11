using System;

namespace SkilmeAI.GameOS.Runtime.Event;

/// <summary>
/// Runtime 事件总线契约。唯一 API 表面：Publish / Subscribe / ExportObservation。
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// 总线名（"world" 或 "entity:&lt;id&gt;"），用于 observation dump。
    /// </summary>
    string BusName { get; }

    /// <summary>
    /// 发布事件。调用方仅调用一次；Scope 由 payload marker interface 决定。
    /// </summary>
    /// <typeparam name="T">事件 payload 类型。</typeparam>
    /// <param name="event">事件数据。</param>
    void Publish<T>(in T @event) where T : struct, IEvent;

    /// <summary>
    /// 订阅事件。返回 IDisposable 作为唯一退订手段。
    /// </summary>
    /// <typeparam name="T">事件 payload 类型。</typeparam>
    /// <param name="handler">事件处理委托。</param>
    /// <returns>用于退订的 IDisposable。</returns>
    IDisposable Subscribe<T>(Action<T> handler) where T : struct, IEvent;

    /// <summary>
    /// 导出 eventbus-dump.json 用于调试。
    /// </summary>
    /// <param name="path">输出文件路径。</param>
    void ExportObservation(string path);
}
