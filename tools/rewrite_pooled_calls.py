#!/usr/bin/env python3
"""把 168 处 RPC 序列化调用点指向 Pooled 精确签名的重载。

背景
----
GameTypeSerializers.cs 里的方法声明为 (Writer, T) / (Reader)，但走 RPC 的
调用点传的实参静态类型是 PooledWriter / PooledReader。PooledWriter 继承
Writer，所以 C# 编译零报错，但 IL2CPP 把委托调用编成 call_indirect，
Emscripten 的 invoke_* 在运行时比对签名对不上，抛
  RuntimeError: function signature mismatch
表现为加载场景时进度条卡在 90%。

做法：只改**调用点**的限定名，从 GameTypeSerializers 换成
GameTypeSerializersPooled，让实参静态类型与被调方法的形参类型严格一致。
方法名一个字符都不动——名字里编码了 FishNet 为哪个命名空间生成的。

安全设计：默认 dry-run，必须显式 --write 才落盘，且写前备份。
这个脚本会做双向校验：改完后统计实参类型，如果仍有 pooledWriter /
PooledReader0 配GameTypeSerializers 的组合，说明漏改了，必须报错退出。
"""
import argparse
import re
import shutil
import sys
from pathlib import Path

#脚本在 <root>/tools/ 下，Unity 工程在 <root>/ExportedProject/。
# 少写一层 parent 会指向 <root>/Assets，那是不存在的路径——第一版就栽在这。
ROOT = Path(__file__).resolve().parent.parent
SCRIPTS = ROOT / "ExportedProject" / "Assets" / "Scripts"

# 调用点实参名 -> 期望的限定类
POOLED_ARGS = ("pooledWriter", "PooledReader0")

# GameTypeSerializers.GXxx(  ->  GameTypeSerializersPooled.GXxx(
CALL_RE = re.compile(r"\bGameTypeSerializers\.(G(?:Write|Read)___[A-Za-z0-9_]+)\(")

# 找出「调用是 GameTypeSerializers. 且 首参是 pooled 类型」的行
CALLER_RE = re.compile(
    r"GameTypeSerializers\.(G(?:Write|Read)___[A-Za-z0-9_]+)\((pooledWriter|PooledReader0)\b"
)


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--write", action="store_true", help="真正写盘（默认只报告）")
    args = ap.parse_args()

    if not SCRIPTS.is_dir():
        print(f"找不到脚本目录: {SCRIPTS}", file=sys.stderr)
        return 2

    total_hits = 0
    changed: list[tuple[Path, int]] = []

    for path in sorted(SCRIPTS.rglob("*.cs")):
        # 跳过被改写者自身，否则会把类名改到自己头上
        if path.name == "GameTypeSerializersPooled.cs":
            continue
        text = path.read_text(encoding="utf-8", errors="strict")
        if "GameTypeSerializers." not in text:
            continue

        hits = CALLER_RE.findall(text)
        if not hits:
            continue

        new_text = CALL_RE.sub(r"GameTypeSerializersPooled.\1(", text)
        n = len(hits)
        total_hits += n

        if new_text != text:
            changed.append((path, n))
            if args.write:
                shutil.copy2(path, path.with_suffix(".cs.bak"))
                path.write_text(new_text, encoding="utf-8")

    mode = "已写盘" if args.write else "dry-run（加 --write 生效）"
    print(f"模式: {mode}")
    print(f"命中需改写的调用点: {total_hits} 处，分布在 {len(changed)} 个文件:")
    for path, n in changed:
        print(f"   {n:>4}  {path.relative_to(ROOT)}")

    if total_hits == 0:
        print("\n没有命中任何 pooled 调用点。", file=sys.stderr)
        print("这可能意味着调用点已经改写过，或者实参变量名与预期不同——", file=sys.stderr)
        print("先跑 dry-run 看统计，别直接当成功。", file=sys.stderr)
        return 1

    # 双向校验的另一半：改完之后不能再有残留。
    leftover = 0
    for path in sorted(SCRIPTS.rglob("*.cs")):
        if path.name == "GameTypeSerializersPooled.cs":
            continue
        t = path.read_text(encoding="utf-8", errors="replace")
        leftover += len(CALLER_RE.findall(t))
    if args.write and leftover:
        print(f"\n仍有 {leftover} 处未改写，请检查。", file=sys.stderr)
        return 1
    if args.write:
        print("\n校验通过：无残留的 GameTypeSerializers + pooled 实参组合。")
        print("备份文件是 *.cs.bak，确认无误后可删。")
    return 0


if __name__ == "__main__":
    sys.exit(main())
