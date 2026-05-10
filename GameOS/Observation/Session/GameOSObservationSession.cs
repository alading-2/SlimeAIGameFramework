namespace SkilmeAI.GameOS.Observation;

/// <summary>
/// 捕获单次 scene runner 注入的观测路径。
/// </summary>
public sealed class GameOSObservationSession : IDisposable
{
    private GameOSJsonlLogSink? jsonlSink;

    private GameOSObservationSession(
        string scenePath,
        string mode,
        string? runDirectory,
        string? runDirectoryRelative,
        string? sceneDirectory,
        string? sceneDirectoryRelative,
        string? screenshotDirectory,
        string? screenshotDirectoryRelative,
        string? artifactDirectory,
        string? artifactDirectoryRelative)
    {
        ScenePath = scenePath;
        Mode = mode;
        RunDirectory = runDirectory;
        RunDirectoryRelative = runDirectoryRelative;
        SceneDirectory = sceneDirectory;
        SceneDirectoryRelative = sceneDirectoryRelative;
        ScreenshotDirectory = screenshotDirectory;
        ScreenshotDirectoryRelative = screenshotDirectoryRelative;
        ArtifactDirectory = artifactDirectory;
        ArtifactDirectoryRelative = artifactDirectoryRelative;

        if (!string.IsNullOrWhiteSpace(ArtifactDirectory))
        {
            Directory.CreateDirectory(ArtifactDirectory);
            LogDirectory = System.IO.Path.Combine(ArtifactDirectory, "logs");
            Directory.CreateDirectory(LogDirectory);
            JsonlLogPath = System.IO.Path.Combine(LogDirectory, "scene-log.jsonl");

            if (GameOSLog.Options.EnableJsonl)
            {
                jsonlSink = new GameOSJsonlLogSink(JsonlLogPath);
                GameOSLog.AddSink(jsonlSink);
            }
        }
    }

    public string ScenePath { get; }

    public string Mode { get; }

    public string? RunDirectory { get; }

    public string? RunDirectoryRelative { get; }

    public string? SceneDirectory { get; }

    public string? SceneDirectoryRelative { get; }

    public string? ScreenshotDirectory { get; }

    public string? ScreenshotDirectoryRelative { get; }

    public string? ArtifactDirectory { get; }

    public string? ArtifactDirectoryRelative { get; }

    public string? LogDirectory { get; }

    public string? JsonlLogPath { get; }

    public static GameOSObservationSession FromEnvironment(string scenePath, string mode)
    {
        return FromEnvironment(scenePath, mode, fallbackArtifactDirectory: null);
    }

    public static GameOSObservationSession FromEnvironment(string scenePath, string mode, string? fallbackArtifactDirectory)
    {
        var artifactDirectory = Read("GODOT_SCENE_TEST_ARTIFACT_DIR");
        if (string.IsNullOrWhiteSpace(artifactDirectory) && !string.IsNullOrWhiteSpace(fallbackArtifactDirectory))
        {
            artifactDirectory = fallbackArtifactDirectory;
        }

        return new GameOSObservationSession(
            scenePath,
            mode,
            Read("GODOT_SCENE_TEST_RUN_DIR"),
            Read("GODOT_SCENE_TEST_RUN_DIR_REL"),
            Read("GODOT_SCENE_TEST_SCENE_DIR"),
            Read("GODOT_SCENE_TEST_SCENE_DIR_REL"),
            Read("GODOT_SCENE_TEST_SCREENSHOT_DIR"),
            Read("GODOT_SCENE_TEST_SCREENSHOT_DIR_REL"),
            artifactDirectory,
            Read("GODOT_SCENE_TEST_ARTIFACT_DIR_REL"));
    }

    public string? CreateArtifactPath(string fileName)
    {
        if (string.IsNullOrWhiteSpace(ArtifactDirectory))
        {
            return null;
        }

        Directory.CreateDirectory(ArtifactDirectory);
        return System.IO.Path.Combine(ArtifactDirectory, fileName);
    }

    public void Dispose()
    {
        if (jsonlSink == null)
        {
            return;
        }

        GameOSLog.RemoveSink(jsonlSink);
        jsonlSink.Dispose();
        jsonlSink = null;
    }

    private static string? Read(string key)
    {
        var value = Environment.GetEnvironmentVariable(key);
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
