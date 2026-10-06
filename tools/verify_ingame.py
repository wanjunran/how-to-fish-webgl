#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""点进单玩家模式，然后截图 —— 也就是去真正看一眼「碧海蓝天」。

## 为什么需要这个脚本

之前所有截图都停在启动 Logo 页，我当时把原因归到「进不了场景」。
查过之后发现**归因错了**：游戏没有卡住，也没有进不去。

`MainMenuManager.Start()` 跑的是`DelayedMainMenu()`协程：

    yield return null;
    ToggleMenu(enabled: true);

而 `ToggleMenu(true)` 做的事情里没有任何「切换场景」—— 它只是把
`_menuStuff` 打开、播菜单音乐、显示 UI。**游戏就停在主菜单等玩家
点击，这是设计如此，不是 bug。**

而 `tools/verify_load.py` 只做三件事：打开页面、等加载层消失、截图。
它**从不点击**。所以「截图里没有碧海蓝天」的真正原因是没人点那个
按钮，不是渲染坏了。

判据来源（全部来自场景文件，不是推测）：

- `Game.unity` 里 `SingleplayerButton`（GameObject `&1710`）存在且
  `m_IsActive: 1`
- 它的 Button 组件（`&5185`）`m_Interactable: 1`
- 它的 `m_OnClick` 绑定了 `ButtonManager.CreateLocalLobbyButton`
  （`m_TargetAssemblyTypeName: ButtonManager, Assembly-CSharp`，
  `m_MethodName: CreateLocalLobbyButton`，`m_CallState: 2` = 启用）
- `CreateLocalLobbyButton()` 走 `ConnectionManager.CreateOfflineLobby()`，
  **纯本地、不碰Steam**

也就是说这条点击路径在数据上是通的。能不能点得动，要看画面上那个
按钮实际在哪 —— Unity UI跑在 WebGL canvas 上，DOM 里没有对应元素，
所以只能按屏幕坐标点，而坐标得从场景里的 RectTransform 算。

## 为什么坐标是「算」出来的而不是拍脑袋填的

按钮 RectTransform 是 `&4617`。anchor/pivot 决定它在屏幕上的位置。
本脚本从场景 YAML 里读出真实数值，算出中心点，而不是写死一个
「大概是这里」。写死的坐标在分辨率一变就失效，而且失效时表现为
「点了没反应」，很容易被误判成「按钮不可用」。

**但仍然可能失败**：如果 anchor 算出来的位置和实际不符（比如有
CanvasScaler 缩放、父节点带偏移），点击就点不到。所以脚本会：

1. 先截「点击前」一张，对比画面有没有变化
2. 点了之后等一段时间，再截「点击后」
3. 两张都存下来，**让人看**，不靠脚本自己下结论

## 刻意不做的事

- 不判断「进没进成功」。判断成功需要读游戏内部状态，而 WebGL 构建
  没有暴露接口。截图给人看，比脚本猜一个阈值可靠。
- 不用 `document.querySelector` 找 canvas 里的元素 —— Unity UI 全部
  画在 canvas 上，DOM 里根本没有对应的 HTML 节点。这点很容易想当然。
- 不做多点几次/轮询点击。一次一点，点了就等。反复点击会让画面
  状态难以归因（到底是第几下点中的）。

用法
----
    python3 tools/verify_ingame.py --url http://127.0.0.1:8080
    python3 tools/verify_ingame.py --url ... --shots /tmp/shots
"""
from __future__ import annotations

import argparse
import glob
import os
import re
import sys
import time

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SCENE = os.path.join(ROOT, "Assets", "Scenes", "Game.unity")

# `SingleplayerButton`（GameObject &1710）的组件 id。写死是安全的：
# 场景文件是静态资产，改它就等于改了游戏 UI 布局，本来也该重新评估。
SINGLEPLAYER_GO = 1710
BUTTON_COMPONENT = 5185
RECT_COMPONENT = 4617

WIDTH, HEIGHT = 1280, 800


def _block(text: str, type_id: int, comp_id: int) -> str:
    """取场景里某个组件的 YAML 块正文。"""
    m = re.search(rf"^--- !u!{type_id} &{comp_id}\n(.*?)(?=\n--- !u!|\Z)",
                  text, re.S | re.M)
    return m.group(1) if m else ""


def read_button_layout() -> dict:
    """从场景 YAML 读出按钮的 anchor / pivot / 尺寸，算出屏幕中心点。

    判据全部取自 Unity 的 RectTransform 序列化格式：

        m_AnchorMin: {x: 0, y: 0}      左下角锚点（0..1 归一化）
        m_AnchorMax: {x: 0, y: 0}      右上角锚点
        m_AnchoredPosition: {x: .., y: ..}  相对锚点的偏移（像素）
        m_SizeDelta: {x: .., y: ..}   尺寸
        m_Pivot: {x: .., y: ..}       轴心（0..1，影响 anchoredPosition 含义）

    **必须沿m_Father 一路累加到根，不能只看按钮自己那层。**
    这是实测撞到的：SingleplayerButton 自己的 anchor 全是 (0,0)、
    anchoredPosition 也是 (0,0)，单看它算出来的中心是 (0, 0) ——
    屏幕左上角，会点中「Steam Relay Status」那行字而不是按钮。
    它的父级是 `&4174`，位置得从父级一路累加下来才有意义。

    返回的 (x, y) 是1920x1080 参考分辨率下的像素坐标。
    """
    if not os.path.isfile(SCENE):
        return {"ok": False, "why": f"场景文件不存在: {SCENE}"}
    with open(SCENE, encoding="utf-8", errors="replace") as f:
        text = f.read()

    go = _block(text, 1, SINGLEPLAYER_GO)
    if not go:
        return {"ok": False, "why": f"场景里找不到 GameObject &{SINGLEPLAYER_GO}"}
    if not re.search(r"m_IsActive:\s*1", go):
        return {"ok": False, "why": f"SingleplayerButton(&{SINGLEPLAYER_GO}) 不活跃"}

    btn = _block(text, 114, BUTTON_COMPONENT)
    if not btn:
        return {"ok": False, "why": f"找不到 Button 组件 &{BUTTON_COMPONENT}"}
    if re.search(r"m_Interactable:\s*0", btn):
        return {"ok": False, "why": "SingleplayerButton 的 m_Interactable=0（不可点）"}
    m = re.search(r"m_MethodName:\s*(\S+)", btn)
    if not m:
        return {"ok": False, "why": "Button 没有 m_OnClick 绑定"}
    if m.group(1) != "CreateLocalLobbyButton":
        return {"ok": False,
                "why": f"绑定的方法是 {m.group(1)}，不是 CreateLocalLobbyButton"}
    cs = re.search(r"m_CallState:\s*(\d+)", btn)
    if cs and cs.group(1) == "0":
        return {"ok": False, "why": "m_CallState=0，绑定被禁用"}

    # ---- 沿 m_Father 上溯，收集每一层 RectTransform ----
    chain: list[tuple[str, dict]] = []
    cur = RECT_COMPONENT
    seen: set[int] = set()
    while cur and cur not in seen:
        seen.add(cur)
        blk = _block(text, 224, cur) or _block(text, 4, cur)
        if not blk:
            break
        f = re.search(r"m_Father:\s*\{fileID:\s*(\d+)\}", blk)
        chain.append((str(cur), _rect_vals(blk)))
        cur = int(f.group(1)) if f else 0
        if len(chain) > 20:      # 防环
            break

    # ---- 累加 ----
    # Unity 的 RectTransform 变换顺序（从父到子）：
    #   子.中心 = 父中心 + 父轴心偏移到父锚点的差 + 子.anchoredPosition
    # 这里只做「锚点 + anchoredPosition」的逐层累加；pivot 只在需要
    # 精确对齐时影响 anchoredPosition 的参考点，对「找中心点」而言，
    # 逐层 center 累加即可（误差在按钮自身尺寸量级，不影响点中）。
    cx = cy = 0.0
    trace: list[str] = []
    for cid, v in reversed(chain):
        cx += v["anchor_min"][0] * 1920.0 / 2.0 + v["anchor_max"][0] * 1920.0 / 2.0 \
              + v["anchored_position"][0]
        cy += v["anchor_min"][1] * 1080.0 / 2.0 + v["anchor_max"][1] * 1080.0 / 2.0 \
              + v["anchored_position"][1]
        trace.append(f"      &{cid}: anchor={v['anchor_min']}..{v['anchor_max']} "
                     f"pos={v['anchored_position']} size={v['size_delta']} "
                     f"-> 累加到 ({cx:.1f}, {cy:.1f})")

    own = chain[0][1] if chain else {}
    return {
        "ok": True,
        "method": m.group(1),
        "depth": len(chain),
        "own_anchor_min": own.get("anchor_min"),
        "own_anchor_max": own.get("anchor_max"),
        "own_pivot": own.get("pivot"),
        "own_size_delta": own.get("size_delta"),
        "center_1920x1080": (cx, cy),
        "trace": trace,
    }


def _rect_vals(blk: str) -> dict:
    """从一个 RectTransform 块里抽出 anchor / pivot / 位置 / 尺寸。"""
    def vec(key: str) -> tuple[float, float]:
        mm = re.search(
            rf"{key}:\s*\{{x:\s*(-?[\d.eE+]+),\s*y:\s*(-?[\d.eE+]+)\}}", blk)
        return (float(mm.group(1)), float(mm.group(2))) if mm else (0.0, 0.0)
    return {
        "anchor_min": vec("m_AnchorMin"),
        "anchor_max": vec("m_AnchorMax"),
        "pivot": vec("m_Pivot"),
        "anchored_position": vec("m_AnchoredPosition"),
        "size_delta": vec("m_SizeDelta"),
    }


def find_chrome() -> str | None:
    for p in ("/usr/bin/chromium", "/usr/bin/chromium-browser",
              "/usr/bin/google-chrome", "/usr/bin/google-chrome-stable"):
        if os.path.exists(p):
            return p
    hits = glob.glob("/root/.cache/ms-playwright/chromium-*/chrome-linux/chrome")
    return hits[0] if hits else None


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--url", required=True)
    ap.add_argument("--shots", default="/tmp")
    ap.add_argument("--settle", type=int, default=20,
                    help="点击后等待秒数")
    ap.add_argument("--layout-only", action="store_true",
                    help="只算按钮坐标，不开浏览器")
    a = ap.parse_args()

    layout = read_button_layout()
    if not layout["ok"]:
        print(f"::error::按钮位置不可用 —— {layout['why']}")
        print("这一节为空**不代表**按钮点不了，只代表拿不到坐标。")
        return 1
    cx, cy = layout["center_1920x1080"]
    print("== SingleplayerButton 布局（从 Game.unity 读出，非硬编码）")
    print(f"   绑定方法   : {layout['method']}")
    print(f"   自身 anchor: {layout['own_anchor_min']} .. {layout['own_anchor_max']}"
          f"  pivot={layout['own_pivot']}  size={layout['own_size_delta']}")
    print(f"   祖先链深度: {layout['depth']}")
    print("   逐层累加:")
    for ln in layout.get("trace", []):
        print(ln)
    print(f"   中心(1920x1080): ({cx:.1f}, {cy:.1f})")
    if a.layout_only:
        return 0

    # 坐标落在参考分辨率之外 -> 布局读错了或假设不成立。
    # 不静默继续点：点一个明显错的位置会产出误导性的截图。
    if not (0 <= cx <= 1920 and 0 <= cy <= 1080):
        print(f"::error::算出的中心点 ({cx:.1f}, {cy:.1f}) 落在 1920x1080 之外，"
              f"布局假设（anchor 归一化 / 参考分辨率）不成立。")
        print("**不执行点击** —— 在明显错的位置上点，产出的截图会误导判断。")
        return 1

    chrome = find_chrome()
    if not chrome:
        print("::error::找不到 Chrome/Chromium，跳过（不代表构建有问题）")
        return 0

    try:
        from playwright.sync_api import sync_playwright
    except ImportError as e:
        print(f"::error::缺 playwright: {e}")
        return 0

    os.makedirs(a.shots, exist_ok=True)
    logs: list[str] = []

    def say(s: str) -> None:
        print(s)
        logs.append(s)

    with sync_playwright() as pw:
        br = pw.chromium.launch(
            executable_path=chrome,
            args=["--use-gl=swiftshader", "--enable-unsafe-swiftshader",
                  "--ignore-gpu-blocklist", "--enable-webgl",
                  "--no-sandbox", "--disable-dev-shm-usage"])
        ctx = br.new_context(viewport={"width": WIDTH, "height": HEIGHT})
        pg = ctx.new_page()
        errs: list[str] = []
        pg.on("pageerror", lambda e: errs.append(str(e)[:200]))
        pg.on("console", lambda m: errs.append(m.text[:200])
              if m.type == "error" else None)

        say(f"打开 {a.url}")
        pg.goto(a.url, wait_until="domcontentloaded", timeout=90_000)

        # 等加载层消失
        t0 = time.time()
        ready = False
        while time.time() - t0 < 300:
            time.sleep(5)
            try:
                st = pg.evaluate("() => { const l=document.querySelector("
                                 "'#unity-loading-bar'); const c=document.querySelector("
                                 "'#unity-canvas'); return {ld: l?l.style.display:'missing',"
                                 " cv: c?c.width+'x'+c.height:'none'}; }")
            except Exception as e:
                say(f"  evaluate 失败: {str(e)[:150]}")
                break
            say(f"  [{time.time()-t0:5.0f}s] 加载层={st['ld']} 画布={st['cv']}")
            if st["ld"] == "none":
                ready = True
                break
        if not ready:
            say("加载层一直没消失，不点击（点下去也没意义）")
            pg.screenshot(path=os.path.join(a.shots, "ingame_before.png"))
            br.close()
            return 1

        say("加载完成，再等 12s 让主菜单稳定")
        time.sleep(12)
        # 拿到 canvas 真实像素尺寸，按它缩放参考坐标
        cv = pg.evaluate("() => { const c=document.querySelector('#unity-canvas');"
                         " return c?{w:c.width,h:c.height}:null; }")
        if not cv:
            say("拿不到 canvas 尺寸")
            pg.screenshot(path=os.path.join(a.shots, "ingame_before.png"))
            br.close()
            return 1
        say(f"canvas 实际 {cv['w']}x{cv['h']}（参考 1920x1080，缩放 {cv['w']/1920:.3f}）")

        before = os.path.join(a.shots, "ingame_before.png")
        pg.screenshot(path=before)
        say(f"点击前截图: {before}")

        # 缩放到 canvas 像素坐标。Playwright 的 mouse 用的是 CSS 像素，
        # canvas 的 CSS 尺寸与它的 width/height 属性可能不同，所以分两步。
        box = pg.evaluate("() => { const c=document.querySelector('#unity-canvas');"
                          " const r=c.getBoundingClientRect();"
                          " return {x:r.x,y:r.y,w:r.width,h:r.height}; }")
        sx = box["w"] / 1920.0
        sy = box["h"] / 1080.0
        tx = box["x"] + cx * sx
        ty = box["y"] + cy * sy
        say(f"点击屏幕坐标: ({tx:.0f}, {ty:.0f})"
            f"（canvas 盒子 x={box['x']:.0f} y={box['y']:.0f} "
            f"{box['w']:.0f}x{box['h']:.0f}）")

        pg.mouse.move(tx, ty)
        time.sleep(0.4)
        pg.mouse.down()
        time.sleep(0.12)
        pg.mouse.up()
        say("已点击（一次，不重复点）")

        say(f"等 {a.settle}s 让场景切换/加载")
        time.sleep(a.settle)

        after = os.path.join(a.shots, "ingame_after.png")
        pg.screenshot(path=after)
        say(f"点击后截图: {after}")

        # 只报告，不下结论 —— 成功与否要人看图判断。
        # 理由：判断「进没进游戏场景」需要读游戏内部状态，
        # 而 WebGL构建没暴露这种接口。凭像素差猜阈值不可靠，
        # 猜错了会让人以为「没进去」而重跑一轮 CI。
        try:
            from PIL import Image, ImageChops
            a1 = Image.open(before).convert("RGB")
            a2 = Image.open(after).convert("RGB")
            if a1.size != a2.size:
                say(f"两张图尺寸不同（{a1.size} vs {a2.size}），不算差异")
            else:
                diff = ImageChops.difference(a1, a2)
                bbox = diff.getbbox()
                hist = diff.convert("L").histogram()
                changed = sum(hist[16:])  # 差异 > 16 的像素数
                total = a1.size[0] * a1.size[1]
                say(f"前后差异像素: {changed}/{total} = "
                    f"{changed*100.0/total:.2f}%  差异区域 bbox={bbox}")
                if changed == 0:
                    say("**两帧完全相同** —— 点击很可能没生效。"
                        "可能是坐标算错、按钮被别的层挡住，"
                        "或游戏确实没响应。**这一句不等于「进不去」**，"
                        "看图确认。")
        except ImportError:
            say("缺 Pillow，跳过差异统计")

        if errs:
            say(f"页面错误 {len(errs)} 条（前 5）:")
            for e in errs[:5]:
                say(f"  ! {e}")

        br.close()

    try:
        with open(os.path.join(a.shots, "ingame-verify.log"), "w",
                  encoding="utf-8") as f:
            f.write("\n".join(logs) + "\n")
    except OSError:
        pass
    print("\n两张截图已存。请**人眼看** before/after 的差别 —— "
          "脚本不下「是否进成功」的结论。")
    return 0


if __name__ == "__main__":
    sys.exit(main())
