using SkilmeAI.GameOS.Runtime.Data;

namespace SkilmeAI.GameOS.Runtime.Schedule;

/// <summary>
/// Schedule 和系统预设 authoring 使用的运行时 DataKey。
/// </summary>
public static class ScheduleDataKeys
{
    /// <summary>系统唯一 Id。</summary>
    public static readonly DataKey<string> SystemId = DataKey.Create<string>("Schedule.SystemId",
        defaultValue: string.Empty,
        category: ScheduleCategory.System);

    /// <summary>系统挂载分组。</summary>
    public static readonly DataKey<SystemGroup> MountGroup = DataKey.Create<SystemGroup>("Schedule.MountGroup",
        defaultValue: SystemGroup.Else,
        category: ScheduleCategory.System);

    /// <summary>系统标签。</summary>
    public static readonly DataKey<string> Tags = DataKey.Create<string>("Schedule.Tags",
        defaultValue: string.Empty,
        category: ScheduleCategory.System);

    /// <summary>是否为必需系统。</summary>
    public static readonly DataKey<bool> Required = DataKey.Create<bool>("Schedule.Required",
        defaultValue: false,
        category: ScheduleCategory.System);

    /// <summary>默认是否自动装载。</summary>
    public static readonly DataKey<bool> AutoLoad = DataKey.Create<bool>("Schedule.AutoLoad",
        defaultValue: true,
        category: ScheduleCategory.System);

    /// <summary>首次纳入管理时是否启用。</summary>
    public static readonly DataKey<bool> StartEnabled = DataKey.Create<bool>("Schedule.StartEnabled",
        defaultValue: true,
        category: ScheduleCategory.System);

    /// <summary>加载优先级。</summary>
    public static readonly DataKey<int> Priority = DataKey.Create<int>("Schedule.Priority",
        defaultValue: 0,
        category: ScheduleCategory.System);

    /// <summary>允许的流程状态。</summary>
    public static readonly DataKey<string> AllowedFlowStates = DataKey.Create<string>("Schedule.AllowedFlowStates",
        defaultValue: string.Empty,
        category: ScheduleCategory.System);

    /// <summary>要求存在的覆盖层。</summary>
    public static readonly DataKey<string> RequiredOverlays = DataKey.Create<string>("Schedule.RequiredOverlays",
        defaultValue: string.Empty,
        category: ScheduleCategory.System);

    /// <summary>禁止存在的覆盖层。</summary>
    public static readonly DataKey<string> BlockedOverlays = DataKey.Create<string>("Schedule.BlockedOverlays",
        defaultValue: string.Empty,
        category: ScheduleCategory.System);

    /// <summary>允许的模拟状态。</summary>
    public static readonly DataKey<string> AllowedSimulationStates = DataKey.Create<string>("Schedule.AllowedSimulationStates",
        defaultValue: string.Empty,
        category: ScheduleCategory.System);

    /// <summary>依赖系统 Id 列表。</summary>
    public static readonly DataKey<string> Dependencies = DataKey.Create<string>("Schedule.Dependencies",
        defaultValue: string.Empty,
        category: ScheduleCategory.System);

    /// <summary>预设名称。</summary>
    public static readonly DataKey<string> PresetName = DataKey.Create<string>("Schedule.PresetName",
        defaultValue: string.Empty,
        category: ScheduleCategory.Preset);

    /// <summary>预设是否激活。</summary>
    public static readonly DataKey<bool> PresetIsActive = DataKey.Create<bool>("Schedule.Preset.IsActive",
        defaultValue: false,
        category: ScheduleCategory.Preset);

    /// <summary>预设启用标签。</summary>
    public static readonly DataKey<string> PresetEnabledTags = DataKey.Create<string>("Schedule.Preset.EnabledTags",
        defaultValue: string.Empty,
        category: ScheduleCategory.Preset);

    /// <summary>预设显式启用系统。</summary>
    public static readonly DataKey<string> PresetEnabledSystemIds = DataKey.Create<string>("Schedule.Preset.EnabledSystemIds",
        defaultValue: string.Empty,
        category: ScheduleCategory.Preset);

    /// <summary>预设显式禁用系统。</summary>
    public static readonly DataKey<string> PresetDisabledSystemIds = DataKey.Create<string>("Schedule.Preset.DisabledSystemIds",
        defaultValue: string.Empty,
        category: ScheduleCategory.Preset);

    /// <summary>描述文本。</summary>
    public static readonly DataKey<string> Description = DataKey.Create<string>("Schedule.Description",
        defaultValue: string.Empty,
        category: ScheduleCategory.System);

    /// <summary>默认波次持续时间。</summary>
    public static readonly DataKey<float> WaveDuration = DataKey.Create<float>("Schedule.Spawn.WaveDuration",
        defaultValue: 60f,
        category: ScheduleCategory.Spawn,
        minValue: 0f);

    /// <summary>最大波次数量。</summary>
    public static readonly DataKey<int> MaxWaves = DataKey.Create<int>("Schedule.Spawn.MaxWaves",
        defaultValue: -1,
        category: ScheduleCategory.Spawn,
        minValue: -1f);

    /// <summary>波次间隔时间。</summary>
    public static readonly DataKey<float> WaveBreakTime = DataKey.Create<float>("Schedule.Spawn.WaveBreakTime",
        defaultValue: 0f,
        category: ScheduleCategory.Spawn,
        minValue: 0f);

    /// <summary>生成规则是否启用。</summary>
    public static readonly DataKey<bool> SpawnRuleEnabled = DataKey.Create<bool>("Spawn.IsEnabled",
        defaultValue: false,
        category: ScheduleCategory.Spawn);

    /// <summary>生成位置策略。</summary>
    public static readonly DataKey<string> SpawnPositionStrategy = DataKey.Create<string>("Spawn.PositionStrategy",
        defaultValue: "Rectangle",
        category: ScheduleCategory.Spawn);

    /// <summary>最小生成波次。</summary>
    public static readonly DataKey<int> SpawnMinWave = DataKey.Create<int>("Spawn.MinWave",
        defaultValue: 1,
        category: ScheduleCategory.Spawn,
        minValue: 0f);

    /// <summary>最大生成波次。</summary>
    public static readonly DataKey<int> SpawnMaxWave = DataKey.Create<int>("Spawn.MaxWave",
        defaultValue: -1,
        category: ScheduleCategory.Spawn,
        minValue: -1f);

    /// <summary>生成间隔。</summary>
    public static readonly DataKey<float> SpawnInterval = DataKey.Create<float>("Spawn.Interval",
        defaultValue: 1f,
        category: ScheduleCategory.Spawn,
        minValue: 0f);

    /// <summary>单波最大生成数量。</summary>
    public static readonly DataKey<int> SpawnMaxCountPerWave = DataKey.Create<int>("Spawn.MaxCountPerWave",
        defaultValue: -1,
        category: ScheduleCategory.Spawn,
        minValue: -1f);

    /// <summary>单次生成数量。</summary>
    public static readonly DataKey<int> SpawnSingleCount = DataKey.Create<int>("Spawn.SingleCount",
        defaultValue: 1,
        category: ScheduleCategory.Spawn,
        minValue: 0f);

    /// <summary>单次生成数量波动。</summary>
    public static readonly DataKey<int> SpawnSingleVariance = DataKey.Create<int>("Spawn.SingleVariance",
        defaultValue: 0,
        category: ScheduleCategory.Spawn,
        minValue: 0f);

    /// <summary>波次开始后的首次生成延迟。</summary>
    public static readonly DataKey<float> SpawnStartDelay = DataKey.Create<float>("Spawn.StartDelay",
        defaultValue: 0f,
        category: ScheduleCategory.Spawn,
        minValue: 0f);

    /// <summary>生成权重。</summary>
    public static readonly DataKey<int> SpawnWeight = DataKey.Create<int>("Spawn.Weight",
        defaultValue: 1,
        category: ScheduleCategory.Spawn,
        minValue: 0f);

    /// <summary>显式触发静态 DataKey 注册。</summary>
    public static void RegisterAll()
    {
        _ = SystemId;
        _ = MountGroup;
        _ = Tags;
        _ = Required;
        _ = AutoLoad;
        _ = StartEnabled;
        _ = Priority;
        _ = AllowedFlowStates;
        _ = RequiredOverlays;
        _ = BlockedOverlays;
        _ = AllowedSimulationStates;
        _ = Dependencies;
        _ = PresetName;
        _ = PresetIsActive;
        _ = PresetEnabledTags;
        _ = PresetEnabledSystemIds;
        _ = PresetDisabledSystemIds;
        _ = Description;
        _ = WaveDuration;
        _ = MaxWaves;
        _ = WaveBreakTime;
        _ = SpawnRuleEnabled;
        _ = SpawnPositionStrategy;
        _ = SpawnMinWave;
        _ = SpawnMaxWave;
        _ = SpawnInterval;
        _ = SpawnMaxCountPerWave;
        _ = SpawnSingleCount;
        _ = SpawnSingleVariance;
        _ = SpawnStartDelay;
        _ = SpawnWeight;
    }
}
