namespace SlimeAI.GameOS.Runtime.CommandBuffer;

/// <summary>
/// 第一阶段支持的延迟命令类型。
/// </summary>
public enum DeferredCommandKind
{
    Spawn = 0,
    Destroy = 1,
    Attach = 2,
    Detach = 3,
    QueuedEvent = 4,
    ResourceRequest = 5,
    GodotNodeInstantiate = 6,
    GodotNodeFree = 7
}
