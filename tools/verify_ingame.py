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

# ---------------------------------------------------------------------------
# 目标按钮：**从场景自动解析**，不写死 fileID
# ---------------------------------------------------------------------------
# 这里换过两次目标，原因是前两次都点错了按钮，而点击差异都是 0.00%，
# 报告只说「点击很可能没生效」，从没说「可能点的是另一个按钮」。
#
# ButtonManager 里有两套名字很像的按钮：
#
#   CreateServerButton   -> _createServerButton   激活=开
#       WebGL 单人版真正的入口。场景里就开着。
#   SingleplayerButton   -> SavedServer 列表里的那一项，绑定
#       CreateLocalLobbyButton。激活=开，但那个面板要**先选中一个存档**
#       才会出现 —— 新用户没有存档，于是点了等于点在空气上。
#
# 所以判据改成「找出 ButtonManager 里那个激活的、绑定 CreateServer 的按钮」，
# 由场景数据说话。写死 fileID 的问题不只是「值可能变」：一旦布局调整，
# 写死的值会安静地点到别的东西上，而报告显示「已点击」。
BUTTON_MANAGER_GUID = "488b2771a904a6b76cd4192c654bb696"
# 期望的入口绑定。实测：CreateServerButton 自己的 m_OnClick 是 SetActive
# （只负责打开「创建存档」面板），真正建档的是面板里那个按钮 ->
# ButtonManager.CreateNewServer -> SaveManager.CreateServer。
# 所以「能点开入口」就够了，进游戏要两步，点第一下是正确目标。
EXPECTED_METHOD = None      # 入口按钮不直接建档，不按方法名筛
# 兜底候选（按优先级），字段名取自 Game.unity 的 ButtonManager 组件。
FALLBACK_FIELDS = ("_createServerButton",)

WIDTH, HEIGHT = 1280, 800


def _block(text: str, type_id: int, comp_id: int) -> str:
    """取场景里某个组件的 YAML 块正文。"""
    m = re.search(rf"^--- !u!{type_id} &{comp_id}\n(.*?)(?=\n--- !u!|\Z)",
                  text, re.S | re.M)
    return m.group(1) if m else ""


def host_of(text: str, comp_fid: int) -> int:
    """组件 fileID -> 宿主 GameObject 的 fileID。

    `_block_any` 可能先命中同一宿主上的另一个 MonoBehaviour —— 那样
    再取一次 m_GameObject 就不是 GameObject 了。所以这里必须按
    classID 1 回溯到真正的 GameObject。
    """
    for m in re.finditer(r"^--- !u!(\d+) &(\d+)\s*\n(.*?)(?=\n--- !u!|\Z)",
                         text, re.S | re.M):
        if int(m.group(2)) != comp_fid:
            continue
        if int(m.group(1)) == 1:          # GameObject 本体
            return comp_fid
        g = re.search(r"^\s*m_GameObject:\s*\{fileID:\s*(\d+)\}", m.group(3), re.M)
        if g:
            return int(g.group(1))
    return comp_fid


def _block_any(text: str, fid: int) -> str:
    """按 fileID 取块，不关心 classID（224/4 两种 RectTransform 都可能）。"""
    m = re.search(rf"^--- !u!\d+ &{fid}\n(.*?)(?=\n--- !u!|\Z)", text, re.S | re.M)
    return m.group(1) if m else ""


def _rect_of_go(text: str, comp_fid: int) -> int | None:
    """找出该按钮所在 GameObject 的 RectTransform fileID。

    场景里 RectTransform 可能是 class 224（带 rootOrder 的那种）
    也可能是 class 4。两种都要认，只查一种会在另一种布局上返回空。
    """
    go = host_of(text, comp_fid)
    # 先在按钮自身的 RectTransform 里找：class 224 的块含 m_Father
    for fid_candidate in _all_rect_ids(text):
        b = _block_any(text, fid_candidate)
        m = re.search(r"^\s*m_GameObject:\s*\{fileID:\s*(\d+)\}", b, re.M)
        if m and int(m.group(1)) == go:
            return fid_candidate
    return None


def _all_rect_ids(text: str) -> list[int]:
    out = []
    for m in re.finditer(r"^--- !u!(224|4) &(\d+)\s*\n", text, re.M):
        out.append(int(m.group(2)))
    return out


def resolve_target_button(text: str) -> dict:
    """从 Game.unity 找出「该点哪个按钮」，而不是写死 fileID。

    判据链（每一步都能单独解释为什么）：
      1. 按 GUID 找到 ButtonManager 组件块
      2. 读出它所有按钮类字段（名字形如 _xxxButton）
      3. 逐个回溯到 GameObject，取真实名字与激活状态
      4. 优先选**激活且绑定 CreateServer** 的那个
      5. 找不到就报出来，并列出所有候选 —— 让人能一眼看出该点哪个

    第 5 步是关键：判据失效时不能只说「失败」，得把候选摊开。
    前两版就把失败原因写成「点击很可能没生效」，指向浏览器/坐标，
    而真实原因是「点的是另一个按钮」，方向完全错。
    """
    i = text.find(BUTTON_MANAGER_GUID)
    if i < 0:
        return {"ok": False, "why": f"场景里找不到 ButtonManager（guid {BUTTON_MANAGER_GUID}）"}
    head = text.rfind("--- !u!", 0, i)
    end = text.find("\n--- !u!", i)
    bm = text[head:end]

    # 场景里每个文档块：--- !u!<classID> &<fileID>
    blocks: dict[int, str] = {}
    blocks_cls: dict[int, int] = {}
    marks = [(m.start(), int(m.group(1)), int(m.group(2)))
             for m in re.finditer(r"^--- !u!(\d+) &(\d+)\s*\n", text, re.M)]
    for idx, (start, _c, fid) in enumerate(marks):
        stop = marks[idx + 1][0] if idx + 1 < len(marks) else len(text)
        blocks[fid] = text[start:stop]
        blocks_cls[fid] = _c

    def host(fid: int) -> int:
        """组件 fileID -> 它挂的 GameObject fileID。

        必须按 classID 判定：`blocks` 里同一个 GameObject 挂着多个
        MonoBehaviour，若按「带 m_GameObject 的块」回溯，先命中的可能是
        另一个 MonoBehaviour，于是 host(6031) 返回 6031 自己而不是
        1559（GameObject）。后面按宿主找 RectTransform 就全错位了。
        """
        cls = blocks_cls.get(fid)
        if cls == 1:
            return fid
        b = blocks.get(fid, "")
        m = re.search(r"^\s*m_GameObject:\s*\{fileID:\s*(\d+)\}", b, re.M)
        return int(m.group(1)) if m else fid

    def go_name(fid: int) -> str:
        b = blocks.get(host(fid), "")
        m = re.search(r"^\s*m_Name:\s*(.*)$", b, re.M)
        return (m.group(1).strip() if m else "") or f"<&{fid}>"

    def go_active(fid: int) -> bool | None:
        b = blocks.get(host(fid), "")
        m = re.search(r"^\s*m_IsActive:\s*(\d)", b, re.M)
        return bool(int(m.group(1))) if m else None

    def bound_method(fid: int) -> tuple[str | None, int | None]:
        """按钮组件上的 m_OnClick 绑定方法与调用状态。"""
        b = blocks.get(host(fid), "")
        # 组件自身若无 Button 子组件，往下找同宿主的 Button
        cand = [fid] + [k for k, v in blocks.items()
                        if host(k) == host(fid)
                        and re.search(r"^\s*m_OnClick:", v, re.M)]
        for c in cand:
            bb = blocks.get(c, "")
            if not re.search(r"^\s*m_OnClick:", bb, re.M):
                continue
            m = re.search(r"m_MethodName:\s*(\S+)", bb)
            cs = re.search(r"m_CallState:\s*(\d+)", bb)
            inter = re.search(r"m_Interactable:\s*(\d)", bb)
            if m and cs and int(cs.group(1)) != 0 and \
                    (not inter or inter.group(1) != "0"):
                return m.group(1), int(cs.group(1))
        return None, None

    cands: list[dict] = []
    for m in re.finditer(r"^  (_[A-Za-z0-9_]*Button):\s*\{fileID:\s*(\d+)\}",
                         bm, re.M):
        field, fid = m.group(1), int(m.group(2))
        meth, _cs = bound_method(fid)
        cands.append({
            "field": field, "fid": fid, "go": go_name(fid),
            "active": go_active(fid), "method": meth,
        })

    if not cands:
        return {"ok": False,
                "why": "ButtonManager 里没有任何 *Button 字段 —— 场景结构变了"}

    if EXPECTED_METHOD:
        for c in cands:
            if c["method"] == EXPECTED_METHOD and c["active"]:
                return {"ok": True, **c, "candidates": cands,
                        "why": f"选中 {c['go']}（{c['field']}，"
                               f"绑定 {c['method']}，激活）"}

    for c in cands:
        if c["field"] in FALLBACK_FIELDS and c["active"]:
            return {"ok": True, **c, "candidates": cands,
                    "why": f"选中 {c['go']}（{c['field']}，激活，"
                           f"绑定 {c['method']}）"}

    return {"ok": False, "candidates": cands,
            "why": "场景里没有**激活**的 "
                   + (f"绑定 {EXPECTED_METHOD} 的" if EXPECTED_METHOD else "")
                   + f"入口按钮（找过字段 {FALLBACK_FIELDS}）—— "
                   f"WebGL 单人版的入口不见了，UI 结构可能变了"}


def read_button_layout() -> dict:
    """从场景 YAML 读出按钮的 anchor / pivot / 尺寸，算出屏幕中心点。

    判据全部取自 Unity 的 RectTransform 序列化格式：

        m_AnchorMin: {x: 0, y: 0}      左下角锚点（0..1 归一化）
        m_AnchorMax: {x: 0, y: 0}      右上角锚点
        m_AnchoredPosition: {x: .., y: ..}  相对锚点的偏移（像素）
        m_SizeDelta: {x: .., y: ..}   尺寸
        m_Pivot: {x: .., y: ..}       轴心（0..1，影响 anchoredPosition 含义）

    **必须沿m_Father 一路累加到根，不能只看按钮自己那层。**
    这是实测撞到的：按钮自己的 anchor 全是 (0,0)、
    anchoredPosition 也是 (0,0)，单看它算出来的中心是 (0, 0) ——
    屏幕左上角，会点中「Steam Relay Status」那行字而不是按钮。
    它的父级位置得从父级一路累加下来才有意义。

    返回的 (x, y) 是1920x1080 参考分辨率下的像素坐标。
    """
    if not os.path.isfile(SCENE):
        return {"ok": False, "why": f"场景文件不存在: {SCENE}"}
    with open(SCENE, encoding="utf-8", errors="replace") as f:
        text = f.read()

    tgt = resolve_target_button(text)
    if not tgt.get("ok"):
        return {"ok": False, "why": tgt.get("why", "按钮解析失败"),
                "candidates": tgt.get("candidates", [])}
    RECT_COMPONENT = _rect_of_go(text, tgt["fid"])
    if not RECT_COMPONENT:
        return {"ok": False,
                "why": f"{tgt['go']} 上找不到 RectTransform（type 224）"}

    go = _block(text, 1, host_of(text, tgt["fid"]))
    if not go:
        return {"ok": False,
                "why": f"场景里找不到 GameObject &{host_of(text, tgt['fid'])}"}
    if not re.search(r"m_IsActive:\s*1", go):
        return {"ok": False, "why": f"{tgt['go']} 当前不活跃"}

    # 激活状态 / Interactable /绑定方法 / CallState 都已在
    # resolve_target_button 里查过 —— 那里是唯一判据出口，避免两处
    # 各查一遍、结论却不一致（那种不一致最难查，因为你看到两个
    # 都「检查通过」的判据，行为却相反）。
    #
    # 这里只把「选了哪个按钮」记进返回值，供报告展示。

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

    # ---- 逐层求矩形，最后取中心 ----
    #
    # 旧写法是「把每层当点累加」：
    #     cx += anchor_min.x * 960 + anchor_max.x * 960 + pos.x
    # 对 anchor=(0,0)..(1,1) 的全屏拉伸层，算出来是 0*960+1*960=960，
    # 于是**每遇到一层全屏容器就凭空多出 960 像素**。
    # `CreateServerButton` 的祖先链里有 3 层全屏容器，累加结果就是
    # (3215, 3200) —— 比 1920x1080 还大，整条链的判断全废。
    #
    # 正确做法：维护矩形 (x0,y0,x1,y1)，每层按
    #     子矩形在父矩形内的偏移 = anchor * 父尺寸 + anchoredPosition
    #     子尺寸 = (anchor_max - anchor_min) * 父尺寸 + sizeDelta
    # 逐层算下来，最后取中心。这是 Unity 自己的 RectTransform 公式。
    W, H = 1920.0, 1080.0
    # 根矩形：整屏，左下角原点。
    #
    # 参考分辨率不是猜的 —— Game.unity 里 CanvasScaler 写着
    #     m_ReferenceResolution: {x: 1920, y: 1080}
    #     m_UiScaleMode: 1        （Scale With Screen Size）
    # 换掉这个数，UI 布局会整体变化，所以这里从场景读，不写死。
    ref = re.search(r"m_ReferenceResolution:\s*\{x:\s*(-?[\d.]+),\s*y:\s*(-?[\d.]+)\}",
                    text)
    if ref:
        W, H = float(ref.group(1)), float(ref.group(2))
    # 根矩形：左下角原点、右上角 (W,H)
    rx0, ry0, rx1, ry1 = 0.0, 0.0, W, H
    trace: list[str] = []
    for cid, v in reversed(chain):
        pw, ph = rx1 - rx0, ry1 - ry0
        # 父矩形退化（宽或高为 0）说明上一层是根 Canvas 层：
        # Unity 里根 Canvas 的 RectTransform sizeDelta 就是 (0,0)，
        # 它直接铺满整个参考分辨率。此时用参考分辨率兜底，
        # 否则整条链会塌成 0 宽 0 高（实测：CreateServerButton 的
        # 祖先链最内两层就是这种，累加结果 (0,0)-(0,0)）。
        if pw <= 0:
            pw = W
            rx0 = 0.0
        if ph <= 0:
            ph = H
            ry0 = 0.0
        w = (v["anchor_max"][0] - v["anchor_min"][0]) * pw + v["size_delta"][0]
        h = (v["anchor_max"][1] - v["anchor_min"][1]) * ph + v["size_delta"][1]
        x0 = rx0 + v["anchor_min"][0] * pw + v["anchored_position"][0]
        y0 = ry0 + v["anchor_min"][1] * ph + v["anchored_position"][1]
        rx0, ry0, rx1, ry1 = x0, y0, x0 + w, y0 + h
        trace.append(f"      &{cid}: anchor={v['anchor_min']}..{v['anchor_max']} "
                     f"pos={v['anchored_position']} size={v['size_delta']} "
                     f"-> 矩形 ({rx0:.1f}, {ry0:.1f}) .. ({rx1:.1f}, {ry1:.1f})")
    cx = (rx0 + rx1) / 2.0
    cy = (ry0 + ry1) / 2.0
    # 最终矩形完全落在参考分辨率内 -> 累加逻辑成立。
    # 不成立说明还有没建模的变换（Canvas Scaler 之类），
    # 这时点下去必然点空，必须让调用方看见而不是硬点。
    in_view = (-1e-6 <= rx0 and rx1 <= W + 1e-6
               and -1e-6 <= ry0 and ry1 <= H + 1e-6)

    own = chain[0][1] if chain else {}
    return {
        "ok": True,
        "method": tgt.get("method"),
        "depth": len(chain),
        "own_anchor_min": own.get("anchor_min"),
        "own_anchor_max": own.get("anchor_max"),
        "own_pivot": own.get("pivot"),
        "own_size_delta": own.get("size_delta"),
        "center_1920x1080": (cx, cy),
        "rect": (rx0, ry0, rx1, ry1),
        "in_view": in_view,
        "trace": trace,
        # 把「选了哪个按钮」和「还有哪些候选」一起带出去。
        # 前两版只报坐标不报按钮名，于是点击差异 0.00% 时，
        # 报告写「点击很可能没生效」—— 把排查方向指向浏览器和坐标，
        # 而真实原因是点错了按钮对象。方向错比查不到更费时间。
        "target_go": tgt.get("go"),
        "target_field": tgt.get("field"),
        "target_why": tgt.get("why"),
        "candidates": tgt.get("candidates", []),
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
    print("== 目标按钮（从 Game.unity 按GUID 解析，不写死 fileID）")
    print(f"   选中对象   : {layout.get('target_go')}"
          f"（字段 {layout.get('target_field')}）")
    print(f"   选中理由   : {layout.get('target_why')}")
    cands = layout.get("candidates") or []
    if cands:
        print("   全部候选（点错按钮就是从这张表里选错的）:")
        for c in cands:
            act = {True: "开", False: "关", None: "?"}[c["active"]]
            print(f"      {c['field']:<26} {c['go']:<28} "
                  f"激活={act}  绑定={c['method']}")
    print()
    print(f"== {layout.get('target_go')} 布局（从 Game.unity 读出，非硬编码）")
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

        # ---- 点击前先查「悬停副作用」 ----
        #
        # 场景里有这么两个按钮（实测自 Game.unity）：
        #   PrevVisibilityButton / NextVisibilityButton
        #   绑的是 EventTrigger 的 **PointerEnter**（m_Mode: 6）
        #   -> ChangeMultiplayerMode(bool)
        #
        # 而 ChangeMultiplayerMode 里有一个三态循环会把 _useSteam
        # **改回 true**（ButtonManager.cs 第 777 / 791 行）。也就是说
        # **鼠标只是扫过那个面板，联机模式就被切回去了**，单玩家入口
        # 可能随之消失 —— 而那正是我们要点的入口。
        #
        # 这两个按钮在 NewGameHolder 里，SingleplayerButton 在
        # DevLayout 里，两者坐标不重叠，所以直接点不会踩到。但
        # Playwright 的 mouse.move 是一次「瞬移」，中间路径不产生
        # 悬停事件，风险其实很低；可一旦坐标算偏了、或者将来布局
        # 变了，「点不中」会被误判成「按钮不可用」，排查成本很高。
        #
        # 所以这里**把鼠标从起点直接移到目标，不走中间路径**，
        # 并在点击前后各截一张 —— 如果 after 出现面板切换，
        # 两张图能立刻看出来。
        #
        # 注意这条防护**只能降低风险、不能消除**：真要彻底解决，得
        # 让 ButtonManager 在 WebGL 下无视 ChangeMultiplayerMode
        # （它本来就是给玩家在主菜单里切联机/单人的 UI），
        # 那样连真实玩家用鼠标扫过都不会改 _useSteam。
        say("从画布左上角直接移到目标（不走中间路径，避免悬停触发）")
        pg.mouse.move(box["x"] + 2, box["y"] + 2)
        time.sleep(0.2)
        pg.mouse.move(tx, ty, steps=1)
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
