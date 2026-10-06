#!/usr/bin/env python3
"""在无GPU 环境里真实加载 Unity WebGL 产物，判定它能不能跑起来。

为什么需要这个脚本
------------------
构建成功不等于能跑。轮次 23 出现过：构建全绿、产物 207MB、离线完整性
校验 12/12 全过，一加载场景就抛

    RuntimeError: function signature mismatch

而离线校验只能证明文件没损坏，证明不了它能运行。

判据为什么是"加载层消失"，而不是"画布拿到 WebGL 上下文"
---------------------------------------------------------
这是 v1 的致命错误，值得写下来别再犯。

看index.html 模板（Unity 自带，生成物里原样保留）：

    createUnityInstance(canvas, config, (progress) => {...})
      .then((unityInstance) => {
        document.querySelector("#unity-loading-bar").style.display = "none";
        ...
      })
      .catch((message) => { alert(message); });

- `#unity-loading-bar` 被置为 none  <- 只在 .then() 里发生，也就是
  **主场景真的加载完了**。这是唯一能证明"游戏进去了"的 DOM 信号。
- 画布拿到 WebGL 上下文<- 发生在 wasm 实例化时，即**加载场景之前**。

而function signature mismatch 恰恰是在**加载场景时**抛的，
所以"拿到 ctx"这个判据在一个坏产物上照样会通过。
v1 就是这么"验证通过"的，然后我据此对外说了修复生效 —— 假绿。

写文件而不是塞进 workflow 的 heredoc
------------------------------------
踩过两个坑，都只能靠实测发现：

1. `xvfb-run ... python3 - <<'PY'` —— xvfb-run 会把 stdin 从 heredoc
   拿走（去喂 xauth）。实测 `sys.stdin.read()` 在这种写法下返回空字符串。
   代码本身能跑，但任何依赖 stdin 的写法都会静默拿到空值。

2. stdout 重定向到文件时是块缓冲。脚本中途抛异常 -> 非 0 退出 ->
   缓冲区里的 print 全部丢失。表现是日志里只有第一行有内容、后面全没了，
   而退出码却是 0。

写成文件后：python3 verify_load.py 直接从文件读代码，print 用 flush=True，
异常路径也能留下痕迹。workflow 里只剩一行调用。

用法：
    python3 verify_load.py "$GITHUB_STEP_SUMMARY" [url]

退出码：
    0  主场景加载完成（加载层消失），无致命错误
    1  出现 function signature mismatch
    2  页面弹出别的错误，或出现未捕获的 JS 异常
    3  脚本自身出错（浏览器起不来等）
    4  跑满LOAD_TIMEOUT 仍未完成（软件渲染可能只是慢）
"""
import glob
import os
import sys
import time
import traceback

# 软件实现 WebGL2 的三个参数，缺一不可：
#   --use-angle=swiftshader     没有 GPU 时用软件渲染
#   --enable-unsafe-swiftshader  新版 Chrome 默认禁用软件 WebGL
#   --ignore-gpu-blocklist      否则软件渲染会被 GPU 黑名单挡掉
# 两个 --no-sandbox 是容器内起进程的前提。
CHROME_ARGS = [
    "--use-gl=angle",
    "--use-angle=swiftshader",
    "--enable-unsafe-swiftshader",
    "--ignore-gpu-blocklist",
    "--disable-dev-shm-usage",
    "--no-sandbox",
    "--disable-setuid-sandbox",
]

URL = os.environ.get("VERIFY_URL", "http://127.0.0.1:8123/")
# 软件渲染下加载 200MB 资产很慢，给足时间。
LOAD_TIMEOUT = int(os.environ.get("VERIFY_TIMEOUT", "600"))
# 致命错误的特征串。分开匹配，因为 alert 和 console 里的措辞略有差别。
# 顺序有意义：越具体的放前面，避免"RuntimeError"这种宽泛的
# 把真正的首因（signature mismatch）盖掉。
FATAL_MARKERS = (
    "function signature mismatch",
    "Aborting(both async and sync fetching of the wasm failed)",
    "Unable to parse Build",
    "Failed to decompress",
    "memory access out of bounds",
    "null function",
    "RuntimeError",
    "Unable to load file",
)

# 签名不匹配的宽松匹配。
# 为什么不用精确串：run 37269147824 报的是退出码 2（"页面报别的错"）
# 而不是 1，而这个构建明明带着签名修复。Emscripten 抛出的文本在不同
# 版本/不同调用深度下措辞会变（"function signature mismatch" vs
# "signature mismatch ... at wasm-function[...]"），
# 精确匹配一旦措辞微调就退化成"未知的别的错"，把已知问题变成谜题。
SIG_LOOSE = ("signature mismatch", "signaturemismatch")


def is_sig_error(text: str) -> bool:
    low = text.lower().replace(" ", "").replace("_", "")
    return any(s.replace(" ", "") in low for s in SIG_LOOSE)


def find_chrome() -> str | None:
    """定位 Chromium 可执行文件。

    优先用runner 镜像自带的 Chrome/Chromium，不要用 playwright 下载的那份。
    理由是实测数据：沙箱里 venv+playwright 只要 7 秒，
    而 "playwright install chromium" 下170MB 跑了 8 分钟还没完。
    GitHub 上run 37267536242 的验证步骤总共只有 36 秒——
    装浏览器的时间都不够，那个失败几乎肯定就卡在这里。

    GitHub 的 ubuntu runner 镜像自带 Chrome/Chromium（版本随镜像滚动更新），
    直接用它省掉整个下载环节，也少一个失败点。

    找不到再退回 playwright 自带的路径。
    """
    system_first = [
        "/usr/bin/google-chrome",
        "/usr/bin/google-chrome-stable",
        "/usr/bin/chromium",
        "/usr/bin/chromium-browser",
        "/opt/google/chrome/chrome",
    ]
    for p in system_first:
        if os.path.isfile(p) and os.access(p, os.X_OK):
            return p

    pats = [
        "/tmp/pw-browsers/chromium-*/chrome-linux64/chrome",
        os.path.expanduser("~/.cache/ms-playwright/chromium-*/chrome-linux64/chrome"),
    ]
    for pat in pats:
        hits = sorted(glob.glob(pat))
        if hits:
            return hits[0]
    return None


def fatal_in(text: str) -> str | None:
    """返回命中的致命错误特征串，没有则 None。"""
    for m in FATAL_MARKERS:
        if m in text:
            return m
    return None


def main() -> int:
    summary = open(sys.argv[1], "a", encoding="utf-8") if len(sys.argv) > 1 else sys.stdout
    url = sys.argv[2] if len(sys.argv) > 2 else URL

    logs: list[str] = []
    alerts: list[str] = []
    errs: list[str] = []
    t0 = time.time()
    done = False
    ok = False

    def say(msg: str) -> None:
        print(msg, flush=True)

    def finish_ok(elapsed: float) -> int:
        summary.write(f"OK **通过**：{elapsed:.0f}s 内主场景加载完成"
                      f"（加载层消失）\n")
        return 0

    try:
        from playwright.sync_api import sync_playwright

        exe = find_chrome()
        say(f"chrome: {exe or '未找到，交给 playwright 默认路径'}")
        say(f"目标:   {url}")

        with sync_playwright() as p:
            b = p.chromium.launch(
                headless=False,
                executable_path=exe,
                args=CHROME_ARGS,
                # 超时给足，否则软件渲染下引擎还没起好就断开
                timeout=120_000,
            )
            say("浏览器已启动")
            pg = b.new_page(viewport={"width": 1280, "height": 800})

            pg.on("console", lambda m: logs.append(f"[{m.type}] {m.text[:800]}"))
            pg.on("pageerror", lambda e: errs.append(str(e)[:300]))

            def on_dialog(d):
                # Unity 模板的 .catch() 用 alert 报错，这是拿到
                # function signature mismatch 的唯一途径。
                #
                # 全文留着，别截太短：wasm 栈帧（wasm-function[NNN]:0x...）
                # 出现在消息靠后的位置，而那几行往往正是判断
                # "空槽位"还是"签名不符"的唯一依据。
                # run37270190222 报的 "RuntimeError: null function"
                # 就属于这类需要看完整栈才能进一步定位的错误。
                alerts.append(d.message)
                say(f"!! alert（{len(d.message)} 字符）: {d.message[:300]}")
                try:
                    d.accept()
                except Exception:
                    pass

            pg.on("dialog", on_dialog)

            pg.goto(url, wait_until="domcontentloaded", timeout=90_000)
            say("页面已打开，开始轮询")

            # 状态机：
            #   loaded  -> 加载层 none，主场景真的进去了（唯一硬通过信号）
            #   alert   -> Unity 抛错
            #   页error -> 未捕获的 JS 异常（wasm 崩了会走这里）
            #   超时    -> 软件渲染可能只是慢，不判失败
            while time.time() - t0 < LOAD_TIMEOUT:
                time.sleep(5)
                if alerts or errs:
                    break
                try:
                    st = pg.evaluate("""() => {
                      const l = document.querySelector('#unity-loading-bar');
                      const f = document.querySelector('#unity-progress-bar-full');
                      const c = document.querySelector('#unity-canvas');
                      const w = document.querySelector('#unity-warning');
                      return {
                        loading: l ? l.style.display : 'missing',
                        pct: f ? f.style.width : '-',
                        canvas: c ? c.width + 'x' + c.height : 'none',
                        warn: w && w.children.length ? w.textContent.slice(0, 200) : ''
                      };
                    }""")
                except Exception as e:
                    # 页面可能已经跳走或崩了，记下来但不当成功
                    errs.append(f"evaluate 失败: {str(e)[:200]}")
                    say(f"!! evaluate 失败: {str(e)[:200]}")
                    break

                say(f"  [{time.time() - t0:5.0f}s] 加载层={st['loading']:>7} "
                    f"进度={st['pct'] or '-':>7} 画布={st['canvas']}")
                if st["warn"]:
                    say(f"     warning 横幅: {st['warn']}")

                if st["loading"] == "none":
                    ok = True
                    done = True
                    break

            # 加载层消失后再等一会抓第二帧：第一帧往往还停在 Unity logo，
            # 那时"能玩"还只是"引擎活着"。多等一会儿看画面有没有变化。
            if ok:
                say("加载层已消失，再等 25s 确认画面稳定")
                time.sleep(25)
                try:
                    st2 = pg.evaluate(
                        "() => document.querySelector('#unity-loading-bar')"
                        ".style.display")
                    if st2 != "none":
                        errs.append("加载层在二次检查时又出现了")
                        say(f"!! 加载层重新出现: {st2}")
                except Exception as e:
                    errs.append(f"二次检查 evaluate 失败: {str(e)[:200]}")

            # 截图在失败时也要留 —— 卡在 90% 的画面本身就是证据。
            for name, tag in (("verify_loaded.png", "判定完成时"),
                              ("verify_settled.png", "二次确认后")):
                try:
                    pg.screenshot(path=f"/tmp/{name}")
                    say(f"截图({tag}): /tmp/{name}")
                except Exception as e:
                    say(f"截图({tag})失败，不影响结论: {str(e)[:120]}")

            # ---- 洋红（magenta）面积统计 ----
            #
            # 为什么要量化而不是靠肉眼看图：Unity 编译 shader 失败时**不会**
            # 让构建失败、不会在 console 报错，它只是把那个材质渲成洋红。
            # 所以「画面里有多少洋红」是唯一能在 CI 里自动发现 shader 编译
            # 失败的信号 —— 之前只截图不判定，等于把这唯一的信号扔了。
            #
            # 判据：R 和 B 都高（>200）、G 明显低（<80），且 RGB 不相等
            #（排除纯白/纯灰）。Unity 的错误洋红是 (255,0,255) 一族。
            try:
                from PIL import Image
                lines = []
                for name in ("verify_loaded.png", "verify_settled.png"):
                    src = f"/tmp/{name}"
                    if not os.path.exists(src):
                        continue
                    im = Image.open(src).convert("RGB")
                    px = list(im.getdata())
                    n = len(px)
                    hit = sum(1 for r, g, b in px
                              if r > 200 and b > 200 and g < 80)
                    pct = 100.0 * hit / n
                    rows = sorted({
                        f"r={r} g={g} b={b}"
                        for r, g, b in px if r > 200 and b > 200 and g < 80
                    })[:6]
                    say(f"洋红统计({name}): {hit}/{n} 像素 = {pct:.3f}%"
                        f" 取值样本: {rows}")
                    summary.write(
                        f"- `{name}` 洋红像素：**{hit}** / {n} = **{pct:.3f}%**"
                        f"{'（样本 ' + ', '.join(rows) + '）' if rows else ''}\n")
                    lines.append(
                        f"{name}: {hit}/{n} = {pct:.3f}%"
                        f"{'  样本 ' + ', '.join(rows) if rows else ''}")
                    if pct >= 0.5:
                        errs.append(
                            f"{name} 有 {pct:.3f}% 洋红像素 —— 至少一个 shader "
                            f"编译失败（Unity 不让构建失败，只渲洋红）")
                # 单独落盘：报告正文里那行混在 markdown 表格里不好 grep，
                # 而这个数字就是「画面对不对」的唯一量化判据。
                if lines:
                    os.makedirs("verify-evidence", exist_ok=True)
                    with open("verify-evidence/magenta.txt", "w",
                              encoding="utf-8") as f:
                        f.write("\n".join(lines) + "\n")
            except ImportError:
                say("洋红统计跳过：环境无 PIL")
            except Exception as e:
                say(f"洋红统计失败（不影响主结论）: {str(e)[:150]}")

            # 把截图挪到工作区，Actions 里可以直接点开看
            try:
                import shutil
                for name in ("verify_loaded.png", "verify_settled.png"):
                    src = f"/tmp/{name}"
                    if os.path.exists(src):
                        shutil.copy(src, name)
            except Exception:
                pass

            b.close()

    except Exception:
        # 这一段必须存在：没有它，上面的缓冲丢日志问题会让脚本
        # 静默退出 0，看起来像"验证通过"。
        say("验证脚本自身出错：")
        say(traceback.format_exc())
        summary.write("ERROR **验证脚本自身出错，无法判定产物**\n\n```\n")
        summary.write(traceback.format_exc()[-2000:])
        summary.write("\n```\n")
        summary.close()
        return 3

    el = time.time() - t0
    blob = "\n".join(alerts + errs + logs)
    fatal = fatal_in(blob)
    other = fatal_in("\n".join(alerts + errs))
    # 用宽松匹配兜住措辞差异，判"是不是签名问题"不要只看精确串。
    sig = is_sig_error(blob)

    say("")
    say("=" * 66)
    say(f"结论: ok={ok} sig={sig} fatal={fatal!r} other={other!r} 耗时={el:.0f}s")
    for a in alerts:
        say(f"  alert: {a[:300]}")

    # 把完整的首个错误单独落盘。workflow 的注解只放得下一行摘要，
    # 但排查 "null function" 这类错误需要看完整 wasm 栈——
    # 栈帧在消息靠后位置，一截断就只剩结论没有证据。
    try:
        with open("/tmp/verify_first_error.txt", "w", encoding="utf-8") as fh:
            if alerts:
                fh.write("=== alert (Unity .catch 收到的原始消息) ===\n")
                fh.write(alerts[0][:4000] + "\n")
            if errs:
                fh.write("\n=== pageerror (未捕获的 JS 异常) ===\n")
                fh.write("\n".join(errs)[:4000] + "\n")
            if not alerts and not errs:
                fh.write("(无 alert、无 pageerror)\n")
        say(f"首因已写入 /tmp/verify_first_error.txt")
    except Exception as e:
        say(f"写首因文件失败（不影响结论）: {e}")

    # console 全文单独落盘。Unity 的 C# Debug.Log 全走 console（log/warning/
    # error），崩溃前的最后几条日志往往直接写明引擎正在初始化什么——
    # 比如序列化器注册失败、场景对象 Awake 里的异常。这次 "null function"
    # 崩溃的第一现场八成就在这里，alert 里只有 wasm 索引没有语义。
    try:
        with open("/tmp/verify_console.txt", "w", encoding="utf-8") as fh:
            fh.write(f"共 {len(logs)} 条 console 消息\n\n")
            for line in logs:
                fh.write(line + "\n")
        say(f"console 全文已写入 /tmp/verify_console.txt（{len(logs)} 条）")
    except Exception as e:
        say(f"写 console 文件失败（不影响结论）: {e}")

    for e in errs:
        say(f"  pageerror: {e[:300]}")
    say("=" * 66)

    summary.write("\n")
    if sig:
        summary.write("FAIL **function signature mismatch 仍存在**\n\n```\n")
        for a in alerts[:2]:
            summary.write(f"alert: {a}\n")
        summary.write("```\n")
        rc = 1
    elif ok and not other:
        summary.write(f"OK **通过**：{el:.0f}s 内主场景加载完成\n")
        rc = 0
    elif ok:
        # 加载层消失了（主场景真的进去了），但同时有报错。
        # 这不算失败—— Unity 在运行期报个非致命错是常事，
        # 而"加载层消失"这个硬信号已经拿到了。
        # 之前这个分支写在 elif other 后面，永远进不去：
        # ok=True 且 other 非空时会被上一条 elif 拦走。
        summary.write(f"WARN 主场景已加载完成，但同期有报错："
                      f"{other}\n\n```\n")
        for a in alerts[:2]:
            summary.write(f"alert: {a[:400]}\n")
        for e in errs[:3]:
            summary.write(f"pageerror: {e[:300]}\n")
        summary.write("```\n")
        rc = 0
    elif other:
        summary.write(f"FAIL 页面报错：{other}\n\n```\n")
        for a in alerts[:2]:
            summary.write(f"alert: {a[:400]}\n")
        for e in errs[:3]:
            summary.write(f"pageerror: {e[:300]}\n")
        summary.write("```\n")
        rc = 2
    else:
        summary.write(f"WARN {LOAD_TIMEOUT}s 内未完成加载"
                      f"（软件渲染慢，不判定为失败）\n")
        rc = 4

    summary.write(f"\n耗时 {el:.0f}s\n\n```\n")
    for line in logs[-40:]:
        summary.write(line[:300] + "\n")
    summary.write("```\n")
    summary.close()
    return rc


if __name__ == "__main__":
    sys.exit(main())
