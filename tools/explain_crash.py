#!/usr/bin/env python3
"""把WebGL.wasm 的加载崩溃解成一条可读的调用链。

背景
----
云构建上只能看到alert 里的三行：

    RuntimeError: function signature mismatch
      at WebGL.wasm:wasm-function[103867]:0x1c91f10

光有索引没法定位——IL2CPP 每次构建都会重排函数编号，
跨构建比较"索引只差4"是没有意义的（我走过这条弯路）。
真正的信息在**完整 JS 栈**里，而栈只能在浏览器里抓。

所以分工是：
  1. 浏览器抓栈      -> trace_crash.py（3秒，本地复现）
  2. 本脚本解栈      -> 把 wasm-function[103867] 翻成签名和表项
  3. 对照 IL2CPP 语义 -> 判断是委托 thunk还是业务方法

已验证的判读方法
----------------
IL2CPP 生成的委托Invoke 桩长这样（func[157585]）：

    local.get 2 / local.get 1 / local.get 0
    call_indirect 0 (type 2)

只有三条 local.get 加一条 call_indirect，没有别的指令。
看到这个形状就知道它是 thunk，不是业务代码。

而type N 的定义要从 wasm-objdump -x 的 Type[] 段读：
    - type[0] (i32, i32) -> i32
    - type[1] (i32, i32, i32) -> nil<- 3 参
    - type[2] (i32, i32) -> nil     <- 2 参

调用点写死 type 2（2 参），而被调函数是 type 1（3 参）——
参数个数对不上，Emscripten 直接抛 signature mismatch。
这跟"槽位是不是空的"无关，是两回事，别混。

用法：
    python3 explain_crash.py <WebGL.wasm> 103867 [157585 ...]
    python3 explain_crash.py <WebGL.wasm> --stack-file /tmp/crash_trace/stack.txt
"""
import argparse
import re
import shutil
import subprocess
import sys
from pathlib import Path

# IL2CPP 委托 Invoke 桩的指令数上限。桩里只有若干 local.get
# 加一条 call_indirect，真实业务方法一定有别的指令（memory 读写、
# 算术、跳转、metadata 初始化）。
THUNK_MAX_INSNS = 8


def run_xxd(path: Path) -> str | None:
    exe = shutil.which("wasm-objdump")
    if not exe:
        return None
    try:
        r = subprocess.run([exe, "-x", str(path)], capture_output=True,
                           text=True, timeout=1200)
    except Exception:
        return None
    return r.stdout if "Type[" in r.stdout else None


def parse_types(x: str) -> dict[int, str]:
    types: dict[int, str] = {}
    for m in re.finditer(r"^\s*-\s*type\[(\d+)\]\s*(\([^)]*\)\s*->\s*\S+)\s*$",
                         x, re.M):
        types[int(m.group(1))] = m.group(2).strip()
    return types


def parse_func_sigs(x: str) -> dict[int, int]:
    sigs: dict[int, int] = {}
    for m in re.finditer(r"^\s*-\s*func\[(\d+)\]\s+sig=(\d+)", x, re.M):
        sigs[int(m.group(1))] = int(m.group(2))
    return sigs


def parse_slots(x: str) -> dict[int, int]:
    """func index -> 它在函数表里的槽位。"""
    slots: dict[int, int] = {}
    for m in re.finditer(r"elem\[(\d+)\]\s*=\s*ref\.func:(\d+)", x):
        slots.setdefault(int(m.group(2)), int(m.group(1)))
    return slots


def disasm(path: Path, funcs: list[int], timeout: int = 2400) -> str | None:
    """反汇编。整份 66 MB的 wasm 要几分钟，本地有 32 核扛得住。"""
    exe = shutil.which("wasm-objdump")
    if not exe:
        return None
    try:
        r = subprocess.run([exe, "-d", str(path)], capture_output=True,
                           text=True, timeout=timeout)
    except Exception:
        return None
    return r.stdout if r.returncode == 0 else None


def extract_function(d: str, idx: int) -> list[str]:
    """从整份反汇编里切出某个函数的指令行。"""
    start = None
    pat = re.compile(r"^[0-9a-f]+ func\[%d\]:$" % idx)
    for i, ln in enumerate(d.splitlines()):
        if pat.match(ln.strip()):
            start = i + 1
            break
    if start is None:
        return []
    body: list[str] = []
    for ln in d.splitlines()[start:]:
        s = ln.strip()
        if re.match(r"^[0-9a-f]+ func\[\d+\]:$", s):
            break
        body.append(s)
    return body


def classify(body: list[str]) -> str:
    ops = [ln.split("|")[-1].strip() if "|" in ln else "" for ln in body]
    ops = [o for o in ops if o]
    if not ops:
        return "空函数"
    calls_indirect = [o for o in ops if o.startswith("call_indirect")]
    if not calls_indirect:
        return "业务方法（无间接调用）"
    # 桩的形状：几乎全是 local.get + 一条 call_indirect
    non_get = [o for o in ops
               if not o.startswith("local.get") and not o.startswith("call_indirect")
               and o not in ("end",)]
    if len(ops) <= THUNK_MAX_INSNS and not non_get:
        return "**委托 Invoke 桩（thunk）**"
    return f"含间接调用的方法（{len(ops)} 条指令）"


def read_stack_file(path: Path) -> list[int]:
    text = path.read_text(errors="replace")
    seen: list[int] = []
    for m in re.finditer(r"wasm-function\[(\d+)\]", text):
        i = int(m.group(1))
        if i not in seen:
            seen.append(i)
    return seen


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("wasm")
    ap.add_argument("funcs", nargs="*", help="函数索引")
    ap.add_argument("--stack-file", help="从抓到的 JS 栈文本里读索引")
    ap.add_argument("--no-disasm", action="store_true",
                    help="只读签名与表项，跳过反汇编（快很多）")
    args = ap.parse_args()

    path = Path(args.wasm)
    if not path.is_file():
        print(f"找不到 {path}", file=sys.stderr)
        return 2

    funcs = [int(x) for x in args.funcs if x.isdigit()]
    if args.stack_file:
        funcs = read_stack_file(Path(args.stack_file)) + funcs
    funcs = list(dict.fromkeys(funcs))

    if not funcs:
        print(__doc__)
        return 2

    x = run_xxd(path)
    if x is None:
        print("拿不到 wasm-objdump 输出，wabt 装了吗？", file=sys.stderr)
        return 3

    types = parse_types(x)
    sigs = parse_func_sigs(x)
    slots = parse_slots(x)

    # 函数表规模：能判断槽位是否越界
    total = 0
    start_slot = 0
    m = re.search(r"segment\[0\] flags=0 table=0 count=(\d+)\s*-\s*init i32=(\d+)", x)
    if m:
        total, start_slot = int(m.group(1)), int(m.group(2))

    print("=" * 72)
    print(f"文件 {path.name}  {path.stat().st_size/1048576:.1f} MB")
    print(f"函数表: {total} 项，起始槽位 {start_slot}")
    if start_slot > 0:
        print(f"  槽位 0..{start_slot-1} 无条目 —— 调这些槽位得到 null function")
    print(f"类型: {len(types)} 个，函数: {len(sigs)} 个")
    print("=" * 72)

    d = None
    if not args.no_disasm:
        print("\n[反汇编中，66 MB 的 wasm 需要几分钟...]", flush=True)
        d = disasm(path, funcs)
        if d is None:
            print("反汇编没成功，只给签名信息", file=sys.stderr)

    for idx in funcs:
        print()
        print("-" * 72)
        sig = sigs.get(idx)
        slot = slots.get(idx)
        if sig is None:
            print(f"func[{idx}]  不在函数清单里（可能是 import 或索引有误）")
            continue
        s = types.get(sig, f"type[{sig}]?")
        nparams = s.count("i32") + s.count("i64") + s.count("f32") + s.count("f64")
        print(f"func[{idx}]  {s}   (type {sig}，约 {nparams} 个参数)")
        print(f"  表槽位: {slot if slot is not None else '不在表中'}")

        # 委托桩的典型特征：函数表里出现，且旁边就是别的桩
        near = sorted(k for k in slots if abs(k - idx) <= 4)
        if len(near) > 1:
            print(f"  邻近表项: {[(k, slots[k]) for k in near]}")

        if d:
            body = extract_function(d, idx)
            if body:
                print(f"  判定: {classify(body)}")
                # 直接把 call_indirect 那几行打出来
                for ln in body:
                    if "call_indirect" in ln:
                        print(f"    >> {ln}")
                print(f"  指令数: {len(body)}")
            else:
                print("  反汇编里没找到这个函数")
        else:
            print("  （跳过了反汇编）")

    print()
    print("=" * 72)
    print("怎么读上面的结论")
    print("-" * 72)
    print("""
1. 看到"委托 Invoke 桩" -> 那是 IL2CPP 生成的 thunk，
   它的 call_indirect 类型就是上层期望的签名。

2. 调用点 type 与被调函数 type 不一致（参数个数不同）
   = function signature mismatch 的直接原因。
   跟槽位是不是空的无关，别往 null function 方向想。

3. 如果被调的那个是业务方法、参数也对得上，
   那问题在它内部某次间接调用，继续往下追那一层。

4. 跨构建比较函数索引没有意义——IL2CPP 每次重排。
   要比对就跑inspect_wasm.py 看签名和表项，那是稳定的。
""")
    return 0


if __name__ == "__main__":
    sys.exit(main())
