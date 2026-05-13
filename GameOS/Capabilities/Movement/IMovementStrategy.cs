using SlimeAI.GameOS.Runtime.Data;
using SlimeAI.GameOS.Runtime.Entity;

namespace SlimeAI.GameOS.Capabilities.Movement;

/// <summary>
/// 运动策略接口，只负责把移动参数转换为速度和朝向意图。
/// </summary>
public interface IMovementStrategy
{
    /// <summary>进入策略时调用一次。</summary>
    void OnEnter(IEntity entity, Data data, in MovementParams movementParams)
    {
    }

    /// <summary>每帧计算速度和朝向意图。</summary>
    MovementUpdateResult Update(IEntity entity, Data data, float delta, in MovementParams movementParams);

    /// <summary>停止策略时调用一次。</summary>
    void OnStop(IEntity entity, Data data, in MovementStopContext context)
    {
    }
}
