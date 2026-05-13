using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using SlimeAI.GameOS.Runtime.Resource;

namespace SlimeAI.GameOS.Runtime.Data;

/// <summary>
/// DataOS 生成的 typed runtime snapshot。
/// </summary>
public sealed class RuntimeDataSnapshot
{
    /// <summary>当前 snapshot schema 版本。</summary>
    public int SchemaVersion { get; init; }

    /// <summary>生成时间，UTC ISO-8601 字符串。</summary>
    public string GeneratedAtUtc { get; init; } = string.Empty;

    /// <summary>typed snapshot manifest。</summary>
    public RuntimeSnapshotManifest Manifest { get; init; } = new();

    /// <summary>DataKey descriptor mirror。</summary>
    public List<RuntimeDataDescriptor> Descriptors { get; init; } = new();

    /// <summary>运行时数据记录。</summary>
    public List<RuntimeDataRecord> Records { get; init; } = new();

    /// <summary>资源路径映射。</summary>
    public List<RuntimeResourceEntry> Resources { get; init; } = new();

    /// <summary>
    /// 从 JSON 字符串读取 DataOS snapshot。
    /// </summary>
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
    /// 把一条 snapshot 记录写入运行时 Data；失败时抛出包含 report code 的异常。
    /// </summary>
    public int ApplyRecord(Data data, RuntimeDataRecord record)
    {
        var report = ApplyRecordWithReport(data, record);
        if (report.ErrorCount > 0)
        {
            var issue = report.Errors[0];
            throw new InvalidOperationException(
                $"{issue.Code}: {issue.Summary}; table={issue.TableId}; record={issue.RecordId}; key={issue.StableKey}; expected={issue.ExpectedType}; actual={issue.ActualType}");
        }

        return report.AppliedFieldCount;
    }

    /// <summary>
    /// 把一条 snapshot 记录写入运行时 Data，并返回结构化 report。
    /// </summary>
    public RuntimeSnapshotApplyReport ApplyRecordWithReport(Data data, RuntimeDataRecord record)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(record);

        var descriptorByKey = BuildDescriptorIndex();
        var report = new RuntimeSnapshotApplyReport
        {
            TableId = record.Table,
            RecordId = record.Id
        };

        foreach (var pair in record.Fields)
        {
            var stableKey = pair.Key;
            var field = pair.Value;
            if (!descriptorByKey.TryGetValue(stableKey, out var descriptor))
            {
                report.AddError("snapshot.missing_descriptor", stableKey, "", field.Type, "record field has no descriptor");
                continue;
            }

            if (!data.Catalog.TryResolve(stableKey, out var key))
            {
                report.AddError("snapshot.unknown_key", stableKey, descriptor.ValueType, field.Type, "field key is not present in active catalog");
                continue;
            }

            var expectedType = GetDataOsValueType(key);
            if (!string.Equals(descriptor.ValueType, expectedType, StringComparison.Ordinal))
            {
                report.AddError("snapshot.descriptor_type_drift", stableKey, expectedType, descriptor.ValueType, "descriptor type differs from DataKey type");
                continue;
            }

            if (!string.Equals(field.Type, descriptor.ValueType, StringComparison.Ordinal))
            {
                report.AddError("snapshot.field_type_mismatch", stableKey, descriptor.ValueType, field.Type, "field type differs from descriptor");
                continue;
            }

            if (!ValidateDefaultMirror(key, descriptor, out var defaultError))
            {
                report.AddError("snapshot.default_drift", stableKey, Convert.ToString(key.DefaultValueBoxed, CultureInfo.InvariantCulture) ?? "", descriptor.DefaultValue, defaultError);
                continue;
            }

            if (!field.TryToObject(key, out var value, out var actualType))
            {
                report.AddError("snapshot.wrong_type", stableKey, descriptor.ValueType, actualType, "field value cannot be converted to resolved DataKey type");
                continue;
            }

            if (!data.TryApplyUntyped(key, value, out _, out _))
            {
                report.AddError("snapshot.apply_rejected", stableKey, descriptor.ValueType, actualType, "Data rejected typed field value");
                continue;
            }

            report.AppliedFieldCount++;
        }

        return report;
    }

    /// <summary>
    /// 把 snapshot 中的资源映射注册到 ResourceCatalog。
    /// </summary>
    public RuntimeSnapshotApplyReport RegisterResourcesWithReport()
    {
        var report = new RuntimeSnapshotApplyReport();
        for (var i = 0; i < Resources.Count; i++)
        {
            var resource = Resources[i];
            if (!string.Equals(resource.OwnerCapability, "shared", StringComparison.Ordinal)
                && !Manifest.EnabledCapabilities.Contains(resource.OwnerCapability))
            {
                report.AddError("snapshot.resource_disabled_capability", resource.Key, "enabled capability", resource.OwnerCapability, "resource belongs to disabled capability");
                continue;
            }

            if (!Enum.TryParse<ResourceCategory>(resource.Category, ignoreCase: false, out var category))
            {
                report.AddError("snapshot.resource_category", resource.Key, nameof(ResourceCategory), resource.Category, "resource category is invalid");
                continue;
            }

            ResourceCatalog.Register(resource.Key, category, resource.Path);
            report.RegisteredResourceCount++;
        }

        return report;
    }

    /// <summary>
    /// 把 snapshot 中的资源映射注册到 ResourceCatalog。
    /// </summary>
    public int RegisterResources()
    {
        var report = RegisterResourcesWithReport();
        if (report.ErrorCount > 0)
        {
            throw new InvalidOperationException(report.Errors[0].Summary);
        }

        return report.RegisteredResourceCount;
    }

    private Dictionary<string, RuntimeDataDescriptor> BuildDescriptorIndex()
    {
        var result = new Dictionary<string, RuntimeDataDescriptor>(StringComparer.Ordinal);
        for (var i = 0; i < Descriptors.Count; i++)
        {
            result[Descriptors[i].StableKey] = Descriptors[i];
        }

        return result;
    }

    private static string GetDataOsValueType(IDataKey key)
    {
        var type = Nullable.GetUnderlyingType(key.ValueType) ?? key.ValueType;
        if (type == typeof(bool)) return "bool";
        if (type == typeof(int) || type == typeof(uint)) return "int";
        if (type == typeof(float)) return "float";
        if (type == typeof(double)) return "double";
        return "string";
    }

    private static bool ValidateDefaultMirror(IDataKey key, RuntimeDataDescriptor descriptor, out string error)
    {
        if (!key.TryConvert(descriptor.DefaultValue, out var converted))
        {
            error = "descriptor default cannot convert to DataKey type";
            return false;
        }

        if (ValuesEqual(key.DefaultValueBoxed, converted))
        {
            error = "";
            return true;
        }

        error = "descriptor default differs from DataKey default";
        return false;
    }

    private static bool ValuesEqual(object? expected, object? actual)
    {
        if (expected == null || actual == null)
        {
            return expected == actual;
        }

        if (expected is float expectedFloat)
        {
            return Math.Abs(expectedFloat - Convert.ToSingle(actual, CultureInfo.InvariantCulture)) < 0.0001f;
        }

        if (expected is double expectedDouble)
        {
            return Math.Abs(expectedDouble - Convert.ToDouble(actual, CultureInfo.InvariantCulture)) < 0.0001;
        }

        if (expected.GetType().IsEnum)
        {
            return string.Equals(expected.ToString(), actual.ToString(), StringComparison.Ordinal);
        }

        return Equals(expected, actual);
    }
}

/// <summary>
/// Snapshot manifest。
/// </summary>
public sealed class RuntimeSnapshotManifest
{
    /// <summary>schema 版本。</summary>
    public int SchemaVersion { get; init; }

    /// <summary>生成时间。</summary>
    public string GeneratedAtUtc { get; init; } = string.Empty;

    /// <summary>profile id。</summary>
    public string Profile { get; init; } = string.Empty;

    /// <summary>catalog id。</summary>
    public string CatalogId { get; init; } = string.Empty;

    /// <summary>启用的 capabilities。</summary>
    public List<string> EnabledCapabilities { get; init; } = new();

    /// <summary>descriptor 数量。</summary>
    public int DescriptorCount { get; init; }

    /// <summary>record 数量。</summary>
    public int RecordCount { get; init; }

    /// <summary>resource 数量。</summary>
    public int ResourceCount { get; init; }

    /// <summary>validation summary。</summary>
    public RuntimeValidationSummary Validation { get; init; } = new();
}

/// <summary>
/// Snapshot descriptor mirror。
/// </summary>
public sealed class RuntimeDataDescriptor
{
    /// <summary>稳定 DataKey。</summary>
    public string StableKey { get; init; } = string.Empty;

    /// <summary>owner capability。</summary>
    public string OwnerCapability { get; init; } = string.Empty;

    /// <summary>owner skill。</summary>
    public string OwnerSkill { get; init; } = string.Empty;

    /// <summary>DataOS 值类型。</summary>
    public string ValueType { get; init; } = string.Empty;

    /// <summary>默认值镜像。</summary>
    public string DefaultValue { get; init; } = string.Empty;

    /// <summary>显示名。</summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>描述。</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>图标路径。</summary>
    public string IconPath { get; init; } = string.Empty;

    /// <summary>分类。</summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>下限。</summary>
    public float? MinValue { get; init; }

    /// <summary>上限。</summary>
    public float? MaxValue { get; init; }

    /// <summary>选项。</summary>
    public List<string> Options { get; init; } = new();

    /// <summary>百分比标记。</summary>
    public bool IsPercentage { get; init; }

    /// <summary>是否允许 modifier。</summary>
    public bool SupportsModifiers { get; init; }

    /// <summary>是否 computed。</summary>
    public bool IsComputed { get; init; }
}

/// <summary>
/// Snapshot 中的一条数据记录。
/// </summary>
public sealed class RuntimeDataRecord
{
    /// <summary>表 Id，例如 unit.enemy。</summary>
    public string Table { get; init; } = string.Empty;

    /// <summary>记录稳定 Id。</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>显示名或旧 DataNew Name。</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>字段集合，键为稳定 DataKey。</summary>
    public Dictionary<string, RuntimeDataField> Fields { get; init; } = new(StringComparer.Ordinal);
}

/// <summary>
/// Snapshot 字段值。
/// </summary>
public sealed class RuntimeDataField
{
    /// <summary>字段值类型。</summary>
    public string Type { get; init; } = "string";

    /// <summary>JSON 原始字段值。</summary>
    public JsonElement Value { get; init; }

    /// <summary>
    /// 严格转为运行时 Data 可接受的 CLR 值。
    /// </summary>
    public bool TryToObject(IDataKey key, out object? value, out string actualType)
    {
        actualType = Value.ValueKind.ToString();
        value = null;

        try
        {
            object? raw = Type switch
            {
                "bool" => ReadBool(out actualType),
                "int" => ReadInt(out actualType),
                "float" => ReadFloat(out actualType),
                "double" => ReadDouble(out actualType),
                "string" => Value.ValueKind == JsonValueKind.Null ? null : Value.GetString() ?? string.Empty,
                _ => Value.ValueKind == JsonValueKind.String ? Value.GetString() : Value.GetRawText()
            };

            if (!key.TryConvert(raw, out value))
            {
                return false;
            }

            actualType = Type;
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 兼容旧调用点的非严格转换。新 loader 不使用它吞错。
    /// </summary>
    public object? ToObject()
    {
        return Type switch
        {
            "bool" => ReadBool(out _),
            "int" => ReadInt(out _),
            "float" => ReadFloat(out _),
            "double" => ReadDouble(out _),
            "string" => Value.ValueKind == JsonValueKind.Null ? null : Value.GetString() ?? string.Empty,
            _ => Value.ValueKind == JsonValueKind.String ? Value.GetString() : Value.GetRawText()
        };
    }

    private bool ReadBool(out string actualType)
    {
        actualType = Value.ValueKind.ToString();
        if (Value.ValueKind == JsonValueKind.True) return true;
        if (Value.ValueKind == JsonValueKind.False) return false;
        if (Value.ValueKind == JsonValueKind.String
            && bool.TryParse(Value.GetString(), out var parsed))
        {
            actualType = "bool";
            return parsed;
        }

        throw new FormatException("Invalid bool value.");
    }

    private int ReadInt(out string actualType)
    {
        actualType = Value.ValueKind.ToString();
        if (Value.ValueKind == JsonValueKind.Number && Value.TryGetInt32(out var parsed))
        {
            actualType = "int";
            return parsed;
        }

        if (Value.ValueKind == JsonValueKind.String
            && int.TryParse(Value.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed))
        {
            actualType = "int";
            return parsed;
        }

        throw new FormatException("Invalid int value.");
    }

    private float ReadFloat(out string actualType)
    {
        actualType = Value.ValueKind.ToString();
        if (Value.ValueKind == JsonValueKind.Number && Value.TryGetSingle(out var parsed))
        {
            actualType = "float";
            return parsed;
        }

        if (Value.ValueKind == JsonValueKind.String
            && float.TryParse(Value.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
        {
            actualType = "float";
            return parsed;
        }

        throw new FormatException("Invalid float value.");
    }

    private double ReadDouble(out string actualType)
    {
        actualType = Value.ValueKind.ToString();
        if (Value.ValueKind == JsonValueKind.Number && Value.TryGetDouble(out var parsed))
        {
            actualType = "double";
            return parsed;
        }

        if (Value.ValueKind == JsonValueKind.String
            && double.TryParse(Value.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
        {
            actualType = "double";
            return parsed;
        }

        throw new FormatException("Invalid double value.");
    }
}

/// <summary>
/// Snapshot 资源映射。
/// </summary>
public sealed class RuntimeResourceEntry
{
    /// <summary>ResourceCategory 名称。</summary>
    public string Category { get; init; } = string.Empty;

    /// <summary>稳定资源键。</summary>
    public string Key { get; init; } = string.Empty;

    /// <summary>Godot res:// 路径。</summary>
    public string Path { get; init; } = string.Empty;

    /// <summary>owner capability。</summary>
    public string OwnerCapability { get; init; } = "shared";

    /// <summary>legacy 状态。</summary>
    public string LegacyStatus { get; init; } = "active";
}

/// <summary>
/// Snapshot apply report。
/// </summary>
public sealed class RuntimeSnapshotApplyReport
{
    /// <summary>表 Id。</summary>
    public string TableId { get; init; } = string.Empty;

    /// <summary>记录 Id。</summary>
    public string RecordId { get; init; } = string.Empty;

    /// <summary>已应用字段数。</summary>
    public int AppliedFieldCount { get; set; }

    /// <summary>已注册资源数。</summary>
    public int RegisteredResourceCount { get; set; }

    /// <summary>跳过字段数。</summary>
    public int SkippedFieldCount { get; set; }

    /// <summary>警告数。</summary>
    public int WarningCount => Warnings.Count;

    /// <summary>错误数。</summary>
    public int ErrorCount => Errors.Count;

    /// <summary>错误列表。</summary>
    public List<RuntimeSnapshotIssue> Errors { get; } = new();

    /// <summary>警告列表。</summary>
    public List<RuntimeSnapshotIssue> Warnings { get; } = new();

    /// <summary>添加错误。</summary>
    public void AddError(string code, string stableKey, string expectedType, string actualType, string summary)
    {
        SkippedFieldCount++;
        Errors.Add(new RuntimeSnapshotIssue(code, "error", TableId, RecordId, stableKey, expectedType, actualType, summary));
    }
}

/// <summary>
/// Snapshot validation issue。
/// </summary>
public readonly record struct RuntimeSnapshotIssue(
    string Code,
    string Severity,
    string TableId,
    string RecordId,
    string StableKey,
    string ExpectedType,
    string ActualType,
    string Summary);

/// <summary>
/// Validation summary。
/// </summary>
public sealed class RuntimeValidationSummary
{
    /// <summary>warning count。</summary>
    public int WarningCount { get; init; }

    /// <summary>error count。</summary>
    public int ErrorCount { get; init; }
}
