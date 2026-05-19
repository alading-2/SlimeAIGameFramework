using SlimeAI.GameOS.Observation;
using static TestAssert;

internal partial class Program
{
    static void TestObservationLogLevelFilterAndFormatting()
    {
        GameOSLog.Reset(new GameOSLogOptions { EnableStdout = false, MinimumLevel = GameOSLogLevel.Info, EnableJsonl = false });
        var memory = new GameOSMemoryLogSink();
        GameOSLog.AddSink(memory);

        var log = GameOSLog.For("ObservationTest");
        log.Debug("filtered");
        log.Info("hello world", new Dictionary<string, object?>
        {
            ["count"] = 2,
            ["name"] = "alpha beta"
        });
        log.Pass("check ok");

        AssertEqual("filtered count", 2, memory.Entries.Count);
        AssertEqual("first level", GameOSLogLevel.Info, memory.Entries[0].Level);
        AssertEqual("formatted text", "[INFO][ObservationTest] hello world count=2 name=\"alpha beta\"", memory.Entries[0].FormatText());

        GameOSLog.Reset(new GameOSLogOptions { EnableStdout = false, EnableJsonl = false });
    }

    static void TestObservationJsonlSerialization()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"gameos-observation-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var path = Path.Combine(tempDir, "scene-log.jsonl");
            using (var sink = new GameOSJsonlLogSink(path))
            {
                sink.Emit(new GameOSLogEntry(GameOSLogLevel.Pass, "JsonlTest", "json ok", new Dictionary<string, object?>
                {
                    ["value"] = 7
                }));
            }

            var line = File.ReadAllLines(path).Single();
            AssertEqual("jsonl has pass", true, line.Contains("\"level\":\"PASS\"", StringComparison.Ordinal));
            AssertEqual("jsonl has context", true, line.Contains("\"context\":\"JsonlTest\"", StringComparison.Ordinal));
            AssertEqual("jsonl has formatted text", true, line.Contains("[PASS][JsonlTest] json ok value=7", StringComparison.Ordinal));
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    static void TestSceneValidationSessionAggregatesFailures()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"gameos-scene-validation-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        GameOSLog.Reset(new GameOSLogOptions { EnableStdout = false, EnableJsonl = true });
        try
        {
            using var observation = GameOSObservationSession.FromEnvironment(
                "res://Scenes/Validation/Test.tscn",
                "validation",
                tempDir);
            using var validation = new SceneValidationSession(
                observation,
                "ValidationTest",
                "Runtime/Test",
                "validation.json",
                new[] { "DependencyA" },
                new[] { "NoteA" });

            validation.Check("passes", "Core", () => CheckResult.Pass("ok", new Dictionary<string, object?> { ["seen"] = true }));
            validation.Check("fails", "Core", () => CheckResult.Fail("not ok"));
            var artifactPath = validation.WriteArtifact();

            AssertEqual("validation success", false, validation.Success);
            AssertEqual("failure count", 1, validation.FailureReasons.Count);
            AssertEqual("check count", 2, validation.Checks.Count);
            AssertEqual("artifact written", true, File.Exists(artifactPath));
            AssertEqual("jsonl written", true, File.Exists(Path.Combine(tempDir, "logs", "scene-log.jsonl")));

            var artifact = File.ReadAllText(artifactPath!);
            AssertEqual("artifact status lower", true, artifact.Contains("\"status\": \"fail\"", StringComparison.Ordinal));
            AssertEqual("artifact includes fail reason", true, artifact.Contains("fails: not ok", StringComparison.Ordinal));
        }
        finally
        {
            GameOSLog.Reset(new GameOSLogOptions { EnableStdout = false, EnableJsonl = false });
            Directory.Delete(tempDir, recursive: true);
        }
    }

    static void TestSceneValidationSessionWritesStandardAnswerFields()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"gameos-scene-validation-standard-answer-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        GameOSLog.Reset(new GameOSLogOptions { EnableStdout = false, EnableJsonl = true });
        try
        {
            using var observation = GameOSObservationSession.FromEnvironment(
                "res://Scenes/Validation/Test.tscn",
                "validation",
                tempDir);
            using var validation = new SceneValidationSession(
                observation,
                "ValidationStandardAnswerTest",
                "Runtime/Test",
                "validation-standard-answer.json",
                new[] { "DependencyA" },
                new[] { "NoteA" },
                expectedInputs: new[] { "input-a" },
                expectedObservations: new[] { "observation-a" },
                passCriteria: new[] { "pass-a" },
                failCriteria: new[] { "fail-a" });

            validation.Check("passes", "Core", () => CheckResult.Pass("ok"));
            var artifactPath = validation.WriteArtifact();

            var artifact = File.ReadAllText(artifactPath!);
            using var json = System.Text.Json.JsonDocument.Parse(artifact);
            var root = json.RootElement;

            AssertEqual("expected inputs field exists", true, root.TryGetProperty("expectedInputs", out _));
            AssertEqual("expected observations field exists", true, root.TryGetProperty("expectedObservations", out _));
            AssertEqual("pass criteria field exists", true, root.TryGetProperty("passCriteria", out _));
            AssertEqual("fail criteria field exists", true, root.TryGetProperty("failCriteria", out _));
            AssertEqual("artifact path field exists", true, root.TryGetProperty("artifactPath", out _));
            AssertEqual("expected inputs preserved", "input-a", root.GetProperty("expectedInputs")[0].GetString());
            AssertEqual("expected observations preserved", "observation-a", root.GetProperty("expectedObservations")[0].GetString());
            AssertEqual("pass criteria preserved", "pass-a", root.GetProperty("passCriteria")[0].GetString());
            AssertEqual("fail criteria preserved", "fail-a", root.GetProperty("failCriteria")[0].GetString());
        }
        finally
        {
            GameOSLog.Reset(new GameOSLogOptions { EnableStdout = false, EnableJsonl = false });
            Directory.Delete(tempDir, recursive: true);
        }
    }
}
