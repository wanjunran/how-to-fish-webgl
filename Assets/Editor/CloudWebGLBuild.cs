using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace WebGLCloudBuild
{
    /// <summary>
    /// 云构建（GameCI unity-builder v6）调用的 WebGL 构建入口。
    /// 由 .github/workflows/build-webgl.yml 的 buildMethod 指定为
    /// WebGLCloudBuild.CloudWebGLBuild.Perform。
    /// <para>
    /// unity-builder 会把输出根目录以 -buildPath 传入；未传入时回退到
    /// build/WebGL，最终产物落在 <输出目录>/WebGL/ 下，与 v6 默认的
    /// buildsPath 约定一致。
    /// </para>
    /// </summary>
    public static class CloudWebGLBuild
    {
        private const string LogPrefix = "[CloudWebGLBuild]";

        public static void Perform()
        {
            string outputDir = ResolveOutputDir();
            Directory.CreateDirectory(outputDir);

            string[] scenes = ResolveScenes();
            Log($"{LogPrefix} 输出目录: {outputDir}");
            Log($"{LogPrefix} 参与构建的场景 ({scenes.Length}):");
            foreach (string scene in scenes)
            {
                Log($"{LogPrefix}   - {scene}");
            }

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = Path.Combine(outputDir, "index.html"),
                target = BuildTarget.WebGL,
                targetGroup = BuildTargetGroup.WebGL,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            Log($"{LogPrefix} ===== 构建结果 =====");
            Log($"{LogPrefix} Result      : {summary.result}");
            Log($"{LogPrefix} TotalSize    : {summary.totalSize} bytes");
            Log($"{LogPrefix} TotalErrors  : {summary.totalErrors}");
            Log($"{LogPrefix} TotalWarnings: {summary.totalWarnings}");
            Log($"{LogPrefix} Duration     : {summary.totalTime}");

            if (summary.result == BuildResult.Succeeded)
            {
                string[] files = Directory.Exists(outputDir)
                    ? Directory.GetFiles(outputDir, "*", SearchOption.AllDirectories)
                    : new string[0];

                long total = files.Sum(f => new FileInfo(f).Length);
                Log($"{LogPrefix} 产物文件数: {files.Length}, 合计: {total / 1024 / 1024} MB");
                Log($"{LogPrefix} BUILD_OK");
            }
            else
            {
                Log($"{LogPrefix} BUILD_FAILED");
            }
        }

        private static string ResolveOutputDir()
        {
            string buildPath = Environment.GetEnvironmentVariable("BUILD_PATH");
            if (!string.IsNullOrEmpty(buildPath))
            {
                return buildPath;
            }

            return Path.Combine(Directory.GetCurrentDirectory(), "build", "WebGL");
        }

        private static string[] ResolveScenes()
        {
            var enabled = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

            if (enabled.Length > 0)
            {
                return enabled;
            }

            Debug.LogWarning($"{LogPrefix} EditorBuildSettings 中没有启用的场景，回退到默认场景列表。");
            return new[]
            {
                "Assets/Scenes/Game.unity",
                "Assets/Scenes/Islands/Island1.unity"
            };
        }

        private static void Log(string message)
        {
            Debug.Log(message);
            Console.WriteLine(message);
        }
    }
}
