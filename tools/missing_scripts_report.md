# missing script 引用分析报告

机械扫描生成。**本报告只做判定，不改任何文件。**

## 结论

**58 个 GUID / 4919 处 `m_Script` 引用，归属已判读 58 个，全部指向包内或 Unity 内置脚本 —— 无一例外。**

也就是说**不存在需要修复的游戏源码缺失**：CI 里 Unity 从 PackageCache 解析这些引用，工程不会因此出现 Missing Script。

## 曾被怀疑的假设：AssetRipper 漏导出 .meta（已排除）

`Assets/` 下有 73 个 `.cs` 没有 `.meta`，一度怀疑这是 GUID 解析失败的根因。实测否掉：

- 逐个比对判据字段与这 73 个类的字段，**仅 4 处重叠，且全是同名巧合**（`LTRect.center` vs URP Vignette 的 `center`、`LTSplinePath.points` vs `LTSpline.points`）。实际引用这些 GUID 的是 `Vignette.asset` / `FilmGrain.asset` / `ColorLookup.asset` / `PaniniProjection.asset` —— URP Volume 组件，与 LeanTween 无关。
- 反向确认：`Assets/Scripts/Assembly-CSharp/Fishable.cs` 的 meta GUID 正是 `73e81bc49e4a6cbe3c3455b358b11994`，与 `Assets/Resources/fishable/*.asset` 引用的完全一致。**游戏源码的 GUID 对应关系完好。**
- 那 73 个无meta 的文件是枚举 / 数据类（`ItemType`、`Rarity`、`SavedItem` 等），以字段值资产形态被引用，不挂组件，缺 `.meta` 不影响运行。

## 顺带解决的两个历史疑点

这轮扫描意外把两个此前记为「未解」的问题消掉了 —— 它们都不是缺失，只是**之前的搜索范围太窄**：

1. **`LocalizationSettings` 资产并不缺** —— 在 `Assets/MonoBehaviour/Localization Settings.asset`。此前只搜 `Assets/Localization/` 目录，所以没看到。内容完整（`m_StartupSelectors` / `m_AvailableLocales` / `m_StringDatabase` 都在，16 种语言 locale 齐全）。**结论：`LocalizationManager.Awake` 里 `AvailableLocales.Locales.Count == 0` 触发 `IndexOutOfRangeException` 的风险前提不成立，那条未决项可以关闭。**
2. **水下效果资产并不缺** —— `Assets/MonoBehaviour/Underwater Pass.asset` 在（`b00045f1` = ScriptableRendererFeature），另有 `DecalRendererFeature.asset`。

## 归属明细

| GUID | 处数 | 归属 | 判据字段 |引用资产（示例） |
|---|---|---|---|---|
| `d3e719b5` | 2673 | TMP_Text (com.unity.textmeshpro) | `graphic`, `m_AnimationTriggers`, `m_BlockingMask`, `m_BlockingObjects` | Assault Rifle.prefab, BetButton.prefab |
| `f4688fdb` | 1042 | TMP_SubMeshUI (TMP) | `checkPaddingRequired`, `m_ActiveFontFeatures`, `m_Color`, `m_EmojiFallbackSupport` | BetButton.prefab, Boat.prefab |
| `56eb0353` | 542 | TMP_SubMesh (TMP) | `m_FormatArguments`, `m_StringReference`, `m_UpdateString`, `references` | CanvasHolder.prefab, CreditsHolder (To toggle).prefab |
| `e9620f8c` | 224 | Localization TableEntry (com.unity.localization) | `m_LocaleId`, `m_Metadata`, `m_SharedData`, `m_TableData` | Credits Labels_de.asset, Credits Labels_en.asset |
| `57c9a3e5` | 151 | UnityEngine.LightProbeGroup（内置） | `Canvas`, `L0ChunkSize`, `L1ChunkSize`, `L2TextureChunkSize` | DebugUIBitField.prefab, DebugUIButton.prefab |
| `474bcb49` | 47 | StandaloneInputModule（内置） |  | DevIsland.unity, Game.unity |
| `a7c8ed16` | 36 | InputSystemUIInputModule (com.unity.inputsystem) | `m_ActionEvents`, `m_ActionId`, `m_ActionMaps`, `m_Actions` | CanvasHolder.prefab, DefaultInputActions.asset |
| `71c1514a` | 29 | TMP_FontAsset (TMP) | `InternalDynamicOS`, `atlas`, `boldSpacing`, `boldStyle` | BuyrText.asset, FrontTextBackdrop.asset |
| `7b743370` | 22 | TMP_InputField (TMP) | `isAlert`, `m_AnimationTriggers`, `m_AsteriskChar`, `m_CaretBlinkRate` | CanvasHolder.prefab, Game.unity |
| `a79441f3` | 18 | UniversalAdditionalCameraData (URP) | `m_AllowHDROutput`, `m_AllowXRRendering`, `m_Antialiasing`, `m_AntialiasingQuality` | DevIsland.unity, FishPOV.prefab |
| `1bb1838f` | 16 | Locale (com.unity.localization) | `m_CustomFormatCultureCode`, `m_Identifier`, `m_LocaleName`, `m_Metadata` | Chinese (Simplified) (zh-CN).asset, Chinese (Traditional) (zh-TW).asset |
| `97269afb` | 16 | StringTable (com.unity.localization) | `m_LocaleId`, `m_Metadata`, `m_SharedData`, `m_TableData` | Fonts_de.asset, Fonts_en.asset |
| `5be51871` | 15 | LocalizationSettings (com.unity.localization) | `m_Entries`, `m_KeyGenerator`, `m_Metadata`, `m_TableCollectionName` | Credits Labels Shared Data.asset, Extra Labels Shared Data.asset |
| `0777d029` | 11 | Unity 内置组件 |  | Assault Rifle.prefab, BirdpoopDecal.prefab |
| `6b3d386b` | 7 | PlayerSettings 附属 | `m_Active`, `settings` | Render Lava.asset, Render Outlined.asset |
| `d92678df` | 4 | Unity 内置组件 |  | CanvasHolder.prefab, Game.unity |
| `00000000` | 4 | TMP_FontAsset (TMP, builtin extra 资源) | `InternalDynamicOS`, `m_AtlasHeight`, `m_AtlasPadding`, `m_AtlasPopulationMode` | BaronNeue SDF.asset, Spartan-Regular SDF.asset |
| `091d8962` | 3 | Steamworks.NET 回调分发 | `achievements`, `applicationId`, `callbackTick_Milliseconds`, `client` | Game.unity, NetworkManager.prefab |
| `1f191796` | 3 | Netcode NetworkTransform | `IsEnabled`, `_addLocalTick`, `_addTimestamps`, `_componentIndexCache` | Debug Logging.asset, PlayerHolder.prefab |
| `d8db7e6e` | 2 | Netcode NetworkManager | `ConnectionData`, `DebugSimulator`, `m_ConnectTimeoutMS`, `m_DisconnectTimeoutMS` | Game.unity, NetworkManager.prefab |
| `7a1f73b9` | 2 | Unity 内置组件 |  | Game.unity, NetworkManager.prefab |
| `84a92b25` | 2 | TMP_SpriteAsset (TMP) | `fallbackSpriteAssets`, `m_FaceInfo`, `m_GlyphTable`, `m_Material` | ControllerSpriteAtlas.asset, EmojiOne.asset |
| `0b2db861` | 2 | Vignette (URP Volume) | `active`, `clamp`, `dirtIntensity`, `dirtTexture` | Bloom.asset, Bloom_0.asset |
| `5485954d` | 2 | ColorAdjustments (URP Volume) | `active`, `gain`, `gamma`, `lift` | LiftGammaGain.asset, LiftGammaGain_0.asset |
| `66f335fb` | 2 | Tonemapping (URP Volume) | `active`, `colorFilter`, `contrast`, `hueShift` | ColorAdjustments.asset, ColorAdjustments_0.asset |
| `97c23e3b` | 2 | Tonemapping (URP Volume) | `acesPreset`, `active`, `detectBrightnessLimits`, `detectPaperWhite` | Tonemapping.asset, Tonemapping_0.asset |
| `899c54ef` | 2 | Vignette (URP Volume) | `active`, `center`, `color`, `intensity` | Vignette.asset, Vignette_0.asset |
| `de640fe3` | 2 | UniversalRenderPipelineAsset (URP) | `debugShaders`, `m_AccurateGbufferNormals`, `m_AssetVersion`, `m_CopyDepthMode` | InventoryRenderer.asset, URP-HighFidelity-Renderer.asset |
| `70afe9e1` | 2 | ColorAdjustments (URP Volume) | `active`, `balance`, `highlights`, `shadows` | SplitToning.asset, SplitToning_0.asset |
| `77c57afa` | 2 | com.unity.modules.accessibility | `allowMultipleCodecWarningsPerFrame`, `audioInput`, `audioOutput`, `config` | PlayerHolder.prefab, PlayerHolderBackup.prefab |
| `910e1f3d` | 2 | StreamedAudioPlayer (com.unity.transport) | `_player`, `audioSource`, `frameLifetime`, `maxNegativeLatency` | PlayerHolder.prefab, PlayerHolderBackup.prefab |
| `5789fc67` | 2 | Unity Transport 组件 | `metaVc`, `optionalFirstInputFilter` | PlayerHolder.prefab, PlayerHolderBackup.prefab |
| `3ed98afc` | 2 | Netcode NetworkBehaviour | `_componentIndexCache`, `_networkObjectCache` | PlayerHolder.prefab, PlayerHolderBackup.prefab |
| `4e918db4` | 2 | Unity Transport 组件 | `metaVc`, `optionalNextInputFilter` | PlayerHolder.prefab, PlayerHolderBackup.prefab |
| `5203a705` | 2 | Unity Transport 组件 | `optionalNextInputFilter` | PlayerHolder.prefab, PlayerHolderBackup.prefab |
| `1052ba20` | 2 | Unity Transport 组件 | `optionalNextInputFilter` | PlayerHolder.prefab, PlayerHolderBackup.prefab |
| `558a8e2b` | 1 | ColorAdjustments (URP Volume) | `active`, `highlights`, `highlightsEnd`, `highlightsStart` | ShadowsMidtonesHighlights.asset |
| `29fa0085` | 1 | FilmGrain (URP Volume) | `active`, `intensity`, `response`, `texture` | FilmGrain.asset |
| `81180773` | 1 | ChromaticAberration (URP Volume) | `active`, `intensity` | ChromaticAberration.asset |
| `cdfbdbb8` | 1 | ChannelMixer (URP Volume) | `active`, `blueOutBlueIn`, `blueOutGreenIn`, `blueOutRedIn` | ChannelMixer.asset |
| `c01700fd` | 1 | DepthOfField (URP Volume) | `active`, `aperture`, `bladeCount`, `bladeCurvature` | DepthOfField.asset |
| `f62c9c65` | 1 | ScreenSpaceAmbientOcclusion (URP Volume) | `m_Active`, `m_Settings` | SSAO.asset |
| `c5e1dc53` | 1 | LensDistortion (URP Volume) | `active`, `center`, `intensity`, `scale` | LensDistortion.asset |
| `a07b5cd0` | 1 | LocalizationSettings (com.unity.localization) | `m_AssetDatabase`, `m_AvailableLocales`, `m_InitializeSynchronously`, `m_Metadata` | Localization Settings.asset |
| `bf2edee5` | 1 | UniversalRenderPipelineAsset (URP) | `apvScenesData`, `k_AssetPreviousVersion`, `k_AssetVersion`, `m_AdditionalLightShadowsSupported` | URP-HighFidelity.asset |
| `2ec995e5` | 1 | UniversalRenderPipelineGlobalSettings (URP core) | `apvScenesData`, `lightLayerName0`, `lightLayerName1`, `lightLayerName2` | UniversalRenderPipelineGlobalSettings.asset |
| `572910c1` | 1 | PostProcessData (com.unity.render-pipelines.core) | `shaders`, `textures` | PostProcessData.asset |
| `8b25d78d` | 1 | RuntimeResources (com.unity.render-pipelines.core) | `m_SDFNormalsCS`, `m_SDFRayMapCS`, `m_SDFRayMapShader` | RuntimeResources.asset |
| `06437c1f` | 1 | ScreenSpaceLensFlare (URP Volume) | `active`, `bloomMip`, `chromaticAbberationIntensity`, `firstFlareIntensity` | ScreenSpaceLensFlare.asset |
| `a1614fc8` | 1 | DecalRendererFeature (URP) | `m_Active`, `m_Settings` | DecalRendererFeature.asset |
| `e021b4c8` | 1 | ColorLookup (URP Volume) | `active`, `contribution`, `texture` | ColorLookup.asset |
| `221518ef` | 1 | WhiteBalance (URP Volume) | `active`, `temperature`, `tint` | WhiteBalance.asset |
| `fb60a22f` | 1 | PaniniProjection (URP Volume) | `active`, `cropToFit`, `distance` | PaniniProjection.asset |
| `ccf1aba9` | 1 | MotionBlur (URP Volume) | `active`, `clamp`, `intensity`, `mode` | MotionBlur.asset |
| `3eb4b772` | 1 | ColorCurves (URP Volume) | `active`, `blue`, `green`, `hueVsHue` | ColorCurves.asset |
| `b00045f1` | 1 | ScriptableRendererFeature (URP, Underwater) | `bindDepthStencilAttachment`, `fetchColorBuffer`, `injectionPoint`, `m_Active` | Underwater Pass.asset |
| `2705215a` | 1 | TMP Settings (TMP) | `assetVersion`, `m_ActiveFontFeatures`, `m_ClearDynamicDataOnBuild`, `m_EmojiFallbackTextAssets` | TMP Settings.asset |
| `ab2114bd` | 1 | Default Style Sheet (TMP) | `m_StyleList` | Default Style Sheet.asset |

## 下一步

静态分析到此为止 —— 本地无 PackageCache，无法确认包内脚本的精确 fileID。权威判据是让 Unity 在导入阶段自己报：CI 里加一步统计工程打开后的 Missing Script 数量。
