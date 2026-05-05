using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using SkilmeAI.GameOS.Runtime.Resource;

namespace SkilmeAI.GameOS.Runtime.Data;

/// <summary>
/// DataOS 生成的运行时数据快照。
/// </summary>
public sealed class RuntimeDataSnapshot
{
    /// <summary>当前 snapshot schema 版本。</summary>
    public int SchemaVersion { get; init; }

    /// <summary>生成时间，UTC ISO-8601 字符串。</summary>
    public string GeneratedAtUtc { get; init; } = string.Empty;

    /// <summary>运行时数据记录。</summary>
    public List<RuntimeDataRecord> Records { get; init; } = new();

    /// <summary>资源路径映射。</summary>
    public List<RuntimeResourceEntry> Resources { get; init; } = new();

    /// <summary>
    /// 从 JSON 字符串读取 DataOS snapshot。
    /// </summary>
    /// <param name="json">DataOS generator 输出的 JSON。</param>
    public static RuntimeDataSnapshot FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        return JsonSerializer.Deserialize<RuntimeDataSnapshot>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException("DataOS snapshot JSON 解析失败。");
    }

    /// <summary>
    /// 查找指定表中的记录，优先匹配 Id，其次匹配 Name。
    /// </summary>
    /// <param name="tableId">DataOS 表 Id。</param>
    /// <param name="recordIdOrName">记录 Id 或显示名。</param>
    /// <param name="record">匹配到的记录。</param>
    public bool TryFindRecord(string tableId, string recordIdOrName, out RuntimeDataRecord record)
    {
        for (var i = 0; i < Records.Count; i++)
        {
            var current = Records[i];
            if (current.Table == tableId
                && (current.Id == recordIdOrName || current.Name == recordIdOrName))
            {
                record = current;
                return true;
            }
        }

        record = default!;
        return false;
    }

    /// <summary>
    /// 把一条 snapshot 记录写入运行时 Data。
    /// </summary>
    /// <param name="data">目标 Data 容器。</param>
    /// <param name="record">DataOS snapshot 记录。</param>
    public int ApplyRecord(Data data, RuntimeDataRecord record)
    {
        ArgumentNullException.ThrowIfNull(data);
        var applied = 0;
        foreach (var pair in record.Fields)
        {
            data.Set(pair.Key, pair.Value.ToObject());
            applied++;
        }

        return applied;
    }

    /// <summary>
    /// 把 snapshot 中的资源映射注册到 ResourceCatalog。
    /// </summary>
    public int RegisterResources()
    {
        var registered = 0;
        for (var i = 0; i < Resources.Count; i++)
        {
            var resource = Resources[i];
            if (!Enum.TryParse<ResourceCategory>(resource.Category, ignoreCase: false, out var category))
            {
                continue;
            }

            ResourceCatalog.Register(resource.Key, category, resource.Path);
            registered++;
        }

        return registered;
    }
}

/// <summary>
/// DataOS snapshot 中的一条数据记录。
/// </summary>
public sealed class RuntimeDataRecord
{
    /// <summary>表 Id，例如 unit.enemy。</summary>
    public string Table { get; init; } = string.Empty;

    /// <summary>记录稳定 Id。</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>显示名或旧 DataNew Name。</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>字段集合，键为运行时 DataKey。</summary>
    public Dictionary<string, RuntimeDataField> Fields { get; init; } = new(StringComparer.Ordinal);
}

/// <summary>
/// DataOS snapshot 字段值。
/// </summary>
public sealed class RuntimeDataField
{
    /// <summary>字段值类型。</summary>
    public string Type { get; init; } = "string";

    /// <summary>JSON 原始字段值。</summary>
    public JsonElement Value { get; init; }

    /// <summary>
    /// 转为运行时 Data 可接受的 CLR 值。
    /// </summary>
    public object? ToObject()
    {
        return Type switch
        {
            "bool" => ReadBool(),
            "int" => ReadInt(),
            "float" => ReadFloat(),
            "double" => ReadDouble(),
            "string" => Value.ValueKind == JsonValueKind.Null ? null : Value.GetString() ?? string.Empty,
            _ => Value.ValueKind == JsonValueKind.String ? Value.GetString() : Value.GetRawText()
        };
    }

    private bool ReadBool()
    {
        if (Value.ValueKind == JsonValueKind.True) return true;
        if (Value.ValueKind == JsonValueKind.False) return false;
        return bool.TryParse(Value.GetString(), out var parsed) && parsed;
    }

    private int ReadInt()
    {
        if (Value.ValueKind == JsonValueKind.Number && Value.TryGetInt32(out var parsed)) return parsed;
        return int.TryParse(Value.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed) ? parsed : 0;
    }

    private float ReadFloat()
    {
        if (Value.ValueKind == JsonValueKind.Number && Value.TryGetSingle(out var parsed)) return parsed;
        return float.TryParse(Value.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out parsed) ? parsed : 0f;
    }

    private double ReadDouble()
    {
        if (Value.ValueKind == JsonValueKind.Number && Value.TryGetDouble(out var parsed)) return parsed;
        return double.TryParse(Value.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out parsed) ? parsed : 0.0;
    }
}

/// <summary>
/// DataOS snapshot 资源映射。
/// </summary>
public sealed class RuntimeResourceEntry
{
    /// <summary>ResourceCategory 名称。</summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>稳定资源键。</summary>
    public string Key { get; init; } = string.Empty;

    /// <summary>Godot res:// 路径。</summary>
    public string Path { get; init; } = string.Empty;
}
