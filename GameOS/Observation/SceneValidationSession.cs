using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SkilmeAI.GameOS.Observation;

/// <summary>
/// Reusable helper for Godot scene validation checks and artifacts.
/// </summary>
public sealed class SceneValidationSession : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly GameOSMemoryLogSink memorySink = new();
    private readonly GameOSContextLog log;
    private readonly List<SceneValidationCheck> checks = new();
    private readonly List<string> failureReasons = new();
    private readonly List<string> dependencies = new();
    private readonly List<string> notes = new();

    public SceneValidationSession(
        GameOSObservationSession observation,
        string context,
        string layer,
        string artifactFileName,
        IEnumerable<string>? dependencies = null,
        IEnumerable<string>? notes = null)
    {
        Observation = observation;
        Context = context;
        Layer = layer;
        ArtifactFileName = artifactFileName;
        if (dependencies != null)
        {
            this.dependencies.AddRange(dependencies);
        }

        if (notes != null)
        {
            this.notes.AddRange(notes);
        }

        GameOSLog.AddSink(memorySink);
        log = GameOSLog.For(context);
    }

    public GameOSObservationSession Observation { get; }

    public string Context { get; }

    public string Layer { get; }

    public string ArtifactFileName { get; }

    public IReadOnlyList<SceneValidationCheck> Checks => checks;

    public IReadOnlyList<string> FailureReasons => failureReasons;

    public bool Success => failureReasons.Count == 0;

    public void Info(string message, IReadOnlyDictionary<string, object?>? values = null) => log.Info(message, values);

    public void Pass(string message, IReadOnlyDictionary<string, object?>? values = null) => log.Pass(message, values);

    public void Fail(string message, IReadOnlyDictionary<string, object?>? values = null) => log.Fail(message, values);

    public CheckResult Check(string name, string category, Func<CheckResult> validate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(category);
        ArgumentNullException.ThrowIfNull(validate);

        log.Info($"check {name} start");
        try
        {
            var result = validate();
            checks.Add(new SceneValidationCheck(name, result.Success ? "pass" : "fail", category, result.Details));
            if (result.Success)
            {
                log.Pass($"check {name} {result.Message}");
            }
            else
            {
                var reason = $"{name}: {result.Message}";
                failureReasons.Add(reason);
                log.Fail($"check {name} {result.Message}");
            }

            return result;
        }
        catch (Exception ex)
        {
            var details = new Dictionary<string, object?>
            {
                ["exceptionType"] = ex.GetType().FullName,
                ["message"] = ex.Message
            };
            checks.Add(new SceneValidationCheck(name, "fail", category, details));
            var reason = $"{name}: unexpected exception {ex.GetType().Name}: {ex.Message}";
            failureReasons.Add(reason);
            log.Fail(reason);
            return CheckResult.Fail(reason, details);
        }
    }

    public string? WriteArtifact()
    {
        var artifactPath = Observation.CreateArtifactPath(ArtifactFileName);
        if (string.IsNullOrWhiteSpace(artifactPath))
        {
            return null;
        }

        log.Info($"writing artifact {artifactPath}");
        var artifact = new SceneValidationArtifact(
            Success ? "pass" : "fail",
            Observation.ScenePath,
            Layer,
            Observation.Mode,
            checks,
            memorySink.Entries.Select(SceneValidationLogEntry.From).ToArray(),
            failureReasons,
            dependencies,
            notes);

        File.WriteAllText(artifactPath, $"{JsonSerializer.Serialize(artifact, JsonOptions)}\n");
        return artifactPath;
    }

    public void Dispose()
    {
        GameOSLog.RemoveSink(memorySink);
    }
}

public sealed record SceneValidationCheck(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("category")] string Category,
    [property: JsonPropertyName("details")] IReadOnlyDictionary<string, object?> Details);

public sealed record SceneValidationLogEntry(
    [property: JsonPropertyName("timestamp")] string Timestamp,
    [property: JsonPropertyName("level")] string Level,
    [property: JsonPropertyName("context")] string Context,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("values")] IReadOnlyDictionary<string, object?> Values)
{
    public static SceneValidationLogEntry From(GameOSLogEntry entry)
    {
        return new SceneValidationLogEntry(
            entry.Timestamp.ToString("O"),
            entry.Level.ToString().ToUpperInvariant(),
            entry.Context,
            entry.Message,
            entry.Values);
    }
}

public sealed record SceneValidationArtifact(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("scene")] string Scene,
    [property: JsonPropertyName("layer")] string Layer,
    [property: JsonPropertyName("mode")] string Mode,
    [property: JsonPropertyName("checks")] IReadOnlyList<SceneValidationCheck> Checks,
    [property: JsonPropertyName("logs")] IReadOnlyList<SceneValidationLogEntry> Logs,
    [property: JsonPropertyName("failureReasons")] IReadOnlyList<string> FailureReasons,
    [property: JsonPropertyName("dependencies")] IReadOnlyList<string> Dependencies,
    [property: JsonPropertyName("notes")] IReadOnlyList<string> Notes);
