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
import re
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


# ---------- 从字节码声明段读资源布局 ----------
# HLSLcc 在 DecodeDeclaration 阶段就要查 RDEF 的资源绑定表（例如
# dcl_resource_structured 要用它拿 stride），缺一条就解引用空指针崩。
# Unity 的序列化表只有 CB 名和变量偏移，没有纹理/sampler 绑定，也会漏掉
# Unity 内部插入的 CB，所以这里直接读字节码自己的 dcl_* 声明 —— 权威且不猜。

DIS = None


def _disasm():
    global DIS
    if DIS is None:
        sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
        from dxbc2asm_wasmtime import Disasm
        DIS = Disasm("/tmp/dxbc2sl/renderdoc.min.wasm")
    return DIS


# RTYPE_（ShaderInfo.h）
RTYPE = {"cbuffer": 0, "tbuffer": 1, "texture": 2, "sampler": 3,
         "uav_rwtyped": 4, "structured": 5}
# REFLECT_RESOURCE_DIMENSION
TEXDIM = {"texture1d": 2, "texture2d": 4, "texture3d": 8, "texturecube": 9,
          "texture2dms": 6, "texture1darray": 3, "texture2darray": 5}


def parse_dcls(dxbc):
    """返回 (cb_sizes{槽位:字节数}, resources[(名,rtype,dim,bind,count)],
    structured[(名, stride)])。"""
    text = None
    for four, data in parse_container(dxbc):
        if four in (b"SHDR", b"SHEX"):
            text = _disasm().run(data)
            break
    if not text:
        raise RuntimeError("反汇编失败")

    cb_sizes, res, structured = {}, [], []
    for m in re.finditer(r"dcl_constantbuffer\s+cb(\d+)\[(\d+)\]", text):
        cb_sizes[int(m.group(1))] = int(m.group(2)) * 16
    for m in re.finditer(r"dcl_sampler\s+s(\d+)", text):
        res.append((f"_Sampler_s{m.group(1)}", RTYPE["sampler"], 0,
                    int(m.group(1)), 1))
    for m in re.finditer(r"dcl_resource_(\w+)\s*(?:\([^)]*\)\s*)?t(\d+)"
                         r"(?:\s*,\s*(\d+))?", text):
        kind, reg, stride = m.group(1), int(m.group(2)), m.group(3)
        if kind == "structured":
            # structured buffer 在 RDEF 里既是资源绑定又是一条 CB 记录，
            # HLSLcc 用它的 TotalSizeInBytes 当作 stride。
            name = f"_Structured_t{reg}"
            res.append((name, RTYPE["structured"], 1, reg, 1))
            structured.append((name, int(stride) if stride else 4))
        else:
            res.append((f"_Texture_t{reg}", RTYPE["texture"],
                        TEXDIM.get(kind, 4), reg, 1))
    return cb_sizes, res, structured


# ---------- RDEF 构造 ----------

def build_rdef(cbs, res, major):
    """cbs: [(名字, 字节大小, [(变量名, 偏移, dim, rows), ...]), ...]
       res: [(名字, rtype, dim, bindPoint, bindCount), ...]"""
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
    for name, *_ in res:
        intern(name)
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
    for name, rtype, dim, bind, count in res:
        # NameOffset, eType, ReturnType, Dimension, NumSamples,
        # BindPoint, BindCount, Flags —— 共 8 个 uint32（32 字节一条）。
        # 只有 SM5.1+ 才会再读 Space/RangeID，Unity 最高到 SM5.0。
        res_recs += struct.pack("<IIIIIIII", str_off[name], rtype, 0, dim, 0,
                                bind, count, 0)

    data = struct.pack("<IIIIIII", len(cbs), cb_off, len(res), res_off,
                       (major << 4), 0, 0)
    buf = bytearray(data + bytes(strs) + bytes(cb_recs))
    # 把缓冲区撑到资源绑定表的起点（各表内部偏移都是从文件头算的绝对偏移）
    buf.extend(b"\x00" * (res_off - len(buf)))
    for at, blob in var_blobs:
        buf[at:at + len(blob)] = blob
    for at, blob in type_blobs:
        buf[at:at + len(blob)] = blob
    buf.extend(res_recs)
    return bytes(buf)


CB_CTOR = re.compile(r"\b([A-Za-z_]\w*)\.(vec|ivec|uvec|bvec)([234])\(([^()]*)\)")


def normalize_glsl(text):
    """修 HLSLcc 自身的输出缺陷：需要多分量而变量表给的是标量时，它会把
    构造函数写到成员位置，生成 `CB.vec4(a, b, c, d)` 这种非法 GLSL。

    改写成合法的 `vec4(CB.a, CB.b, CB.c, CB.d)` —— 只是把前缀挪进括号，
    不改动任何一条运算。
    """
    def repl(m):
        cb, kind, n, args = m.groups()
        names = [a.strip() for a in args.split(",") if a.strip()]
        inner = ", ".join(a if a.startswith(cb + ".") else f"{cb}.{a}"
                          for a in names)
        return f"{kind}{n}({inner})"
    prev = None
    while prev != text:
        prev = text
        text = CB_CTOR.sub(repl, text)
    return text


def add_rdef(dxbc, cbs, res):
    major, _ = sm_version(dxbc)
    chunks = parse_container(dxbc)
    return rebuild(chunks, build_rdef(cbs, res, major))


# ---------- 按 blobIndex 精确定位每个变体 ----------

def blob_index_table(blob):
    """blob 头部是 (偏移, 长度, 0) 三元组表，条目数在 blob[0]。

    Unity 给每个 sub program 编了 m_BlobIndex，直接查这张表就能拿到它在
    blob 里的确切位置，不必再靠全文搜 "DXBC" 猜顺序 —— 后者无法区分
    多 Pass shader 里同一个 stage 的不同 Pass。
    """
    n = struct.unpack_from("<I", blob, 0)[0]
    if not (0 < n < 100000):
        return []
    tab = []
    for i in range(n):
        p = 4 + i * 12
        if p + 12 > len(blob):
            break
        off, ln, _ = struct.unpack_from("<III", blob, p)
        tab.append((off, ln))
    return tab


def carve_at(blob, off, ln=0, window=4096):
    """从 sub program 数据起点向后找 DXBC 容器（头部是变长序列化头，
    大 shader 的头能到几百字节，所以扫描窗口给到 4K，但绝不越过本槽位）。"""
    end = min(len(blob), off + (ln or window), off + window)
    i = blob.find(b"DXBC", off, end)
    if i < 0 or i + 32 > len(blob):
        return None
    total, nchunk = struct.unpack_from("<II", blob, i + 24)
    if not (0 < total <= len(blob) - i and 0 < nchunk <= 16):
        return None
    return blob[i:i + total]


def resolve_blocks(blocks, indices):
    """在解压出的平台块里找出 (偏移表, 数据块) 的配对。

    小 shader 是表和数据同在一块；大 shader（如 DefaultShader，8MB）则把
    偏移表单独放在一块、字节码放在另一块。这里拿真实 blobIndex 去试，
    能定位到 DXBC 的配对才算数，不猜。
    """
    tabs = [(b, blob_index_table(b)) for b in blocks]
    tabs = [(b, t) for b, t in tabs if t]
    probe = list(indices)[:24] or [1]
    for tb, tab in tabs:
        for db in blocks:
            if db is tb:
                continue
            if all(i < len(tab) and carve_at(db, *tab[i]) for i in probe if i):
                return tab, db
    for tb, tab in tabs:
        if all(i < len(tab) and carve_at(tb, *tab[i]) for i in probe if i):
            return tab, tb
    return None, None


def iter_variants(obj):
    """枚举全部 (subshader, pass, stage, keyword) 变体。

    只取 m_PlayerSubPrograms（玩家包里带的是这些；编辑器包的 m_SubPrograms
    在玩家包里是空的）。每个元素带 pass 对象，用来取该 Pass 自己的 CB 布局。
    """
    for si, sub in enumerate(obj.m_ParsedForm.m_SubShaders):
        for pi, p in enumerate(sub.m_Passes):
            inv = {v: k for k, v in dict(p.m_NameIndices).items()}
            for stage, attr in (("VS", "progVertex"), ("PS", "progFragment")):
                pr = getattr(p, attr, None)
                if pr is None:
                    continue
                for group in getattr(pr, "m_PlayerSubPrograms", []) or []:
                    for sp in group:
                        kws = [inv.get(k, str(k)) for k in sp.m_KeywordIndices]
                        yield dict(si=si, pi=pi, stage=stage, prog=p,
                                   blob_index=int(sp.m_BlobIndex), kw=kws,
                                   gpt=int(sp.m_GpuProgramType))


# ---------- 从 Unity 序列化数据取 CB 布局 ----------

def pad_vars(vars_, total_size):
    """用 _padN 填掉变量表覆盖不到的字节。

    必须按 4 字节粒度而不是 16 字节槽位来补：HLSLcc 查变量时会把 swizzle
    折算成字节偏移（cb0[25].w -> 25*16+12），只补整槽会让 .y/.z/.w 访问
    落进空洞，GetShaderVarFromOffset 返回空指针直接崩。
    """
    covered = set()
    for _, off, dim, rows in vars_:
        sz = 4 * dim * rows if rows > 1 else 4 * dim
        for b in range(off, off + sz, 4):
            covered.add(b)
    out = list(vars_)
    for s in range(0, total_size, 16):
        missing = [b for b in range(s, s + 16, 4) if b not in covered]
        if not missing:
            continue
        # 一个槽位只有 4 个 4 字节单位
        if len(missing) == 4:
            out.append((f"_pad{s}", s, 4, 1))      # 整槽空闲：一个 vec4 搞定
        else:
            for b in missing:                       # 补真实变量留下的缺口
                out.append((f"_pad{b}", b, 1, 1))
    out.sort(key=lambda t: t[1])
    return out


def cb_layout_for_pass(pass_obj, stage, cb_sizes):
    """按字节码声明的槽位组装 CB 表：槽位与大小来自 dcl_constantbuffer，
    名字与变量偏移来自 Unity 序列化（两边对不上时以字节码为准补空壳）。"""
    prog = getattr(pass_obj, stage)
    cp = prog.m_CommonParameters
    inv = {v: k for k, v in dict(pass_obj.m_NameIndices).items()}

    slot_name = {}
    for b in cp.m_ConstantBufferBindings:
        slot_name.setdefault(int(b.m_Index),
                             inv.get(b.m_NameIndex, f"cb{b.m_Index}"))
    var_by_name, order = {}, []
    for cb in cp.m_ConstantBuffers:
        nm = inv.get(cb.m_NameIndex)
        if nm is None:
            continue
        vs = [(inv.get(x.m_NameIndex, "?"), int(x.m_Index), int(x.m_Dim), 1)
              for x in cb.m_VectorParams]
        vs += [(inv.get(x.m_NameIndex, "?"), int(x.m_Index), 4,
                int(x.m_RowCount)) for x in cb.m_MatrixParams]
        vs.sort(key=lambda t: t[1])
        var_by_name[nm] = vs
        order.append(nm)

    # 序列化表里多出来的 CB（没有显式绑定槽位）补进空槽位。
    # 要与空槽的尾部对齐 —— Unity 会把 UnityPerMaterial 这类"多出来"的 CB
    # 排在最后，往前面的空槽塞会让材质变量整体错位（实测 cb5 才是
    # UnityPerMaterial，cb4 是 Unity 内部的）。
    spare = [n for n in order if n not in slot_name.values()]
    empties = [s for s in sorted(cb_sizes) if s not in slot_name]
    for i, nm in enumerate(spare):
        if i < len(empties):
            slot_name[empties[len(empties) - len(spare) + i]] = nm
    for slot in sorted(cb_sizes):
        slot_name.setdefault(slot, f"_cb{slot}")

    out = []
    for slot in sorted(cb_sizes):
        name = slot_name.get(slot, f"_cb{slot}")
        vars_ = var_by_name.get(name, [])
        declared = cb_sizes[slot]
        end = max([v[1] + 4 * v[2] * v[3] for v in vars_], default=0)
        # 取声明值与变量表推导值的较大者：HLSLcc 会对 dcl 的 N 做类型分析，
        # 大小不够就解引用到变量表外。
        size = max(declared, (end + 15) // 16 * 16 + 16)
        out.append((name, size, pad_vars(vars_, size)))
    return out


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--shader", required=True)
    ap.add_argument("--data", default=D.DEFAULT_DATA)
    ap.add_argument("--out", default="/tmp/hlslcc")
    ap.add_argument("--list", action="store_true", help="只列出变体，不转换")
    ap.add_argument("--pass", dest="only_pass", type=int, default=None,
                    help="只处理指定 Pass 下标")
    ap.add_argument("--stage", choices=["VS", "PS"], default=None,
                    help="只处理指定 stage")
    a = ap.parse_args()

    if not os.path.exists(HLSLCC_CLI):
        sys.exit(f"缺少 {HLSLCC_CLI}，先编译 HLSLcc CLI")

    name = a.shader.split("/")[-1].replace(" ", "_")
    outdir = os.path.join(a.out, name)
    os.makedirs(outdir, exist_ok=True)

    print(f"① 读取 {a.shader} ...")
    blocks, obj, srcfile = D.step1_load_shader(a.data, a.shader)
    variants = list(iter_variants(obj))
    # 每个 GpuProgramType（VS SM40 / PS SM40 / PS SM50 ...）的数据放在不同块里
    by_gpt = {}
    for v in variants:
        by_gpt.setdefault(v["gpt"], []).append(v["blob_index"])
    blocks_for = {}
    for gpt, idxs in by_gpt.items():
        blocks_for[gpt] = resolve_blocks(blocks, idxs)
    if not any(t for t, _ in blocks_for.values()):
        sys.exit(f"{srcfile}: 无法定位 D3D11 数据块（{len(blocks)} 块："
                 + ", ".join(str(len(b)) for b in blocks) + "）")
    print(f"   来自 {os.path.basename(srcfile)}，{len(blocks)} 个平台块，"
          + "  ".join(f"type{g}={'有' if t else '无'}"
                      for g, (t, _) in sorted(blocks_for.items())))

    print(f"② 按 m_BlobIndex 定位变体 ... {len(variants)} 个")

    if a.list:
        for v in variants:
            off, ln = tab[v["blob_index"]]
            i = blob.find(b"DXBC", off, off + 256)
            print(f"   sub{v['si']} pass{v['pi']} {v['stage']} "
                  f"blobIdx={v['blob_index']:>3} dxbc@{i} kw={v['kw']}")
        return

    print("③ 补 RDEF + 官方 HLSLcc 转换 ...")
    # 同一个 sub program 可能被多个 Pass 引用，按 blobIndex 去重
    done = {}
    stats = {"ok": 0, "bad_dxbc": 0, "no_cb": 0, "conv": 0, "skip": 0}
    for n, v in enumerate(variants):
        if a.only_pass is not None and v["pi"] != a.only_pass:
            continue
        if a.stage and v["stage"] != a.stage:
            continue
        bi = v["blob_index"]
        tab, blob = blocks_for.get(v["gpt"], (None, None))
        tag = f"sub{v['si']}/pass{v['pi']}/{v['stage']}/idx{bi}"
        if bi in done:
            stats["skip"] += 1
            continue
        done[bi] = True

        kwt = "_".join(v["kw"])[:40].replace("/", "_") if v["kw"] else "base"
        if tab is None or bi >= len(tab):
            stats["bad_dxbc"] += 1
            print(f"   {tag:<32} 跳过：type{v['gpt']} 无数据块")
            continue
        dx = carve_at(blob, tab[bi][0], tab[bi][1])
        if dx is None:
            stats["bad_dxbc"] += 1
            print(f"   {tag:<32} 跳过：槽位不是 DXBC（长度 {tab[bi][1]}）")
            continue
        # 资源布局直接读字节码的声明段（CB 槽位/大小 + 纹理 + sampler）
        try:
            cb_sizes, res, structured = parse_dcls(dx)
        except Exception as ex:
            stats["no_cb"] += 1
            print(f"   {tag:<32} 跳过：声明段解析失败 {ex}")
            continue
        # 变量名/偏移来自该 Pass 自己的序列化表（多 Pass 各不相同）
        cbs = cb_layout_for_pass(v["prog"],
                                 "progVertex" if v["stage"] == "VS"
                                 else "progFragment", cb_sizes)
        # structured buffer 也要占一条 CB 记录：HLSLcc 拿它的
        # TotalSizeInBytes 当 stride，所以尺寸必须精确、不补 pad
        cbs += [(nm, stride, []) for nm, stride in structured]
        # 绑定表里还必须有 CB 自己的记录 —— HLSLcc 靠名字把绑定记录映射到
        # CB 表，缺了就全部落到 psConstantBuffers[0]，cb1 的偏移会拿 cb0 的
        # 变量表去查，轻则名字错、重则越界崩。
        res = [(nm, RTYPE["cbuffer"], 1, slot, 1)
               for slot, (nm, _, _) in enumerate(cbs[:len(cb_sizes)])] + res
        kind, sm = D.stage_of(dx)
        base = f"{n:02d}_p{v['pi']}_{kind}_sm{sm.replace('.', '')}_{kwt}"
        open(f"{outdir}/{base}.dxbc", "wb").write(add_rdef(dx, cbs, res))
        r = subprocess.run([HLSLCC_CLI, f"{outdir}/{base}.dxbc", "es300"],
                           capture_output=True, text=True)
        glsl = normalize_glsl(r.stdout)
        open(f"{outdir}/{base}.glsl", "w").write(glsl)
        lines = len(glsl.splitlines())
        if r.returncode == 0 and lines > 3:
            stats["ok"] += 1
            print(f"   {base[:44]:<44} OK  glsl={lines:>4} 行")
        else:
            stats["conv"] += 1
            print(f"   {base[:44]:<44} 失败 {r.stderr.strip()[:60]}")

    print("   " + "  ".join(f"{k}={v}" for k, v in stats.items()))
    print(f"产物目录: {outdir}")


if __name__ == "__main__":
    main()
