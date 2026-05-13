using SkilmeAI.GameOS.Capabilities.Movement;
using SkilmeAI.GameOS.Capabilities.Damage;
using SkilmeAI.GameOS.Runtime.Data;
using SkilmeAI.GameOS.Runtime.Entity;

namespace SkilmeAI.GameOS.Capabilities.AI;

/// <summary>
/// 检查当前 AI 目标是否在指定距离内。
/// </summary>
public sealed class IsTargetInRangeCondition : BehaviorNode
{
    private readonly DataKey<float>? rangeDataKey;
    private readonly float defaultRange;

    /// <summary>
    /// 创建范围检测条件。
    /// </summary>
    /// <param name="range">固定范围，-1 表示不限距离。</param>
    public IsTargetInRangeCondition(float range = -1f) : base("IsTargetInRange")
    {
        defaultRange = range;
    }

    /// <summary>
    /// 创建范围检测条件。
    /// </summary>
    /// <param name="rangeDataKey">从 Entity.Data 读取范围的 DataKey。</param>
    /// <param name="defaultRange">DataKey 未配置时的回退范围，-1 表示不限距离。</param>
    public IsTargetInRangeCondition(DataKey<float> rangeDataKey, float defaultRange = -1f) : base("IsTargetInRange")
    {
        this.rangeDataKey = rangeDataKey;
        this.defaultRange = defaultRange;
    }

    /// <inheritdoc />
    public override AIState Evaluate(AIContext context)
    {
        var range = rangeDataKey == null
            ? defaultRange
            : context.Entity.Data.Get<float>(rangeDataKey, defaultRange);
        if (range < 0f)
        {
            return TryGetTargetPosition(context, out _) ? AIState.Success : AIState.Failure;
        }

        var origin = context.Entity.Data.Get<Vector2Value>(MovementDataKeys.Position, Vector2Value.Zero);
        return TryGetTargetPosition(context, out var targetPosition)
            && Vector2Value.Distance(origin, targetPosition) <= range
                ? AIState.Success
                : AIState.Failure;
    }

    private static bool TryGetTargetPosition(AIContext context, out Vector2Value targetPosition)
    {
        var target = context.Entity.Data.Get<IEntity?>(AIDataKeys.TargetEntity, null);
        if (target != null && !target.Data.Get<bool>(DamageDataKeys.IsDead, false))
        {
            targetPosition = target.Data.Get<Vector2Value>(MovementDataKeys.Position, Vector2Value.Zero);
            return true;
        }

        if (context.Entity.Data.Get<bool>(AIDataKeys.HasTargetPosition, false))
        {
            targetPosition = context.Entity.Data.Get<Vector2Value>(AIDataKeys.TargetPosition, Vector2Value.Zero);
            return true;
        }

        targetPosition = Vector2Value.Zero;
        return false;
    }
}
