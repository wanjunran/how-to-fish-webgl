#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""检查启动场景的天空盒链路（碧海蓝天就是它渲出来的）。

## 为什么单独查这个

用户最想看到的三样东西——碧海蓝天、绿草地、暖沙——里，**天空盒是
唯一一个在启动场景里、不依赖任何游戏内逻辑就能出结果的**。绿草地和
暖沙要进到 Island 场景、等光照和贴图加载；天空盒只要相机 ClearFlags
是 Skybox、游戏能启动，它就该在第一帧出现。

所以它是最好的「渲染管线通没通」的探针：启动 Logo 页能正确画出
天空盒，说明 camera / skybox / shader / SRP 这条链全通；画不出来
说明问题在管线而不是在玩法逻辑上。

## 查什么

按数据往上游追，每一环都可能断：

1. 场景`RenderSettings.m_SkyboxMaterial` 有没有指向有效材质
2. 该材质的 `m_Shader` 在不在本仓库（包内 shader 的话这里会找不到，
   那属于正常，见下）
3. 材质里有没有实际颜色（`_TopDayColor` / `_BottomDayColor` 等）
4. **材质的 m_TexEnvs 里有没有贴图** —— 这是最容易断的一环：
   天空盒可以纯程序化生成（不贴图），但如果原版是贴图而AssetRipper
   没导出，画面就会是纯色
5. 相机的 `m_ClearFlags` 是不是 1（Skybox）—— 是 0/2 就永远看不到天空
6. 天空盒 shader 的 `#pragma target` —— WebGL2 == GLES 3.0，
   target 3.5+ 会要SSBO，浏览器里没有

## 已实测的现状（别当成理所当然）

-场景 `Game.unity` 26 万行、**2 个 Camera**，都是 `ClearFlags=1`（Skybox）
- `RenderSettings.m_SkyboxMaterial` -> `Assets/Material/SkyboxTesting.mat`
- 该材质用的是**游戏自研** `Shader Graphs/SkyboxShader`（364 行，
  **非空壳**），不是 URP 自带 Skybox
- 颜色数据齐全且正确：`_TopDayColor` = (0, 0.466, 1) 湛蓝、
  `_BottomDayColor` = (0.263, 0.576, 1) 海蓝、`_CloudColor` = 白
- `#pragma target 3.0`（已为 WebGL2 改过），零 include、零条件分支

也就是说**这条链的数据是全的**。若画面仍是纯黑，原因只可能落在
「shader 运行时报错」或「根本没进到渲染天空盒的相机」上 ——
这两件事本脚本看不到，判决权在CI 的 shader 错误扫描和截图里。

## 刻意不做的事

不判定「天空盒必须不是纯黑」。程序化天空盒本来就是纯色渐变，
拿「有没有颜色」当判据会把正常状态判成异常。这里的判据是
**数据完整性**（材质/颜色/相机/pragma 在不在），不是画面观感。

用法
----
    python3 tools/check_skybox.py           # 人看的报告
    python3 tools/check_skybox.py --json    # 机读
"""
from __future__ import annotations

import argparse
import glob
import json
import os
import re
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SCENES = [
    os.path.join(ROOT, "Assets", "Scenes", "Game.unity"),
]

# 纯程序化天空盒的典型属性名。判断「有没有颜色数据」用这一组。
COLOR_KEYS = (
    "_TopDayColor", "_BottomDayColor", "_TopColor", "_BottomColor",
    "_DayColor", "_NightColor", "_SunColor", "_MoonColor", "_CloudColor",
)
CAMERA_RE = re.compile(r"--- !u!\d+ &(\d+)\nCamera:(.*?)(?=\n--- !u!|\Z)", re.S)
RENDER_SETTINGS_RE = re.compile(
    r"RenderSettings:\n(.*?)(?=\n--- !u!|\Z)", re.S)
SKYBOX_REF_RE = re.compile(r"m_SkyboxMaterial:\s*\{fileID:\s*(-?\d+),"
                           r"\s*guid:\s*([0-9a-f]{32})")
CLEARFLAGS_RE = re.compile(r"m_ClearFlags:\s*(\d+)")
SHADER_REF_RE = re.compile(r"m_Shader:\s*\{fileID:\s*\d+,\s*guid:\s*([0-9a-f]{32})")
TARGET_RE = re.compile(r"#pragma\s+target\s+([\d.]+)")


def guid_of_meta(path: str) -> str | None:
    try:
        with open(path + ".meta", encoding="utf-8", errors="replace") as f:
            m = re.search(r"guid:\s*([0-9a-f]{32})", f.read())
        return m.group(1) if m else None
    except OSError:
        return None


def find_by_guid(guid: str, ext: str) -> list[str]:
    """按 guid 找资产文件（只扫自有Assets，不进 Library）。"""
    out = []
    for meta in glob.glob(os.path.join(ROOT, "Assets", "**", "*.meta"),
                          recursive=True):
        if not meta.endswith(ext + ".meta"):
            continue
        try:
            with open(meta, encoding="utf-8", errors="replace") as f:
                if f"guid: {guid}" in f.read(4096):
                    out.append(meta[:-len(".meta")])
        except OSError:
            continue
    return out


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--json", action="store_true")
    a = ap.parse_args()

    data: dict = {"scenes": [], "problems": [], "notes": []}

    for scene in SCENES:
        if not os.path.isfile(scene):
            data["problems"].append(f"场景不存在: {scene}")
            continue
        with open(scene, encoding="utf-8", errors="replace") as f:
            text = f.read()

        sname = os.path.basename(scene)
        info: dict = {"scene": sname, "cameras": [], "skybox": {}}

        # ---- 相机 ----
        cams = []
        for m in CAMERA_RE.finditer(text):
            body = m.group(2)
            cf = CLEARFLAGS_RE.search(body)
            cams.append({
                "id": m.group(1),
                "clear_flags": int(cf.group(1)) if cf else None,
                # 1 = Skybox。0=SolidColor 2=Color 3=Depth 4=Nothing
                "sees_skybox": bool(cf and cf.group(1) == "1"),
            })
        info["cameras"] = cams
        n_sky = sum(1 for c in cams if c["sees_skybox"])
        if not cams:
            data["problems"].append(f"{sname}: 场景里没有 Camera")
        elif n_sky == 0:
            data["problems"].append(
                f"{sname}: {len(cams)} 个相机全部不是 ClearFlags=1(Skybox)"
                f" -> 天空盒永远不会被渲染，画面必然是纯色背景")
        else:
            data["notes"].append(
                f"{sname}: {n_sky}/{len(cams)} 个相机 ClearFlags=1(Skybox)")

        # ---- 场景级天空盒 ----
        rs = RENDER_SETTINGS_RE.search(text)
        if not rs:
            data["problems"].append(f"{sname}: 找不到 RenderSettings 块")
            data["scenes"].append(info)
            continue
        ref = SKYBOX_REF_RE.search(rs.group(1))
        if not ref:
            data["problems"].append(f"{sname}: RenderSettings 里没有 m_SkyboxMaterial")
            data["scenes"].append(info)
            continue
        file_id, guid = ref.group(1), ref.group(2)
        info["skybox"]["guid"] = guid
        if file_id == "0":
            data["problems"].append(
                f"{sname}: m_SkyboxMaterial 是空引用(fileID: 0) -> 没有天空盒")
            data["scenes"].append(info)
            continue

        mats = find_by_guid(guid, ".mat")
        info["skybox"]["materials"] = mats
        if not mats:
            data["problems"].append(
                f"{sname}: 天空盒材质 guid {guid} 在仓库里找不到"
                f"（若是包内材质属正常，但天空盒通常是自研的）")
            data["scenes"].append(info)
            continue
        mpath = mats[0]
        with open(mpath, encoding="utf-8", errors="replace") as f:
            mtext = f.read()
        info["skybox"]["material"] = os.path.relpath(mpath, ROOT)

        # ---- 材质的 shader ----
        sref = SHADER_REF_RE.search(mtext)
        if not sref:
            data["problems"].append(f"{os.path.basename(mpath)}: 材质没有 m_Shader")
            data["scenes"].append(info)
            continue
        sguid = sref.group(1)
        info["skybox"]["shader_guid"] = sguid
        shad_paths = find_by_guid(sguid, ".shader")
        if not shad_paths:
            # 包内 shader（URP 自带 Skybox）会走到这里。不直接判成问题，
            # 因为库缓存里本来就不导出包内资产。
            info["skybox"]["shader"] = None
            data["notes"].append(
                f"天空盒 shader guid {sguid} 不在自有 Assets 里"
                f"（若是 URP 包自带 Skybox 属正常）")
        else:
            spath = shad_paths[0]
            with open(spath, encoding="utf-8", errors="replace") as f:
                stext = f.read()
            snm = re.search(r'^\s*Shader\s+"([^"]+)"', stext, re.M)
            stub = "DummyShaderTextExporter" in stext[:65536]
            tgt = TARGET_RE.search(stext)
            npass = len(re.findall(
                r"(?:^|\{|\})[ \t]*Pass[ \t\r\n]*\{",
                re.sub(r"//[^\n]*", "", stext), re.M))
            info["skybox"].update({
                "shader": os.path.relpath(spath, ROOT),
                "shader_name": snm.group(1) if snm else None,
                "is_stub": stub,
                "target": tgt.group(1) if tgt else None,
                "passes": npass,
            })
            if stub:
                data["problems"].append(
                    f"天空盒 shader 是AssetRipper 空壳 -> 渲不出天空")
            # WebGL2 == GLES 3.0，没有 SSBO。target 3.5+ 会要求它。
            if tgt and float(tgt.group(1)) >= 3.5:
                data["problems"].append(
                    f"天空盒 shader #pragma target {tgt.group(1)}"
                    f" >= 3.5，WebGL2(GLES 3.0) 没有 SSBO，编译会失败")

        # ---- 材质里的颜色数据 ----
        found_colors = [k for k in COLOR_KEYS if f"{k}:" in mtext]
        info["skybox"]["colors"] = found_colors
        if not found_colors:
            data["problems"].append(
                f"{os.path.basename(mpath)}: 天空盒材质里一个颜色字段都没有"
                f" -> 即便shader 正常也只会渲出纯色")
        else:
            data["notes"].append(
                f"天空盒颜色字段 {len(found_colors)} 个: "
                f"{', '.join(found_colors[:5])}"
                f"{' ...' if len(found_colors) > 5 else ''}")

        # ---- 贴图 ----
        # 刻意只报告、不判为问题：程序化天空盒本来就不贴图。
        n_tex = len(re.findall(r"m_Texture:\s*\{fileID:\s*(?!0)\d+", mtext))
        info["skybox"]["textures"] = n_tex
        data["notes"].append(
            f"天空盒材质贴图数 {n_tex}"
            f"（0 是正常的 —— 程序化渐变天空盒不贴图；"
            f"但若原版本该有贴图而这里 0，说明 AssetRipper 没导出）")

        data["scenes"].append(info)

    data["ok"] = not data["problems"]
    if a.json:
        print(json.dumps(data, ensure_ascii=False, indent=2))
    else:
        for s in data["scenes"]:
            print(f"== {s['scene']}")
            for c in s["cameras"]:
                flag = "Skybox" if c["sees_skybox"] else f"ClearFlags={c['clear_flags']}"
                print(f"   相机 &{c['id']}: {flag}")
            sb = s["skybox"]
            if sb:
                print(f"   天空盒材质: {sb.get('material', '（找不到）')}")
                if sb.get("shader"):
                    print(f"     shader: {sb['shader']}"
                          f"  [{sb.get('shader_name')}]")
                    print(f"     target={sb.get('target')}  Pass数={sb.get('passes')}"
                          f"  空壳={sb.get('is_stub')}")
                print(f"     颜色字段: {len(sb.get('colors', []))} 个")
        if data["notes"]:
            print("\n说明：")
            for n in data["notes"]:
                print(f"  - {n}")
        print()
        if data["problems"]:
            print(f"发现 {len(data['problems'])} 个问题：")
            for p in data["problems"]:
                print(f"  ! {p}")
        else:
            print("天空盒链路数据完整。")
            print("**但这不等于画面里就有天空** —— 若截图仍纯黑，原因在"
                  "shader 运行时或没进到渲染天空盒的相机，")
            print("判决权在 CI 的 shader 错误扫描和截图上，不在本脚本。")

    return 0


if __name__ == "__main__":
    sys.exit(main())
