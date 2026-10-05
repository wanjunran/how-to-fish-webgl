#!/usr/bin/env python3
"""读 WebGL.wasm 的函数表，报告"空槽位"和给定函数索引的签名。

为什么需要这个
--------------
run 37271103725 报的错换了：

    签名修复前:  RuntimeError: function signature mismatch
                 at wasm-function[103867]:0x1c91f10
    签名修复后:  RuntimeError: null function
                 at wasm-function[103863]:0x1c920ad

两个索引只差 4。用 wabt 查旧产物（签名修复前）：

    func[103863] sig=6 -> (i32) -> i32          <- 新的报错点
    func[103867] sig=2 -> (i32, i32) -> nil    <- 旧的报错点
    elem[23838] = ref.func:103863              <- 槽位只差 4
    elem[23842] = ref.func:103867

也就是说它们是同一批委托表项，相邻 4 个。
signature mismatch = 表项在，签名不符
null function     = 槽位本身指向空

前者靠手写精确签名重载修掉了；后者是编译期的事，
源码层面看不出来——33 个方法名、签名、调用点都核对无误
（读 16/16、写 17/17，无差异）。只能读产物。

而产物在构建机上。让它自己分析并把结论写进注解，
比把195 MB 下载到本地快得多（沙箱上行只有 77 KB/s）。

优先用 wabt 的 wasm-objdump（标准工具，解析正确），
没有时回退到内置的精简解析器。

用法：
    python3 tools/inspect_wasm.py <WebGL.wasm> [索引1 索引2 ...]
"""
import shutil
import subprocess
import sys
from pathlib import Path

WASM_MAGIC = b"\x00asm"


def inspect_with_wabt(path: Path, wanted: list[int]) -> str | None:
    """用 wasm-objdump 取数据。没有 wabt 或解析失败则返回 None。"""
    exe = shutil.which("wasm-objdump")
    if not exe:
        return None
    try:
        r = subprocess.run([exe, "-x", str(path)], capture_output=True,
                           text=True, timeout=900)
    except Exception:
        return None
    if r.returncode != 0 or "Section Details" not in r.stdout:
        return None

    out: list[str] = ["--- wabt wasm-objdump ---"]

    # elem 段：函数表内容
    seg_lines = [ln for ln in r.stdout.splitlines() if "segment[" in ln]
    for ln in seg_lines[:3]:
        out.append("  " + ln.strip())

    # 表项总数
    for ln in r.stdout.splitlines():
        if "count=" in ln and "segment[" in ln:
            import re
            m = re.search(r"count=(\d+)", ln)
            if m:
                out.append(f"  函数表总项数: {m.group(1)}")
            m2 = re.search(r"init i32=(-?\d+)", ln)
            if m2:
                off = int(m2.group(1))
                out.append(f"  表起始槽位: {off}")
                if off > 0:
                    out.append(f"  槽位 0..{off - 1} 无条目 -> 调这些槽位就是"
                               f" null function")
            break

    # 查询索引
    for idx in wanted:
        for ln in r.stdout.splitlines():
            if f"func[{idx}] sig=" in ln or f"func[{idx}] size=" in ln:
                out.append("  " + ln.strip())
        # 该 func 在表里的槽位
        hits = [ln.strip() for ln in r.stdout.splitlines()
                if f"ref.func:{idx}" in ln]
        if hits:
            out.append(f"    func[{idx}] 表项: {hits[0]}")
        else:
            out.append(f"    func[{idx}] 不在函数表中")

    # type 定义：把 sig=N 翻译成可读签名
    types: dict[int, str] = {}
    for ln in r.stdout.splitlines():
        s = ln.strip()
        if s.startswith("- type["):
            body = s[len("- type["):].rstrip("]")
            idx_s, _, rest = body.partition("] ")
            try:
                types[int(idx_s)] = rest
            except ValueError:
                pass
    for idx in wanted:
        for ln in r.stdout.splitlines():
            s = ln.strip()
            if s.startswith(f"- func[{idx}] sig="):
                n = s.split("sig=")[1].strip()
                sig = types.get(int(n), "?")
                out.append(f"    func[{idx}] 签名: {sig}")
                break

    return "\n".join(out)


def uleb(b: bytes, p: int) -> tuple[int, int]:
    r = s = 0
    while True:
        if p >= len(b):
            raise ValueError("LEB128 读越界")
        x = b[p]
        p += 1
        r |= (x & 0x7F) << s
        if not (x & 0x80):
            return r, p
        s += 7
        if s > 35:
            raise ValueError("LEB128 过长，文件可能已损坏")


def sleb(b: bytes, p: int) -> tuple[int, int]:
    r = s = 0
    while True:
        x = b[p]
        p += 1
        r |= (x & 0x7F) << s
        s += 7
        if not (x & 0x80):
            if x & 0x40 and s < 64:
                r |= -(1 << s)
            return r, p
        if s > 35:
            raise ValueError("LEB128 过长")


def parse(path: Path) -> dict:
    b = path.read_bytes()
    if b[:4] != WASM_MAGIC:
        raise SystemExit(f"{path} 不是 wasm 文件（魔数不对）")

    types: list[tuple[list[int], list[int]]] = []
    func_types: list[int] = []       # func index -> type index
    code_bodies: dict[int, tuple[int, int]] = {}   # func index -> (file offset, size)
    table: list[int] = []             # 函数表的实际内容
    imports = 0

    p = 8
    while p < len(b):
        sid = b[p]
        p += 1
        size, p = uleb(b, p)
        end = p + size

        if sid == 1:  # type
            n, q = uleb(b, p)
            for _ in range(n):
                form = b[q]
                q += 1
                if form != 0x60:
                    # 非 function 类型，参数/返回值布局未定义
                    np_, q = uleb(b, q)
                    q += np_
                    nr, q = uleb(b, q)
                    q += nr
                    types.append(([], []))
                    continue
                npr, q = uleb(b, q)
                params = list(b[q:q + npr])
                q += npr
                nres, q = uleb(b, q)
                res = list(b[q:q + nres])
                q += nres
                types.append((params, res))

        elif sid == 2:  # import
            n, q = uleb(b, p)
            for _ in range(n):
                ml, q = uleb(b, q); q += ml
                nl, q = uleb(b, q); q += nl
                kind = b[q]; q += 1
                if kind == 0x00:      # func
                    _, q = uleb(b, q)
                    imports += 1
                elif kind == 0x01:    # table
                    _, q = uleb(b, q)
                    lim = b[q]; q += 1
                    _, q = uleb(b, q)
                    if lim & 1:
                        _, q = uleb(b, q)
                elif kind == 0x02:    # memory
                    lim = b[q]; q += 1
                    _, q = uleb(b, q)
                    if lim & 1:
                        _, q = uleb(b, q)
                elif kind == 0x03:    # global
                    _, q = uleb(b, q)
                    q += 1
                else:
                    raise ValueError(f"未知 import kind {kind}")

        elif sid == 3:  # function
            n, q = uleb(b, p)
            for _ in range(n):
                t, q = uleb(b, q)
                func_types.append(t)

        elif sid == 9:  # element
            n, q = uleb(b, p)
            for _ in range(n):
                flags, q = uleb(b, q)
                # bit0: passive/declarative
                # bit1: offset 是显式 s33 表达式
                # bit2: 带 table index
                # bit3: 元素是 func index 列表（否则是表达式列表）
                passive = flags & 1
                explicit = flags & 2
                has_table_idx = flags & 4
                if has_table_idx:
                    _, q = uleb(b, q)
                if not passive:
                    if explicit:
                        _, q = sleb(b, q)
                    else:
                        _, q = uleb(b, q)
                if passive:
                    raise ValueError("passive element segment，本脚本未支持")
                if flags & 16:
                    # 元素是 (ref.func idx) / (ref.null) 之类，逐个读表达式。
                    # 委托表（Emscripten 的 __indirect_function_table）走的是
                    # func index 列表那条路，所以这里不实现。
                    raise ValueError("表达式形式的 element segment，本脚本未支持")
                if not (flags & 8):
                    # elemkind 字节
                    q += 1
                cnt, q = uleb(b, q)
                # 关键：cnt 可以是几十万个（这份产物是 1,040,354 字节的段）。
                # 早先的版本误以为"1 段 = 1 个元素"，把整张表读成了 1 项，
                # 于是所有函数都报告"不在表中"。别再犯这个。
                vals = []
                for i in range(cnt):
                    v, q = uleb(b, q)
                    vals.append(v)
                table.extend(vals)

        elif sid == 10:  # code
            n, q = uleb(b, p)
            for i in range(n):
                bsz, q2 = uleb(b, q)
                code_bodies[imports + i] = (q2, bsz)
                q = q2 + bsz

        p = end

    return {
        "types": types, "func_types": func_types,
        "table": table, "code": code_bodies, "imports": imports,
    }


def valtype(v: int) -> str:
    return {0x7F: "i32", 0x7E: "i64", 0x7D: "f32", 0x7C: "f64",
            0x7B: "v128", 0x70: "funcref", 0x6F: "externref"}.get(v, f"0x{v:02x}")


def main() -> int:
    if len(sys.argv) < 2:
        print(__doc__)
        return 2
    path = Path(sys.argv[1])
    wanted = [int(x) for x in sys.argv[2:] if x.isdigit()]
    if not wanted:
        # 没给索引时，报错位置无从得知，但"槽位 0 是否为空"仍值得看——
        # null function 的成因之一就是调用点落进了空槽。
        wanted = []

    wabt = inspect_with_wabt(path, wanted)
    if wabt:
        print(wabt)

    if not wanted:
        return 0

    info = parse(path)
    types = info["types"]
    fts = info["func_types"]
    table = info["table"]
    code = info["code"]
    imports = info["imports"]

    print()
    print("--- 内置解析器（wabt 不可用时的回退）---")
    print(f"文件: {path.name}  ({path.stat().st_size / 1048576:.1f} MB)")
    print(f"import 的函数数: {imports}")
    print(f"定义的函数数:   {len(fts)}  (func 索引 {imports}..{imports + len(fts) - 1})")
    print(f"类型数:         {len(types)}")
    print(f"函数表槽位数:   {len(table)}")

    for idx in wanted:
        print("-" * 60)
        if imports <= idx < imports + len(fts):
            ti = fts[idx - imports]
            pr, rs = types[ti]
            sig = f"({','.join(valtype(x) for x in pr)})->({','.join(valtype(x) for x in rs)})"
            print(f"func[{idx}]  type[{ti}]  {sig}")
            slots = [i for i, v in enumerate(table) if v == idx]
            print(f"  在函数表里的槽位: {slots if slots else '（不在表中）'}")
            if idx in code:
                off, sz = code[idx]
                body = path.read_bytes()[off:off + min(sz, 24)]
                print(f"  代码体 @ {off} ({sz} 字节) 开头: {body.hex(' ')}")
        else:
            print(f"func[{idx}]  超出定义范围（import {imports}，"
                  f"定义到 {imports + len(fts) - 1}）")
            if 0 <= idx < imports:
                print(f"  属于 import 区，可能是宿主提供的运行时函数")
    return 0


if __name__ == "__main__":
    sys.exit(main())
