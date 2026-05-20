using System.Collections.Generic;
using Godot;

namespace SlimeAI.GameOS.GodotBridge;

/// <summary>
/// Godot 单位组合结果。
/// </summary>
public sealed class GodotUnitCompositionResult
{
    private readonly List<Node> adapters = new();

    /// <summary>组合是否成功。</summary>
    public bool Success { get; internal set; } = true;

    /// <summary>失败原因；成功时为空。</summary>
    public string FailureReason { get; internal set; } = string.Empty;

    /// <summary>加载出的视觉根节点。</summary>
    public Node? VisualRoot { get; internal set; }

    /// <summary>创建出的 Hurtbox 碰撞形状。</summary>
    public CollisionShape2D? HurtboxShape { get; internal set; }

    /// <summary>创建出的 Adapter 节点。</summary>
    public IReadOnlyList<Node> Adapters => adapters;

    /// <summary>本次追加后通过 BridgeContext 注册的 Adapter 数。</summary>
    public int RegisteredAdapterCount { get; internal set; }

    internal void AddAdapter(Node adapter)
    {
        adapters.Add(adapter);
    }
}
