#!/usr/bin/env python3
"""用 Unity 官方 HLSLcc 把原版 DXBC 字节码交叉编译成 GLSL ES 3.0（WebGL 目标语言）。

与 tools/decompile_unity_shader.py（自拼的 dxbc2sl 路线）的区别：
  这里调用 Unity 官方的 HLSLcc —— README 明确写着 "This library is used to
  generate all shaders in Unity for OpenGL, OpenGL ES 3.0+, Metal and Vulkan"，
  也就是 Unity 自己生成 WebGL shader 用的那个转换器，输出与官方 WebGL 管线同源。

关键前提：Unity 的 blob 里 sub program 只带 ISGN/OSGN/SHDR，没有 RDEF。
HLSLcc 在数据类型分析阶段要靠 RDEF 知道常量缓冲区布局，缺了会崩。
所以本脚本先把 Unity 序列化数据里的 CB 布局（变量偏移/分量数）机械地
拼成 RDEF 补进容器，再交给 HLSLcc。整个过程不改写任何一条 shader 指令。

用法:
    python3 tools/hlslcc_decompile.py --shader "Shader Graphs/SkyboxShader" --out /tmp/hlslcc
产物: <out>/<名字>/<序号>_<PS|VS>_sm<版本>.{dxbc,glsl}
"""
import argparse
import os
import struct
import subprocess
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import decompile_unity_shader as D  # 复用 step1/step2（读 blob + carve DXBC)

HLSLCC_CLI = "/tmp/hlslcc_cli"

# D3D_SHADER_VARIABLE_CLASS
SVC_SCALAR, SVC_VECTOR, SVC_MATRIX_ROWS, SVC_MATRIX_COLUMNS = 0, 1, 2, 3
SVT_FLOAT = 3

FOURCC_RDEF = b"RDEF"


# ---------- DXBC 容器读写 ----------

def parse_container(d):
    assert d[:4] == b"DXBC", "不是 DXBC 容器"
    total, n = struct.unpack_from("<II", d, 24)
    offs = struct.unpack_from("<%dI" % n, d, 32)
    chunks = []
    for o in offs:
        four = d[o:o + 4]
        size = struct.unpack_from("<I", d, o + 4)[0]
        chunks.append((four, d[o + 8:o + 8 + size]))
    return chunks


def rebuild(chunks, rdef_data):
    all_chunks = list(chunks) + [(FOURCC_RDEF, rdef_data)]
    n = len(all_chunks)
    cursor = 32 + 4 * n
    offs, body = [], bytearray()
    for four, data in all_chunks:
        offs.append(cursor)
        body += four + struct.pack("<I", len(data)) + data
        cursor += 8 + len(data)
    total = 32 + 4 * n + len(body)
    header = b"DXBC" + b"\x00" * 16 + struct.pack("<III", 0, total, n)
    return header + struct.pack("<%dI" % n, *offs) + bytes(body)


def sm_version(d):
    """返回 (major, minor)，用于决定变量记录长度（SM5 多 4 个字段）。"""
    _, n = struct.unpack_from("<II", d, 24)
    for o in struct.unpack_from("<%dI" % n, d, 32):
        if d[o:o + 4] in (b"SHDR", b"SHEX"):
            v = struct.unpack_from("<I", d, o + 8)[0]
            return (v >> 4) & 0xF, v & 0xF
    return 5, 0


# ---------- RDEF 构造 ----------

def build_rdef(cbs, major):
    """cbs: [(名字, 字节大小, [(变量名, 偏移, dim, rows), ...]), ...]"""
    strs = bytearray(b"\x00")            # 偏移 0 留空串（creator 用）
    str_off = {}

    def intern(s):
        if s not in str_off:
            str_off[s] = 28 + len(strs)
            strs.extend(s.encode() + b"\x00")
        return str_off[s]

    for name, _, _ in cbs:
        intern(name)
    for _, _, vs in cbs:
        for vname, _, _, _ in vs:
            intern(vname)
    while len(strs) % 4:
        strs.append(0)

    head = 28
    pos = head + len(strs)

    cb_off = pos
    cb_recs = bytearray()
    var_blobs, type_blobs = [], []
    for name, size, vs in cbs:
        var_off = 0
        if vs:
            # 变量区与类型区按 CB 顺序排列，偏移稍后回填
            var_off = 0  # placeholder
        cb_recs += struct.pack("<IIIIII", str_off[name], len(vs), 0, size, 0, 0)

    # 先算出变量区/类型区的起始（紧跟 CB 记录之后）
    var_base = cb_off + len(cb_recs)
    # 变量记录：SM4 = 6 个 uint32，SM5 = 10 个
    var_stride = 24 if major < 5 else 40
    type_base = var_base + sum(len(vs) * var_stride for _, _, vs in cbs)
    type_stride = 16

    # 重排：为每个 CB 记录回填真正的 VarOffset
    cb_recs = bytearray()
    vcur, tcur = var_base, type_base
    for name, size, vs in cbs:
        cb_recs += struct.pack("<IIIIII", str_off[name], len(vs),
                               vcur if vs else 0, size, 0, 0)
        for vname, off, dim, rows in vs:
            if dim == 1:
                cls, cols = SVC_SCALAR, 1
            elif rows > 1:
                cls, cols = SVC_MATRIX_COLUMNS, dim
            else:
                cls, cols = SVC_VECTOR, dim
            vsize = 4 * dim * rows if rows > 1 else 4 * dim
            rec = struct.pack("<IIIIII", str_off[vname], off, vsize, 0, tcur, 0)
            if major >= 5:
                rec += struct.pack("<IIII", 0, 0, 0, 0)
            var_blobs.append((vcur, rec))
            type_blobs.append((tcur, struct.pack("<HHHHHHI", cls, SVT_FLOAT,
                                                 rows, cols, 1, 0, 0)))
            vcur += var_stride
            tcur += type_stride

    res_off = tcur
    res_recs = bytearray()
    for i, (name, _, _) in enumerate(cbs):
        res_recs += struct.pack("<IIIIIIII", str_off[name], 0, 0, 0, 0, i, 1, 0)

    data = struct.pack("<IIIIIII", len(cbs), cb_off, len(cbs), res_off, 0, 0, 0)
    buf = bytearray(data + bytes(strs) + bytes(cb_recs))
    buf.extend(b"\x00" * (var_base + (type_base - var_base) + (res_off - type_base)))
    for at, blob in var_blobs:
        buf[at:at + len(blob)] = blob
    for at, blob in type_blobs:
        buf[at:at + len(blob)] = blob
    buf.extend(res_recs)
    return bytes(buf)


def add_rdef(dxbc, cbs):
    major, _ = sm_version(dxbc)
    chunks = parse_container(dxbc)
    return rebuild(chunks, build_rdef(cbs, major))


# ---------- 从 Unity 序列化数据取 CB 布局 ----------

def pad_vars(vars_, total_size):
    """用 _padN 填掉变量表覆盖不到的 16 字节槽位。"""
    covered = set()
    for _, off, dim, rows in vars_:
        sz = 4 * dim * rows if rows > 1 else 4 * dim
        for b in range(off, off + sz):
            covered.add(b - b % 16)
    out = list(vars_)
    for s in range(0, total_size, 16):
        if s not in covered:
            out.append((f"_pad{s}", s, 4, 1))
    out.sort(key=lambda t: t[1])
    return out


def stage_layout(pass_obj, stage):
    """返回 [(cb名, size, [(变量名, 偏移, dim, rows), ...]), ...]"""
    prog = getattr(pass_obj, stage)
    cp = prog.m_CommonParameters
    inv = {v: k for k, v in dict(pass_obj.m_NameIndices).items()}
    bind = {}
    for b in cp.m_ConstantBufferBindings:
        # m_Index 是该 CB 绑定的槽位(cbX)，m_NameIndex 指向名字表
        bind[b.m_Index] = inv.get(b.m_NameIndex, f"cb{b.m_Index}")
    out = []
    for i, cb in enumerate(cp.m_ConstantBuffers):
        name = inv.get(cb.m_NameIndex, bind.get(i, f"cb{i}"))
        # 槽位即数组下标；Unity 的 m_ConstantBuffers 顺序与 bind 槽位一致
        slot = None
        for k, v in bind.items():
            if v == name and (slot is None or k < slot):
                slot = k
        vars_ = []
        for x in cb.m_VectorParams:
            vars_.append((inv.get(x.m_NameIndex, "?"), x.m_Index, x.m_Dim, 1))
        for x in cb.m_MatrixParams:
            vars_.append((inv.get(x.m_NameIndex, "?"), x.m_Index,
                          4, x.m_RowCount))
        vars_.sort(key=lambda t: t[1])
        # CB 大小：变量末尾向上取 16 的倍数，再多留一个 float4。
        # 多留的那个是因为 dcl_constantbuffer cbX[N] 里的 N 会被 HLSLcc 当作
        # 偏移 N*16 去做类型分析，落在变量表外就会解引用空指针崩溃。
        end = max([v[1] + 4 * v[2] * v[3] for v in vars_], default=0)
        size = (end + 15) // 16 * 16 + 16
        vars_ = pad_vars(vars_, size)
        out.append((slot if slot is not None else i, name, size, vars_))
    out.sort(key=lambda t: t[0])
    return [(name, size, vars_) for _, name, size, vars_ in out]


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--shader", required=True)
    ap.add_argument("--data", default=D.DEFAULT_DATA)
    ap.add_argument("--out", default="/tmp/hlslcc")
    a = ap.parse_args()

    if not os.path.exists(HLSLCC_CLI):
        sys.exit(f"缺少 {HLSLCC_CLI}，先编译 HLSLcc CLI")

    name = a.shader.split("/")[-1].replace(" ", "_")
    outdir = os.path.join(a.out, name)
    os.makedirs(outdir, exist_ok=True)

    print(f"① 读取 {a.shader} ...")
    blob, obj, srcfile = D.step1_load_shader(a.data, a.shader)
    print(f"   来自 {os.path.basename(srcfile)}，blob {len(blob)} 字节")

    print("② carve DXBC ...")
    dxbcs = D.step2_carve(blob)
    print(f"   {len(dxbcs)} 个容器")

    p = obj.m_ParsedForm.m_SubShaders[0].m_Passes[0]
    layouts = {"VS": stage_layout(p, "progVertex"),
               "PS": stage_layout(p, "progFragment")}
    for k, v in layouts.items():
        print(f"   {k} 布局: " + ", ".join(f"{n}({len(vs)}变量)" for n, _, vs in v))

    print("③ 补 RDEF + 官方 HLSLcc 转换 ...")
    for idx, dx in enumerate(dxbcs):
        kind, sm = D.stage_of(dx)
        if kind not in layouts:
            continue
        cbs = layouts[kind]
        if not cbs:
            print(f"   {idx:02d} {kind}: 无 CB 布局，跳过")
            continue
        with_rdef = add_rdef(dx, cbs)
        base = f"{idx:02d}_{kind}_sm{sm.replace('.', '')}"
        open(f"{outdir}/{base}.dxbc", "wb").write(with_rdef)
        r = subprocess.run([HLSLCC_CLI, f"{outdir}/{base}.dxbc", "es300"],
                           capture_output=True, text=True)
        lines = r.stdout.splitlines()
        open(f"{outdir}/{base}.glsl", "w").write(r.stdout)
        status = "OK " if r.returncode == 0 else "失败"
        print(f"   {base:<20} {status} glsl={len(lines):>4} 行"
              + (f"  {r.stderr.strip()[:90]}" if r.returncode else ""))
    print(f"产物目录: {outdir}")


if __name__ == "__main__":
    main()
