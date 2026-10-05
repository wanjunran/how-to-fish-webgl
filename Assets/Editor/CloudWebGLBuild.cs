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
                // 传目录，不是目录下的文件名。
                //
                // Unity 的 WebGL 构建规则：locationPathName 若不带扩展名，就当作
                // 目录处理，index.html 与 Build/、StreamingAssets/ 直接铺在它
                // 下面。带了文件名则会被当成目录名，产物就多套一层：
                //   build/WebGL/index.html/index.html
                //   build/WebGL/index.html/Build/index.html.wasm.gz
                //
                // game-ci 的 validateBuild 在 build/ 下按平台目录找标志文件，
                // 多这一层就找不到，把成功的构建判成failure，Upload 步骤被
                // skipped。轮次 20 就死在这里：82 个文件 207MB 全都生成好了，
                // 整轮却报"There was an error building the project"。
                locationPathName = outputDir,
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
            // 环境变量，所以两处都要看。
            //
            // 轮次 20 的教训：不要假设 v6 传入的值长什么样，也不要假设它已经包含
            // 你想要的层级。实测它传的是
            //     /github/workspace/build/WebGL/WebGL
            // 比 artifact 收集路径 build/ 多了两层。产物因此落在
            //     build/WebGL/WebGL/index.html/...
            // 而 game-ci 的 validateBuild 在 build/ 下按平台目录找标志文件，
            // 找不到就把成功的一轮判为 failure，Upload 步骤被 skipped。
            //
            // 稳妥做法：剥掉末尾重复的 "WebGL"，让产物落在 build/WebGL/。
            // artifact 步骤收的是整个 build/，所以少一层不影响上传。
            string customBuildPath = ReadCommandLineValue("-customBuildPath");
            if (!string.IsNullOrEmpty(customBuildPath))
            {
                return NormalizeWebGLPath(customBuildPath);
            }

            string buildPath = Environment.GetEnvironmentVariable("BUILD_PATH");
            if (!string.IsNullOrEmpty(buildPath))
            {
                return NormalizeWebGLPath(buildPath);
            }

            return Path.Combine(Directory.GetCurrentDirectory(), "build", "WebGL");
        }

        /// <summary>
        /// 把 v6 传入的路径里末尾多余的 "WebGL" 段去掉。
        ///
        /// v6 传的路径已经以 WebGL 结尾（build/WebGL/WebGL），而 Unity 又会在
        /// locationPathName 指定的文件名下建目录，于是实际层级是
        /// build/WebGL/WebGL/index.html/。这里只保证 build 目录下面是单个
        /// WebGL 平台目录，validateBuild 才认得出。
        ///
        /// 用 Path.GetFileName 逐段判断，不用字符串 EndsWith 硬剥 —— 后者会把
        /// 恰好叫 "WebGLSomething" 的目录也切错。
        /// </summary>
        private static string NormalizeWebGLPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            string normalized = path.Replace('\\', '/').TrimEnd('/');
            // 只剥一层：v6 传的是 .../WebGL/WebGL，留下一层给平台目录。
            if (Path.GetFileName(normalized) == "WebGL" &&
                Path.GetFileName(Path.GetDirectoryName(normalized) ?? "") == "WebGL")
            {
                normalized = Path.GetDirectoryName(normalized) ?? normalized;
            }

            return normalized;
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
