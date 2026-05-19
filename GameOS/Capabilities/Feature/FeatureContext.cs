using System;
using System.Collections.Generic;
using SlimeAI.GameOS.Runtime.Entity;

namespace SlimeAI.GameOS.Capabilities.Feature;

/// <summary>
/// Feature 生命周期上下文。
/// </summary>
public sealed class FeatureContext
{
    private object? activationData;
    private object? executeResult;
    private object? sourceEventData;
    private IFeatureActivationPayload? activationPayload;
    private IFeatureExecutionResult? executionResult;
    private IFeatureActivationPayload? sourceEventPayload;

    /// <summary>拥有者。</summary>
    public IEntity? Owner { get; init; }

    /// <summary>Feature 实体。</summary>
    public IEntity? Feature { get; init; }

    /// <summary>Feature 定义。</summary>
    public FeatureDefinition? Definition { get; init; }

    /// <summary>类型化激活载荷。</summary>
    public IFeatureActivationPayload? ActivationPayload
    {
        get => activationPayload;
        init
        {
            activationPayload = value;
            activationData ??= value;
        }
    }

    /// <summary>类型化执行结果。</summary>
    public IFeatureExecutionResult? ExecutionResult
    {
        get => executionResult;
        set
        {
            executionResult = value;
            executeResult = value;
        }
    }

    /// <summary>类型化触发源事件载荷。</summary>
    public IFeatureActivationPayload? SourceEventPayload
    {
        get => sourceEventPayload;
        init
        {
            sourceEventPayload = value;
            sourceEventData ??= value;
        }
    }

    /// <summary>子系统激活数据。新代码使用 <see cref="ActivationPayload"/> 或 <see cref="TryGetActivation{T}"/>。</summary>
    [Obsolete("Use ActivationPayload or TryGetActivation<T>() for typed Feature execution payloads.")]
    public object? ActivationData
    {
        get => activationData ?? activationPayload;
        init
        {
            activationData = value;
            if (value is IFeatureActivationPayload typed)
            {
                activationPayload ??= typed;
            }
        }
    }

    /// <summary>执行结果。新代码使用 <see cref="ExecutionResult"/> 或 <see cref="TryGetExecutionResult{T}"/>。</summary>
    [Obsolete("Use ExecutionResult or TryGetExecutionResult<T>() for typed Feature execution results.")]
    public object? ExecuteResult
    {
        get => executeResult ?? executionResult;
        set
        {
            executeResult = value;
            executionResult = value as IFeatureExecutionResult;
        }
    }

    /// <summary>触发源事件数据。新代码使用 <see cref="SourceEventPayload"/>。</summary>
    [Obsolete("Use SourceEventPayload for typed Feature source event payloads.")]
    public object? SourceEventData
    {
        get => sourceEventData ?? sourceEventPayload;
        init
        {
            sourceEventData = value;
            if (value is IFeatureActivationPayload typed)
            {
                sourceEventPayload ??= typed;
            }
        }
    }

    /// <summary>临时扩展数据。新框架契约禁止继续把它作为隐藏 object bag。</summary>
    [Obsolete("Use typed Feature activation payloads, execution results, Runtime DataKeys, or explicit game-side records.")]
    public Dictionary<string, object> ExtraData { get; } = new();

    /// <summary>
    /// 尝试读取类型化激活载荷。
    /// </summary>
    public bool TryGetActivation<T>(out T payload)
        where T : class, IFeatureActivationPayload
    {
        if (ActivationPayload is T typed)
        {
            payload = typed;
            return true;
        }

        payload = null!;
        return false;
    }

    /// <summary>
    /// 尝试读取类型化执行结果。
    /// </summary>
    public bool TryGetExecutionResult<T>(out T result)
        where T : class, IFeatureExecutionResult
    {
        if (ExecutionResult is T typed)
        {
            result = typed;
            return true;
        }

        result = null!;
        return false;
    }
}
