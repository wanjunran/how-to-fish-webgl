#!/usr/bin/env python3
"""把 FishNet 源码搬进 Unity 工程，替掉那个签了对不上的预编译 DLL。

为什么要换
----------
游戏里带的是别人编译好的 `Assets/Plugins/FishNet.Runtime.dll`。
它内部的委托 thunk 签名与当前 Unity 6 的派发 ABI 对不上，
加载场景时call_indirect 直接抛 function signature mismatch
（详见 加载崩溃诊断.md）。

根因是 AssetRipper 反编译时跳过了 FishNet 的 ILPostProcessor，
那 33 个游戏类型的序列化器从来没被生成过。放进源码后，
codegen（Mono.Cecil 后处理器）会在构建时重新生成，签名天然一致。

依赖关系（已验证，全部自包含）
------------------------------
    FishNet/Runtime/FishNet.Runtime.asmdef
    FishNet/Runtime/Plugins/GameKit/Dependencies/GameKit.Dependencies.asmdef
    FishNet/CodeGenerating/Unity.FishNet.CodeGen.asmdef  -> GameKit.Dependencies
    FishNet/CodeGenerating/cecil-0.11.4/MonoFN.Cecil.asmdef

GameKit.Dependencies 就在 FishNet 仓库的 Runtime/Plugins/ 下，
不需要另外去 GameKit 仓库拉——那个仓库主线已经把
Dependencies 拆走了，反而找不到。

codegen 里那三个 .cs 有 `using GameKit.Dependencies.Utilities;`
但一行都没用到（已逐文件核实），是死引用；asmdef 里那条
references 记录是真需要保留的，因为 asmdef 编译要解析。

不做的事
--------
- 不搬 Demos/（35 个文件，纯示例，会拖慢构建还可能引Unity 版本冲突）
- 不碰 Transports 里游戏没用到的部分——但 Multipass 必须带，
  ConnectionManager 在用 _multipass.SetClientTransport / StopConnection
- 不改 FishNet 源码。codegen 是给所有 NetworkBehaviour 生成序列化器，
  改了它就不认识游戏里的类型了

用法：
    python3 tools/install_fishnet_source.py --from /tmp/x-4.1.0/FishNet-4.1.0/Assets/FishNet [--apply]
"""
import argparse
import shutil
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent


def find_unity_assets() -> Path:
    """定位 Unity 工程的 Assets 目录。

    别写死成 ROOT / "ExportedProject" / "Assets"——这个仓库里
    Assets 就在 ROOT / "Assets"，多套一层会装到
    ROOT/ExportedProject/Assets 去，一个 Unity 根本不扫的目录。
    我第一版就是这么错的：脚本"成功"了，DLL 也"移走"了，
    但工程里什么都没变。

    判据：Assets 下面必须有 Scripts/Assembly-CSharp，
    这是 AssetRipper 导出工程的固定结构。
    """
    cands = [ROOT / "Assets", ROOT / "ExportedProject" / "Assets"]
    for c in cands:
        if (c / "Scripts" / "Assembly-CSharp").is_dir():
            return c
    # 兜底：再往下找一层，但只认有 Assembly-CSharp 的
    for c in ROOT.rglob("Assets"):
        if (c / "Scripts" / "Assembly-CSharp").is_dir():
            return c
    raise SystemExit(
        f"找不到 Unity 工程的 Assets 目录。\n"
        f"试过: {', '.join(str(c) for c in cands)}\n"
        f"判据是 Assets/Scripts/Assembly-CSharp 存在。"
    )


ASSETS = find_unity_assets()
PROJ = ASSETS.parent
TARGET = ASSETS / "FishNet"
OLD_DLL = ASSETS / "Plugins" / "FishNet.Runtime.dll"

# 排除的目录。理由写在文件头的 docstring 里。
EXCLUDE_DIRS = {
    "Demos",# 示例工程，游戏不用，且可能引Unity 版本冲突的 API
    "Documentation~",
    ".git",
}

# 一定要带的东西（相对 FishNet 根）。列出来是为了移植后能自检。
REQUIRED = [
    "Runtime/FishNet.Runtime.asmdef",
    "Runtime/Transporting/Transports/Multipass/Multipass.cs",
    "Runtime/Plugins/GameKit/Dependencies/GameKit.Dependencies.asmdef",
    "CodeGenerating/Unity.FishNet.CodeGen.asmdef",
    "CodeGenerating/cecil-0.11.4/MonoFN.Cecil.asmdef",
]


def human(n: float) -> str:
    for u in ("B", "KB", "MB", "GB"):
        if n < 1024 or u == "GB":
            return f"{n:.1f} {u}"
        n /= 1024
    return f"{n:.1f} GB"


def count_files(root: Path) -> tuple[int, int]:
    """返回 (文件数, 字节数)，排除 EXCLUDE_DIRS。"""
    nf = nb = 0
    for p in root.rglob("*"):
        if any(part in EXCLUDE_DIRS for part in p.parts):
            continue
        if p.is_file():
            nf += 1
            nb += p.stat().st_size
    return nf, nb


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--from", dest="src", required=True,
                    help="FishNet 源码根目录（含 Runtime/ 与 CodeGenerating/）")
    ap.add_argument("--apply", action="store_true", help="真正写盘（默认只报告）")
    args = ap.parse_args()

    src = Path(args.src)
    if not src.is_dir():
        print(f"找不到源目录: {src}", file=sys.stderr)
        return 2
    if not (src / "Runtime").is_dir() or not (src / "CodeGenerating").is_dir():
        print(f"{src} 不像 FishNet 源码根（缺 Runtime/ 或 CodeGenerating/）",
              file=sys.stderr)
        return 2

    print(f"源:{src}")
    nf, nb = count_files(src)
    print(f"  可搬文件 {nf} 个 / {human(nb)}（已排除 {', '.join(sorted(EXCLUDE_DIRS))}）")
    print(f"  Runtime       {len(list((src/'Runtime').rglob('*.cs')))} 个 .cs")
    print(f"  CodeGenerating {len(list((src/'CodeGenerating').rglob('*.cs')))} 个 .cs")
    print()

    # 移植后自检用
    print("移植后必须存在:")
    for r in REQUIRED:
        print(f"  {'✓' if (src/r).exists() else '✗ 缺失'} {r}")
    missing = [r for r in REQUIRED if not (src / r).exists()]
    if missing:
        print(f"\n源目录不完整，缺 {len(missing)} 项", file=sys.stderr)
        return 1
    print()

    # 风险提示：旧 DLL 必须删，否则类型冲突
    if OLD_DLL.exists():
        sz = human(OLD_DLL.stat().st_size)
        print("必须处理的冲突:")
        print(f"  {OLD_DLL.relative_to(PROJ)} 存在（{sz}）")
        print("  它和源码版 FishNet.Runtime.asmdef 是同名程序集，")
        print("  留着会 CS0433/CS1704 之类。移植时会被移到备份目录。")
    if (ASSETS / "Scripts" / "Assembly-CSharp" / "GameTypeSerializers.cs").exists():
        print("  Assets/Scripts/Assembly-CSharp/GameTypeSerializers.cs 存在")
        print("  codegen 会生成同类方法，手写版会撞车。移植时会被移到备份目录。")
    if (ASSETS / "Scripts" / "Assembly-CSharp" / "GameTypeSerializersPooled.cs").exists():
        print("  GameTypeSerializersPooled.cs 存在，同上。")
    print()

    if not args.apply:
        print("dry-run。加 --apply 执行。")
        print("注意：codegen 跑起来后，生成的类名是 GeneratedWriters___Internal，")
        print("      而调用点现在指向 GameTypeSerializers(.Pooled)。")
        print("      这一步要等第一次构建看codegen 的实际输出再定。")
        return 0

    # ---- 实际执行 ----
    if TARGET.exists():
        bak = TARGET.with_name("FishNet_old")
        if bak.exists():
            shutil.rmtree(bak)
        shutil.move(str(TARGET), str(bak))
        print(f"已把旧的 {TARGET.name} 移为 {bak.name}")

    shutil.copytree(src, TARGET, ignore=shutil.ignore_patterns(*EXCLUDE_DIRS))
    print(f"已复制 -> {TARGET.relative_to(ROOT)}")

    # 旧 DLL 与手写补丁移到工程外的备份目录。
    #
    # 不放Assets/FishNet/_backup_do_not_compile/——那是 FishNet 的
    # 目录名，Unity 会按 asmdef 规则扫整个 Assets，放进去等于
    # 把冲突源又请回来。放到仓库根的 _fishnet_backup/，Unity 不扫。
    backup = ROOT / "_fishnet_backup"
    backup.mkdir(parents=True, exist_ok=True)
    for f in (OLD_DLL, OLD_DLL.with_suffix(".dll.meta")):
        if f.exists():
            shutil.move(str(f), str(backup / f.name))
            print(f"已移出 {f.name} -> _fishnet_backup/")
    for nm in ("GameTypeSerializers.cs", "GameTypeSerializers.cs.meta",
               "GameTypeSerializersPooled.cs", "GameTypeSerializersPooled.cs.meta"):
        p = ASSETS / "Scripts" / "Assembly-CSharp" / nm
        if p.exists():
            shutil.move(str(p), str(backup / nm))
            print(f"已移出 {nm} -> _fishnet_backup/")

    # 备份目录里不该有 .meta，Unity 若扫到会试图导入
    for m in backup.glob("*.meta"):
        m.unlink()

    print()
    print("搞定。构建前请确认:")
    print(f"  1. {TARGET.relative_to(ROOT)} 下有 Runtime/ 和 CodeGenerating/")
    print("  2. _fishnet_backup/ 里是被移出的旧 DLL 与手写补丁")
    print("     （在仓库根，Unity 不扫，可随时还原）")
    print("  3. 调用点仍指向 GameTypeSerializers，codegen 生成的名字可能不同，")
    print("     等第一次构建的报错再对")
    return 0


if __name__ == "__main__":
    sys.exit(main())
