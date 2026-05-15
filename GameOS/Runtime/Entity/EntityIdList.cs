using System;
using System.Collections;
using System.Collections.Generic;

namespace SlimeAI.GameOS.Runtime.Entity;

/// <summary>
/// 多引用业务 DataKey 的 typed 值：包装 <see cref="EntityId"/> 列表，保留插入顺序，提供值相等性。
/// </summary>
/// <remarks>
/// <para>
/// 业务 Capability 多对多引用 MUST 用 <c>DataKey&lt;EntityIdList&gt;</c> 表达，
/// 而不是 <c>DataKey&lt;List&lt;string&gt;&gt;</c> 或 <c>DataKey&lt;HashSet&lt;EntityId&gt;&gt;</c>。
/// </para>
/// <para>
/// <see cref="Add"/> 和 <see cref="Remove"/> 返回新值，不修改既有列表，
/// 保证 Observation snapshot 安全；<see cref="Equals(EntityIdList)"/> 按内容比较。
/// </para>
/// </remarks>
public readonly record struct EntityIdList(IReadOnlyList<EntityId> Ids) : IEnumerable<EntityId>
{
    /// <summary>空列表共享单例。</summary>
    public static EntityIdList Empty { get; } = new EntityIdList(Array.Empty<EntityId>());

    /// <summary>元素数量。<c>default(EntityIdList)</c> 也返回 0。</summary>
    public int Count => Ids?.Count ?? 0;

    /// <summary>按下标取元素。</summary>
    public EntityId this[int index] => (Ids ?? Array.Empty<EntityId>())[index];

    /// <summary>
    /// 返回包含 <paramref name="id"/> 的新列表；若已存在则原样返回当前值。
    /// </summary>
    public EntityIdList Add(EntityId id)
    {
        if (Contains(id))
        {
            return this;
        }

        var current = Ids ?? Array.Empty<EntityId>();
        var next = new EntityId[current.Count + 1];
        for (var i = 0; i < current.Count; i++)
        {
            next[i] = current[i];
        }

        next[current.Count] = id;
        return new EntityIdList(next);
    }

    /// <summary>
    /// 返回移除 <paramref name="id"/> 的新列表；若不存在则原样返回当前值。
    /// </summary>
    public EntityIdList Remove(EntityId id)
    {
        var current = Ids ?? Array.Empty<EntityId>();
        var index = IndexOf(id);
        if (index < 0)
        {
            return this;
        }

        if (current.Count == 1)
        {
            return Empty;
        }

        var next = new EntityId[current.Count - 1];
        var writeIndex = 0;
        for (var i = 0; i < current.Count; i++)
        {
            if (i == index)
            {
                continue;
            }

            next[writeIndex++] = current[i];
        }

        return new EntityIdList(next);
    }

    /// <summary>是否包含指定 <paramref name="id"/>。</summary>
    public bool Contains(EntityId id) => IndexOf(id) >= 0;

    /// <summary>返回与 <paramref name="other"/> 内容相等性（顺序敏感）。</summary>
    public bool Equals(EntityIdList other)
    {
        var left = Ids ?? Array.Empty<EntityId>();
        var right = other.Ids ?? Array.Empty<EntityId>();
        if (left.Count != right.Count)
        {
            return false;
        }

        for (var i = 0; i < left.Count; i++)
        {
            if (!left[i].Equals(right[i]))
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var current = Ids ?? Array.Empty<EntityId>();
        var hash = new HashCode();
        for (var i = 0; i < current.Count; i++)
        {
            hash.Add(current[i]);
        }

        return hash.ToHashCode();
    }

    /// <summary>
    /// 顺序枚举元素，便于 capability 与 observation 使用 <c>foreach</c>。
    /// </summary>
    public IEnumerator<EntityId> GetEnumerator()
    {
        var current = Ids ?? Array.Empty<EntityId>();
        for (var i = 0; i < current.Count; i++)
        {
            yield return current[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private int IndexOf(EntityId id)
    {
        var current = Ids ?? Array.Empty<EntityId>();
        for (var i = 0; i < current.Count; i++)
        {
            if (current[i].Equals(id))
            {
                return i;
            }
        }

        return -1;
    }
}
