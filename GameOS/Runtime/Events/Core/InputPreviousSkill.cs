using SkilmeAI.GameOS.Runtime.Entity;
using SkilmeAI.GameOS.Runtime.Event;

namespace SkilmeAI.GameOS.Runtime.Events.Core;

/// <summary>请求切换到上一个技能。唯一 producer 是 GodotPlayerInputComponent。</summary>
public readonly record struct InputPreviousSkill(IEntity Entity) : IEntityEvent;
