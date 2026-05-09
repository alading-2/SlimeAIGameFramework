using Godot;
using SkilmeAI.GameOS.Capabilities.Movement;
using SkilmeAI.GameOS.Runtime.Entity;

namespace SkilmeAI.GameOS.GodotBridge;

/// <summary>
/// Godot 输入桥接组件：每帧读取 Godot Input，将移动方向写入 Entity Data 的 <see cref="MovementDataKeys.InputDirection" />。
/// <para>挂载到 GodotEntity2D 下，作为 PlayerInput 运动策略的输入来源。</para>
/// <para>也支持 AI 直接覆盖：AI 层写入 <see cref="MovementDataKeys.AIMoveDirection" /> 时，本组件可自动禁用或保持共存。</para>
/// </summary>
public partial class GodotPlayerInputComponent : Node, IGodotComponent
{
    private IEntity? entity;

    /// <summary>是否自动在 <code>_Process</code> 中读取输入。</summary>
    [Export]
    public bool AutoTick { get; set; } = true;

    /// <summary>Godot Input Map 中左移动作名称。</summary>
    [Export]
    public string MoveLeftAction { get; set; } = "MoveLeft";

    /// <summary>Godot Input Map 中右移动作名称。</summary>
    [Export]
    public string MoveRightAction { get; set; } = "MoveRight";

    /// <summary>Godot Input Map 中上移动作名称。</summary>
    [Export]
    public string MoveUpAction { get; set; } = "MoveUp";

    /// <summary>Godot Input Map 中下移动作名称。</summary>
    [Export]
    public string MoveDownAction { get; set; } = "MoveDown";

    /// <summary>当 <see cref="MovementDataKeys.CanMoveInput" /> 为 false 时是否仍读取输入（仅写入 Data，不强制门控）。</summary>
    [Export]
    public bool IgnoreCanMoveInputGate { get; set; } = false;

    /// <summary>Godot Input Map 中释放当前技能动作名称。</summary>
    [Export]
    public string UseSkillAction { get; set; } = "UseSkill";

    /// <summary>Godot Input Map 中切换到上一个技能动作名称。</summary>
    [Export]
    public string PreviousSkillAction { get; set; } = "PreviousSkill";

    /// <summary>Godot Input Map 中切换到下一个技能动作名称。</summary>
    [Export]
    public string NextSkillAction { get; set; } = "NextSkill";

    /// <summary>最近一次读取到的输入方向。</summary>
    public Vector2Value LastInputDirection { get; private set; } = Vector2Value.Zero;

    /// <summary>技能释放按钮是否在本帧刚按下。</summary>
    public bool UseSkillJustPressed { get; private set; }

    /// <summary>上一个技能按钮是否在本帧刚按下。</summary>
    public bool PreviousSkillJustPressed { get; private set; }

    /// <summary>下一个技能按钮是否在本帧刚按下。</summary>
    public bool NextSkillJustPressed { get; private set; }

    /// <summary>输入组件发射的技能按钮事件名：释放当前技能。</summary>
    public const string UseSkillEvent = "input:use_skill";

    /// <summary>输入组件发射的技能按钮事件名：切换到上一个技能。</summary>
    public const string PreviousSkillEvent = "input:previous_skill";

    /// <summary>输入组件发射的技能按钮事件名：切换到下一个技能。</summary>
    public const string NextSkillEvent = "input:next_skill";

    /// <inheritdoc />
    public void OnComponentRegistered(IEntity entity, Node entityNode)
    {
        this.entity = entity;
        MovementDataKeys.RegisterAll();
    }

    /// <inheritdoc />
    public void OnComponentUnregistered(IEntity? entity, Node? entityNode)
    {
        this.entity = null;
        LastInputDirection = Vector2Value.Zero;
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        if (AutoTick)
        {
            TickInput();
        }
    }

    /// <summary>
    /// 手动执行一次输入读取，供测试或外部调度入口调用。
    /// </summary>
    public void TickInput()
    {
        if (entity == null)
        {
            return;
        }

        if (!IgnoreCanMoveInputGate && !entity.Data.Get<bool>(MovementDataKeys.CanMoveInput, true))
        {
            LastInputDirection = Vector2Value.Zero;
            entity.Data.Set(MovementDataKeys.InputDirection, Vector2Value.Zero);
            return;
        }

        var input = Input.GetVector(MoveLeftAction, MoveRightAction, MoveUpAction, MoveDownAction);
        LastInputDirection = new Vector2Value(input.X, input.Y);
        entity.Data.Set(MovementDataKeys.InputDirection, LastInputDirection);

        // 技能按钮：每帧检测并发射事件
        UseSkillJustPressed = Input.IsActionJustPressed(UseSkillAction);
        PreviousSkillJustPressed = Input.IsActionJustPressed(PreviousSkillAction);
        NextSkillJustPressed = Input.IsActionJustPressed(NextSkillAction);

        if (UseSkillJustPressed)
        {
            entity.Events.Emit(UseSkillEvent, new UseSkillEventData(entity));
        }

        if (PreviousSkillJustPressed)
        {
            entity.Events.Emit(PreviousSkillEvent, new PreviousSkillEventData(entity));
        }

        if (NextSkillJustPressed)
        {
            entity.Events.Emit(NextSkillEvent, new NextSkillEventData(entity));
        }
    }

    /// <summary>释放当前技能事件 payload。</summary>
    /// <param name="Entity">触发输入的实体。</param>
    public readonly record struct UseSkillEventData(IEntity Entity);

    /// <summary>切换到上一个技能事件 payload。</summary>
    /// <param name="Entity">触发输入的实体。</param>
    public readonly record struct PreviousSkillEventData(IEntity Entity);

    /// <summary>切换到下一个技能事件 payload。</summary>
    /// <param name="Entity">触发输入的实体。</param>
    public readonly record struct NextSkillEventData(IEntity Entity);
}
