# 技能逻辑参数写法参考

## 适用范围

这个文档只回答一类问题：

- 技能逻辑里参数应该怎么写
- `Ability` / `CastContext` / `AbilityImpactTool` 该怎么组合
- 哪些参数属于“技能触发频率”，哪些属于“单次命中后的持续效果”
- 目标、落点、碰撞、特效、伤害分别应该写在哪一层

如果你还没确认当前任务是不是技能系统问题，先回到上级 [`SKILL.md`](../SKILL.md) 看入口说明。

## 技能流水线

技能系统当前统一入口：

`TryTrigger → CanUse检查 → ConsumeCharge → StartCooldown → ConsumeCost → FeatureSystem.OnFeatureActivated → IFeatureHandler.OnActivated → IFeatureHandler.OnExecute → Ability.Executed → FeatureSystem.OnFeatureEnded`

内置组件职责：

- `CooldownComponent`：冷却管理
- `ChargeComponent`：充能计数
- `TriggerComponent`：触发模式（`Periodic / OnEvent / Manual`）
- `CostComponent`：资源消耗

当前边界：

- `AbilitySystem` 只编排流水线，不做统一自动索敌或点选决策
- 实体目标查询写在具体 Handler 的 `ExecuteAbility`
- `Point` 点选由输入层在正式 `TryTrigger` 前发起，确认后再提交正式 `TryTrigger`
- `TargetingManager` / `TargetingIndicatorControlComponent` 只负责异步点选会话

## 统一触发入口

```csharp
// ✅ 标准触发方式（统一走 TryTrigger 事件）
var context = new CastContext
{
    Ability = abilityEntity,
    Caster = ownerEntity,
    ResponseContext = new EventContext()
};
abilityEntity.Events.Emit(
    GameEventType.Ability.TryTrigger,
    new GameEventType.Ability.TryTriggerEventData(context)
);

// ✅ 读取触发结果
var result = context.ResponseContext?.HasResult == true
    ? context.ResponseContext.GetResult<TriggerResult>()
    : TriggerResult.Failed;
// TriggerResult: Success / Failed
```

## 常用 Data 参数怎么写

```csharp
// 通过 Data 配置，内置组件自动响应
ability.Data.Set(DataKey.AbilityEnabled, true);
ability.Data.Set(DataKey.AbilityCooldown, 5.0f);          // CooldownComponent 自动管理
ability.Data.Set(DataKey.IsAbilityUsesCharges, true);     // 启用充能
ability.Data.Set(DataKey.AbilityChargeMax, 3);            // 最大充能数
ability.Data.Set(DataKey.AbilityTriggerMode, (int)AbilityTriggerMode.Manual);

// 目标选择配置
ability.Data.Set(DataKey.AbilityTargetSelection, (int)AbilityTargetSelection.Entity);
ability.Data.Set(DataKey.AbilityCastRange, 200f);             // 施法距离：由具体 Handler 解释为索敌/点选射程
ability.Data.Set(DataKey.AbilityEffectRadius, 150f);          // 效果半径：AOE 范围/命中半径/技能自定义语义
ability.Data.Set(DataKey.AbilityDamageInterval, 0.5f);        // 持续伤害间隔；0 表示单次
ability.Data.Set(DataKey.AbilityDamageDuration, 3f);          // 持续伤害总时长；0 表示单次
ability.Data.Set(DataKey.AbilityRepeatHitSameTarget, true);   // 是否允许同一施法内重复命中同一目标
ability.Data.Set(DataKey.AbilityApplyImmediateDamage, true);  // DoT 开始时是否先立即造成一次伤害
```

## 两条时间轴不能混

`Ability` 里有两套完全不同的“时间”语义：

### 1. 技能触发频率

- `TriggerComponent + AbilityTriggerMode.Periodic + AbilityCooldown`
- 决定“多久重新执行一次整条技能流水线”
- 每次都会重新进入 `TryTrigger → ExecuteAbility`
- 适合“每 N 秒放一次圈伤 / 回血 / 爆炸”

### 2. 单次技能内部的持续伤害

- `DamageApplyOptions.TickInterval + TotalDuration`
- 决定“这次 ExecuteAbility 启动后，这次命中内部要不要继续跳 DoT”
- 不会重新执行技能流水线，只会在当前命中上下文里继续结算伤害
- 适合“这次命中后挂一个持续灼烧 / 毒伤 / 持续范围伤害”

### 典型误区

`CircleDamage` 当前是第 1 种，不是第 2 种：

- `TriggerComponent` 会按 `AbilityCooldown` 周期触发技能
- `Data/Data/Ability/Ability/CircleDamage/CircleDamage.cs` 里的 `DamageApplyOptions` 没有设置 `TickInterval / TotalDuration`
- 所以每次触发都是“一次即时范围伤害”，不会在单次技能内部继续跳 DoT

如果同时配置：

- `Periodic`
- `DamageApplyOptions.TickInterval / TotalDuration`

那效果就是“每次周期触发都会再启动一条新的内部 DoT”。这是合法设计，但必须确认不是误配。

## 什么时候写在 ExecuteAbility，什么时候写在 OnGranted

- 写在 `AbilityFeatureHandler.ExecuteAbility(CastContext)`：
  - 表示“每次技能被触发时执行一次”
  - 配合 `TriggerComponent.Periodic`，表示“每隔一段时间重新执行一次技能”
  - 配合 `DamageApplyOptions.TickInterval / TotalDuration`，表示“这次技能执行内部再挂一段 DoT”

- 写在 `IFeatureHandler.OnGranted`：
  - 表示“能力一被授予就开始常驻运行”
  - 适合常驻光环、被动灼烧圈、长期监听器、长期计时器
  - 如果自己创建了 `GameTimer` / 订阅 / 持续特效，必须在 `OnRemoved` 显式取消和清理

推荐判断：

- “每隔 N 秒释放一次技能” → `TriggerComponent.Periodic + ExecuteAbility`
- “能力授予后持续存在，直到被移除” → `OnGranted + OnRemoved`
- “单次技能命中后内部继续跳伤害” → `ExecuteAbility + DamageApplyOptions.TickInterval / TotalDuration`

补充约定：

- “每冷却时间自动抛一个投射物 / 炸弹”优先使用 `AbilityTriggerMode.Periodic + AbilityCooldown`
- 不要为了这种需求额外在 Handler 里手写 `TimerManager.Loop(...)`，否则会绕开一部分技能系统语义

## 目标选择怎么落到代码里

| 类型 | 说明 | 建议写法 |
| :-- | :-- | :-- |
| `Entity` | 需要实体目标 | 在 `ExecuteAbility` 内索敌；无目标时返回 0 命中或降级 |
| `Point` | 需要玩家指定位置 | 输入层先点选；Handler 在 `ExecuteAbility` 读取 `TargetPosition` |
| `EntityOrPoint` | 由具体技能自己定义先后策略 | 不写回 `AbilitySystem`；输入层与 Handler 为该技能明确约定 |
| `None` | 不依赖前置目标 | `ExecuteAbility` 自行处理 |

`AbilityTargetSelection` 只保留“输入语义 / 编辑器表达”作用；`AbilitySystem` 不会按它做统一前置选目标。

### Entity 型技能建议

```csharp
internal class ArcShot : AbilityFeatureHandler
{
    public override string FeatureId => global::FeatureId.Ability.Projectile.ArcShot;

    protected override AbilityExecutedResult ExecuteAbility(CastContext context)
    {
        var caster = context.Caster!;
        var ability = context.Ability!;
        var casterNode = (Node2D)caster;

        float castRange = ability.Data.Get<float>(DataKey.AbilityCastRange);
        if (castRange <= 0f)
        {
            castRange = ability.Data.Get<float>(DataKey.AbilityEffectRadius);
        }

        var targets = castRange > 0f
            ? EntityTargetSelector.Query(new TargetSelectorQuery
            {
                Geometry = GeometryType.Circle,
                Origin = casterNode.GlobalPosition,
                Range = castRange,
                CenterEntity = caster,
                TeamFilter = TeamFilter.Enemy,
                Sorting = TargetSorting.Nearest,
                MaxTargets = 1
            })
            : new List<IEntity>();
        if (targets.Count == 0)
        {
            return new AbilityExecutedResult { TargetsHit = 0 };
        }

        context.Targets = new List<IEntity> { targets[0] };
        // 后续生成投射物 / 伤害逻辑
        return new AbilityExecutedResult { TargetsHit = 1 };
    }
}
```

### Point 型技能建议

```csharp
// 输入层：
if (ability.Data.Get<AbilityTargetSelection>(DataKey.AbilityTargetSelection) == AbilityTargetSelection.Point)
{
    if (AbilitySystem.CanUseAbility(ability))
    {
        GlobalEventBus.Global.Emit(
            GameEventType.Targeting.StartTargeting,
            new GameEventType.Targeting.StartTargetingEventData(context) // 施法上下文
        );
    }
}

// Handler：
protected override AbilityExecutedResult ExecuteAbility(CastContext context)
{
    if (!context.TargetPosition.HasValue)
    {
        return new AbilityExecutedResult { TargetsHit = 0 };
    }

    Vector2 targetPos = context.TargetPosition.Value;
    // 后续按落点执行技能
    return new AbilityExecutedResult { TargetsHit = 1 };
}
```

## 推荐优先复用 AbilityImpactTool

当技能需要“目标查询 + 特效 + 伤害结算”时，优先调用 `AbilityImpactTool`，避免在技能处理器里重复手写：

- `TargetSelector`
- `EffectTool`
- `foreach`
- `DamageService`

### 固定落点命中

```csharp
var result = AbilityImpactTool.Execute(caster, new AbilityImpactOptions
{
    Query = new TargetSelectorQuery
    {
        Geometry = GeometryType.Circle,
        Origin = impactPosition,
        Range = ability.Data.Get<float>(DataKey.AbilityEffectRadius),
        CenterEntity = caster,
        TeamFilter = AbilityTargetTeamFilter.Enemy,
        Sorting = TargetSorting.Nearest,
        MaxTargets = -1
    },
    Effect = !string.IsNullOrWhiteSpace(effectScenePath)
        ? new EffectSpawnOptions(effectScenePath, Name: "技能特效")
        : null,
    Damage = new DamageApplyOptions
    {
        Damage = ability.Data.Get<float>(DataKey.AbilityDamage) * caster.Data.Get<float>(DataKey.AbilityDamageBonus) / 100f,
        Type = DamageType.Magical,
        Tags = DamageTags.Ability | DamageTags.Area,
        Attacker = casterNode
    }
});
```

固定落点投掷约定：

- `ParabolaStrategy` 推荐用于固定 `TargetPoint`
- 不推荐把抛物线投射物做成追踪 `TargetNode` 的标准技能
- 原因不是数学错误，而是追踪时终点和曲线会持续重算，命中点与视觉都会变得不稳定

### 跟随施法者当前位置命中

```csharp
var result = AbilityImpactTool.Execute(caster, new AbilityImpactOptions
{
    Query = new TargetSelectorQuery
    {
        Geometry = GeometryType.Circle,
        Origin = casterNode.GlobalPosition,
        OriginProvider = () => casterNode.GlobalPosition,
        Range = ability.Data.Get<float>(DataKey.AbilityEffectRadius),
        CenterEntity = caster,
        TeamFilter = AbilityTargetTeamFilter.Enemy,
        MaxTargets = -1
    },
    Effect = !string.IsNullOrWhiteSpace(effectScenePath)
        ? new EffectSpawnOptions(
            effectScenePath,
            Name: "光环特效",
            Scale: new Vector2(2f, 2f))
        : null,
    Damage = new DamageApplyOptions
    {
        Damage = ability.Data.Get<float>(DataKey.AbilityDamage) * caster.Data.Get<float>(DataKey.AbilityDamageBonus) / 100f,
        Type = DamageType.Magical,
        Tags = DamageTags.Ability | DamageTags.Area,
        Attacker = casterNode,
        TickInterval = ability.Data.Get<float>(DataKey.AbilityDamageInterval),
        TotalDuration = ability.Data.Get<float>(DataKey.AbilityDamageDuration),
        AllowRepeatHitSameTarget = ability.Data.Get<bool>(DataKey.AbilityRepeatHitSameTarget),
        ApplyImmediateTick = ability.Data.Get<bool>(DataKey.AbilityApplyImmediateDamage)
    }
});
```

### AbilityImpactTool 入口语义

- `Execute(caster, options)`：统一入口；位置统一放在 `Query` / `Effect` 参数对象里
- 四个 options 字段都可选：`Query?` / `Targets?` / `Effect?` / `Damage?`
- `Query.Origin / OriginProvider` 是唯一命中锚点来源
- `Targets` 适合碰撞命中、链路里已经明确拿到目标的场景，不必再伪造范围查询
- `Effect.EffectPosition` 仅在特效需要显式偏离命中点时再填写；不填则默认复用 `Query` 原点
- `Damage.ApplyImmediateTick` 用于控制 DoT 在开始时是否先同步结算一次，默认 `true`
- `Damage != null` 但 `Query == null` 且 `Targets == null` 时，不会隐式兜底选目标
- DoT 调度与重复命中控制由 `DamageTool` 统一管理

## 相关文件

- [`SKILL.md`](../SKILL.md)
- `Data/Data/Ability/Ability/CircleDamage/CircleDamage.cs`
