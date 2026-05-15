namespace SlimeAI.GameOS.Runtime.CommandBuffer;

/// <summary>
/// 延迟命令失败原因。
/// </summary>
public enum DeferredCommandFailureReason
{
    None = 0,
    MissingTargetEntity = 1,
    DuplicateEntityId = 2,
    MissingParent = 3,
    MissingChild = 4,
    RelationshipConflict = 5,
    InvalidPayload = 6,
    UnsupportedKind = 7,
    InvalidPhase = 8,
    BridgeTargetUnavailable = 9,
    ResourceUnresolved = 10,
    HandlerException = 11,
    WorldDisposing = 12
}
