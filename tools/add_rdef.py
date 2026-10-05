#!/usr/bin/env python3
"""给 Unity 导出的 DXBC 容器补一个 RDEF（资源反射）chunk。

背景：Unity 的 shader blob 里 sub program 只带 ISGN/OSGN/SHDR，不含 RDEF；
Unity 官方 HLSLcc 在数据类型分析阶段（ShaderInfo::GetShaderVarFromOffset）
依赖 RDEF 提供的常量缓冲区表，缺失时会解引用空指针而段错误。

本脚本按 HLSLcc reflect.cpp 中 ReadResources / ReadConstantBuffer /
ReadResourceBinding 的字段布局，纯机械地拼出 RDEF 并重建容器。
不改写任何 shader 指令 —— SHDR 内容原样保留。

用法:
    python3 tools/add_rdef.py <in.dxbc> <out.dxbc> [cb名 cb名 ...]
CB 名按 bind point 顺序给出；可省略（默认 $Globals / UnityPerMaterial）。
"""
import struct
import sys

FOURCC_RDEF = b"RDEF"
DEFAULT_CBS = [("$Globals", 2048), ("UnityPerMaterial", 256)]
DEFAULT_CB_SIZE = 256


def parse_container(d):
    """解析 DXBC 容器，返回 (chunk列表[(fourcc, data)], ...)"""
    assert d[:4] == b"DXBC", "不是 DXBC 容器"
    total, n = struct.unpack_from("<II", d, 24)
    offs = struct.unpack_from("<%dI" % n, d, 32)
    chunks = []
    for o in offs:
        four = d[o:o + 4]
        size = struct.unpack_from("<I", d, o + 4)[0]
        chunks.append((four, d[o + 8:o + 8 + size]))
    return chunks


def build_rdef(cb_list):
    """按 HLSLcc 期望的布局拼 RDEF chunk 数据。

    cb_list: [(名字, 字节大小), ...]，按 bind point 顺序。
    """
    cb_names = [n for n, _ in cb_list]
    # 头部 6 个字段 + creator string offset（第 7 个，HLSLcc 不读但保持对齐）
    # 布局: NumCB, CBOffset, NumRes, ResOffset, ShaderModel, CompileFlags, CreatorStrOff
    # 之后紧跟字符串区，再是 CB 记录数组、资源绑定记录数组。
    strs = bytearray(b"\x00")          # 偏移 0 是空串（给 creator 用）
    name_off = {}
    for n in cb_names:
        name_off[n] = 28 + len(strs)   # 28 = RDEF 数据内字符串区起点
        strs += n.encode() + b"\x00"
    while len(strs) % 4:
        strs += b"\x00"

    head_len = 28
    cb_off = head_len + len(strs)
    # 注意：ReadConstantBuffer 读 6 个 uint32（NameOffset/VarCount/VarOffset/
    # TotalSizeInBytes/Flags/BufferType），CB 记录是 24 字节而非 12。
    cb_recs = b"".join(
        struct.pack("<IIIIII", name_off[n], 0, 0, size, 0, 0)
        for n, size in cb_list
    )
    res_off = cb_off + len(cb_recs)
    res_recs = b"".join(
        struct.pack(
            "<IIIIIIII",
            name_off[n],  # NameOffset（必须与 CB 同名，HLSLcc 靠名字把两者映射起来）
            0,            # eType = RTYPE_CBUFFER
            0,            # ReturnType
            0,            # Dimension
            0,            # NumSamples
            i,            # BindPoint
            1,            # BindCount
            0,            # Flags
        )
        for i, n in enumerate(cb_names)
    )
    # 注意：SM5.1 及以上还要多 Space/RangeID 两个字段；Unity 这里用的是 SM4.0/5.0 容器，不加。
    data = struct.pack("<IIIIIII", len(cb_names), cb_off, len(cb_names), res_off,
                       0, 0, 0)
    assert len(data) == head_len, len(data)
    return data + bytes(strs) + cb_recs + res_recs


def rebuild(chunks, rdef_data):
    """重建容器：header + 新的 offsets 数组 + 原 chunks + RDEF chunk（追加在末尾）。"""
    all_chunks = list(chunks) + [(FOURCC_RDEF, rdef_data)]
    n = len(all_chunks)
    off_arr_len = 4 * n
    cursor = 32 + off_arr_len
    offs, body = [], bytearray()
    for four, data in all_chunks:
        offs.append(cursor)
        body += four + struct.pack("<I", len(data)) + data
        cursor += 8 + len(data)
    header = b"DXBC" + b"\x00" * 16 + struct.pack("<III", 0, 0, n)
    assert len(header) == 32, len(header)
    total = 32 + off_arr_len + len(body)
    header = b"DXBC" + b"\x00" * 16 + struct.pack("<III", 0, total, n)
    return header + struct.pack("<%dI" % n, *offs) + bytes(body)


def main():
    if len(sys.argv) < 3:
        print(__doc__)
        return 2
    src, dst = sys.argv[1], sys.argv[2]
    # 每项写成 "名字" 或 "名字:字节大小"
    raw = sys.argv[3:] or [n for n, _ in DEFAULT_CBS]
    cb_list = []
    for item in raw:
        if ":" in item:
            n, s = item.rsplit(":", 1)
            cb_list.append((n, int(s)))
        else:
            cb_list.append((item, DEFAULT_CB_SIZE))
    d = open(src, "rb").read()
    chunks = parse_container(d)
    rdef = build_rdef(cb_list)
    cb_names = [n for n, _ in cb_list]
    out = rebuild(chunks, rdef)
    open(dst, "wb").write(out)
    print(f"{src}: {len(chunks)} chunks -> {dst}: {len(chunks)+1} chunks "
          f"(+RDEF {len(rdef)}B, total {len(out)}B) CB={cb_names}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
