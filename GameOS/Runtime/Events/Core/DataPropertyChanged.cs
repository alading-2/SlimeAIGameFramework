using SkilmeAI.GameOS.Runtime.Data;
using SkilmeAI.GameOS.Runtime.Event;

namespace SkilmeAI.GameOS.Runtime.Events.Core;

/// <summary>运行时 Data 值发生变化。唯一 producer 是 EventDataChangeSink（桥接 Data 层）。</summary>
public readonly record struct DataPropertyChanged(DataChangedEventData Change) : IEntityEvent;
