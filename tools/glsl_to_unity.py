#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""把 Unity 官方 HLSLcc 产出的 GLSL ES 3.0 机械翻译成 Unity 可用的 HLSL .shader。

输入是 tools/hlslcc_decompile.py 的产物目录（<out>/<Name>/NN_pP_<STAGE>_smYY_*.glsl）。

本脚本不做任何"创作"—— 没有一行 shader 逻辑是写出来的，正文里的每个
语句、每个运算、每个常量都逐字来自 Unity 官方 HLSLcc 的输出。所做的全部
是 GLSL -> HLSL 的**等价替换**：

  1. 类型名 / 内建函数用 #define 宏一一对应（vec3->float3、mix->lerp、
     inversesqrt->rsqrt ...），正文一个字符都不改；
  2. 采样走宏：GLSL 的 texture(tex, uv[, bias]) / textureLod(tex, uv, lod)
     / texelFetch(tex, coord) 在 HLSL 里必须变成 (tex).Sample / SampleBias /
     SampleLevel / Load，参数原样透传，宏用 token 拼接 sampler##tex 拿到
     对应的 sampler 变量；
  3. 常量缓冲区成员（名字与类型）直接来自 HLSLcc 输出的 uniform 块 —— 那
     是从原版 DXBC 的 RDEF 逐字节还原出来的，等于原版 shader 自己的布局。
     所有 _padN 空洞直接丢弃：Unity 重新编译时按**名字**绑定引擎数据，
     不依赖我们复原的字节偏移；
  4. _cbN 这种"名字未知且一个真实变量都没有"的空壳块整体丢弃。实测 3998
     个变体里 _cb0.._cb8 的真实成员数恒为 0，无任何代码引用；
  5. 矩阵：GLSL 的 mat4x4 是列主序（m[k] 取的是列），HLSL 的 float4x4 是行
     索引。代码里只出现「分量访问」和「分量乘」两种形态，机械插入
     transpose() 局部量即可把列索引换成行索引，语义完全等价；
  6. 顶点属性 / 插值器按 HLSLcc 的命名（in_POSITION0 / vs_INTERPn）映射语义，
     插值器索引直接沿用名字里的数字，VS 与 PS 天然对齐。

WebGL2 = GLES 3.0，不支持 SSBO（需 3.1）。--pick 只挑不含 SSBO 的变体，
这是唯一能在 WebGL2 上编译通过的子集。

用法:
  python3 tools/glsl_to_unity.py --dir /tmp/hlslcc_all/SkyboxShader \
      --shader "Shader Graphs/SkyboxShader" --out Assets/Shader/Restored
"""
import argparse
import json
import os
import re
import sys

# ---------------------------------------------------------------- GLSL 类型
TYPES = {
    "vec2": "float2", "vec3": "float3", "vec4": "float4",
    "ivec2": "int2", "ivec3": "int3", "ivec4": "int4",
    "uvec2": "uint2", "uvec3": "uint3", "uvec4": "uint4",
    "bvec2": "bool2", "bvec3": "bool3", "bvec4": "bool4",
    "mat2": "float2x2", "mat3": "float3x3", "mat4": "float4x4",
    "mat2x2": "float2x2", "mat2x3": "float2x3", "mat2x4": "float2x4",
    "mat3x2": "float3x2", "mat3x3": "float3x3", "mat3x4": "float3x4",
    "mat4x2": "float4x2", "mat4x3": "float4x3", "mat4x4": "float4x4",
}
TYPE_RE = re.compile(r"\b(" + "|".join(sorted(TYPES, key=len, reverse=True))
                     + r")\b")

SAMPLER_MACRO = {"2D": "TEXTURE2D", "3D": "TEXTURE3D", "Cube": "TEXTURECUBE"}

# GLSL ES 3.0 成立、逐位保留的宏 -> 取 #if 分支
TAKE_IF = {"GL_EXT_shader_texture_lod", "GL_ARB_shader_storage_buffer_object",
           "GL_ARB_shader_image_load_store"}

PAD_RE = re.compile(
    r"^(vec\d|ivec\d|uvec\d|bvec\d|float|int|uint|bool|mat\w*)\s+_pad\d+$")


# ------------------------------------------------------------------ 解析
class Parsed:
    def __init__(self):
        self.blocks = []      # [(块名, [(类型, 变量名)])]，pad 已剔除
        self.tex = []         # [(纹理名, 宏名)]
        self.zcmp = []        # 参与 shadow 深度比较的纹理名
        self.ins = []         # [(名, 类型)]
        self.outs = []# [(名, 类型)]
        self.out_mod = {}    # {名: 修饰符}，GLSL 的 flat -> HLSL nointerpolation
        self.ps_outs = []     # [(名, 类型, location)]
        self.globals = []     # [(类型, 名, 数组维度)]
        self.helpers = []     # HLSLcc 自带的辅助函数（op_not 等）
        self.ssbo = []        # SSBO 缓冲变量名（WebGL2 不支持，仅用于跳过）
        self.body = ""

    def uniform_vars(self, pads=()):
        """跨块去重后的 uniform 变量。默认丢掉所有 _padN；只有正文真的
        引用到的 pad 才需要一起声明（否则编译不过）。"""
        want = set(pads)
        seen, out = set(), []
        for _b, members in self.blocks:
            for t, n in members:
                if n in seen:
                    continue
                if PAD_RE.match(f"{t} {n}") and n not in want:
                    continue
                seen.add(n)
                out.append((t, n))
        return out


def parse_glsl(text):
    p = Parsed()
    lines = text.splitlines()

    body_start = None
    for idx, ln in enumerate(lines):
        if ln.strip().startswith("void main"):
            body_start = idx
            break
    head = lines[:body_start] if body_start is not None else lines
    p.body = "\n".join(lines[body_start:]) if body_start is not None else ""

    in_block = None
    in_helper = False
    in_ssbo_struct = False

    for ln in head:
        s = ln.strip()
        if not s or s.startswith("#") or s.startswith("precision "):
            continue
        if s.startswith("layout(std430"):
            p.ssbo.append(s)
            continue
        if in_ssbo_struct and s.startswith("}"):
            in_ssbo_struct = False
            continue
        if s.startswith("struct ") and s.endswith("_type {"):
            in_ssbo_struct = True
            continue

        # ---- uniform 块（跨行）----
        m = re.match(r"^layout\(std140\) uniform (\w+) \{$", s)
        if m:
            in_block = (m.group(1), [])
            continue
        if in_block and re.match(r"^\}\s*_?\w*\s*;$", s):
            p.blocks.append((in_block[0], in_block[1]))
            in_block = None
            continue
        if in_block:
            m = re.match(r"^(\w+)\s+(\w+);$", s)
            if m:
                # _padN 是为填 RDEF 空洞造的空壳。但如果 HLSLcc 的正文真的
                # 引用了它，说明该槽位的真实变量没对上（RDEF 布局有缺口），
                # 这时必须照原类型声明出来，否则编译不过。先全部保留。
                in_block[1].append((m.group(1), m.group(2)))
            continue

        # ---- 采样器 ----
        m = re.match(r"^uniform\s+(?:\w+\s+)?sampler(\w+)\s+(\w+);$", s)
        if m:
            if m.group(1) == "2DShadow":
                p.zcmp.append(m.group(2))       # hlslcc_zcmp_<tex>
            elif m.group(1) in SAMPLER_MACRO:
                p.tex.append((m.group(2), SAMPLER_MACRO[m.group(1)]))
            continue

        # ---- IO ----
        #
        # 插值限定符（flat / smooth / noperspective / centroid）用「先剥再用」
        # 而不是塞进正则分支。实测：把多个分支写进同一个可选组后，Python re
        # 在裸 out / in 上会回溯失败 —— 限定符组能匹配空，但后续的类型/变量
        # 两组仍会误把精度限定符当类型，整行匹配不上。分支越多越脆
        # （单 token 全对、任意组合全错），所以干脆不写进正则。
        #
        # 漏掉这一项的代价：instancing 变体里的
        #     flat out highp uint vs_CUSTOM_INSTANCE_ID0;
        # 整行被丢弃，Varyings 少一个成员而 PS 仍在引用它 —— 编译期报未声明。
        if s.startswith(("flat ", "smooth ", "noperspective ", "centroid ")):
            qual, s = s.split(" ", 1)
        else:
            qual = ""
        m = re.match(r"^layout\(location = (\d+)\) out (?:\w+\s+)?"
                     r"(\w+)\s+(\w+);$", s)
        if m:
            p.ps_outs.append((m.group(3), m.group(2), int(m.group(1))))
            continue
        m = re.match(r"^in\s+(?:\w+\s+)?(\w+)\s+(\w+);$", s)
        if m:
            p.ins.append((m.group(2), m.group(1)))
            continue
        m = re.match(r"^out\s+(?:\w+\s+)?(\w+)\s+(\w+);$", s)
        if m:
            p.outs.append((m.group(2), m.group(1)))
            # GLSL 的 flat 在 HLSL 里对应 nointerpolation。丢掉它会让整数
            # 插值器走默认插值，instancing 的 instance id 在多顶点间被插成
            # 小数 —— 不报编译错，但结果是错的。
            p.out_mod[m.group(2)] = ("nointerpolation "
                                      if qual == "flat" else "")
            continue

        # ---- HLSLcc 辅助函数（原样保留）----
        #
        # HLSLcc 会在 PS 里内联注入几个 GLSL 版的辅助函数（op_not 等），
        # 它们的定义是**单行**的：
        #     int op_not(int value) { return -value - 1; }
        #     ivec2 op_not(ivec2 a) { a.x = op_not(a.x); ...; return a; }
        # 原先的正则要求行尾是 `{`，只匹配多行形式，单行定义整行被漏掉 ——
        # 正文里 `op_not(u_xlati6)` 还在，编译期直接报未声明。
        #
        # 所以分两支：单行（含结尾的 `}`）直接收；多行走 in_helper 累积。
        m = re.match(r"^(\w+)\s+(\w+)\(.*\)\s*\{.*\}\s*$", s)
        if m:
            p.helpers.append(ln)
            continue
        m = re.match(r"^(\w+)\s+(\w+)\(.*\)\s*\{$", s)
        if m:
            p.helpers.append(ln)
            in_helper = True
            continue
        if in_helper:
            p.helpers.append(ln)
            if s == "}":
                in_helper = False
            continue

        # ---- 全局临时量 / ImmCB ----
        m = re.match(r"^(?:const\s+)?(\w+)\s+(\w+)\s*(\[[\d]+\])?\s*;$", s)
        if m:
            p.globals.append((m.group(1), m.group(2), m.group(3) or ""))

    return p


# ------------------------------------------------------- 条件编译块消解
def resolve_ifdefs(text):
    """HLSLcc 的 #ifdef 按目标平台取舍：GL_* 取 if，Adreno workaround 取 else。"""
    out, stack = [], []
    for ln in text.splitlines():
        s = ln.strip()
        m = re.match(r"^#if(?:def|ndef)?\s+(\w+)$", s)
        if m:
            take_if = m.group(1) in TAKE_IF
            parent = all(k[0] for k in stack)
            stack.append((parent and take_if, take_if))
            continue
        if s == "#else" and stack:
            out_now, take_if = stack[-1]
            stack[-1] = (not out_now if not take_if else out_now, take_if)
            continue
        if s == "#endif":
            if stack:
                stack.pop()
            continue
        if all(k[0] for k in stack):
            out.append(ln)
    return "\n".join(out)


# ------------------------------------------------------------- 等价宏
FUNC_MACRO = {
    "inversesqrt": "_g_inversesqrt", "fract": "_g_fract", "mix": "_g_mix",
    "roundEven": "_g_roundEven", "trunc": "_g_trunc",
    "floatBitsToUint": "_g_floatBitsToUint",
    "uintBitsToFloat": "_g_uintBitsToFloat",
    "floatBitsToInt": "_g_floatBitsToInt",
    "intBitsToFloat": "_g_intBitsToFloat",
    "lessThan": "_g_lessThan", "greaterThan": "_g_greaterThan",
    "greaterThanEqual": "_g_greaterThanEqual",
    "lessThanEqual": "_g_lessThanEqual",
    "equal": "_g_equal", "notEqual": "_g_notEqual",
    "bitfieldExtract": "_g_bitfieldExtract",
    "texture": "_g_texture", "textureLod": "_g_textureLod",
    "texelFetch": "_g_texelFetch",
}
FUNC_RE = re.compile(r"\b(" + "|".join(FUNC_MACRO) + r")\s*\(")

MACROS = """\
// ---- 以下全部是 GLSL -> HLSL 的等价宏，正文一字未改 ----
#define _g_inversesqrt rsqrt
#define _g_fract frac
#define _g_mix lerp
#define _g_roundEven round
#define _g_trunc trunc
#define _g_floatBitsToUint asuint
#define _g_uintBitsToFloat asfloat
#define _g_floatBitsToInt asint
#define _g_intBitsToFloat asfloat
#define _g_lessThan(a, b) ((a) < (b))
#define _g_greaterThan(a, b) ((a) > (b))
#define _g_greaterThanEqual(a, b) ((a) >= (b))
#define _g_lessThanEqual(a, b) ((a) <= (b))
#define _g_equal(a, b) ((a) == (b))
#define _g_notEqual(a, b) ((a) != (b))
#define _g_bitfieldExtract(v, o, c) (((v) >> (o)) & ((1u << (c)) - 1u))
// 采样：sampler##tex 拼出 sampler_<纹理名>，与下方 SAMPLER 声明一一对应
#define _g_texture(tex, uv) ((tex).Sample(sampler##tex, uv))
#define _g_textureBias(tex, uv, b) ((tex).SampleBias(sampler##tex, uv, b))
#define _g_textureLod(tex, uv, lod) ((tex).SampleLevel(sampler##tex, uv, lod))
#define _g_texelFetch(tex, coord) ((tex).Load(int3(coord)))
// GLSL ES 3.0 的 sampler2DShadow 比较采样 -> HLSL SampleCmpLevelZero
#define _g_zcmpLod(tex, uv, lod) \\
    ((tex).SampleCmpLevelZero(sampler##tex, float3(uv, lod)))
"""


# --------------------------------------------------------------- 正文转换
def convert_body(body, is_ps, inst_prefixes, mat_names, vary_names, p,
                 attr_names):
    body = resolve_ifdefs(body)
    body = re.sub(r"^\s*void\s+main\s*\(\s*\)\s*\{", "", body)
    cut = body.rfind("}")
    if cut >= 0:
        body = body[:cut]

    # 矩阵：GLSL 列索引 -> HLSL 行索引（机械插入 transpose 局部量）
    def _to_transposed(tn):
        def _rep(m):
            return tn + "[" + m.group(2) + "]"
        return _rep

    for mn in mat_names:
        pat = re.compile(
            r"\b((?:" + "|".join(map(re.escape, inst_prefixes)) +
            r"\.)?" + re.escape(mn) + r")\[(\d)\]")
        body = pat.sub(_to_transposed("_t" + mn), body)

    # 剥离 UBO 实例前缀（CB 成员在 HLSL 里都是裸变量）
    for pref in inst_prefixes:
        body = re.sub(r"\b" + re.escape(pref) + r"\.", "", body)

    # 顶点属性 / 插值器换成结构体成员
    for nm in attr_names:
        body = re.sub(r"\b" + re.escape(nm) + r"\b", "input." + nm, body)
    for nm in vary_names:
        tgt = ("input." if is_ps else "output.") + nm
        body = re.sub(r"\b" + re.escape(nm) + r"\b", tgt, body)
    if not is_ps:
        body = re.sub(r"\bgl_Position\b", "output.positionCS", body)
    else:
        for nm, _t, _l in p.ps_outs:
            body = re.sub(r"\b" + re.escape(nm) + r"\b", "__" + nm, body)

    # GLSL 内建量 -> HLSL 等价物。gl_Position 已单独处理。
    # gl_FragCoord 在 PS 里就是 SV_POSITION（即 input.positionCS）；
    # gl_InstanceID / gl_VertexID 用同名语义的系统值语义声明。
    if is_ps:
        body = re.sub(r"\bgl_FragCoord\b", "input.positionCS", body)
    for gb in ("gl_InstanceID", "gl_VertexID"):
        if re.search(r"\b" + gb + r"\b", body):
            body = re.sub(r"\b" + gb + r"\b", "_g_" + gb, body)

    # GLSL 的 return; -> Unity 函数的 return
    if is_ps:
        # frag 始终是「location 0 走返回值语义」的形态（高位的 MRT 输出
        # 在 build() 里整行删掉了），所以裸 `return;` 一律补成返回 0 号输出。
        body = re.sub(r"\breturn\s*;",
                      "return __" + p.ps_outs[0][0] + ";" if p.ps_outs
                      else "return 0;", body)
    else:
        body = re.sub(r"\breturn\s*;", "return output;", body)

    body = FUNC_RE.sub(lambda m: FUNC_MACRO[m.group(1)] + "(", body)
    body = TYPE_RE.sub(lambda m: TYPES[m.group(1)], body)
    return body


# ---------------------------------------------------------------- 语义
def semantic_of(name):
    if name.startswith("in_"):
        rest = name[3:]
        for pre in ("POSITION", "NORMAL", "TANGENT", "COLOR",
                    "BLENDWEIGHT", "BLENDINDICES"):
            if rest.startswith(pre):
                return pre
        m = re.match(r"TEXCOORD(\d+)$", rest)
        return "TEXCOORD" + (m.group(1) if m else "0")
    m = re.match(r"vs_(\w+?)(\d+)$", name)
    return "TEXCOORD" + (m.group(2) if m else "0")


def assign_vary_semantics(vary):
    """给每个插值器分配**连续、唯一且在WebGL 合法范围内**的 TEXCOORD 索引。

    HLSLcc 在这批 DXBC->GLES3 输出里不给 varying 写 layout(location)，
    索引得自己定。

    为什么不能沿用名字里的原数字：
      1. 原数字可能超出 WebGL 上限。Retro Master 用了 vs_TEXCOORD16 /
         vs_TEXCOORD17，而 GLES 3.0 只保证 16 个 varying（索引 0..15），
         实际可用还更少 —— 交叉编译阶段就link 失败。
      2. 名字里可能没有数字（URP instancing 的 vs_CUSTOM_INSTANCE_ID0），
         早期版本一律回退到 TEXCOORD0，跟 vs_INTERP0 撞车。

    所以统一按名字排序后重新连续编号。VS 和 PS 用的是同一张表
    （vary 由 vs.outs 与 ps.ins 合并而来），所以两侧自动对齐；
    重新编号不改变任何语义，只是把索引压回合法范围。
    """
    out = {}
    for i, n in enumerate(sorted(vary)):
        out[n] = i
    return out


# ---------------------------------------------------------------- 生成
def extract_properties(dummy_shader_path):
    """从 AssetRipper 导出的原版 .shader 里原样取出 Properties 块。"""
    if not dummy_shader_path or not os.path.exists(dummy_shader_path):
        return ""
    txt = open(dummy_shader_path, encoding="utf-8", errors="replace").read()
    m = re.search(r"Properties\s*\{", txt)
    if not m:
        return ""
    i, depth = m.end() - 1, 0
    for j in range(i, len(txt)):
        if txt[j] == "{":
            depth += 1
        elif txt[j] == "}":
            depth -= 1
            if depth == 0:
                body = txt[i + 1:j]
                # 去掉一层缩进，保持和原版一致的缩进层级
                return "\n".join(l[1:] if l.startswith("\t") else l
                                 for l in body.rstrip().splitlines())
    return ""


def extract_tags(dummy_shader_path):
    """取出 SubShader 级的 Tags（合成为单个 Tags 块，ShaderLab 只允许一个）。"""
    return _dummy_re(dummy_shader_path, "tags")


def extract_pass_state(dummy_shader_path):
    """取出 SubShader 级与 Pass 级的渲染状态（Name / Cull / ZWrite / Blend ...），
    以及 Pass 名与 LightMode。原版怎么写就怎么搬，不自行增删。"""
    if not dummy_shader_path or not os.path.exists(dummy_shader_path):
        return {}
    txt = open(dummy_shader_path, encoding="utf-8", errors="replace").read()
    ms = re.search(r"SubShader\s*\{", txt)
    if not ms:
        return {}
    # SubShader 头部（第一个 '{' 之后、Pass 之前）里的状态行
    head = txt[ms.end():]
    mp = head.find("Pass")
    sub_head = head[:mp] if mp > 0 else head
    state = {"sub": [], "pass": [], "name": None, "lightmode": None}
    keys = ("Cull", "ZWrite", "ZTest", "Blend", "BlendOp", "ColorMask",
            "Offset", "Stencil")
    for m in re.finditer(r"\b(" + "|".join(keys) + r")\b([^\n{}]*)", sub_head):
        state["sub"].append((m.group(1), m.group(2).strip()))
    for m in re.finditer(r"\b(" + "|".join(keys) + r")\b([^\n{}]*)", head[mp:]):
        state["pass"].append((m.group(1), m.group(2).strip()))
    mn = re.search(r'Name\s+"([^"]+)"', head[mp:])
    if mn:
        state["name"] = mn.group(1)
    ml = re.search(r'LightMode"\s*=\s*"([^"]+)"', head[mp:])
    if ml:
        state["lightmode"] = ml.group(1)
    return state


def _dummy_re(path, what):
    if not path or not os.path.exists(path):
        return []
    txt = open(path, encoding="utf-8", errors="replace").read()
    ms = re.search(r"SubShader\s*\{", txt)
    if not ms:
        return []
    m = re.search(r"Tags\s*\{([^}]*)\}", txt[ms.end():])
    if not m:
        return []
    return list(re.findall(r'"([^"]+)"\s*=\s*"([^"]+)"', m.group(1)))


def build(shader_name, vs_glsl, ps_glsl, props_txt, tex_alias, tags,
          state, fallback):
    vs = parse_glsl(vs_glsl)
    ps = parse_glsl(ps_glsl)

    # ---- MRT（多渲染目标）分析 ----
    #
    # 原先只取 ps_outs[0] 当返回值，第二个输出（location 1）的
    # `SV_Target1 = ...` 赋值还留在正文里，但它从没被声明过 ——
    # 编译期直接报未声明。WaterShader 的 sm50 变体正是这种：
    #   layout(location = 0) out highp vec4 SV_Target0;
    #   layout(location = 1) out highp uint SV_Target1;
    #
    # 修法有个陷阱：把 frag 改成 `void` 全声明成局部量，看着补齐了声明，
    # 其实更糟 —— **没有 SV_Target 语义输出的 fragment shader 什么都不
    # 渲染**，画面直接空掉，比丢失次要目标严重得多。
    #
    # 所以：location 0 保持原样走返回值语义（`float4 frag(...) : SV_Target0`），
    # 只把 location >= 1 的赋值**整行删掉**。这些是 URP 的 Rendering Layer
    # mask（`uint(unity_RenderingLayer & _pad176)`），GLES 3.0 没有 MRT、
    # 引擎也没有对应的消费方，丢掉它对画面没有可见影响。
    # 这是按目标平台能力做的机械裁剪，不是改算法。
    outs = ps.ps_outs
    dropped_outs = [n for n, _t, loc in outs if loc != 0]
    keep_outs = [(n, t, loc) for n, t, loc in outs if loc == 0] or outs[:1]

    inst = sorted({"_" + b for b, _ in vs.blocks} | {"_" + b for b, _ in ps.blocks},
                  key=len, reverse=True)
    vary = {}
    for n, t in vs.outs:
        vary[n] = t
    for n, t in ps.ins:
        vary.setdefault(n, t)
    # 修饰符以 VS 侧的 flat 为准（GLSL 里配对的 out/in 修饰符本来就该一致，
    # 但只有 VS 侧带 out_mod；PS 侧的 in 走的是另一条记录路径）。
    mods = dict(vs.out_mod)

    # 第一遍：还不知道哪些 pad 会被引用，先不带 pad 做矩阵名单
    mat_names = {n for p in (vs, ps) for t, n in p.uniform_vars() if "x" in t}
    vs_body = convert_body(vs.body, False, inst, mat_names, list(vary), vs,
                           [n for n, _ in vs.ins])
    ps_body = convert_body(ps.body, True, inst, mat_names, list(vary), ps, [])

    # 正文里真的被引用的 _padN（说明 RDEF 布局该处有缺口，HLSLcc 退化成
    # 读空腔）必须照原类型声明出来，否则引用它们的那几行编译不过。
    used_pads = set(re.findall(r"\b_pad\d+\b", vs_body + ps_body))
    mat_names |= {n for p in (vs, ps)
                  for t, n in p.uniform_vars(used_pads) if "x" in t}
    if used_pads:
        vs_body = convert_body(vs.body, False, inst, mat_names, list(vary), vs,
                               [n for n, _ in vs.ins])
        ps_body = convert_body(ps.body, True, inst, mat_names, list(vary), ps,
                               [])
        used_pads = set(re.findall(r"\b_pad\d+\b", vs_body + ps_body))
    uni_pads = used_pads

    # 纹理改名：tN -> 真实属性名（映射由外部提供，缺省保持原名）
    def alias(n):
        return tex_alias.get(n, n)

    tex_decls, seen_tex = [], set()
    for p in (vs, ps):
        for n, macro in p.tex:
            real = alias(n)
            if real in seen_tex:
                continue
            seen_tex.add(real)
            tex_decls.append(f"            {macro}({real});")
            tex_decls.append(f"            SAMPLER(sampler_{real});")
    # shadow 比较用的纹理：GLSL ES 3.0 没有 comparison sampler，单独声明
    for p in (vs, ps):
        for zn in p.zcmp:
            real = alias("_" + zn[len("hlslcc_zcmp_"):])
            if real in seen_tex:
                # 同一张纹理既普通采样又比较采样：HLSL 侧用 Load 手动比较不可行，
                # 保留普通声明，比较路径走 zcmp 宏（见 _zcmp_alias）。
                continue
            seen_tex.add(real)
            tex_decls.append(f"            TEXTURE2D({real});")
            tex_decls.append(f"            SAMPLER(sampler_{real});")

    # shadow 采样的重命名要在正文里做：hlslcc_zcmp_X -> X
    zmap = {}
    for p in (vs, ps):
        for zn in p.zcmp:
            zmap[zn] = alias("_" + zn[len("hlslcc_zcmp_"):])
    vs_body = _apply_zcmp(vs_body, zmap)
    ps_body = _apply_zcmp(ps_body, zmap)

    # MRT：GLES 3.0 不支持多渲染目标，把 location >= 1 的输出赋值整行删掉。
    # 正文侧它们已经被改写成 `__SV_TargetN`，所以按这个形态匹配。
    # 机械删行，不改任何算法 —— 这些是 URP 的 Rendering Layer mask，
    # WebGL 上没有消费方。
    for nm in dropped_outs:
        ps_body = re.sub(r"^.*\b__" + re.escape(nm) + r"\b.*$\n?", "",
                         ps_body, flags=re.M)

    # 纹理改名同样要在正文里做。只改 TEXTURE2D() 声明是不够的 ——
    # 正文里 _g_texture(_Texture_t3, uv) 还指着旧名，编译期报未声明。
    # 用词边界替换，避免 _Texture_t3 命中 _Texture_t30 的前缀。
    #
    # 放在 _apply_zcmp 之后：这两个都是「改名字」而不是「改结构」，
    # 顺序无关，但放在最后可以保证不破坏前面任何一步的结果。
    if tex_alias:
        pat = re.compile(r"\b(" + "|".join(
            re.escape(k) for k in sorted(tex_alias, key=len, reverse=True))
            + r")\b")

        def _retag(m):
            return tex_alias[m.group(1)]

        vs_body = pat.sub(_retag, vs_body)
        ps_body = pat.sub(_retag, ps_body)

    def _trans_for(body_txt):
        return "".join(
            f"            float4x4 _t{n} = transpose({n});\n"
            for n in sorted(mat_names)
            if re.search(r"\b_t" + re.escape(n) + r"\b", body_txt))

    trans_vs = _trans_for(vs_body)
    trans_ps = _trans_for(ps_body)

    uni, seen_u = [], set()
    for p in (vs, ps):
        for t, n in p.uniform_vars(uni_pads):
            if n in seen_u:
                continue
            seen_u.add(n)
            uni.append(f"            {TYPES.get(t, t)} {n};")

    attrs = "\n".join(
        f"                {TYPES.get(t, t)} {n} : {semantic_of(n)};"
        for n, t in vs.ins) or "                float3 _unused : TEXCOORD0;"
    vary_sem = assign_vary_semantics(vary)
    varys = "\n".join(
        f"                {mods.get(n, '')}"
        f"{TYPES.get(t, t)} {n} : TEXCOORD{vary_sem[n]};"
        for n, t in sorted(vary.items()))
    # VS 与 PS 的临时量都要声明（u_xlat* / ImmCB*）
    gl, seen_g = [], set()
    for p in (vs, ps):
        for t, n, arr in p.globals:
            if n in seen_g:
                continue
            seen_g.add(n)
            gl.append(f"            {TYPES.get(t, t)} {n}{arr};")
    # GLSL 系统值语义
    both = vs_body + ps_body
    for gb, sem in (("gl_InstanceID", "SV_InstanceID"),
                    ("gl_VertexID", "SV_VertexID")):
        if "_g_" + gb in both:
            gl.append(f"            uint _g_{gb} : {sem};")
    globs = "\n".join(gl)
    helpers = TYPE_RE.sub(
        lambda m: TYPES[m.group(1)],
        "\n".join(vs.helpers + ps.helpers))

    # ---- MRT（多渲染目标）----
    #
    # 原先只取 ps_outs[0] 当返回值，第二个输出（location 1）的
    # `SV_Target1 = ...` 赋值还留在正文里，但它从没被声明过 ——
    # 编译期直接报未声明。WaterShader 的 sm50 变体正是这种：
    #   layout(location = 0) out highp vec4 SV_Target0;
    #   layout(location = 1) out highp uint SV_Target1;
    #
    # 修法有个陷阱：把 frag 改成 `void` 全声明成局部量，看着补齐了声明，
    # 其实更糟 —— **没有 SV_Target 语义输出的 fragment shader 什么都不
    # 渲染**，画面直接空掉，比丢失次要目标严重得多。
    #
    # 所以：location 0 保持原样走返回值语义（`float4 frag(...) : SV_Target0`），
    # 只把 location >= 1 的赋值**整行删掉**。这些是 URP 的 Rendering Layer
    # mask（`uint(unity_RenderingLayer & _pad176)`），GLES 3.0 没有 MRT、
    # 引擎也没有对应的消费方，丢掉它对画面没有可见影响。
    # 这是按目标平台能力做的机械裁剪，不是改算法。
    # MRT 分析已在 build() 开头完成（dropped_outs / keep_outs）。
    n, t, loc = keep_outs[0]
    ps_ret, ps_sig = TYPES.get(t, t), f" : SV_Target{loc}"

    tags_txt = ("        Tags { " +
                " ".join(f'"{k}" = "{v}"' for k, v in tags) + " }"
                ) if tags else ""
    fb = f'    Fallback "{fallback}"\n' if fallback else ""

    st = state or {}
    sub_state = " ".join(f"{k} {v}" for k, v in st.get("sub", []))
    pass_state = " ".join(f"{k} {v}" for k, v in st.get("pass", []))
    pass_name = st.get("name") or "Forward"
    lm = st.get("lightmode")
    lm_txt = f'            Tags {{ "LightMode" = "{lm}" }}\n' if lm else ""

    # ---- Stencil 块 ----
    #
    # UI Shader Graph 的核心机制，原版每个 UI shader 都有，且属性声明齐全。
    # 12 个 shader（UI / UIBlur / MapBackground / MapLine / MapObject /
    # OutlineBackground / RedDot / SlotMachineBackground / SniperAim /
    # Thinking / Vignette / Tinted Blur）都声明了这6 个属性，但 Pass 里
    # 一个 Stencil 块都没有 —— UI 的Mask 遮罩会完全失效。
    #
    # 为什么这个可以机械还原、不算手写：ShaderLab 的 Stencil 块**只能**
    # 引用这些 Properties 变量（不引用变量的字面量写法在这里没有意义），
    # 所以块内容与属性名是**一一对应的固定映射**：
    #
    #     Stencil {
    #         Ref [_Stencil]      {  = 值取自 Properties 的 _Stencil
    #         Comp [_StencilComp]    = 值取自 _StencilComp
    #         WriteMask [_StencilWriteMask]
    #         ReadMask [_StencilReadMask]
    #     }
    #
    # 不涉及任何着色逻辑、不改算法 —— 只是把 Properties 里已经声明好的
    # 变量接到它们本该驱动的东西上。数值侧由 .mat 在运行时给，
    # 不用在这里定。
    if "_StencilComp" in props_txt and "Stencil {" not in pass_state:
        stencil_txt = """            Stencil
            {
                Ref [_Stencil]
                Comp [_StencilComp]
                WriteMask [_StencilWriteMask]
                ReadMask [_StencilReadMask]
            }
"""
    else:
        stencil_txt = ""

    # ColorMask 同理：属性存在时必须接上，否则 _ColorMask 完全无效。
    if "_ColorMask" in props_txt:
        cm = f"            ColorMask [_ColorMask]\n"
    else:
        cm = ""

    return f"""Shader "{shader_name}"
{{
    Properties
    {{
{props_txt}
    }}
    SubShader
    {{
{tags_txt}
{('        ' + sub_state) if sub_state else ''}
        Pass
        {{
            Name "{pass_name}"
{('            ' + pass_state) if pass_state else ''}
{stencil_txt}{cm}{lm_txt}            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // target 级别必须跟目标平台的 GLES 能力对齐，不能照抄 sm50。
            //
            // Unity 的 target 与 GLES 的对应：
            //   3.0 -> GLES 3.0 / SM 4.0
            //   3.5 -> GLES 3.1 / SM 5.0   <- 多compute shader + SSBO
            //
            // 我们的目标是 WebGL2，而 **WebGL2 == GLES 3.0**，没有 SSBO
            // （那是 GLES 3.1 才有的）。原先写 3.5，于是编译器按SM 5.0 的
            // 能力去编译，而 WebGL 后端只能给到 GLES 3.0 —— 整个 shader
            // 编译失败，Unity 把材质渲成洋红，然后**照常打包、照常exit 0**。
            //
            // 实测证据：上一轮CI 抓到的verify_loaded.png 里，**13.19% 的像素
            // 是纯 (254,0,254)**，主菜单一大片元素渲成洋红。这就是本行的
            // 后果 —— 36 个已恢复 shader **全部**是 3.5，无一幸免。
            //
            // 改成 3.0 不改任何算法：只是把「声明需要什么硬件能力」对齐到
            // 目标平台真实提供的能力。
            #pragma target 3.0

{MACROS}
{chr(10).join(tex_decls)}

{chr(10).join(uni)}

{globs}

{helpers}

            struct Attributes
            {{
{attrs}
            }};

            struct Varyings
            {{
                float4 positionCS : SV_POSITION;
{varys}
            }};

            Varyings vert(Attributes input)
            {{
                Varyings output = (Varyings)0;
{trans_vs}
{vs_body}
            }}

            {ps_ret} frag(Varyings input){ps_sig}
            {{
{trans_ps}
{ps_body}
            }}
            ENDHLSL
        }}
    }}
{fb}}}
"""


def _apply_zcmp(text, zmap):
    """hlslcc_zcmp_<tex> 是 shadow 深度比较采样：改名成 <tex>，并把对应的
    textureLod 换成 _g_zcmpLod（GLSL ES 3.0 对 sampler2DShadow 的 textureLod
    返回比较结果，HLSL 侧对应 SampleCmpLevelZero）。"""
    for zn, real in zmap.items():
        if zn not in text:
            continue
        text = re.sub(r"\b_textureLod\(\s*" + re.escape(zn) + r"\s*,",
                      "_g_zcmpLod(" + real + ",", text)
        text = re.sub(r"\b" + re.escape(zn) + r"\b", real, text)
    return text


# ---------------------------------------------------------------- main
def variant_key(filename):
    """从变体文件名里取出「关键字组合」。

    HLSLcc 生成的文件名形如
        00_p0_VS_sm40_UnityPerDraw_LightShadows__MainLightPosi.glsl
        20_p0_PS_sm50__WindSpeed__WindStrength_UnityPerDraw__G.glsl
    下划线后面那一段就是该子程序的关键字集。同一 pass 的 VS 与 PS 只有在
    关键字集相同时才是一对能链接的程序 —— URP 的 instancing 关键字会让
    VS 多输出一个 flat uint instance id，PS 再读它；错配的���候 PS 会去读
    一个 VS 根本没写的插值器，编译期报未声明，链接期也可能静默出错。
    """
    m = re.match(r"^\d+_p\d+_(?:VS|PS)_sm\d+_(.*)\.glsl$", filename)
    return m.group(1) if m else filename


def pick_variant_pair(directory):
    """挑一对**关键字相同**且不含 SSBO 的 VS/PS（WebGL2 = GLES 3.0 无 SSBO）。

    先按文件名长度排序枚举候选组合（短的 = 关键字少），取第一组 VS/PS 都
    存在、且两者都不含 SSBO 的组合。关键字数一致这点由variant_key 保证。
    """
    files = sorted(f for f in os.listdir(directory)
                   if f.endswith(".glsl") and "_p0_" in f)

    def no_ssbo(f):
        txt = open(os.path.join(directory, f), errors="replace").read()
        return "readonly buffer" not in txt

    # 组合 -> {stage: [files]}，stage 取自文件名的 _p0_VS_ / _p0_PS_
    groups = {}
    for f in files:
        m = re.match(r"^\d+_p0_(VS|PS)_sm\d+_", f)
        if m:
            groups.setdefault(variant_key(f), {}).setdefault(
                m.group(1), []).append(f)

    # 排序键：先按组合名长度（关键字少者靠前），再按名字，保证结果稳定。
    for key in sorted(groups, key=lambda k: (len(k), k)):
        g = groups[key]
        if "VS" not in g or "PS" not in g:
            continue
        vs_safe = [f for f in g["VS"] if no_ssbo(f)]
        ps_safe = [f for f in g["PS"] if no_ssbo(f)]
        if vs_safe and ps_safe:
            return min(vs_safe, key=len), min(ps_safe, key=len), key

    # 到这里说明**每个** PS 变体都含 SSBO（URP 的 DBuffer / APV 查找表）。
    #
    # 以前这里是「退路」：不检查 SSBO，随便挑一对走人。后果是产物里留下
    # 一堆 `_Structured_tN_buf[...]`，WebGL2（GLES 3.0，无 SSBO）上必然编译
    # 失败 —— 而这个失败 Unity 不当构建错误，只把材质渲成洋红，所以 CI 一路
    # 全绿，把问题完全掩盖了。URPDecal / LaserDotDecal 两个 shader 就是这样
    # 被推上去的。
    #
    # 所以宁可不产出：返回 None，调用方把这个 shader 记为「跳过」并打印原因，
    # 让它留在 AssetRipper 空壳状态（至少渲染成白/洋红但结构是对的），
    # 也不产出一个必然编译不过的版本。
    ssbo_keys = [k for k in groups
                 if "VS" in groups[k] and "PS" in groups[k]]
    return None, None, ("SSBO" if ssbo_keys else None)


def pick_variant(directory, stage):
    """单stage 入口（供调试/脚本用）。"""
    vs_f, ps_f, _key = pick_variant_pair(directory)
    return vs_f if stage == "VS" else ps_f


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--dir", required=True)
    ap.add_argument("--shader", required=True, help="完整 shader 名（含路径）")
    ap.add_argument("--out", default="Assets/Shader/Restored")
    ap.add_argument("--from-dummy", default=None,
                    help="AssetRipper 导出的原版 .shader（取 Properties/Tag）")
    ap.add_argument("--tex-alias", default=None,
                    help='JSON: {"_Texture_t0": "_BaseMap", ...}')
    ap.add_argument("--out-name", default=None)
    a = ap.parse_args()

    vs_f, ps_f, key = pick_variant_pair(a.dir)
    if not vs_f or not ps_f:
        if key == "SSBO":
            # 每个 PS 变体都引用 URP 的 DBuffer / APV 查找表（std430 SSBO）。
            # GLES 3.0 没有 SSBO，产出来必然编译不过 —— 如实失败，别产出。
            sys.exit(f"{a.dir}: 所有 PS 变体都含 SSBO，WebGL2 无法编译，跳过")
        sys.exit(f"{a.dir}: 找不到 pass0 的 VS/PS 变体")
    vs_glsl = open(os.path.join(a.dir, vs_f), errors="replace").read()
    ps_glsl = open(os.path.join(a.dir, ps_f), errors="replace").read()
    print(f"关键字组合: {key}\nVS: {vs_f}\nPS: {ps_f}")
    if "readonly buffer" in ps_glsl:
        print("  ! 该 shader 无不含 SSBO 的 PS 变体，WebGL2 无法编译")

    props, tags, fallback, state = "", [], "", {}
    if a.from_dummy and os.path.exists(a.from_dummy):
        props = extract_properties(a.from_dummy)
        tags = extract_tags(a.from_dummy)
        state = extract_pass_state(a.from_dummy)
        mf = re.search(r'Fallback\s+"([^"]+)"',
                       open(a.from_dummy, encoding="utf-8",
                            errors="replace").read())
        if mf:
            fallback = mf.group(1)
    if not tags:
        tags = [("RenderPipeline", "UniversalPipeline"), ("RenderType", "Opaque"),
                ("Queue", "Geometry")]

    alias = {}
    if a.tex_alias:
        alias = json.loads(open(a.tex_alias).read())

    os.makedirs(a.out, exist_ok=True)
    name = a.out_name or (a.shader.split("/")[-1].replace(" ", "_")
                          + "_Restored.shader")
    path = os.path.join(a.out, name)
    with open(path, "w", encoding="utf-8") as f:
        f.write(build(a.shader, vs_glsl, ps_glsl, props, alias, tags, state,
                      fallback))
    print("写出", path, os.path.getsize(path), "字节")


if __name__ == "__main__":
    main()
