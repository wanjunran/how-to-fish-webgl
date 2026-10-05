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
                Log($"{LogPrefix} 产物根目录: {outputDir}");

                // 关键：这几个文件是 WebGL 能不能跑起来的全部依据。
                // 直接点名比"打印文件数"有用得多—— 之前两轮我都是看到
                // 82 个文件 207MB 就以为成功了，实际路径错了两轮。
                // 缺任何一个都说明 Unity 的输出布局变了，要立刻看出来。
                //
                // 名字必须带 .gz。WebGL 默认开启压缩，产物是
                //   Build/WebGL.wasm.gz / WebGL.data.gz / WebGL.framework.js.gz
                // 而这一版最初写的是无压缩名，于是每次都打印 4 个 [缺]，
                // 看起来像布局坏了，其实是校验自己写错了——
                // 一守卫恒报错就等于没守卫，人会学会忽略它。
                foreach (string required in new[]
                         {
                             "index.html",
                             Path.Combine("Build", "WebGL.loader.js"),
                             Path.Combine("Build", "WebGL.wasm.gz"),
                             Path.Combine("Build", "WebGL.data.gz"),
                             Path.Combine("Build", "WebGL.framework.js.gz")
                         })
                {
                    string full = Path.Combine(outputDir, required);
                    Log($"{LogPrefix}   [{(File.Exists(full) ? "有" : "缺")}] {required}");
                }

                Log($"{LogPrefix} BUILD_OK");

                // 下面这三行是让整轮构建判为 success 的唯一原因。
                //
                // game-ci 的 validateBuild（src/model/unity/build-validation/
                // unity-build-validation.ts）只认两个信号：
                //   1. 日志里有字面量 "Build succeeded!"
                //   2. 否则正则匹配 "# Build results #" 段落里的 "Errors: N"
                //
                // 那个 "Build succeeded!" 由它自带的
                // dist/default-build-script/.../StdOutReporter.cs 打印，
                // 而那段代码只在**它自己的** Builder.BuildProject() 里调用。
                // 本工程用 buildMethod 指向了本类，那个脚本根本不会执行，
                // 于是两个信号都没有 -> validateBuild 无条件抛
                //   "There was an error building the project."
                //
                // 澄清一个我搞了两轮的错误认知：validateBuild **完全不看
                // 产物路径**。轮次 20 我认定它是在 build/ 下找不到产物，
                // 那是猜的 —— 读源码后发现它只做上面两个字符串判断。
                // 路径修正本身有价值（产物结构现在是对的），但它不是失败原因。
                //
                // 打印这行 + 退出码 0，validateBuild 才会返回通过，
                // 后续 Upload WebGL build 步骤（if-no-files-found: error）
                // 才会执行。
                Console.WriteLine("Build succeeded!");
                Console.Out.Flush();
                EditorApplication.Exit(0);
            }
            else
            {
                Log($"{LogPrefix} BUILD_FAILED");
                Console.WriteLine("Build failed!");
                Console.Out.Flush();
                EditorApplication.Exit(101);
            }
        }

        private static string ResolveOutputDir()
        {
            // unity-builder v6 通过 -customBuildPath 传入输出目录，不设 BUILD_PATH
            // 环境变量，所以两处都要看。
            //
            // 实测 v6 传的是 /github/workspace/build/WebGL/WebGL —— 外层WebGL
            // 是平台目录，内层是它自己加的 buildName（-customBuildName WebGL）。
            // 这里剥掉内层，让产物落在 build/WebGL/，与 Unity 的默认输出
            // 布局一致，也让下面 LocationPathName 的写法成立。
            //
            // 注意：剥这一层**不是**为了让 game-ci 认出产物。读CLI 源码
            // （unity-build-validation.ts）后确认 validateBuild 只检查日志里有没有
            // "Build succeeded!"，根本不看文件路径。轮次 20 我把它当成路径问题，
            // 方向就错了。路径本身该修的是另一个原因：那时
            // locationPathName 传的是 outputDir/index.html，Unity 把它当目录名，
            // 产物多嵌一层，Upload 步骤的 path: build 收集到的结构不对。
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
