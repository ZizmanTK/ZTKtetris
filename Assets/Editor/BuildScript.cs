using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Headless builds for itch.io:
//   Unity.exe -batchmode -quit -projectPath . -executeMethod BuildScript.BuildAll
// Output goes to Builds/WebGL and Builds/Windows (both gitignored).
public static class BuildScript
{
    const string BuildRoot = "Builds";

    [MenuItem("ZKTris/Build WebGL")]
    public static void BuildWebGL()
    {
        // itch.io does not always send Content-Encoding headers, so let the
        // loader decompress in JavaScript when the browser didn't.
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.WebGL.dataCaching = true;
        // Default is "shorter build time"; release builds should be small.
        UnityEditor.WebGL.UserBuildSettings.codeOptimization = UnityEditor.WebGL.WasmCodeOptimization.DiskSizeLTO;
        Build(BuildTarget.WebGL, Path.Combine(BuildRoot, "WebGL"));
    }

    [MenuItem("ZKTris/Build Windows")]
    public static void BuildWindows()
    {
        Build(BuildTarget.StandaloneWindows64, Path.Combine(BuildRoot, "Windows", PlayerSettings.productName + ".exe"));
    }

    [MenuItem("ZKTris/Build All")]
    public static void BuildAll()
    {
        BuildWebGL();
        BuildWindows();
    }

    static void Build(BuildTarget target, string locationPath)
    {
        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled && !string.IsNullOrEmpty(s.path)).Select(s => s.path).ToArray();
        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = locationPath,
            target = target,
            options = BuildOptions.None,
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;
        Debug.Log($"[BuildScript] {target}: {summary.result}, {summary.totalSize / (1024f * 1024f):F1} MB, {summary.totalErrors} errors -> {locationPath}");
        if (summary.result != BuildResult.Succeeded && Application.isBatchMode)
            EditorApplication.Exit(1);
    }
}
