using BetterExperience.BConfigManager;
using BetterExperience.BLogSpace;
using HarmonyLib;
using nel;
using Spine;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityModBase.HTranslatorSpace;
using XX;
using Object = UnityEngine.Object;

namespace BetterExperience.Patches.ReplaceTexture
{
    // All hooks and resource creation run on the Unity main thread.
    internal static class PortraitRuntime
    {
        private sealed class State
        {
            internal Bundle Current;
            internal int Attempt = -1;
            internal string JsonKey;
        }

        private sealed class Bundle : IDisposable
        {
            internal PortraitPackage Package;
            internal Texture2D Image;
            internal TextAsset JsonText;
            internal TextAsset AtlasText;
            internal SpineAtlasAsset Atlas;
            internal SkeletonDataAsset Data;
            internal Material StagingMaterial;

            internal void Bind(Material[] materials)
            {
                Atlas.materials = materials;
                foreach (var page in Atlas.GetAtlas(false).Pages) page.rendererObject = materials[0];
            }

            public void Dispose()
            {
                if (Data != null) Object.Destroy(Data);
                if (Atlas != null) Object.Destroy(Atlas);
                if (JsonText != null) Object.Destroy(JsonText);
                if (AtlasText != null) Object.Destroy(AtlasText);
                if (StagingMaterial != null) Object.Destroy(StagingMaterial);
                if (Image != null) Object.Destroy(Image);
            }
        }

        private sealed class MaterialLoader : TextureLoader
        {
            private readonly Material material;
            internal MaterialLoader(Material material) { this.material = material; }
            public void Load(AtlasPage page, string path) { page.rendererObject = material; }
            public void Unload(object texture) { }
        }

        private static readonly Dictionary<BetobetoManager.SvTexture, State> states = new Dictionary<BetobetoManager.SvTexture, State>();
        private static readonly List<WeakReference> viewers = new List<WeakReference>();
        private static readonly List<Bundle> retired = new List<Bundle>();
        private static PortraitCatalog catalog = new PortraitCatalog();
        private static string settings;
        private static int revision;
        private static bool stopped;
        private static bool initialized;
        private static readonly FieldInfo svAtlas = AccessTools.Field(typeof(BetobetoManager.SvTexture), "SpAtlasAsset");
        private static readonly FieldInfo svData = AccessTools.Field(typeof(BetobetoManager.SvTexture), "SpDataAsset");
        private static readonly FieldInfo depth = AccessTools.Field(typeof(BetobetoManager.SvTexture), "AZBufRect");
        private static readonly MethodInfo allocate = AccessTools.Method(typeof(BetobetoManager.SvTexture), "allocTexture");
        private static readonly FieldInfo viewerAtlas = AccessTools.Field(typeof(SpineViewer), "SpAtlasAsset");
        private static readonly FieldInfo viewerData = AccessTools.Field(typeof(SpineViewer), "SpDataAsset");
        private static readonly FieldInfo viewerContainer = AccessTools.Field(typeof(SpineViewer), "AtlasContainer");
        private static readonly FieldInfo viewerTexture = AccessTools.Field(typeof(SpineViewer), "Tex");
        private static readonly FieldInfo animator = AccessTools.Field(typeof(SpineViewer), "charaAnim");
        private static readonly FieldInfo reserved = AccessTools.Field(typeof(SpineViewer), "ORsvData");

        internal static bool HasActive(BetobetoManager.SvTexture texture)
        {
            return states.TryGetValue(texture, out var state) && state.Current != null;
        }

        internal static bool IsReservedImage(string path) => catalog.ReservedImages.Contains(Path.GetFullPath(path));
        internal static bool HasWork => states.Values.Any(state => state.Current != null);

        internal static void Initialize()
        {
            if (new MemberInfo[] { svAtlas, svData, depth, allocate, viewerAtlas, viewerData, viewerContainer, viewerTexture, animator, reserved }.Any(m => m == null))
            {
                BLog.Warn("Portrait replacement disabled: game Spine viewer interfaces do not match.");
                return;
            }
            initialized = true;
            stopped = false;
            Reload();
        }

        private static string Settings => stopped + "|" + ConfigManager.EnableReplacePortrait?.Value + "|"
            + string.Join("\n", EnabledIds()) + "|" + ConfigManager.EnableSensitivities?.Value;

        internal static void PollSettings()
        {
            if (!initialized) return;
            if (settings != Settings) Reload();
            Collect();
        }

        internal static void Reload()
        {
            if (!initialized) return;
            settings = Settings;
            try
            {
                // 先全量发现并把扫描结果同步进启用列表（自动追加新包行），再按行内开关激活；扫描失败时沿用上一个目录。
                catalog = PortraitCatalog.Discover(PatchInfo.ReplaceImagePath, PatchInfo.ReplaceSensitiveImagePath,
                    ConfigManager.EnableSensitivities?.Value == true);
                SyncPackRows(catalog);
                PortraitCatalog.Activate(catalog, !stopped && ConfigManager.EnableReplacePortrait?.Value == true,
                    EnabledIds());
                foreach (string error in catalog.Errors) BLog.Warn("Portrait: " + error);
            }
            catch (Exception ex) { BLog.Error("Portrait discovery failed; keeping the previous catalog.", ex); }
            revision++;
        }

        private static HashSet<string> EnabledIds()
        {
            return PortraitCatalog.EnabledIds(ConfigManager.EnabledPortraitPacks?.Value);
        }

        // 仅在行集合真的变化时写回，避免每次刷新都触发配置事件并打断界面上未提交的列表编辑。
        private static void SyncPackRows(PortraitCatalog scanned)
        {
            var entry = ConfigManager.EnabledPortraitPacks;
            if (entry == null) return;
            var current = entry.Value ?? new List<(string, bool)>();
            var synced = PortraitCatalog.SyncRows(scanned.Discovered, current);
            if (!synced.SequenceEqual(current)) entry.Value = synced;
        }

        /// <summary>
        /// 刷新贴图热键入口：重扫目录后让已显示的立绘立即走一次游戏自身的切换流程。
        /// clearAnim 前缀会触发 Change 重建 Bundle，游戏本体则重建网格与渲染目标，
        /// 因此改文件、新启用、停用三种情况都会立即生效；动画会从头重播。
        /// </summary>
        internal static void Refresh()
        {
            Reload();
            foreach (var viewer in LiveViewers())
            {
                try
                {
                    var texture = viewer.getSvTexture();
                    if (texture == null) continue;
                    string key = viewer.replace_json_key ?? texture.MtiText.default_json_key;
                    // 只处理与立绘包相关的 viewer：已激活，或当前目标在新目录中存在候选包（新启用）。
                    if (!HasActive(texture) && !catalog.Packages.ContainsKey(texture.key + "\n" + key)) continue;
                    string anim = viewer.getBaseAnimName();
                    // 尚未开始动画的 viewer 没有可恢复的播放状态，留给下一次自然切换。
                    if (anim == null) continue;
                    var entry = viewer.getTrack(0);
                    int loopFrame = entry?.Animation == null ? -1000 : viewer.getAnmLoopFrame(entry.Animation);
                    viewer.clearAnim(anim, loopFrame);
                }
                catch (Exception ex)
                {
                    BLog.Error("Portrait refresh failed for one viewer.", ex);
                }
            }
            Collect();
        }

        internal static void Stop()
        {
            // Existing renderers keep their assets until a safe switch, even when the plugin component stops.
            stopped = true;
            Reload();
        }

        internal static void Register(SpineViewerNel viewer)
        {
            if (!initialized) return;
            if (!viewers.Any(weak => ReferenceEquals(weak.Target, viewer))) viewers.Add(new WeakReference(viewer));
        }

        internal static void BeforeSwitch(SpineViewerNel viewer, string jsonKey = null)
        {
            if (!initialized) return;
            Register(viewer);
            var texture = viewer.getSvTexture();
            string key = jsonKey ?? viewer.replace_json_key ?? texture.MtiText.default_json_key;
            Change(texture, key, viewer.getMaterial(), viewer);
            // switchSkeletonJson calls prepareMaterial with the default key if these fields are empty.
            if (jsonKey != null && states.TryGetValue(texture, out var state) && state.Current != null && state.JsonKey == key)
            {
                viewerAtlas.SetValue(viewer, state.Current.Atlas);
                viewerData.SetValue(viewer, state.Current.Data);
                viewerContainer.SetValue(viewer, state.Current.Atlas.GetAtlas(false));
            }
        }

        internal static bool Prepare(BetobetoManager.SvTexture texture, string jsonKey, Material[] materials,
            out SpineAtlasAsset atlas, out SkeletonDataAsset data)
        {
            atlas = null;
            data = null;
            if (!initialized) return false;
            string key = jsonKey ?? texture.MtiText.default_json_key;
            // First preparation is already a safe boundary; subsequent changes only happen in clearAnim/switchSkeletonJson.
            if (!states.TryGetValue(texture, out var existing) || existing.Attempt < 0) Change(texture, key, materials[0], null);
            if (!states.TryGetValue(texture, out var state) || state.Current == null) return false;
            var bundle = state.Current;
            if (state.JsonKey != key) throw new InvalidOperationException("Portrait JSON variant changed outside a safe switch.");
            bundle.Bind(materials);
            svAtlas.SetValue(texture, bundle.Atlas);
            svData.SetValue(texture, bundle.Data);
            texture.preapreAtlasDepth();
            atlas = bundle.Atlas;
            data = bundle.Data;
            return true;
        }

        private static bool KeepPrevious(Bundle bundle, bool switchingPackage = false)
        {
            return bundle != null && PortraitCatalog.CanRetain(bundle.Package, PatchInfo.ReplaceImagePath, PatchInfo.ReplaceSensitiveImagePath,
                ConfigManager.EnableSensitivities?.Value == true, !stopped && ConfigManager.EnableReplacePortrait?.Value == true,
                EnabledIds(), switchingPackage);
        }

        private static void Change(BetobetoManager.SvTexture texture, string key, Material material, SpineViewerNel switching)
        {
            if (!states.TryGetValue(texture, out var state)) states.Add(texture, state = new State());
            if (state.Attempt == revision && state.JsonKey == key) return;
            if (material == null) return; // During CSV construction the game has not assigned its material yet.
            var old = state.Current;
            var switchingAnimator = switching == null ? null : animator.GetValue(switching);
            if (LiveViewers().Any(v => v != switching && v.enabled && v.getSvTexture() == texture
                && !ReferenceEquals(animator.GetValue(v), switchingAnimator))) return;
            string identity = texture.key + "\n" + key;
            Bundle candidate = null;
            bool keep = old != null && state.JsonKey == key && KeepPrevious(old);
            if (!stopped && ConfigManager.EnableReplacePortrait?.Value == true && catalog.Packages.TryGetValue(identity, out var package)
                && EnabledIds().Contains(package.Id))
            {
                try { candidate = Build(texture, package, material); }
                catch (Exception ex)
                {
                    BLog.Error("Portrait rejected " + package.Id + "; retaining the last usable resource.", ex);
                    if (old != null && state.JsonKey == key && KeepPrevious(old, true)) { state.Attempt = revision; return; }
                }
            }
            else if (keep)
            {
                // Invalid/conflicting edits retain the last working bundle; deleting or deselecting its manifest restores vanilla.
                state.Attempt = revision;
                return;
            }
            state.Attempt = revision;
            state.JsonKey = key;
            if (old == null && candidate == null) return;
            // No Unity objects visible to renderers were changed before this point.
            Invalidate(texture, old);
            state.Current = candidate;
            texture.releaseTexture();
            depth.SetValue(texture, null);
            texture.atlas_depth_written = false;
            svAtlas.SetValue(texture, candidate?.Atlas);
            svData.SetValue(texture, candidate?.Data);
            if (old != null) retired.Add(old);
            BLog.Info(candidate == null ? "Portrait restored: " + texture.key : "Portrait activated: " + candidate.Package.Id);
        }

        private static Bundle Build(BetobetoManager.SvTexture texture, PortraitPackage package, Material material)
        {
            var bundle = new Bundle { Package = package };
            try
            {
                PortraitCatalog.Allowed(PatchInfo.ReplaceSensitiveImagePath, ConfigManager.EnableSensitivities?.Value == true,
                    package.ManifestPath, package.JsonPath, package.AtlasPath, package.ImagePath);
                foreach (string path in new[] { package.ManifestPath, package.JsonPath, package.AtlasPath, package.ImagePath })
                    PortraitCatalog.Resolve(PatchInfo.ReplaceImagePath, Path.GetDirectoryName(path), Path.GetFileName(path));
                texture.MtiText.addLoadKey("_SV");
                SpineViewer.prepareAtlasAssetsS(texture.MtiText, out var originalAtlas, out var original, package.JsonKey);
                string merged;
                var atlas = PortraitCatalog.ReadAtlas(package.AtlasText);
                {
                    if (atlas.Pages[0].width > SystemInfo.maxTextureSize || atlas.Pages[0].height > SystemInfo.maxTextureSize)
                        throw new InvalidDataException("Portrait atlas exceeds the device texture size limit.");
                    merged = PortraitMerger.Merge(original.skeletonJSON.text, package.Json, atlas);
                }
                bundle.Image = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!bundle.Image.LoadImage(package.Image)) throw new InvalidDataException("PNG decoding failed.");
                bundle.Image.name = Path.GetFileNameWithoutExtension(package.ImagePath);
                bundle.Image.wrapMode = TextureWrapMode.Clamp;
                bundle.Image.filterMode = FilterMode.Bilinear;
                bundle.StagingMaterial = new Material(material) { mainTexture = bundle.Image };
                bundle.AtlasText = new TextAsset(package.AtlasText);
                bundle.JsonText = new TextAsset(merged);
                bundle.Atlas = SpineAtlasAsset.CreateRuntimeInstance(bundle.AtlasText, new[] { bundle.StagingMaterial }, false,
                    asset => new MaterialLoader(bundle.StagingMaterial));
                bundle.Data = ScriptableObject.CreateInstance<SkeletonDataAsset>();
                bundle.Data.atlasAssets = new AtlasAssetBase[] { bundle.Atlas };
                bundle.Data.skeletonJSON = bundle.JsonText;
                bundle.Data.scale = original.scale;
                bundle.Data.defaultMix = original.defaultMix;
                bundle.Data.fromAnimation = original.fromAnimation == null ? new string[0] : (string[])original.fromAnimation.Clone();
                bundle.Data.toAnimation = original.toAnimation == null ? new string[0] : (string[])original.toAnimation.Clone();
                bundle.Data.duration = original.duration == null ? new float[0] : (float[])original.duration.Clone();
                if (bundle.Data.GetSkeletonData(false) == null) throw new InvalidDataException("Game Spine parser rejected the portrait.");
                bundle.Bind(new[] { material });
                return bundle;
            }
            catch { bundle.Dispose(); throw; }
        }

        internal static bool Clean(BetobetoManager.SvTexture texture)
        {
            if (!states.TryGetValue(texture, out var state) || state.Current == null) return false;
            var image = state.Current.Image;
            var previousTarget = RenderTexture.active;
            try
            {
                allocate.Invoke(texture, new object[] { image.width, image.height, texture.key });
                var target = texture.getRendered();
                BLIT.PasteTo(target, image, target.width * 0.5f, target.height * 0.5f, 1f);
                texture.dirt_index = 0;
                return true;
            }
            finally { RenderTexture.active = previousTarget; }
        }

        private static IEnumerable<SpineViewerNel> LiveViewers()
        {
            viewers.RemoveAll(weak => !weak.IsAlive);
            return viewers.Select(weak => weak.Target).OfType<SpineViewerNel>().ToArray();
        }

        private static void Invalidate(BetobetoManager.SvTexture texture, Bundle old)
        {
            foreach (var viewer in LiveViewers().Where(v => v.getSvTexture() == texture))
            {
                viewerAtlas.SetValue(viewer, null);
                viewerData.SetValue(viewer, null);
                viewerContainer.SetValue(viewer, null);
                viewerTexture.SetValue(viewer, null);
                if (old != null && reserved.GetValue(viewer) is IDictionary cache) cache.Remove(old.Data);
            }
        }

        internal static void Released(BetobetoManager.SvTexture texture)
        {
            if (!states.TryGetValue(texture, out var state)) return;
            Invalidate(texture, state.Current);
            if (state.Current != null) retired.Add(state.Current);
            states.Remove(texture);
            depth.SetValue(texture, null);
            Collect();
        }

        internal static void Collect()
        {
            if (retired.Count == 0) return;
            var live = LiveViewers().Select(v => animator.GetValue(v) as SkeletonAnimation).Where(a => a != null).ToArray();
            for (int i = retired.Count - 1; i >= 0; i--)
            {
                if (live.Any(a => a.skeletonDataAsset == retired[i].Data)) continue;
                retired[i].Dispose();
                retired.RemoveAt(i);
            }
        }
    }

    [HarmonyPatch]
    internal static class PortraitResourcePatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(BetobetoManager.SvTexture), nameof(BetobetoManager.SvTexture.prepareAtlasAssets))]
        private static bool PrepareAtlasPrefix(BetobetoManager.SvTexture __instance, ref SpineAtlasAsset _SpAtlasAsset,
            ref SkeletonDataAsset _SpDataAsset, Material[] AMtr, string replace_json_key)
        {
            if (!PortraitRuntime.Prepare(__instance, replace_json_key, AMtr, out var atlas, out var data)) return true;
            _SpAtlasAsset = atlas;
            _SpDataAsset = data;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BetobetoManager.SvTexture), nameof(BetobetoManager.SvTexture.cleanExecute))]
        private static bool Clean(BetobetoManager.SvTexture __instance, ref bool __result)
        {
            if (!PortraitRuntime.HasActive(__instance)) return true;
            __result = PortraitRuntime.Clean(__instance);
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BetobetoManager.SvTexture), nameof(BetobetoManager.SvTexture.releaseAtlasData))]
        private static void Release(BetobetoManager.SvTexture __instance) => PortraitRuntime.Released(__instance);
    }

    [HarmonyPatch]
    internal static class PortraitViewerPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SpineViewerNel), "prepareAtlasAssets")]
        private static void Register(SpineViewerNel __instance) => PortraitRuntime.Register(__instance);

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SpineViewerNel), nameof(SpineViewerNel.clearAnim))]
        private static void BeforeSwitch(SpineViewerNel __instance) => PortraitRuntime.BeforeSwitch(__instance);

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SpineViewerNel), nameof(SpineViewerNel.clearAnim))]
        private static void AfterSwitch() => PortraitRuntime.Collect();

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SpineViewer), nameof(SpineViewer.switchSkeletonJson))]
        private static void SwitchJson(SpineViewer __instance, string _jsonkey)
        {
            if (__instance is SpineViewerNel viewer) PortraitRuntime.BeforeSwitch(viewer, _jsonkey);
        }
    }
}
