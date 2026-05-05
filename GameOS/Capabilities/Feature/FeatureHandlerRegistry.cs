using System;
using System.Collections.Generic;

namespace SkilmeAI.GameOS.Capabilities.Feature;

/// <summary>
/// Feature 处理器注册表。
/// </summary>
public static class FeatureHandlerRegistry
{
    private static readonly Dictionary<string, IFeatureHandler> Handlers = new(StringComparer.Ordinal);

    /// <summary>
    /// 注册处理器。
    /// </summary>
    /// <param name="handler">处理器。</param>
    public static void Register(IFeatureHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        if (string.IsNullOrEmpty(handler.FeatureId))
        {
            throw new ArgumentException("Feature handler id cannot be empty.", nameof(handler));
        }

        Handlers[handler.FeatureId] = handler;
    }

    /// <summary>
    /// 获取处理器。
    /// </summary>
    /// <param name="handlerId">处理器 Id。</param>
    public static IFeatureHandler? Get(string handlerId)
    {
        return !string.IsNullOrEmpty(handlerId) && Handlers.TryGetValue(handlerId, out var handler)
            ? handler
            : null;
    }

    /// <summary>
    /// 判断处理器是否存在。
    /// </summary>
    /// <param name="handlerId">处理器 Id。</param>
    public static bool HasHandler(string handlerId)
    {
        return !string.IsNullOrEmpty(handlerId) && Handlers.ContainsKey(handlerId);
    }

    /// <summary>
    /// 清空注册表，主要用于测试。
    /// </summary>
    public static void Clear()
    {
        Handlers.Clear();
    }
}
