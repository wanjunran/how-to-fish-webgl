#!/usr/bin/env python3
"""把场景/资产里对旧 FishNet DLL（已不存在）的脚本引用重写到源码 .cs 的 GUID。

背景
----
这个项目是 AssetRipper 的导出物。原游戏以 DLL 形式携带 FishNet：
场景里的 NetworkManager / TimeManager / TransportManager / Multipass /
ObserverManager / PredictionManager / PlayerSpawner / DebugManager /
BandwidthDisplay / NetworkTinker 等组件的 m_Script 指向
`guid: 1f191796bb5acf747b48ef6cf55d4ccf`（原 FishNet.Runtime.dll 的 meta，
该 DLL 与 meta 都不在本项目里），Tugboat 指向 `guid: 7285fd9d...`
（AssetRipper 当时分配给反编译脚本的 GUID，同样已不存在）。

引用断裂的后果：Unity 加载场景时这些组件全部 missing，NetworkManager
及其全部子管理器为空 -> Awake 链上先是 NullReferenceException，随后
某个通过虚调用/委托访问缺失对象的路径踩到空函数指针，wasm 直接
"null function" 崩溃（run 37284359206 / 37288739608 的加载 90% 崩溃）。

身份认定依据（见 git 历史）
--------------------------
每个 fileID 对应的类，是按该 MonoBehaviour 块的序列化字段与源码逐一
对照认定的（如 _tickRate/_pingInterval -> TimeManager，_transports ->
Multipass，_playerPrefab -> PlayerSpawner），不是猜的。

规则
----
DLL 脚本引用形如 `m_Script: {fileID: <hash>, guid: 1f191796..., type: 3}`，
源码脚本引用一律 `fileID: 11500000`。替换按 (fileID, guid) 二元组精确
匹配，不影响同文件里其它内容的任何字节。
"""
import pathlib
import sys

DLL_GUID = "1f191796bb5acf747b48ef6cf55d4ccf"

# (旧 fileID, 旧 guid) -> 新 guid（源码 .cs 的 meta GUID，fileID 统一 11500000）
MAPPING = {
    ("712360743", DLL_GUID): "d2c95dfde7d73b54dbbdc23155d35d36",  # NetworkManager
    ("1622004857", DLL_GUID): "7d331f979d46e8e4a9fc90070c596d44",  # ObserverManager
    ("1389819861", DLL_GUID): "211a9f6ec51ddc14f908f5acc0cd0423",  # PlayerSpawner
    ("123909284", DLL_GUID): "34e4a322dca349547989b14021da4e23",   # TransportManager
    ("1925408564", DLL_GUID): "3fdaae44044276a49a52229c1597e33b",  # TimeManager
    ("1490210678", DLL_GUID): "e08bb003fce297d4086cf8cba5aa459a",  # PredictionManager
    ("-447204932", DLL_GUID): "314b449d3505bd24487ba69b61c2fda5",  # Multipass
    ("804016564", DLL_GUID): "6d0962ead4b02a34aae248fccce671ce",   # DebugManager
    ("-1123774612", DLL_GUID): "756c28cd3141c4140ae776188ee26729",  # StatisticsManager
    ("58154073", DLL_GUID): "8bc8f0363ddc75946a958043c5e49a83",    # BandwidthDisplay
    ("1201363339", DLL_GUID): "26b716c41e9b56b4baafaf13a523ba2e",  # NetworkObject
    ("253579214", DLL_GUID): "3ad70174b079c2f4ebc7931d3dd1af6f",   # DefaultPrefabObjects
    # Tugboat：AssetRipper 时代是 11500000 源码格式，但 GUID 已不存在
    ("11500000", "7285fd9de17a5b1a34953aa66a69f60a"): "6f48f002b825cbd45a19bd96d90f9edb",
}

# ===== 第二批：原游戏以 DLL 形式携带的 TMP / Unity Localization =====
# 67dfb1fd... = Unity.TextMeshPro.dll（本项目改用 com.unity.textmeshpro 2.0.1 源码包）
# 76ad3cbe... = Unity.Localization DLL（本项目用 com.unity.localization 1.5.13）
# 两批包内脚本 GUID 由 CI workflow extract-guids.yml 从 Library/PackageCache
# 的 .meta 里直接抓取（权威值），类身份按序列化字段/资产内容认定。
TMP_DLL = "67dfb1fdfb2b407222eda8e23ac8b724"
LOC_DLL = "76ad3cbe9d98d83f7de53e0a92768023"

MAPPING.update({
    ("1453722849", TMP_DLL): "f4688fdb7df04437aeb418b961361dc5",  # TextMeshProUGUI
    ("-1620774994", TMP_DLL): "7b743370ac3e4ec2a1668f5455a8ef8a",  # TMP_Dropdown
    ("2019389346", TMP_DLL): "84a92b25f83d49b9bc132d206b370281",   # TMP_SpriteAsset
    ("-667331979", TMP_DLL): "71c1514a6bd24e1e882cebbe1904ce04",   # TMP_FontAsset
    ("-395462249", TMP_DLL): "2705215ac5b84b70bacc50632be6e391",   # TMP_Settings
    ("-1936749209", TMP_DLL): "ab2114bdc8544297b417dfefe9f1e410",  # TMP_StyleSheet
    ("814988327", LOC_DLL): "56eb0353ae6e5124bb35b17aff880f16",    # LocalizeStringEvent
    ("-622809992", LOC_DLL): "e9620f8c34305754d8cc9a7e49e852d9",   # StringTable
    ("-2074915446", LOC_DLL): "1bb1838fe8befb0429646b938e757ff3",  # Locale
    ("-149288805", LOC_DLL): "97269afb30aa84742ae3f3342603618a",   # AssetTable
    ("-299433217", LOC_DLL): "5be51871efa6c3e4eae1703925c8f5ac",   # StringTableCollection
    ("-179382495", LOC_DLL): "a07b5cd0b1b829245bc8c4b6978793e8",   # LocalizationSettings
})

# ===== 第三批：URP 运行时（c88ab7b3 = Unity.RenderPipelines.Universal.Runtime 的 AssetRipper GUID）=====
# 覆盖 62 个资产的引用：47 处 UniversalAdditionalLightData（每盏灯）、18 处 UniversalAdditionalCameraData
# （每相机）、11 处 DecalProjector（贴花 prefab，DecalManager.SpawnDecal 依赖）+ 20 余个后处理 Volume
# 组件（Vignette/Bloom/Tonemapping...）与 URP 配置资产（PipelineAsset/RendererData/GlobalSettings）。
# 类身份用 fileID 的确定性 hash 逆向认定：fileID = int32_le(MD4(b's\\0\\0\\0' + namespace + classname)[:4])，
# 29/29 全部命中 Graphics 仓库 URP Runtime 类名（先用 TMP 批次已知 3 对验证过算法）；
# GUID 取官方 .cs.meta（Graphics master，Unity 包内资产 GUID 版本间保持稳定）。
URP_DLL = "c88ab7b37c4f350242674d2efd621c19"
MAPPING.update({
    ("304953222", URP_DLL): "572910c10080c0945a0ef731ccedc739",  # PostProcessData
    ("-549186028", URP_DLL): "bf2edee5c58d82540a51f03df9d42094",  # UniversalRenderPipelineAsset
    ("1667301410", URP_DLL): "0777d029ed3dffa4692f417d4aba19ca",  # DecalProjector
    ("464656391", URP_DLL): "0b2db86121404754db890f4c8dfe81b2",  # Bloom
    ("-1142122322", URP_DLL): "cdfbdbb87d3286943a057f7791b43141",  # ChannelMixer
    ("456897975", URP_DLL): "81180773991d8724ab7f2d216912b564",  # ChromaticAberration
    ("-1728269046", URP_DLL): "66f335fb1ffd8684294ad653bf1c7564",  # ColorAdjustments
    ("1948069495", URP_DLL): "3eb4b772797da9440885e8bd939e9560",  # ColorCurves
    ("257616187", URP_DLL): "e021b4c809a781e468c2988c016ebbea",  # ColorLookup
    ("-2085558323", URP_DLL): "c01700fd266d6914ababb731e09af2eb",  # DepthOfField
    ("-534242816", URP_DLL): "29fa0085f50d5e54f8144f766051a691",  # FilmGrain
    ("-1562019045", URP_DLL): "c5e1dc532bcb41949b58bc4f2abfbb7e",  # LensDistortion
    ("282247082", URP_DLL): "5485954d14dfb9a4c8ead8edb0ded5b1",  # LiftGammaGain
    ("224828655", URP_DLL): "ccf1aba9553839d41ae37dd52e9ebcce",  # MotionBlur
    ("-1302714509", URP_DLL): "fb60a22f311433c4c962b888d1393f88",  # PaniniProjection
    ("421016351", URP_DLL): "06437c1ff663d574d9447842ba0a72e4",  # ScreenSpaceLensFlare
    ("815276277", URP_DLL): "558a8e2b6826cf840aae193990ba9f2e",  # ShadowsMidtonesHighlights
    ("-1771661620", URP_DLL): "70afe9e12c7a7ed47911bb608a23a8ff",  # SplitToning
    ("640956576", URP_DLL): "97c23e3b12dc18c42a140437e53d3951",  # Tonemapping
    ("565963466", URP_DLL): "899c54efeace73346a0a16faa3afe726",  # Vignette
    ("-1094021734", URP_DLL): "221518ef91623a7438a71fef23660601",  # WhiteBalance
    ("1619829637", URP_DLL): "a1614fc811f8f184697d9bee70ab9fe5",  # DecalRendererFeature
    ("972750921", URP_DLL): "b00045f12942b46c698459096c89274e",  # FullScreenPassRendererFeature
    ("-1963386888", URP_DLL): "6b3d386ba5cd94485973aee1479b272e",  # RenderObjects
    ("2046858975", URP_DLL): "f62c9c65cf3354c93be831c8bc075510",  # ScreenSpaceAmbientOcclusion
    ("938447500", URP_DLL): "a79441f348de89743a2939f4d699eac1",  # UniversalAdditionalCameraData
    ("-1431003440", URP_DLL): "474bcb49853aa07438625e644c072ee6",  # UniversalAdditionalLightData
    ("474283971", URP_DLL): "2ec995e51a6e251468d2a3fd8a686257",  # UniversalRenderPipelineGlobalSettings
    ("-1575395517", URP_DLL): "de640fe3d0db1804a85f9fc8f5cadab6",  # UniversalRendererData
})

# 无源码对应、保持断裂（missing script 警告无害，功能均为调试/未知空组件）：
#   -771878070  NetworkDebug（ScriptableObject，4.1.0 无此类）-> Debug Logging.asset
#   111782844   DLL 版可实例化的 NetworkBehaviour 具体类 -> PlayerHolder.prefab
#   7a1f73b9... / d92678df...（fileID 11500000 的无字段脚本，.cs 已丢失）-> Game.unity


def main() -> int:
    root = pathlib.Path("Assets")
    targets = [
        p for p in root.rglob("*")
        if p.suffix in (".unity", ".prefab", ".asset") and p.is_file()
    ]
    all_old_guids = {g for (_, g), _ in MAPPING.items()}
    total = 0
    for p in targets:
        text = p.read_text(encoding="utf-8", errors="surrogateescape")
        if not any(g in text for g in all_old_guids):
            continue
        changed = 0
        for (fid, old_guid), new_guid in MAPPING.items():
            old = f"{{fileID: {fid}, guid: {old_guid}, type: 3}}"
            new = f"{{fileID: 11500000, guid: {new_guid}, type: 3}}"
            n = text.count(old)
            if n:
                text = text.replace(old, new)
                changed += n
        # 兜底：数一下还有没有漏网的 DLL 引用（未知 fileID 的组件）
        leftover = text.count(DLL_GUID)
        if changed:
            p.write_text(text, encoding="utf-8", errors="surrogateescape")
            print(f"{p}: 替换 {changed} 处" + (f"，残留 {leftover} 处未知" if leftover else ""))
            total += changed
    print(f"\n合计替换 {total} 处")
    return 0


if __name__ == "__main__":
    sys.exit(main())
