using System;

namespace SlimeAI.GameOS.Runtime.Entity;

/// <summary>
/// 框架内 Entity 身份的 typed value。
/// 用 <see cref="readonly record struct"/> 包装内部 string，编译期阻止 raw <c>string</c> 误传。
/// </summary>
/// <remarks>
/// <para>
/// 框架 MUST 用 <see cref="EntityId"/> 表达 entity 引用；不提供 implicit <c>string</c> 转换。
/// </para>
/// <para>
/// "无引用" 由 <see cref="Empty"/> 表达；<see cref="IsEmpty"/> 同时把 <c>null</c> 与 <c>""</c> 视为 empty，
/// 等价性比较把 <c>null</c> 与 <c>""</c> 视为同一 empty，使 <c>default(EntityId)</c>、
/// <see cref="Empty"/>、<see cref="From(string?)"/> 的 <c>null</c>/<c>""</c> 入口全部相等。
/// </para>
/// </remarks>
public readonly record struct EntityId(string Value)
{
    /// <summary>
    /// 唯一 "无引用" 值；与 <c>default(EntityId)</c> 和 <see cref="From(string?)"/> 的 <c>null</c>/<c>""</c> 入口相等。
    /// </summary>
    public static EntityId Empty { get; } = new EntityId(string.Empty);

    /// <summary>
    /// 显式从可空 string 构造。
    /// <c>null</c> 或 <c>""</c> 输入返回与 <see cref="Empty"/> 相等的值；非空输入返回原值包装。
    /// </summary>
    /// <param name="value">原始 entity id 字符串，可空。</param>
    public static EntityId From(string? value) =>
        string.IsNullOrEmpty(value) ? Empty : new EntityId(value);

    /// <summary>
    /// 是否表达 "无引用"。<c>null</c> 与 <c>""</c> 都视为 empty。
    /// </summary>
    public bool IsEmpty => string.IsNullOrEmpty(Value);

    /// <summary>
    /// 显式返回底层 string；<c>null</c> 也输出空字符串，便于日志与 JSONL 序列化。
    /// </summary>
    public override string ToString() => Value ?? string.Empty;

    /// <summary>
    /// 把 <c>null</c> 与 <c>""</c> normalize 为同一 empty，保证 <c>default(EntityId).Equals(Empty)</c> 与
    /// <see cref="From(string?)"/> 的 <c>null</c>/<c>""</c> 入口都相等。
    /// </summary>
    public bool Equals(EntityId other) =>
        string.Equals(Value ?? string.Empty, other.Value ?? string.Empty, StringComparison.Ordinal);

    /// <inheritdoc/>
    public override int GetHashCode() =>
        StringComparer.Ordinal.GetHashCode(Value ?? string.Empty);
}
