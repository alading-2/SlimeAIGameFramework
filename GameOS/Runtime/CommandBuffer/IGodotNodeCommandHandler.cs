namespace SlimeAI.GameOS.Runtime.CommandBuffer;

/// <summary>
/// Godot 节点命令的可注入处理器；Runtime 默认不直接实例化 Godot 节点。
/// </summary>
public interface IGodotNodeCommandHandler
{
    /// <summary>执行 Godot 节点实例化请求。</summary>
    bool InstantiateNode(GodotNodeInstantiatePayload payload);

    /// <summary>执行 Godot 节点释放请求。</summary>
    bool FreeNode(GodotNodeFreeCommandPayload payload);
}
