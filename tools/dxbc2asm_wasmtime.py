#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""用 wasmtime 驱动 RenderDoc 的 renderdoc.min.wasm 反汇编 DXBC。

等价于 dxbc2sl/dxbc2asm.py，但用 wasmtime 代替 wasmer（wasmer 1.1 不支持 py3.11）。

用法: python3 dxbc2asm_wasmtime.py <file.dxbc> [--wasm renderdoc.min.wasm] [--raw]
      --raw  把输入当作裸 SHDR/SHEX 段（无 DXBC 容器外壳）
"""
import argparse
import struct
import sys
from wasmtime import Engine, Store, Module, Linker, WasiConfig, Instance


def load_chunks(path):
    with open(path, "rb") as f:
        d = f.read()
    i = d.find(b"DXBC")
    if i < 0:
        raise SystemExit("未找到 DXBC magic")
    base = i
    sig, csum, one, file_size, chunk_cnt = struct.unpack_from("<4s16sIII", d, i)
    offs = struct.unpack_from("<%dI" % chunk_cnt, d, i + 32)
    chunks = {}
    for o in offs:
        cs, cz = struct.unpack_from("<4sI", d, base + o)
        chunks[cs] = d[base + o + 8: base + o + 8 + cz]
    return chunks


class Disasm:
    def __init__(self, wasm_path):
        engine = Engine()
        self.store = Store(engine)
        wasi = WasiConfig()
        wasi.stdin_file = "/dev/null"
        self.store.set_wasi(wasi)
        linker = Linker(engine)
        linker.define_wasi()
        module = Module.from_file(engine, wasm_path)
        self.inst = linker.instantiate(self.store, module)
        self.e = self.inst.exports(self.store)

    def _mem(self):
        return self.e["memory"]

    def run(self, data: bytes) -> str:
        mem = self._mem()
        malloc = self.e["malloc"]
        free = self.e["free"]
        strlen = self.e["strlen"]
        get = self.e["DXBCBytecode_Program_GetDisassembly"]
        ptr = malloc(self.store, len(data))
        try:
            mem.write(self.store, data, ptr)
            tptr = get(self.store, ptr, len(data))
            if not tptr:
                return ""
            tl = strlen(self.store, tptr)
            out = bytes(mem.read(self.store, tptr, tptr + tl)).decode("utf-8", "replace")
            free(self.store, tptr)
        finally:
            free(self.store, ptr)
        return out


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("inputs", nargs="+")
    ap.add_argument("--wasm", default="/tmp/dxbc2sl/renderdoc.min.wasm")
    ap.add_argument("--raw", action="store_true")
    ap.add_argument("-o", "--out")
    a = ap.parse_args()

    d = Disasm(a.wasm)
    for inp in a.inputs:
        if a.raw:
            data = open(inp, "rb").read()
        else:
            chunks = load_chunks(inp)
            data = chunks.get(b"SHDR") or chunks.get(b"SHEX")
            if data is None:
                print(f"[跳过] {inp}: 无 SHDR/SHEX", file=sys.stderr)
                continue
        try:
            text = d.run(data)
        except Exception as e:
            print(f"[失败] {inp}: {type(e).__name__} {e}", file=sys.stderr)
            continue
        if a.out and len(a.inputs) == 1:
            open(a.out, "w").write(text)
            print(f">> {a.out}: {len(text.splitlines())} 行")
        else:
            print(f"===== {inp} =====")
            print(text)


if __name__ == "__main__":
    main()
