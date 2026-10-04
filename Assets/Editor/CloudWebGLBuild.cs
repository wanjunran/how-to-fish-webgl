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
            // unity-builder v6 通过 -customBuildPath 传入输出目录，不设 BUILD_PATH
            // 环境变量，所以两处都要看。v6 传入的值形如
            // /github/workspace/build/WebGL，此时直接采用，不要再追加 WebGL，
            // 否则产物会落到 build/WebGL/WebGL，与 artifact 收集路径脱节。
            string customBuildPath = ReadCommandLineValue("-customBuildPath");
            if (!string.IsNullOrEmpty(customBuildPath))
            {
                return customBuildPath;
            }

            string buildPath = Environment.GetEnvironmentVariable("BUILD_PATH");
            if (!string.IsNullOrEmpty(buildPath))
            {
                return buildPath;
            }

            return Path.Combine(Directory.GetCurrentDirectory(), "build", "WebGL");
        }

        /// <summary>
        /// 从 -key value 形式的命令行参数里取值。Unity 的 -customBuildPath 与
        /// -buildPath 都以空格分隔，GetCommandLineArgs 里是相邻两个元素。
        /// </summary>
        private static string ReadCommandLineValue(string key)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], key, StringComparison.OrdinalIgnoreCase))
                {
                    return args[i + 1];
                }
            }

            return null;
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
