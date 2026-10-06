#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""自检：CI 里「算出来的证据」有没有真的能被人看到。

## 这类失效的历史记录

本项目栽在同一个坑上至少五次了，每次形态都不同：

| 轮次 | 证据产在哪| 问题 | 表现 |
|------|-----------|------|------|
| 1 | `verify_load.py` 内、截图之后 | 该步末尾 `exit 1` 把统计掐断 | 洋红长期写「无记录」 |
| 2 | `#pragma target 3.5` 时代| Pillow 缺、`except ImportError` 吞掉 | 统计静默跳过，报告照写「无记录」 |
| 3 | 报告 `head -50` | 包目录清单把结论段挤出窗口 | 「覆盖 N 个」整段消失 |
| 4 | `verify-evidence/cs-errors.txt` | 该目录被 .gitignore 忽略 | C# 扫描等于没做 |
| 5 | 同上，12 处 | 只修了2 处就以为修完了 | 其余继续静默 |
| 6 | `check_dangling_refs.py` | `m_Script` 悬空一律归「包内」 | 判据把真的丢失也算成正常 |
| 7 | `in_report` 判据 | 只统计不判决，且把「产出」当「报告引用」 | 4 份证据产出了没人看 |
| 8 | **校验workflow 本身** | PyYAML 允许重复键，`bash -n` 只看 shell | **run #118–#124 连续 7 次秒挂、0 job** |

第 4/5 两条是最阴的：**证据文件确实生成了、内容也确实正确，
只是没有任何人看得到**。构建绿、报告看着正常、下一轮的我也
以为「这项已经查过了」。

第 8 条更狠一档：它让**第 8 条到第 12 条证据**（一共几十个提交）
**一次都没跑过**。细节见 tools/check_workflow_yaml.py 的 docstring。
它也是本脚本清单里`workflow-yaml.txt` 那一条的由来。

## 所以本脚本查什么

对每个「本该被人看到的产物」，三问：

1. **生成了吗** —— 有没有对应的生产步骤
2. **在报告里吗** —— 报告有没有引用它
3. **提交得了吗** —— 落点会不会被 .gitignore 吃掉

第 3 条是本脚本的重点：它把 `.gitignore` 的规则和 workflow 里
的落点对起来算。这是最难靠肉眼发现的一环 —— 路径拼错一个字母、
或者只是多写了一层目录，文件就静默地不提交了。

## 它刻意不做什么

不检查证据**内容**对不对。那是各检查器自己的事
（check_mat_shader_props / check_shader_keywords / ...）。
本脚本只管「证据有没有送到人眼前」，是元检查。

用法
----
    python3 tools/check_evidence_pipeline.py
    python3 tools/check_evidence_pipeline.py --json
"""
from __future__ import annotations

import argparse
import json
import os
import re
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
WORKFLOW = os.path.join(ROOT, ".github", "workflows", "build-webgl.yml")
GITIGNORE = os.path.join(ROOT, ".gitignore")

# 每份证据声明三件事：
#   key   证据文件名（docs/ci-evidence/ 下的）
#   must  是否**必须**能被人看到。False 表示「生成它只为当前步骤内用」
#   what  它回答什么问题（写进报告，让人知道缺了意味着什么）
EVIDENCE = [
    ("official-shader-sync.txt", True,
     "官方 shader 覆盖到底生效没有（报告第一屏「### 判决：覆盖」的来源）"),
    ("cs-errors.txt", True,
     "C# 有没有编译错误 —— UnityTransport 疑点的判决"),
    ("unitytransport-check.txt", True,
     "UnityTransport 在构建日志里出现过几次（0 次不能证明它不存在）"),
    ("shader-errors.txt", True,
     "shader 编译错误（不报错不失败，只把材质渲成洋红）"),
    ("build-logs-scan.txt", True,
     "日志到底扫到了什么 —— 「没扫到」和「扫了没找到」必须分开"),
    ("mat-shader-props.txt", True,
     "材质设了但 shader 没声明的属性（静默丢弃，Unity 一行日志都不打）"),
    ("shader-keywords.txt", True,
     "A 类 keyword 错配 / B 类多 Pass 缺失"),
    ("dangling-refs.txt", True,
     "悬空引用 —— 区分「包内没导出」（正常）、「真丢失」和「DLL 身份漂移」"),
    ("lightmode.txt", True,
     "LightMode 缺失 —— URP 只在 UniversalForward 注入光照 uniform，"
     "落进 SRPDefaultUnlit 就是没人赋值，物体全黑"),
    ("skybox.txt", True,
     "天空盒链路数据完整性（碧海蓝天的来源）"),
    ("urp-apv-guards.txt", True,
     "URP 里 APV/SSBO 的门控宏（决定 URPDecal 能不能在 WebGL2 上编译）"),
    ("ingame-layout.txt", True,
     "单玩家按钮的屏幕坐标（算错了点击会打中别的元素）"),
    ("ingame-click.txt", True,
     "点击前后对比 —— 「能不能玩」的直接证据"),
    ("workflow-yaml.txt", True,
     "workflow 自身结构合法 + 步数没变 —— run #118 就是这里出的问题"),
]


def load_gitignore_rules() -> list[str]:
    if not os.path.isfile(GITIGNORE):
        return []
    out = []
    for line in open(GITIGNORE, encoding="utf-8", errors="replace"):
        line = line.rstrip("\n")
        if not line.strip() or line.lstrip().startswith("#"):
            continue
        out.append(line.strip())
    return out


def ignored_by(path: str, rules: list[str]) -> bool:
    """简化版 gitignore 匹配：只处理目录规则和无通配符规则。

    刻意不做完整实现 —— 这里的判据是「workflow 里写的那个落点会不会
    被现有规则吃掉」，而那都是简单形式（`verify-evidence/`）。
    遇到 `**` / `!` 否定规则等情况会保守返回 False（不误报）。
    """
    for r in rules:
        neg = r.startswith("!")
        r2 = r[1:] if neg else r
        if r2.startswith("/"):
            r2 = r2[1:]
        if not r2:
            continue
        if any(c in r2 for c in "*?["):
            continue          # 带通配符 -> 保守放过
        p = r2.rstrip("/")
        if path == p or path.startswith(p + "/"):
            return not neg
        # 目录规则：gitignore 里 `foo/` 匹配 foo 下的一切
        if r2.endswith("/") and path.startswith(r2):
            return not neg
    return False


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--json", action="store_true")
    a = ap.parse_args()

    problems: list[str] = []
    notes: list[str] = []
    rows: list[dict] = []

    if not os.path.isfile(WORKFLOW):
        print(f"::error::找不到 {WORKFLOW}")
        return 1
    wf = open(WORKFLOW, encoding="utf-8", errors="replace").read()
    rules = load_gitignore_rules()

    for name, must, what in EVIDENCE:
        rel = f"docs/ci-evidence/{name}"
        row = {"file": rel, "required": must, "answers": what}

        # 1) 生产/复制：有谁写它
        writes = len(re.findall(re.escape(rel), wf))
        row["referenced_in_workflow"] = writes

        # 2) 报告里有没有**真正把它读出来**的那一步
        #
        #    判据原先是「workflow 里出现过 docs/ci-evidence/<name>」，
        #    这条等于没有：产出步骤里的
        #        tee verify-evidence/x.txt docs/ci-evidence/x.txt
        #    自己就满足这个条件。于是「产出」被当成了「报告引用」，
        #    而报告里那一节被删掉也照样通过。
        #
        #    实测栽在这里：把 `## LightMode（...）` 改名后，
        #    tee 那行还在，旧判据报「在报告里=是」—— 但那一节已经没了。
        #
        #    真正要问的是：报告生成区有没有把它读出来。报告里有三种
        #    读法，都算数（早先只认 cat 一种，于是把下面 10 份全判成
        #    「没引用」—— 那是判据的漏洞，不是它们真有问题）：
        #        cat   verify-evidence/x.txt
        #        head -N verify-evidence/x.txt
        #        [ -s  docs/ci-evidence/x.txt ]
        reads = [
            rf"cat\s+verify-evidence/{re.escape(name)}\b",
            rf"(?:head|tail|less|more)\s+[-\w\s]*verify-evidence/{re.escape(name)}\b",
            rf"\[?\s*-s\s+\S*verify-evidence/{re.escape(name)}\b",
            rf"\[?\s*-s\s+docs/ci-evidence/{re.escape(name)}\b",
            rf"cat\s+docs/ci-evidence/{re.escape(name)}\b",
        ]
        hit = next((r for r in reads if re.search(r, wf)), None)
        in_report = hit is not None
        row["in_report"] = in_report
        row["in_report_by"] = hit

        # 3) 会不会被 gitignore 吃掉
        ig = ignored_by(rel, rules)
        row["gitignored"] = ig

        if writes == 0 and must:
            problems.append(f"{rel}: workflow 里完全没出现 —— "
                            f"没有步骤产出或复制它（{what}）")
        if ig and must:
            problems.append(
                f"{rel}: **被 .gitignore 忽略** —— 内容正确也不会提交，"
                f"等于算了个空（{what}）")
        if not in_report and must:
            # 这一条是**补上的**：原先 in_report 只统计不判决，于是
            # 「报告里贴了证据但没有任何说明」和「报告里根本没提」
            # 在结果里长得一模一样。
            #
            # 实测栽在这里：我把报告里的 `## LightMode（...）` 标题
            # 改名，`docs/ci-evidence/lightmode.txt` 字符串仍在
            # workflow 里（被 tee 和白名单引用），所以旧判据照样
            # 报「在报告里=是」—— 但那一节已经不存在了。
            #
            # 后果与前五次同类：文件确实产出了、内容也确实正确，
            # 只是报告里没人解释它意味着什么，于是没人读。
            problems.append(
                f"{rel}: 报告里**没有引用它的位置** —— "
                f"证据产出了却没人看得到（{what}）")
        rows.append(row)

    data = {"ok": not problems, "rows": rows, "problems": problems,
            "notes": notes}
    if a.json:
        print(json.dumps(data, ensure_ascii=False, indent=2))
        return 0 if not problems else 0

    print(f"== 证据管线自检：{len(EVIDENCE)} 份关键证据\n")
    print(f"{'文件':<40} {'落点出现':>8} {'报告引用':>8} {'被忽略':>8}")
    print("-" * 70)
    for r in rows:
        print(f"{os.path.basename(r['file']):<40} "
              f"{r['referenced_in_workflow']:>8} "
              f"{'是' if r['in_report'] else '否':>8} "
              f"{'**是**' if r['gitignored'] else '否':>8}")
    print()
    if problems:
        print(f"发现 {len(problems)} 个问题：")
        for p in problems:
            print(f"  ! {p}")
    else:
        print("全部证据都能送到人眼前。")
    print()
    print("本脚本只管「证据有没有被看见」，**不管内容对不对** ——")
    print("内容判据是各检查器自己的事（check_* / sync_*）。")
    print("「被忽略=否」只说明没命中现有 ignore 规则，不代表一定提交了：")
    print("提交那一步是白名单式 `git add -f`，漏列同样会静默失败，")
    print("那一环由 CI 里暂存区断言负责。")
    return 0


if __name__ == "__main__":
    sys.exit(main())
