#!/usr/bin/env python3
"""从 WebGL 产物里剥掉 Windows 专用资源，并可从工程里移除它们。

为什么有这回事
--------------
AssetRipper 反编译时保留了原 Windows 工程的 Addressables 构建产物：

    Assets/StreamingAssets/aa/StandaloneWindows64/    66 MB
    Assets/StreamingAssets/APVStreamingAssets/       23 MB

判断它们是死重量的三条独立证据（都在写这个脚本时实测过）：

1. Assets/StreamingAssets/aa/settings.json 里写死
       "m_buildTarget": "StandaloneWindows64"
2. catalog.bin 里出现的平台标识只有 StandaloneWindows64，
   没有任何 WebGL 路径 —— 也就是说这个 catalog 是照Windows 平台
   生成��，WebGL 运行时按它去找也找不到对应 bundle。
3. Assets/Scripts/ 全目录grep -i addressable 命中 0 处，
   也没有 AssetReference / LoadAssetAsync 调用。
   Addressables 2.9.1 的构建步骤在 AssetRipper 导出时就没了，
   剩下的只是没被清理的中间文件。

合计 89 MB，占WebGL.data（128 MB）的将近七成。

两种模式
--------
--strip <产物目录>  就地改产物：删掉产物里的这两个目录。
--clean             改工程：把这两个目录从 Assets 下移走（不删除，
                    移到 Assets.disabled_platform_assets/，可随时移回）。

默认是 dry-run，只报告要动什么，不动手。

用法：
    python3 tools/strip_platform_assets.py产物目录
    python3 tools/strip_platform_assets.py --clean
    python3 tools/strip_platform_assets.py --restore
"""
import argparse
import pathlib
import shutil
import sys

ROOT = pathlib.Path(__file__).resolve().parent.parent

# 只删这两个，其余 StreamingAssets 内容（catalog.bin 等）留着。
# 它们只有 76 KB，留着不影响体积，而删掉万一日后 Addressables
# 接线回来了会缺东西。
DEAD_DIRS = (
    pathlib.Path("StreamingAssets/aa/StandaloneWindows64"),
    pathlib.Path("StreamingAssets/APVStreamingAssets"),
)

# 保持 catalog 等小文件，供 build/webgl 目录与工程 Assets 同时使用
RELATIVE = DEAD_DIRS


def human(n: int) -> str:
    return f"{n / 1048576:.1f} MB"


def du(p: pathlib.Path) -> int:
    return sum(f.stat().st_size for f in p.rglob("*") if f.is_file()) if p.is_dir() else 0


def report(base: pathlib.Path, label: str) -> int:
    total = 0
    for rel in RELATIVE:
        p = base / rel
        if p.is_dir():
            sz = du(p)
            total += sz
            print(f"  {label} {rel}  {human(sz)}")
        else:
            print(f"  {label} {rel}  （不存在，跳过）")
    return total


def strip_product(product: pathlib.Path, apply: bool) -> int:
    print(f"产物：{product}")
    if not product.is_dir():
        print("  目录不存在", file=sys.stderr)
        return 2
    total = report(product, "待删")
    if not apply:
        print(f"  dry-run：会腾出 {human(total)}。加 --apply 真的执行。")
        return 0
    for rel in RELATIVE:
        p = product / rel
        if p.is_dir():
            shutil.rmtree(p)
            print(f"  已删 {rel}")
    print(f"  完成，腾出 {human(total)}")
    return 0


def move_engine_assets(apply: bool, restore: bool) -> int:
    staging = ROOT / "Assets.disabled_platform_assets"
    total = 0
    print(f"工程：{ROOT / 'Assets'}")
    for rel in RELATIVE:
        src = ROOT / "Assets" / rel
        dst = staging / rel
        if restore:
            if dst.is_dir():
                dst.parent.mkdir(parents=True, exist_ok=True)
                shutil.move(str(dst), str(src))
                print(f"  已移回 {rel}")
            continue
        if not src.is_dir():
            print(f"  {rel}  （不存在，跳过）")
            continue
        sz = du(src)
        total += sz
        print(f"  待移走 {rel}  {human(sz)}")
        if apply:
            dst.parent.mkdir(parents=True, exist_ok=True)
            shutil.move(str(src), str(dst))
            print(f"  已移到 {dst.relative_to(ROOT)}")
    if restore:
        print("  恢复完毕")
    elif not apply:
        print(f"  dry-run：会腾出 {human(total)}。加 --apply 真的执行。")
    else:
        print(f"  完成，腾出 {human(total)}")
        print(f"  想还原：python3 tools/strip_platform_assets.py --restore")
    return 0


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("product", nargs="?", help="产物目录（含 Build/ 的那一层）")
    ap.add_argument("--apply", action="store_true", help="真的执行（默认 dry-run）")
    ap.add_argument("--clean", action="store_true", help="处理工程 Assets 而不是产物")
    ap.add_argument("--restore", action="store_true", help="把 --clean 移走的目录还原")
    args = ap.parse_args()

    if args.restore or args.clean:
        return move_engine_assets(args.apply, args.restore)
    if args.product:
        return strip_product(pathlib.Path(args.product), args.apply)

    ap.print_help()
    return 2


if __name__ == "__main__":
    sys.exit(main())
