"""沿 Unity 场景 YAML 的 fileID 引用链，取 GameObject 的名字与激活状态。

为什么需要这个
--------------
CI 一直在点一个叫 `SingleplayerButton` 的对象，但点击没生效（两帧差异
0.00%）。查下来ButtonManager 里有**两套**容易混淆的东西：

    _createServerButton    / _createSingleplayerText     <- 创建新档
    _singleplayerText                                 <- 选中存档后显示

`SingleplayerButton` 绑定的是 `CreateLocalLobbyButton`，属于 SavedServer
那一套；而 WebGL 单人版真正该点的是 `_createServerButton`。点错了按钮，
差异自然是 0.00% —— 而报告里那句「点击很可能没生效」只说了现象，
没说「可能点的是另一个按钮」。

这个脚本沿fileID 链把ButtonManager 的每个按钮字段解出真实名字，
避免再靠名字猜。
"""
from __future__ import annotations

import re
import sys

SCENE = sys.argv[1] if len(sys.argv) > 1 else "Assets/Scenes/Game.unity"
BUTTON_MANAGER_GUID = "488b2771a904a6b76cd4192c654bb696"

text = open(SCENE, encoding="utf-8", errors="replace").read()

# 切出所有文档块：--- !u!<classID> &<fileID>\n<body>
BLOCK = re.compile(r"^--- !u!(\d+) &(\d+)\s*\n", re.M)
blocks: dict[int, tuple[int, str]] = {}
marks = [(m.start(), int(m.group(1)), int(m.group(2)))
         for m in BLOCK.finditer(text)]
for idx, (start, _cls, fid) in enumerate(marks):
    end = marks[idx + 1][0] if idx + 1 < len(marks) else len(text)
    blocks[fid] = (_cls, text[start:end])

GO_NAME = re.compile(r"^\s*m_Name:\s*(.*)$", re.M)
ACTIVE = re.compile(r"^\s*m_IsActive:\s*(\d)", re.M)
GAMEOBJ = re.compile(r"^\s*m_GameObject:\s*\{fileID:\s*(\d+)\}", re.M)
PARENT = re.compile(r"^\s*m_Father:\s*\{fileID:\s*(\d+)\}", re.M)


def go_of_component(fid: int) -> int | None:
    """组件 -> 它挂的 GameObject 的 fileID。"""
    if fid not in blocks:
        return None
    m = GAMEOBJ.search(blocks[fid][1])
    return int(m.group(1)) if m else None


def name_of(fid: int) -> str:
    """GameObject 或组件的名字（组件回溯到宿主）。"""
    target = fid
    if fid in blocks:
        m = GAMEOBJ.search(blocks[fid][1])
        if m:
            target = int(m.group(1))
    if target not in blocks:
        return f"<{fid} 不在场景里>"
    m = GO_NAME.search(blocks[target][1])
    return (m.group(1).strip() if m else "") or f"<{target} 无名>"


def active_of(fid: int) -> str:
    target = fid
    if fid in blocks and GAMEOBJ.search(blocks[fid][1]):
        target = int(GAMEOBJ.search(blocks[fid][1]).group(1))
    if target not in blocks:
        return "?"
    m = ACTIVE.search(blocks[target][1])
    return {0: "关", 1: "开"}.get(int(m.group(1)), "?") if m else "?"


def chain_of(fid: int, depth: int = 6) -> list[tuple[int, str]]:
    """沿 m_Father 向上，返回 (fileID, 名字)。"""
    out: list[tuple[int, str]] = []
    cur = fid
    for _ in range(depth):
        if cur == 0 or cur not in blocks:
            break
        m = PARENT.search(blocks[cur][1])
        out.append((cur, name_of(cur)))
        if not m:
            break
        cur = int(m.group(1))
    return out


def button_targets(component_fid: int) -> list[tuple[str, str, str]]:
    """列出 ButtonManager 组件里所有按钮类字段的真实名字。"""
    body = blocks[component_fid][1]
    out = []
    for m in re.finditer(
            r"^  (_[A-Za-z0-9_]*(?:Button|button)[A-Za-z0-9_]*):"
            r"\s*\{fileID:\s*(\d+)\}", body, re.M):
        field, fid = m.group(1), int(m.group(2))
        out.append((field, name_of(fid), active_of(fid)))
    return out


def main() -> int:
    i = text.find(BUTTON_MANAGER_GUID)
    if i < 0:
        print(f"{SCENE} 里找不到 ButtonManager（guid {BUTTON_MANAGER_GUID}）")
        return 1
    head = text.rfind("--- !u!", 0, i)
    end = text.find("\n--- !u!", i)
    body = text[head:end]
    cid = re.match(r"--- !u!(\d+) &(\d+)", body)
    comp_fid = int(cid.group(2))

    print(f"ButtonManager 组件 &{comp_fid}（宿主 GameObject "
          f"{name_of(comp_fid)!r}）\n")
    print("按钮类字段解析结果：")
    print(f"  {'字段':<34} {'真实对象名':<32} {'场景激活'}")
    print("  " + "-" * 76)
    for field, nm, act in button_targets(comp_fid):
        print(f"  {field:<34} {nm:<32} {act}")

    print("\n关键区分（WebGL 单人版该点的是创建按钮，不是 SavedServer 那套）：")
    for field, nm, act in button_targets(comp_fid):
        if field in ("_createServerButton", "_singleplayerText",
                     "_createSingleplayerText"):
            print(f"  {field} -> {nm!r} 激活={act}")
    return 0


if __name__ == "__main__":
    sys.exit(main())