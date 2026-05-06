using SkilmeAI.GameOS.Runtime.Data;

namespace SkilmeAI.GameOS.Runtime.Schedule;

/// <summary>
/// Schedule 和系统预设 authoring 使用的运行时 DataKey。
/// </summary>
public static class ScheduleDataKeys
{
    /// <summary>系统唯一 Id。</summary>
    public static readonly DataMeta SystemId = DataRegistry.Register(new DataMeta
    {
        Key = "Schedule.SystemId",
        DisplayName = "System Id",
        Type = typeof(string),
        Category = ScheduleCategory.System,
        DefaultValue = string.Empty,
        Description = "调度系统使用的稳定系统 Id。"
    });

    /// <summary>系统挂载分组。</summary>
    public static readonly DataMeta MountGroup = DataRegistry.Register(new DataMeta
    {
        Key = "Schedule.MountGroup",
        DisplayName = "Mount Group",
        Type = typeof(SystemGroup),
        Category = ScheduleCategory.System,
        DefaultValue = SystemGroup.Else,
        Description = "系统在 RuntimeSchedule 中的挂载分组。"
    });

    /// <summary>系统标签。</summary>
    public static readonly DataMeta Tags = DataRegistry.Register(new DataMeta
    {
        Key = "Schedule.Tags",
        DisplayName = "System Tags",
        Type = typeof(string),
        Category = ScheduleCategory.System,
        DefaultValue = string.Empty,
        Description = "系统标签 flags 文本，供 DataOS authoring 和预设筛选使用。"
    });

    /// <summary>是否为必需系统。</summary>
    public static readonly DataMeta Required = DataRegistry.Register(new DataMeta
    {
        Key = "Schedule.Required",
        DisplayName = "Required",
        Type = typeof(bool),
        Category = ScheduleCategory.System,
        DefaultValue = false,
        Description = "系统是否为必需系统。"
    });

    /// <summary>默认是否自动装载。</summary>
    public static readonly DataMeta AutoLoad = DataRegistry.Register(new DataMeta
    {
        Key = "Schedule.AutoLoad",
        DisplayName = "Auto Load",
        Type = typeof(bool),
        Category = ScheduleCategory.System,
        DefaultValue = true,
        Description = "没有激活预设时是否默认装载该系统。"
    });

    /// <summary>首次纳入管理时是否启用。</summary>
    public static readonly DataMeta StartEnabled = DataRegistry.Register(new DataMeta
    {
        Key = "Schedule.StartEnabled",
        DisplayName = "Start Enabled",
        Type = typeof(bool),
        Category = ScheduleCategory.System,
        DefaultValue = true,
        Description = "系统首次注册到 RuntimeSchedule 时的启用状态。"
    });

    /// <summary>加载优先级。</summary>
    public static readonly DataMeta Priority = DataRegistry.Register(new DataMeta
    {
        Key = "Schedule.Priority",
        DisplayName = "Priority",
        Type = typeof(int),
        Category = ScheduleCategory.System,
        DefaultValue = 0,
        Description = "系统加载和依赖排序优先级，数值越小越优先。"
    });

    /// <summary>允许的流程状态。</summary>
    public static readonly DataMeta AllowedFlowStates = DataRegistry.Register(new DataMeta
    {
        Key = "Schedule.AllowedFlowStates",
        DisplayName = "Allowed Flow States",
        Type = typeof(string),
        Category = ScheduleCategory.System,
        DefaultValue = string.Empty,
        Description = "系统允许运行的流程状态 flags 文本。"
    });

    /// <summary>要求存在的覆盖层。</summary>
    public static readonly DataMeta RequiredOverlays = DataRegistry.Register(new DataMeta
    {
        Key = "Schedule.RequiredOverlays",
        DisplayName = "Required Overlays",
        Type = typeof(string),
        Category = ScheduleCategory.System,
        DefaultValue = string.Empty,
        Description = "系统要求存在的覆盖层 flags 文本。"
    });

    /// <summary>禁止存在的覆盖层。</summary>
    public static readonly DataMeta BlockedOverlays = DataRegistry.Register(new DataMeta
    {
        Key = "Schedule.BlockedOverlays",
        DisplayName = "Blocked Overlays",
        Type = typeof(string),
        Category = ScheduleCategory.System,
        DefaultValue = string.Empty,
        Description = "系统禁止运行的覆盖层 flags 文本。"
    });

    /// <summary>允许的模拟状态。</summary>
    public static readonly DataMeta AllowedSimulationStates = DataRegistry.Register(new DataMeta
    {
        Key = "Schedule.AllowedSimulationStates",
        DisplayName = "Allowed Simulation States",
        Type = typeof(string),
        Category = ScheduleCategory.System,
        DefaultValue = string.Empty,
        Description = "系统允许运行的模拟状态 flags 文本。"
    });

    /// <summary>依赖系统 Id 列表。</summary>
    public static readonly DataMeta Dependencies = DataRegistry.Register(new DataMeta
    {
        Key = "Schedule.Dependencies",
        DisplayName = "Dependencies",
        Type = typeof(string),
        Category = ScheduleCategory.System,
        DefaultValue = string.Empty,
        Description = "逗号分隔的依赖系统 Id 列表。"
    });

    /// <summary>预设名称。</summary>
    public static readonly DataMeta PresetName = DataRegistry.Register(new DataMeta
    {
        Key = "Schedule.PresetName",
        DisplayName = "Preset Name",
        Type = typeof(string),
        Category = ScheduleCategory.Preset,
        DefaultValue = string.Empty,
        Description = "系统预设稳定名称。"
    });

    /// <summary>预设是否激活。</summary>
    public static readonly DataMeta PresetIsActive = DataRegistry.Register(new DataMeta
    {
        Key = "Schedule.Preset.IsActive",
        DisplayName = "Preset Is Active",
        Type = typeof(bool),
        Category = ScheduleCategory.Preset,
        DefaultValue = false,
        Description = "系统预设是否为当前激活预设。"
    });

    /// <summary>预设启用标签。</summary>
    public static readonly DataMeta PresetEnabledTags = DataRegistry.Register(new DataMeta
    {
        Key = "Schedule.Preset.EnabledTags",
        DisplayName = "Preset Enabled Tags",
        Type = typeof(string),
        Category = ScheduleCategory.Preset,
        DefaultValue = string.Empty,
        Description = "预设启用的系统标签 flags 文本。"
    });

    /// <summary>预设显式启用系统。</summary>
    public static readonly DataMeta PresetEnabledSystemIds = DataRegistry.Register(new DataMeta
    {
        Key = "Schedule.Preset.EnabledSystemIds",
        DisplayName = "Preset Enabled System Ids",
        Type = typeof(string),
        Category = ScheduleCategory.Preset,
        DefaultValue = string.Empty,
        Description = "预设显式启用的系统 Id 列表。"
    });

    /// <summary>预设显式禁用系统。</summary>
    public static readonly DataMeta PresetDisabledSystemIds = DataRegistry.Register(new DataMeta
    {
        Key = "Schedule.Preset.DisabledSystemIds",
        DisplayName = "Preset Disabled System Ids",
        Type = typeof(string),
        Category = ScheduleCategory.Preset,
        DefaultValue = string.Empty,
        Description = "预设显式禁用的系统 Id 列表。"
    });

    /// <summary>描述文本。</summary>
    public static readonly DataMeta Description = DataRegistry.Register(new DataMeta
    {
        Key = "Schedule.Description",
        DisplayName = "Schedule Description",
        Type = typeof(string),
        Category = ScheduleCategory.System,
        DefaultValue = string.Empty,
        Description = "系统或预设描述文本。"
    });

    /// <summary>默认波次持续时间。</summary>
    public static readonly DataMeta WaveDuration = DataRegistry.Register(new DataMeta
    {
        Key = "Schedule.Spawn.WaveDuration",
        DisplayName = "Wave Duration",
        Type = typeof(float),
        Category = ScheduleCategory.Spawn,
        DefaultValue = 60f,
        MinValue = 0f,
        Description = "生成系统默认单波持续时间。"
    });

    /// <summary>最大波次数量。</summary>
    public static readonly DataMeta MaxWaves = DataRegistry.Register(new DataMeta
    {
        Key = "Schedule.Spawn.MaxWaves",
        DisplayName = "Max Waves",
        Type = typeof(int),
        Category = ScheduleCategory.Spawn,
        DefaultValue = -1,
        MinValue = -1f,
        Description = "生成系统最大波次数；-1 表示不限制。"
    });

    /// <summary>波次间隔时间。</summary>
    public static readonly DataMeta WaveBreakTime = DataRegistry.Register(new DataMeta
    {
        Key = "Schedule.Spawn.WaveBreakTime",
        DisplayName = "Wave Break Time",
        Type = typeof(float),
        Category = ScheduleCategory.Spawn,
        DefaultValue = 0f,
        MinValue = 0f,
        Description = "两波之间的默认休息时间。"
    });

    /// <summary>生成规则是否启用。</summary>
    public static readonly DataMeta SpawnRuleEnabled = DataRegistry.Register(new DataMeta
    {
        Key = "Spawn.IsEnabled",
        DisplayName = "Spawn Rule Enabled",
        Type = typeof(bool),
        Category = ScheduleCategory.Spawn,
        DefaultValue = false,
        Description = "单个生成规则是否启用。"
    });

    /// <summary>生成位置策略。</summary>
    public static readonly DataMeta SpawnPositionStrategy = DataRegistry.Register(new DataMeta
    {
        Key = "Spawn.PositionStrategy",
        DisplayName = "Spawn Position Strategy",
        Type = typeof(string),
        Category = ScheduleCategory.Spawn,
        DefaultValue = "Rectangle",
        Description = "生成位置策略，例如 Rectangle 或 Circle。"
    });

    /// <summary>最小生成波次。</summary>
    public static readonly DataMeta SpawnMinWave = DataRegistry.Register(new DataMeta
    {
        Key = "Spawn.MinWave",
        DisplayName = "Spawn Min Wave",
        Type = typeof(int),
        Category = ScheduleCategory.Spawn,
        DefaultValue = 1,
        MinValue = 0f,
        Description = "从第几波开始允许生成。"
    });

    /// <summary>最大生成波次。</summary>
    public static readonly DataMeta SpawnMaxWave = DataRegistry.Register(new DataMeta
    {
        Key = "Spawn.MaxWave",
        DisplayName = "Spawn Max Wave",
        Type = typeof(int),
        Category = ScheduleCategory.Spawn,
        DefaultValue = -1,
        MinValue = -1f,
        Description = "截止第几波允许生成；-1 表示不限制。"
    });

    /// <summary>生成间隔。</summary>
    public static readonly DataMeta SpawnInterval = DataRegistry.Register(new DataMeta
    {
        Key = "Spawn.Interval",
        DisplayName = "Spawn Interval",
        Type = typeof(float),
        Category = ScheduleCategory.Spawn,
        DefaultValue = 1f,
        MinValue = 0f,
        Description = "同一规则两次生成之间的间隔秒数。"
    });

    /// <summary>单波最大生成数量。</summary>
    public static readonly DataMeta SpawnMaxCountPerWave = DataRegistry.Register(new DataMeta
    {
        Key = "Spawn.MaxCountPerWave",
        DisplayName = "Spawn Max Count Per Wave",
        Type = typeof(int),
        Category = ScheduleCategory.Spawn,
        DefaultValue = -1,
        MinValue = -1f,
        Description = "单波次最大生成数量；-1 表示不限制。"
    });

    /// <summary>单次生成数量。</summary>
    public static readonly DataMeta SpawnSingleCount = DataRegistry.Register(new DataMeta
    {
        Key = "Spawn.SingleCount",
        DisplayName = "Spawn Single Count",
        Type = typeof(int),
        Category = ScheduleCategory.Spawn,
        DefaultValue = 1,
        MinValue = 0f,
        Description = "每次触发规则时生成的基础数量。"
    });

    /// <summary>单次生成数量波动。</summary>
    public static readonly DataMeta SpawnSingleVariance = DataRegistry.Register(new DataMeta
    {
        Key = "Spawn.SingleVariance",
        DisplayName = "Spawn Single Variance",
        Type = typeof(int),
        Category = ScheduleCategory.Spawn,
        DefaultValue = 0,
        MinValue = 0f,
        Description = "单次生成数量的随机波动值。"
    });

    /// <summary>波次开始后的首次生成延迟。</summary>
    public static readonly DataMeta SpawnStartDelay = DataRegistry.Register(new DataMeta
    {
        Key = "Spawn.StartDelay",
        DisplayName = "Spawn Start Delay",
        Type = typeof(float),
        Category = ScheduleCategory.Spawn,
        DefaultValue = 0f,
        MinValue = 0f,
        Description = "波次开始后首次生成的延迟秒数。"
    });

    /// <summary>生成权重。</summary>
    public static readonly DataMeta SpawnWeight = DataRegistry.Register(new DataMeta
    {
        Key = "Spawn.Weight",
        DisplayName = "Spawn Weight",
        Type = typeof(int),
        Category = ScheduleCategory.Spawn,
        DefaultValue = 1,
        MinValue = 0f,
        Description = "随机生成池中的权重。"
    });

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
