#!/usr/bin/env python3
"""独立统计截图里的洋红面积 —— 不依赖 verify_load.py 的退出路径。

## 为什么必须独立成一个脚本

原先这段统计写在 `tools/verify_load.py` 里面（截图之后、返回退出码之前）。
而那个步骤的末尾是：

    if [ "$VR" -ne 0 ] && [ "$VR" -ne 4 ]; then exit 1; fi

于是形成一条**只在坏产物时才断的链**：

    主场景加载失败 -> verify_load.py 以非 0 退出 -> 洋红统计没跑到
    -> verify-evidence/magenta.txt 不存在 -> 报告写「无记录」

实测（run 37422020996）就是这个状态：截图**好好地提交回了仓库**
（docs/ci-screenshots/verify_loaded.png 76912字节），本地也能算出
洋红 12.768%、颜色精确是 (254,0,254) —— 而报告里写着「无记录」。

问题在于：主场景加载失败**恰恰是最需要洋红数字的时候**。那正是
「画面为什么不对」的第一现场。把判据放在一个会被失败路径掐断的
地方，等于在最需要它的时刻把它摘掉了。

所以这里独立成脚本，只读已经落盘的 PNG，与 verify 步骤的成败完全解耦。
判据和verify_load.py 里的保持一致（同一套阈值），避免两个地方报出
不同数字而没人知道该信哪个。

## 用法

    python3 tools/magenta_stats.py /tmp/verify_loaded.png /tmp/verify_settled.png

写入 verify-evidence/magenta.txt。
"""
from __future__ import annotations

import os
import sys

# 与 verify_load.py 相同的判据：Unity 编译失败的洋红是 (255,0,255) 一族。
# R/B 都高、G 明显低、且三者不全等（后者排除纯白与纯灰）。
R_MIN, B_MIN, G_MAX = 200, 200, 80


def stats(path: str) -> tuple:
    from PIL import Image
    from collections import Counter
    im = Image.open(path).convert("RGB")
    px = list(im.getdata())
    n = len(px)
    hit = sum(1 for r, g, b in px if r > R_MIN and b > B_MIN and g < G_MAX)
    samples = sorted({f"({r},{g},{b})" for r, g, b in px
                      if r > R_MIN and b > B_MIN and g < G_MAX})[:6]
    common = Counter(px).most_common(5)
    return n, hit, 100.0 * hit / n, samples, im.size, common


def main(argv: list[str]) -> int:
    paths = [a for a in argv[1:] if not a.startswith("-")]
    if not paths:
        # 默认找 CI 上那两张。截图由 verify_load.py 在成功路径上拷出，
        # 拷到工作区根目录，所以两个位置都查。
        for cand in ("verify_loaded.png", "verify_settled.png"):
            if os.path.exists(cand):
                paths.append(cand)
    if not paths:
        print("::warning::找不到截图，洋红统计无法执行 —— "
              "注意这**不代表画面没问题**，只代表没有判据。")
        print("查找位置: 工作区根目录 /tmp")
        return 0

    lines, bad = [], 0
    print(f"洋红判据: R>{R_MIN} 且 B>{B_MIN} 且 G<{G_MAX}")
    for p in paths:
        if not os.path.isfile(p):
            print(f"  [跳过] {p} 不存在")
            continue
        try:
            n, hit, pct, samples, size, common = stats(p)
        except Exception as e:
            print(f"  [失败] {p}: {str(e)[:150]}")
            continue
        lines.append(
            f"{os.path.basename(p)} ({size[0]}x{size[1]}): "
            f"{hit}/{n} = {pct:.3f}%"
            + (f"  样本 {', '.join(samples)}" if samples else ""))
        print(f"  {lines[-1]}")
        print(f"    主要颜色: "
              f"{', '.join(f'{c} {100.0*v/n:.1f}%' for c, v in common)}")
        if pct >= 0.5:
            bad += 1
            print(f"    -> >= 0.5%：至少一个 shader 编译失败"
                  f"（Unity 不让构建失败，只把材质渲成洋红）")

    if not lines:
        print("::warning::没有任何截图统计成功 —— 不代表画面没问题。")
        return 0

    try:
        os.makedirs("verify-evidence", exist_ok=True)
        with open("verify-evidence/magenta.txt", "w", encoding="utf-8") as f:
            f.write("\n".join(lines) + "\n")
        print(f"\n已写入 verify-evidence/magenta.txt")
    except OSError as e:
        print(f"::warning::落盘失败: {e}")

    # 有洋红就打 error 注解：这是唯一能自动发现 shader 编译失败的信号，
    # 而 Unity 自己不会报。步骤**不因此 exit 1** —— 还要继续跑完后面的
    # 截图提交与报告生成，那些证据同样重要。
    if bad:
        print(f"::error::{bad} 张截图洋红 >= 0.5%，见上")
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv))