#!/usr/bin/env python3
"""校验 .github/workflows/*.yml 是不是 GitHub Actions 愿意跑的合法文件。

**这个坑栽过实打实的一次，而且代价是全部推论作废。**

提交 `9a61005`（CI run #118）往 workflow 里插入新步骤时，把上一个
`- name: Publish verification verdict` 那行**连同名带内容一起删掉了**，
只留下它的 `if:` / `uses:` / `with:` 顶在上一个 `run:` 步骤后面。
于是那个 step 变成一个 `run` 步骤后面又挂两个 `if`：

```
      - name: Click into singleplayer and capture
        if: always()        <-第 1 个
        run: |
          ...
        if: always()        <- 第 2 个，'if' is already defined
        uses: actions/github-script@v7   <- run 和uses 不能共存
```

GitHub 的判决：

```
Invalid workflow file: .github/workflows/build-webgl.yml#L1
  (Line: 1116, Col: 9): 'if' is already defined,
  (Line: 1117, Col: 9): Unexpected value 'uses',
  (Line: 1118, Col: 9): Unexpected value 'with'
```

**后果不是某一步失败，是整个 run 秒挂**：`run_started_at == created_at`，
`GET /jobs` 返回 `total_count: 0` —— runner 从未被分配。#118 到 #124
连续 7 个 run 全是这样，累计 10+ 个提交**从未构建过一次**。

## 为什么之前所有校验都没抓到

1. `bash -n` 只验 shell 片段 —— YAML 结构它一点都不管。
2. **PyYAML 默认模式按设计就允许重复键**（后者静默覆盖前者）。
   `yaml.safe_load(...)` 照样成功返回，步骤数从 20 变成 19
   （少的那步被合并进上一步），**看起来只像"步骤变少了"**。
   这是最坏的一种失效：工具报告成功，实际已经错了。
3. 数步骤的判据只判"少没少"，不判"名字对不对"，所以 19 != 20
   这个信号也被我自己当成了正常波动。

所以这个脚本要干的唯一一件事，就是**换一个不会静默覆盖的解析器**，
把重复键变成硬错误，并把两个行号一起打出来。

## 一个绕不开的自指问题

workflow 文件非法时CI **根本不会启动**，所以「在 CI 里检查 workflow
是否合法」是没有意义的。本脚本的定位是**提交前本地门禁**，
但也仍然接进 CI：

- 提交前：`python3 tools/check_workflow_yaml.py .github/workflows/*.yml`
- CI 里：跑到这一步说明文件**已经**合法了，于是它变成一道**回归网**
  —— 防止后续某次机械替换再把某个 `- name:` 行吃掉。
  两种场合都不多余。

## 只报不改

和 `check_lightmode.py` 同一个原则：本脚本不碰workflow。
它报出行号，改不改、怎么改由人判断 —— 机械自动修复 workflow
的代价（改出一个能解析但语义错的文件）远大于收益。

## 用法

    python3 tools/check_workflow_yaml.py                       # 查 .github/workflows/ 下全部
    python3 tools/check_workflow_yaml.py path/to/a.yml [...]   # 查指定文件
    python3 tools/check_workflow_yaml.py --expect-steps build-webgl=20

`--expect-steps` 是给「步骤数不该悄悄变」准备的：#118 那次PyYAML
报19 步而真实值是 20，差1步就是线索。合法文件上它同时是一道断言。

退出码：0 = 全过；1 = 有非法文件或期望步数不符。
"""

from __future__ import annotations

import sys
from pathlib import Path

import yaml

# GitHub Actions 的 workflow 里，这两种写法含义完全不同：
#   steps[].uses  （引Action）
#   steps[].run    （跑 shell）
# 同一个 step 里同时出现必然非法。同理一个 step 只能有一个 if。
# PyYAML 不会拦这类「语义上冲突但键不重复」的情况，所以要自己查。
MUTUALLY_EXCLUSIVE = [("run", "uses"), ("uses", "run")]


class StrictLoader(yaml.SafeLoader):
    """拒绝重复键的 SafeLoader。

    PyYAML 默认实现遇到重复键是 `mapping[key] = value`，后者覆盖前者，
    连警告都没有。这正是 run #118 的 bug 能一路绿灯走完本地校验的原因。
    """


def _no_duplicate_keys(loader: StrictLoader, node, deep: bool = False):
    mapping: dict = {}
    first_seen: dict = {}
    for key_node, value_node in node.value:
        key = loader.construct_object(key_node, deep=deep)
        if key in mapping:
            raise yaml.constructor.ConstructorError(
                "while constructing a mapping",
                node.start_mark,
                f"重复键 '{key}'（第 {key_node.start_mark.line + 1} 行，"
                f"与第 {first_seen[key]} 行重复）",
                key_node.start_mark,
                "GitHub Actions 会拒绝整个 workflow 文件，run 秒挂且不分配 runner。"
                "PyYAML 默认模式会静默覆盖，所以普通 yaml.safe_load 看不到。",
            )
        mapping[key] = loader.construct_object(value_node, deep=deep)
        first_seen[key] = key_node.start_mark.line + 1
    return mapping


StrictLoader.add_constructor(
    yaml.resolver.BaseResolver.DEFAULT_MAPPING_TAG, _no_duplicate_keys
)


def lint(path: Path) -> tuple[list[str], list[str]]:
    """返回 (errors, warnings)。"""
    errors: list[str] = []
    warnings: list[str] = []
    try:
        doc = yaml.load(path.read_text(encoding="utf-8"), Loader=StrictLoader)
    except yaml.YAMLError as exc:
        errors.append(f"{path}: YAML 非法: {exc}")
        return errors, warnings
    except FileNotFoundError:
        errors.append(f"{path}: 文件不存在")
        return errors, warnings

    if not isinstance(doc, dict):
        errors.append(f"{path}: 顶层不是映射（得到 {type(doc).__name__}）")
        return errors, warnings

    jobs = doc.get("jobs")
    if not isinstance(jobs, dict) or not jobs:
        errors.append(f"{path}: 没有 jobs 定义")
        return errors, warnings

    for job_name, job in jobs.items():
        steps = job.get("steps") or []
        if not steps:
            errors.append(f"{path}: job '{job_name}' 没有任何 step")
            continue
        for idx, step in enumerate(steps, 1):
            where = f"{path}: job '{job_name}' 第 {idx} 步"
            if not isinstance(step, dict):
                errors.append(f"{where}: 不是映射（通常是缩进跑偏）")
                continue
            # 吞掉的步骤在这里露出来：#118 就是少了 `- name:` 那一行，
            # 整个 step 被并进上一个，于是这里既没有 name 也没有独立身份。
            #
            # 注意 `- uses: actions/checkout@v4` 是 GitHub 官方允许的简写，
            # 没有 name完全正常，不要报 —— 误报的代价不是"多一行噪音"，
            # 是让人对真正的问题也开始怀疑「这次是不是真的」。
            if not step.get("name") and "uses" not in step:
                warnings.append(f"{where}: 既没有 name 也没有 uses —— 检查 `- name:` 那行是否被删掉过")
            for a, b in MUTUALLY_EXCLUSIVE:
                if a in step and b in step:
                    errors.append(
                        f"{where}: 同时有 '{a}' 和 '{b}'"
                        "（同一个 step 不能既跑 shell 又引 Action）"
                    )
            if "run" not in step and "uses" not in step:
                warnings.append(f"{where}: 既没有 run 也没有 uses")
            # 这里**故意没有**「run 块里出现 `- name:`」这类后置判据。
            # 试过（反向验证第4 例），它在任何情况下都不会触发：
            # 若下一个 step 真被吞进`run: |` 的内容区，YAML 在
            # **解析阶段**就抛 ParserError 了（`expected <block end>,
            # but found '-'`），根本走不到step 循环。
            # 而合法缩进的 `- name:` 在6 空格上，PyYAML 会正确地
            # 把它当成独立 step —— 那不是错误。
            # 留一个永不响的检查冒充覆盖，比没有更糟：
            # 它会让人以为「缩进吃掉step」这类问题有防护。
    return errors, warnings


def main(argv: list[str]) -> int:
    expect: dict[str, int] = {}
    paths: list[Path] = []
    args = list(argv)
    while args:
        a = args.pop(0)
        if a == "--expect-steps":
            spec = args.pop(0)
            name, _, n = spec.partition("=")
            if not n:
                print(f"::error::--expect-steps 格式应为 job=数量，收到 {spec!r}")
                return 1
            expect[name] = int(n)
        else:
            paths.append(Path(a))

    if not paths:
        paths = sorted(Path(".github/workflows").glob("*.yml"))
        if not paths:
            print("::error::.github/workflows/ 下没有 .yml 文件")
            return 1

    total_err = 0
    for p in paths:
        errors, warnings = lint(p)
        for w in warnings:
            print(f"::warning file={p}::{w}")
        if errors:
            for e in errors:
                print(f"::error file={p}::{e}")
            total_err += len(errors)
            continue
        # 重新解析一次拿 step 数。这里刻意不缓存 doc：lint() 里的
        # 解析结果在它内部，多存一份就多一份可能和文件内容脱节的副本
        # —— 而「报出来的步数对不上实际文件」正是本脚本要防的那类错误。
        doc = yaml.load(p.read_text(encoding="utf-8"), Loader=StrictLoader)
        for job_name, job in (doc.get("jobs") or {}).items():
            n = len(job.get("steps") or [])
            note = ""
            if job_name in expect:
                if expect[job_name] == n:
                    note = f"（符合期望 {expect[job_name]}）"
                else:
                    total_err += 1
                    note = f" <<<::error file={p}::（期望 {expect[job_name]} 步，实得 {n} 步）"
            print(f"{p}: job '{job_name}' {n} 步{note}")

    if not total_err:
        print(f"workflow 结构合法：{len(paths)} 个文件，0 个非法。")

    if total_err:
        print(f"共 {total_err} 处问题。workflow 非法时 run 会秒挂且不分配 runner，"
              "不会产生任何日志 —— 所以必须在 push 前拦住。")
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))