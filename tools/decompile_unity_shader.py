#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""Unity Shader 反编译流水线（通用工具）。

从原版 Unity 玩家包里恢复 shader 的可读 HLSL。流水线：

  ① UnityPy 读取 Shader 资产（含 LZ4 压缩的 m_SubProgramBlob）
  ② LZ4 解压 -> carve 出全部 DXBC 容器（D3D11 字节码）
  ③ RenderDoc 的 renderdoc.min.wasm 反汇编（wasmtime 驱动，替代 wasmer）
  ④ dxbc2sl/dxasm2sl.py 把 SM4 汇编翻译成 HLSL
  ⑤ dump Unity 序列化的 ConstantBufferBindings/VectorParams 偏移表，
     供后续把 cb0[n]/cb1[n] 机械替换成真实 uniform 名

用法:
  python3 tools/decompile_unity_shader.py --shader "Shader Graphs/WaterShader" \
      [--data "/path/How to Fish_Data"] [--out /tmp/dec]

产物（--out 目录下）:
  <name>/00_VS_*.dxbc ...          原始字节码容器
  <name>/vs_*.asm / ps_*.asm       SM4 汇编
  <name>/vs_*.hlsl / ps_*.hlsl     可读 HLSL
  <name>/constants.txt             cbuffer 偏移->uniform 名映射（人工核对用）
"""
import argparse
import io
import os
import struct
import subprocess
import sys

DEFAULT_DATA = "/workspace/extracted/How to Fish/How to Fish/How to Fish_Data"
REPO = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
DXBC2SL = "/tmp/dxbc2sl"
WASM = os.path.join(DXBC2SL, "renderdoc.min.wasm")
DISASM_TOOL = os.path.join(REPO, "tools", "dxbc2asm_wasmtime.py")

PTYPE = {0: "PS", 1: "VS", 2: "GS", 3: "HS", 4: "DS", 5: "CS"}


def step1_load_shader(data_dir, want):
    import UnityPy
    import lz4.block
    UnityPy.config.FALLBACK_UNITY_VERSION = "6000.4.4f1"
    for f in sorted(os.listdir(data_dir)):
        if not f.endswith(".assets"):
            continue
        p = os.path.join(data_dir, f)
        try:
            env = UnityPy.load(p)
        except Exception:
            continue
        for o in env.objects:
            if int(o.type) != 48:
                continue
            try:
                d = o.read()
            except Exception:
                continue
            if getattr(d.m_ParsedForm, "m_Name", "") == want:
                comp = bytes(d.compressedBlob)
                dl = d.decompressedLengths
                if isinstance(dl, list) and dl and isinstance(dl[0], (list, tuple)):
                    dec = dl[0][0]
                else:
                    dec = int(dl[0]) if not isinstance(dl, list) else int(dl[0])
                blob = lz4.block.decompress(comp, uncompressed_size=dec)
                return blob, d, p
    raise SystemExit(f"未找到 shader: {want}")


def step2_carve(blob):
    out, i = [], 0
    while True:
        i = blob.find(b"DXBC", i)
        if i < 0:
            break
        if i + 32 > len(blob):
            break
        total, nchunk = struct.unpack_from("<II", blob, i + 24)
        if not (0 < total <= len(blob) - i and 0 < nchunk <= 16):
            i += 1
            continue
        offs = struct.unpack_from("<%dI" % nchunk, blob, i + 32)
        if not all(0 < o < total for o in offs):
            i += 1
            continue
        out.append(blob[i:i + total])
        i += total
    return out


def stage_of(dxbc):
    total, nchunk = struct.unpack_from("<II", dxbc, 24)
    offs = struct.unpack_from("<%dI" % nchunk, dxbc, 32)
    for o in offs:
        fourcc = dxbc[o:o + 4]
        if fourcc in (b"SHDR", b"SHEX"):
            v = struct.unpack_from("<I", dxbc, o + 8)[0]
            return PTYPE.get((v >> 16) & 0x1F, "?"), f"{(v >> 4) & 0xF}.{v & 0xF}"
    return "?", "?"


def step34_disasm_translate(dxbcs, outdir):
    sys.path.insert(0, os.path.join(DXBC2SL))
    from wasmtime import Engine, Store, Module, Linker, WasiConfig
    store = Store(Engine())
    wasi = WasiConfig()
    wasi.stdin_file = "/dev/null"
    store.set_wasi(wasi)
    linker = Linker(store.engine)
    linker.define_wasi()
    inst = linker.instantiate(store, Module.from_file(store.engine, WASM))
    e = inst.exports(store)
    mem = e["memory"]

    def disasm(data):
        ptr = e["malloc"](store, len(data))
        try:
            mem.write(store, data, ptr)
            t = e["DXBCBytecode_Program_GetDisassembly"](store, ptr, len(data))
            if not t:
                return ""
            n = e["strlen"](store, t)
            s = bytes(mem.read(store, t, t + n)).decode("utf-8", "replace")
            e["free"](store, t)
        finally:
            e["free"](store, ptr)
        return s

    results = []
    for idx, blob in enumerate(dxbcs):
        kind, sm = stage_of(blob)
        fourcc = None
        total, nchunk = struct.unpack_from("<II", blob, 24)
        for o in struct.unpack_from("<%dI" % nchunk, blob, 32):
            if blob[o:o + 4] in (b"SHDR", b"SHEX"):
                fourcc = blob[o:o + 4]
                sz = struct.unpack_from("<I", blob, o + 4)[0]
                prog = blob[o + 8:o + 8 + sz]
        if fourcc is None:
            continue
        base = f"{idx:02d}_{kind}_sm{sm.replace('.','')}"
        open(f"{outdir}/{base}.dxbc", "wb").write(blob)
        asm = disasm(prog)
        open(f"{outdir}/{base}.asm", "w").write(asm)
        hlsl = ""
        try:
            r = subprocess.run([sys.executable, os.path.join(DXBC2SL, "dxasm2sl.py"),
                                f"{outdir}/{base}.asm", "hlsl"],
                               capture_output=True, text=True, timeout=120, cwd=DXBC2SL)
            hlsl = r.stdout
        except Exception as ex:
            hlsl = f"// translate failed: {ex}"
        open(f"{outdir}/{base}.hlsl", "w").write(hlsl)
        results.append((base, len(asm.splitlines()), len(hlsl.splitlines())))
        print(f"    {base:<22} asm={len(asm.splitlines()):>4} 行  hlsl={len(hlsl.splitlines()):>4} 行")
    return results


def step5_constants(shader_obj, outdir, want):
    p = shader_obj.m_ParsedForm.m_SubShaders[0].m_Passes[0]
    inv = {v: k for k, v in dict(p.m_NameIndices).items()}
    lines = [f"# {want}", ""]
    for stage in ("progVertex", "progFragment"):
        prog = getattr(p, stage)
        cp = prog.m_CommonParameters
        lines.append(f"## {stage}")
        for b in cp.m_ConstantBufferBindings:
            lines.append(f"  BIND dxcb{b.m_Index} -> {inv.get(b.m_NameIndex, '?')}")
        for cb in cp.m_ConstantBuffers:
            lines.append(f"  CBUFFER {inv.get(cb.m_NameIndex, '?')}:")
            for x in cb.m_VectorParams:
                lines.append(f"    @{x.m_Index:<5} {inv.get(x.m_NameIndex, '?'):<24} dim={x.m_Dim}")
            for x in cb.m_MatrixParams:
                lines.append(f"    @{x.m_Index:<5} {inv.get(x.m_NameIndex, '?'):<24} [matrix] rows={x.m_RowCount}")
        lines.append("")
    open(os.path.join(outdir, "constants.txt"), "w").write("\n".join(lines))
    print("\n".join(lines))


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--shader", required=True)
    ap.add_argument("--data", default=DEFAULT_DATA)
    ap.add_argument("--out", default="/tmp/dec")
    a = ap.parse_args()

    name = a.shader.split("/")[-1].replace(" ", "_")
    outdir = os.path.join(a.out, name)
    os.makedirs(outdir, exist_ok=True)

    print(f"① 读取 {a.shader} ...")
    blob, obj, srcfile = step1_load_shader(a.data, a.shader)
    print(f"   来自 {os.path.basename(srcfile)}, blob {len(blob)} 字节")

    print("② carve DXBC ...")
    dxbcs = step2_carve(blob)
    print(f"   {len(dxbcs)} 个容器")

    print("③④ 反汇编 + 翻译 HLSL ...")
    step34_disasm_translate(dxbcs, outdir)

    print("⑤ 常量映射表 ...")
    step5_constants(obj, outdir, a.shader)
    print(f"\n产物目录: {outdir}")


if __name__ == "__main__":
    main()
