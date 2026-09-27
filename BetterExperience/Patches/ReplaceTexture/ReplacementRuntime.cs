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
using XX;
using Object = UnityEngine.Object;

namespace BetterExperience.Patches.ReplaceTexture
{
    // Unity resources and all replacement state are touched only from the Unity main thread.
    internal static class ReplacementRuntime
    {
        private sealed class SpineState
        {
            internal SpineBundle Current;
            internal int Attempt = -1;
            internal string JsonKey;
            internal string PendingKey;
            internal ReplacementWork<PreparedSpine> Pending;
            internal SkeletonDataAsset PendingOriginal;
            internal float PendingSince;
        }

        private sealed class SpineBundle : IDisposable
        {
            internal readonly List<ReplacementPackage> Sources = new List<ReplacementPackage>();
            internal Texture Image;
            internal bool OwnImage;
            internal TextAsset JsonText;
            internal TextAsset AtlasText;
            internal SpineAtlasAsset Atlas;
            internal SkeletonDataAsset Data;
            internal Material StagingMaterial;
            internal SpineCompositionResult Composition;

            internal void Bind(Material[] materials)
            {
                if (materials == null || materials.Length == 0) throw new InvalidOperationException("Spine materials are missing.");
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
                if (OwnImage && Image != null) Object.Destroy(Image);
            }
        }

        private sealed class MaterialLoader : TextureLoader
        {
            private readonly Material material;
            internal MaterialLoader(Material material) { this.material = material; }
            public void Load(AtlasPage page, string path) { page.rendererObject = material; }
            public void Unload(object texture) { }
        }

        private sealed class MtiRecord
        {
            internal MTIOneImage Container;
            internal string AssetKey;
            internal string ImageKey;
            internal MImage Image;
            internal Texture Original;
            internal Texture2D Replacement;
            internal ReplacementPackage Source;
            internal string SourceIdentity;
            internal int Attempt = -1;
            internal ReplacementWork<byte[]> Pending;
        }

        private sealed class ResourceRecord
        {
            internal string Path;
            internal string ObjectType;
            internal Object Original;
            internal Texture2D Texture;
            internal Object Replacement;
            internal ReplacementPackage Source;
            internal string SourceIdentity;
            internal int Attempt = -1;
            internal ReplacementWork<byte[]> Pending;
        }

        private static readonly Dictionary<BetobetoManager.SvTexture, SpineState> spineStates =
            new Dictionary<BetobetoManager.SvTexture, SpineState>();
        private static readonly List<WeakReference> viewers = new List<WeakReference>();
        private static readonly List<SpineBundle> retired = new List<SpineBundle>();
        private static readonly Dictionary<MTIOneImage, MtiRecord> mtiRecords = new Dictionary<MTIOneImage, MtiRecord>();
        private static readonly Dictionary<string, ResourceRecord> resourceRecords =
            new Dictionary<string, ResourceRecord>(StringComparer.Ordinal);
        private static ReplacementCatalog catalog = new ReplacementCatalog();
        private static readonly ReplacementSelectionDelay selectionDelay = new ReplacementSelectionDelay();
        private static ReplacementSelection selection = new ReplacementSelection(catalog, null, false, false);
        private static ReplacementWork<ReplacementCatalog> scan;
        private static bool lastSensitive;
        private static int revision;
        private static int displayRevision;
        private static int uploadFrame = -1;
        internal static int Revision => displayRevision;
        private static bool stopped;
        private static bool initialized;
        private static bool spineAvailable;

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
        private static readonly FieldInfo mtiImage = AccessTools.Field(typeof(MTIOneImage), "LImage_");
        private static readonly FieldInfo clipping = AccessTools.Field(typeof(SkeletonRenderer), "useClipping");
        private static readonly FieldInfo cachedAtlas = AccessTools.Field(typeof(SpineAtlasAsset), "atlas");
        private static readonly MethodInfo initializeData = AccessTools.Method(typeof(SkeletonDataAsset), "InitializeWithData", new[] { typeof(SkeletonData) });

        internal static bool HasWork => spineStates.Values.Any(state => state.Current != null)
            || mtiRecords.Values.Any(record => record.Replacement != null)
            || resourceRecords.Values.Any(record => record.Replacement != null);

        internal static bool HasActive(BetobetoManager.SvTexture texture)
        {
            return texture != null && spineStates.TryGetValue(texture, out var state) && state.Current != null;
        }

        internal static void Initialize()
        {
            initialized = true;
            stopped = false;
            spineAvailable = new MemberInfo[]
            {
                svAtlas, svData, depth, allocate, viewerAtlas, viewerData, viewerContainer,
                viewerTexture, animator, reserved, cachedAtlas, initializeData
            }.All(member => member != null);
            if (!spineAvailable) BLog.Warn("Spine resource replacement disabled: game interfaces do not match.");
            if (mtiImage == null) BLog.Warn("MTI resource replacement disabled: image container interface does not match.");
            Directory.CreateDirectory(PatchInfo.ReplaceImagePath);
            Directory.CreateDirectory(PatchInfo.ReplaceSensitiveImagePath);
            lastSensitive = ConfigManager.EnableSensitivities?.Value == true;
            selectionDelay.Reset(Settings);
            // 初次 Resources.Load 的返回对象会被游戏持有；必须在注册补丁前建立完整目录。
            // 仅启动时同步扫描，游戏内开关变化使用目录快照，手动刷新走后台扫描。
            try
            {
                AcceptCatalog(ReplacementCatalog.Discover(PatchInfo.ReplaceImagePath,
                    PatchInfo.ReplaceSensitiveImagePath, lastSensitive));
            }
            catch (Exception ex) { BLog.Error("Initial replacement discovery failed.", ex); }
        }

        private static string Settings => stopped + "|" + ConfigManager.EnableResourceReplacement?.Value + "|"
            + string.Join("\n", EnabledIds()) + "|" + ConfigManager.EnableSensitivities?.Value;

        internal static void PollSettings()
        {
            if (!initialized || stopped) return;
            bool sensitive = ConfigManager.EnableSensitivities?.Value == true;
            if (sensitive != lastSensitive)
            {
                lastSensitive = sensitive;
                scan?.Dispose();
                scan = null;
                // 撤销敏感授权立即生效；重新授权时后台补扫可能未载入的敏感目录。
                ApplySelection(false);
                Reload();
            }
            if (scan != null && scan.TryTake(out var scanned, out var error))
            {
                scan = null;
                if (error != null) BLog.Error("Replacement discovery failed; keeping the previous catalog.", error);
                else AcceptCatalog(scanned);
            }
            if (selectionDelay.Ready(Settings, Time.unscaledTime, !Enabled)) ApplySelection(false);
            if (selectionDelay.Waiting) return;
            RetryMtiRecords();
            RefreshResourceRecords();
            PumpSpines();
            Collect();
        }

        private static void AcceptCatalog(ReplacementCatalog scanned)
        {
            SyncPackRows(scanned);
            catalog = scanned;
            foreach (string message in catalog.Errors) BLog.Warn("Replacement: " + message);
            ApplySelection(true);
            BLog.Info("Resource replacement catalog refreshed.");
        }

        internal static void Reload()
        {
            if (!initialized || stopped) return;
            scan?.Dispose();
            string root = PatchInfo.ReplaceImagePath, sensitive = PatchInfo.ReplaceSensitiveImagePath;
            bool allow = ConfigManager.EnableSensitivities?.Value == true;
            scan = new ReplacementWork<ReplacementCatalog>(token => ReplacementCatalog.Discover(root, sensitive, allow, token));
        }

        private static void ApplySelection(bool force)
        {
            var previous = selection;
            selection = new ReplacementSelection(catalog, EnabledIds(), Enabled, ConfigManager.EnableSensitivities?.Value == true);
            selectionDelay.Reset(Settings);
            revision++;
            foreach (var pair in spineStates)
            {
                var state = pair.Value;
                string identity = SpineIdentity(pair.Key.key, state.PendingKey ?? state.JsonKey);
                if (!force && previous.SameSpine(selection, identity)
                    && (state.Current == null || CanRetain(state.Current.Sources, identity)))
                { if (state.Attempt >= 0) state.Attempt = revision; }
                else { Cancel(state); state.Attempt = -1; }
            }
            foreach (var record in mtiRecords.Values)
            {
                if (!force && previous.SameTexture(selection, "mti", record.AssetKey, record.ImageKey, null)
                    && (record.Source == null || CanRetain(new[] { record.Source }, record.SourceIdentity))) record.Attempt = revision;
                else { record.Pending?.Dispose(); record.Pending = null; record.Attempt = -1; }
            }
            foreach (var record in resourceRecords.Values)
            {
                if (!force && previous.SameTexture(selection, "resources", record.Path, null, record.ObjectType)
                    && (record.Source == null || CanRetain(new[] { record.Source }, record.SourceIdentity))) record.Attempt = revision;
                else { record.Pending?.Dispose(); record.Pending = null; record.Attempt = -1; }
            }
            RetryMtiRecords();
            RefreshResourceRecords();
            RefreshMtiSpineTextures(previous, force);
            foreach (var viewer in LiveViewers())
            {
                var texture = viewer.getSvTexture();
                if (texture == null || viewer.getBaseAnimName() == null) continue;
                string key = viewer.replace_json_key ?? texture.MtiText.default_json_key;
                string identity = SpineIdentity(texture.key, key);
                if ((!force && previous.SameSpine(selection, identity)
                    && (!spineStates.TryGetValue(texture, out var unchanged) || unchanged.Attempt == revision))
                    || (!HasActive(texture) && !HasLayers(identity))) continue;
                try
                {
                    if (!viewer.enabled)
                    {
                        // 隐藏姿态等下次显示时再准备，但撤销授权的旧资源立即释放。
                        if (spineStates.TryGetValue(texture, out var hidden) && hidden.Current != null
                            && !CanRetain(hidden.Current.Sources, identity) && Install(texture, hidden, key, null))
                        {
                            Replay(viewer);
                            hidden.Attempt = -1;
                        }
                        continue;
                    }
                    if (Change(texture, key, viewer.getMaterial(), viewer)) Replay(viewer);
                }
                catch (Exception ex) { BLog.Error("Replacement refresh failed for one Spine viewer.", ex); }
            }
        }

        private static List<string> EnabledIds() =>
            ReplacementCatalog.EnabledIds(ConfigManager.EnabledReplacementPacks?.Value);

        private static bool Enabled => !stopped && ConfigManager.EnableResourceReplacement?.Value == true;

        // 清单已从磁盘消失的包，其配置行在每轮扫描后自动清除；敏感开关关闭或清单解析失败都不影响判定，
        // 因为这些包的 id 仍然登记在 DeclaredIds 中。
        private static void SyncPackRows(ReplacementCatalog scanned)
        {
            var entry = ConfigManager.EnabledReplacementPacks;
            if (entry == null) return;
            var current = entry.Value ?? new List<(string, bool)>();
            var synced = scanned.SyncRows(current);
            if (synced.SequenceEqual(current)) return;
            foreach (string removed in RemovedIds(current, synced))
                BLog.Info("Removed the replacement pack row because its manifest no longer exists: " + removed);
            entry.Value = synced;
        }

        private static List<string> RemovedIds(IEnumerable<(string Id, bool Enabled)> current,
            IEnumerable<(string Id, bool Enabled)> synced)
        {
            var kept = new HashSet<string>(synced.Select(row => row.Id?.Trim())
                .Where(id => !string.IsNullOrEmpty(id)), StringComparer.Ordinal);
            var removed = new List<string>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var row in current)
            {
                string id = row.Id?.Trim();
                if (!string.IsNullOrEmpty(id) && !kept.Contains(id) && seen.Add(id)) removed.Add(id);
            }
            return removed;
        }

        internal static void Refresh()
        {
            Reload();
        }

        private static void PumpSpines()
        {
            var visible = new HashSet<BetobetoManager.SvTexture>();
            foreach (var viewer in LiveViewers())
            {
                try
                {
                    var texture = viewer.getSvTexture();
                    if (texture == null || !viewer.enabled) continue;
                    visible.Add(texture);
                    if (!spineStates.TryGetValue(texture, out var state)) continue;
                    if (state.Pending == null ? state.Attempt >= 0 : !state.Pending.IsCompleted) continue;
                    string key = viewer.replace_json_key ?? texture.MtiText.default_json_key;
                    if ((state.Pending == null || key == state.PendingKey) && viewer.getBaseAnimName() != null
                        && Change(texture, key, viewer.getMaterial(), viewer)) Replay(viewer);
                }
                catch (Exception ex) { BLog.Error("Replacement refresh failed for one Spine viewer.", ex); }
            }
            // 已离开画面的请求不长期持有临时明文；再次显示时可重新准备。
            foreach (var pair in spineStates)
                if (!visible.Contains(pair.Key) && pair.Value.Pending != null
                    && Time.unscaledTime - pair.Value.PendingSince > 15f)
                { Cancel(pair.Value); pair.Value.Attempt = -1; }
        }

        private static void Replay(SpineViewerNel viewer)
        {
            string animation = viewer.getBaseAnimName();
            if (animation == null) return;
            string[] skins = CaptureSkinNames(viewer.GetSkeleton()?.SkinList);
            var entry = viewer.getTrack(0);
            int loopFrame = entry?.Animation == null ? -1000 : viewer.getAnmLoopFrame(entry.Animation);
            viewer.clearAnim(animation, loopFrame, skins.FirstOrDefault());
            viewer.mergeSkins(skins);
        }

        internal static string[] CaptureSkinNames(IEnumerable<Skin> skins)
        {
            if (skins == null) return new string[0];
            return skins.Where(skin => skin != null && !string.IsNullOrEmpty(skin.Name))
                .Select(skin => skin.Name).Distinct(StringComparer.Ordinal).ToArray();
        }

        private static void RefreshMtiSpineTextures(ReplacementSelection previous, bool force)
        {
            foreach (var texture in LiveViewers().Select(viewer => viewer.getSvTexture())
                .Where(texture => texture != null && !HasActive(texture)).Distinct())
            {
                if (!mtiRecords.TryGetValue(texture.MtiImage0, out var record) || record.Image == null) continue;
                if (!force && previous.SameTexture(selection, "mti", record.AssetKey, record.ImageKey, null)) continue;
                try { texture.cleanExecute(); }
                catch (Exception ex) { BLog.Error("Failed to refresh an MTI-backed Spine texture: " + texture.key, ex); }
            }
        }

        internal static void Stop()
        {
            if (!initialized || stopped) return;
            stopped = true;
            scan?.Dispose();
            scan = null;
            ApplySelection(true);
            RestoreOrdinary();
        }

        internal static void Resume()
        {
            if (!initialized || !stopped) return;
            stopped = false;
            lastSensitive = ConfigManager.EnableSensitivities?.Value == true;
            ApplySelection(false);
            Reload();
        }

        internal static void Register(SpineViewerNel viewer)
        {
            if (!initialized || viewer == null) return;
            if (!viewers.Any(weak => ReferenceEquals(weak.Target, viewer))) viewers.Add(new WeakReference(viewer));
        }

        internal static void BeforeSwitch(SpineViewerNel viewer, string jsonKey = null)
        {
            if (!spineAvailable || viewer == null) return;
            Register(viewer);
            var texture = viewer.getSvTexture();
            if (texture == null) return;
            string key = jsonKey ?? viewer.replace_json_key ?? texture.MtiText.default_json_key;
            Change(texture, key, viewer.getMaterial(), viewer);
            if (jsonKey != null && spineStates.TryGetValue(texture, out var state)
                && state.Current != null && state.JsonKey == key)
            {
                viewerAtlas.SetValue(viewer, state.Current.Atlas);
                viewerData.SetValue(viewer, state.Current.Data);
                viewerContainer.SetValue(viewer, state.Current.Atlas.GetAtlas(false));
            }
            ApplyClipping(viewer);
        }

        internal static bool Prepare(BetobetoManager.SvTexture texture, string jsonKey, Material[] materials,
            out SpineAtlasAsset atlas, out SkeletonDataAsset data)
        {
            atlas = null;
            data = null;
            if (!spineAvailable || texture == null || materials == null || materials.Length == 0) return false;
            string key = jsonKey ?? texture.MtiText.default_json_key;
            if (!spineStates.TryGetValue(texture, out var existing) || existing.Attempt < 0)
                Change(texture, key, materials[0], null);
            if (!spineStates.TryGetValue(texture, out var state) || state.Current == null) return false;
            if (state.JsonKey != key) throw new InvalidOperationException("Spine JSON variant changed outside a safe switch.");
            state.Current.Bind(materials);
            svAtlas.SetValue(texture, state.Current.Atlas);
            svData.SetValue(texture, state.Current.Data);
            texture.preapreAtlasDepth();
            atlas = state.Current.Atlas;
            data = state.Current.Data;
            return true;
        }

        private static bool Change(BetobetoManager.SvTexture texture, string key, Material material,
            SpineViewerNel switching)
        {
            if (!spineStates.TryGetValue(texture, out var state)) spineStates.Add(texture, state = new SpineState());
            if (state.Pending != null && state.PendingKey != key) { Cancel(state); state.Attempt = -1; }
            if (state.Attempt == revision && state.JsonKey == key && state.Pending == null) return false;
            if (selectionDelay.Waiting && state.JsonKey == key) return false;
            if (material == null) return false;
            var old = state.Current;
            var switchingAnimator = switching == null ? null : animator.GetValue(switching);
            if (LiveViewers().Any(viewer => viewer != switching && viewer.enabled && viewer.getSvTexture() == texture
                && !ReferenceEquals(animator.GetValue(viewer), switchingAnimator))) return false;
            string identity = SpineIdentity(texture.key, key);
            var layers = ActiveLayers(identity);
            SpineBundle candidate = null;
            bool damaged = HasInvalidLayer(identity) || (old != null && HasUnidentifiedErrors(old.Sources));
            if (damaged)
            {
                Cancel(state);
                BLog.Warn("Spine replacement target is damaged: " + texture.key + "/" + key + ".");
                if (old != null && state.JsonKey == key && CanRetain(old.Sources, identity))
                {
                    state.Attempt = revision;
                    return false;
                }
            }
            else if (layers.Count > 0)
            {
                try
                {
                    if (state.Pending == null)
                    {
                        texture.MtiText.addLoadKey("_SV");
                        texture.MtiImage0.addLoadKey("_SV", false);
                        SpineViewer.prepareAtlasAssetsS(texture.MtiText, out var originalAtlas, out var originalData, key);
                        state.PendingOriginal = originalData;
                        string originalJson = originalData.skeletonJSON.text;
                        string originalAtlasText = originalAtlas.atlasFile.text;
                        float originalScale = originalData.scale;
                        string root = PatchInfo.ReplaceImagePath, sensitive = PatchInfo.ReplaceSensitiveImagePath;
                        bool allow = selection.AllowSensitive;
                        state.PendingKey = key;
                        state.PendingSince = Time.unscaledTime;
                        state.Pending = new ReplacementWork<PreparedSpine>(token => ReplacementPreparation.Spine(
                            originalJson, originalAtlasText, originalScale, layers, root, sensitive, allow, token));
                        state.Attempt = revision;
                    }
                    if (!state.Pending.IsCompleted || !ClaimUpload())
                    {
                        if (old == null) { state.JsonKey = key; return false; }
                        if (state.JsonKey == key && CanRetain(old.Sources, identity)) return false;
                        return Install(texture, state, key, null);
                    }
                    var original = state.PendingOriginal;
                    state.Pending.TryTake(out var prepared, out var error);
                    Cancel(state);
                    if (error != null) throw error;
                    if (original == null) throw new InvalidOperationException("Original Spine data was released while preparing a replacement.");
                    candidate = Build(texture, layers, material, prepared, original);
                }
                catch (Exception ex)
                {
                    Cancel(state);
                    BLog.Error("Spine replacement rejected for " + texture.key + "/" + key
                        + "; retaining the last usable composition when authorized.", ex);
                    if (old != null && state.JsonKey == key
                        && CanRetain(old.Sources, identity))
                    {
                        state.Attempt = revision;
                        return false;
                    }
                }
            }
            else
            {
                Cancel(state);
                if (old != null && state.JsonKey == key && CanRetain(old.Sources, identity))
                {
                    state.Attempt = revision;
                    return false;
                }
            }
            return Install(texture, state, key, candidate);
        }

        private static bool Install(BetobetoManager.SvTexture texture, SpineState state, string key, SpineBundle candidate)
        {
            var old = state.Current;
            state.Attempt = revision;
            state.JsonKey = key;
            if (old == null && candidate == null) return false;
            Invalidate(texture, old);
            state.Current = candidate;
            var rendered = texture.getRendered();
            texture.releaseTexture();
            if (rendered != null) Object.Destroy(rendered);
            depth.SetValue(texture, null);
            texture.atlas_depth_written = false;
            svAtlas.SetValue(texture, candidate?.Atlas);
            svData.SetValue(texture, candidate?.Data);
            if (old != null) retired.Add(old);
            displayRevision++;
            BLog.Info(candidate == null ? "Spine replacement restored: " + texture.key
                : "Spine replacement activated: " + string.Join(" + ", candidate.Sources.Select(source => source.Id)));
            return true;
        }

        private static void Cancel(SpineState state)
        {
            state.Pending?.Dispose();
            state.Pending = null;
            state.PendingOriginal = null;
            state.PendingKey = null;
        }

        private static bool ClaimUpload()
        {
            if (uploadFrame == Time.frameCount) return false;
            uploadFrame = Time.frameCount;
            return true;
        }

        private static SpineBundle Build(BetobetoManager.SvTexture texture, List<ReplacementTarget> layers,
            Material material, PreparedSpine prepared, SkeletonDataAsset originalData)
        {
            var bundle = new SpineBundle();
            try
            {
                foreach (var layer in layers) ValidateCurrent(layer);
                foreach (var source in layers.Select(layer => layer.Owner).Distinct()) bundle.Sources.Add(source);
                string atlasText = prepared.AtlasText;
                var metadataAtlas = prepared.Atlas;
                string imagePath = LastPath(layers, layer => layer.ImagePath);
                if (imagePath != null)
                {
                    var image = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    if (!image.LoadImage(prepared.ImageBytes))
                    {
                        Object.Destroy(image);
                        throw new InvalidDataException("PNG decoding failed.");
                    }
                    image.name = Path.GetFileNameWithoutExtension(imagePath);
                    image.wrapMode = TextureWrapMode.Clamp;
                    image.filterMode = FilterMode.Bilinear;
                    bundle.Image = image;
                    bundle.OwnImage = true;
                }
                else
                {
                    bundle.Image = texture.MtiImage0.Image;
                    if (bundle.Image == null) throw new InvalidDataException("Original Spine texture is not loaded yet.");
                    ValidateAtlasTexture(metadataAtlas, bundle.Image);
                }
                if (bundle.Image.width > SystemInfo.maxTextureSize || bundle.Image.height > SystemInfo.maxTextureSize)
                    throw new InvalidDataException("Spine atlas exceeds the device texture size limit.");
                bundle.Composition = prepared.Composition;
                if (bundle.Composition.DirtMode == "auto" && !bundle.Composition.DirtEnabled)
                    BLog.Info("Spine dirt effect disabled because the composed atlas has no compatible EM/ND region: "
                        + texture.key + "/" + layers[0].JsonKey);
                bundle.StagingMaterial = new Material(material) { mainTexture = bundle.Image };
                bundle.AtlasText = new TextAsset(atlasText);
                bundle.JsonText = new TextAsset(bundle.Composition.Json);
                bundle.Atlas = SpineAtlasAsset.CreateRuntimeInstance(bundle.AtlasText,
                    new[] { bundle.StagingMaterial }, false, asset => new MaterialLoader(bundle.StagingMaterial));
                // 后台 Atlas 已按 Unity 的约定翻转 UV；沿用同一对象，避免附件引用旧图集或二次翻转。
                foreach (var page in metadataAtlas.Pages) page.rendererObject = bundle.StagingMaterial;
                cachedAtlas.SetValue(bundle.Atlas, metadataAtlas);
                bundle.Data = ScriptableObject.CreateInstance<SkeletonDataAsset>();
                bundle.Data.atlasAssets = new AtlasAssetBase[] { bundle.Atlas };
                bundle.Data.skeletonJSON = bundle.JsonText;
                bundle.Data.scale = bundle.Composition.Display.SkeletonScale ?? originalData.scale;
                bundle.Data.defaultMix = originalData.defaultMix;
                bundle.Data.fromAnimation = originalData.fromAnimation == null
                    ? new string[0] : (string[])originalData.fromAnimation.Clone();
                bundle.Data.toAnimation = originalData.toAnimation == null
                    ? new string[0] : (string[])originalData.toAnimation.Clone();
                bundle.Data.duration = originalData.duration == null
                    ? new float[0] : (float[])originalData.duration.Clone();
                initializeData.Invoke(bundle.Data, new object[] { bundle.Composition.PreparedData });
                bundle.Bind(new[] { material });
                return bundle;
            }
            catch
            {
                bundle.Dispose();
                throw;
            }
        }

        private static string LastPath(IEnumerable<ReplacementTarget> layers, Func<ReplacementTarget, string> selector)
        {
            string result = null;
            foreach (var layer in layers)
            {
                string value = selector(layer);
                if (value != null) result = value;
            }
            return result;
        }

        private static void ValidateAtlasTexture(Atlas atlas, Texture image)
        {
            var page = atlas.Pages[0];
            if (page.width != image.width || page.height != image.height)
                throw new InvalidDataException("Texture dimensions do not match the single-page atlas.");
            if (page.pma) throw new InvalidDataException("Export a straight-alpha atlas (PMA is unsupported).");
            foreach (var region in atlas.Regions)
                if (region.x < 0 || region.y < 0 || region.width <= 0 || region.height <= 0
                    || (long)region.x + region.width > image.width || (long)region.y + region.height > image.height)
                    throw new InvalidDataException("Invalid atlas bounds: " + region.name);
        }

        internal static bool Clean(BetobetoManager.SvTexture texture)
        {
            if (!spineStates.TryGetValue(texture, out var state) || state.Current == null) return false;
            var image = state.Current.Image;
            var previous = RenderTexture.active;
            try
            {
                allocate.Invoke(texture, new object[] { image.width, image.height, texture.key });
                var target = texture.getRendered();
                BLIT.PasteTo(target, image, target.width * 0.5f, target.height * 0.5f, 1f);
                texture.dirt_index = 0;
                return true;
            }
            finally { RenderTexture.active = previous; }
        }

        internal static bool DirtEnabled(BetobetoManager.SvTexture texture)
        {
            return !spineStates.TryGetValue(texture, out var state) || state.Current == null
                || state.Current.Composition.DirtEnabled;
        }

        internal static string MapBone(SpineViewer viewer, string name)
        {
            if (name == null || !(viewer is SpineViewerNel nelViewer)) return name;
            var texture = nelViewer.getSvTexture();
            if (texture != null && spineStates.TryGetValue(texture, out var state) && state.Current != null
                && state.Current.Composition.BoneMap.TryGetValue(name, out string mapped)) return mapped;
            return name;
        }

        internal static float ApplyDisplay(UIPictureBodySpine body, string property, float value)
        {
            var texture = body?.getViewer()?.getSvTexture();
            if (texture == null || !spineStates.TryGetValue(texture, out var state) || state.Current == null) return value;
            var display = state.Current.Composition.Display;
            switch (property)
            {
                case "scale": return value * (display.ScaleMultiplier ?? 1f);
                case "shift_ux": return value + (display.OffsetX ?? 0f) / 64f;
                case "shift_uy": return value + (display.OffsetY ?? 0f) / 64f;
                case "base_swidth":
                    return display.Width.HasValue ? value * display.Width.Value / GetPrivateFloat(body, "swidth") : value;
                case "base_sheight":
                    return display.Height.HasValue ? value * display.Height.Value / GetPrivateFloat(body, "sheight") : value;
                default: return value;
            }
        }

        private static float GetPrivateFloat(object instance, string name)
        {
            var field = AccessTools.Field(instance.GetType(), name);
            if (field == null) return 1f;
            float value = (float)field.GetValue(instance);
            return value == 0 ? 1f : value;
        }

        internal static void ApplyRightShift(UIPictureBodySpine body, ref float value)
        {
            var texture = body?.getViewer()?.getSvTexture();
            if (texture == null || !spineStates.TryGetValue(texture, out var state) || state.Current == null
                || !state.Current.Composition.Display.RightShift.HasValue) return;
            float old = body.rightshift_px;
            float side = GetPositionRight(body);
            value += side * (state.Current.Composition.Display.RightShift.Value - old) / 64f;
        }

        private static float GetPositionRight(UIPictureBodySpine body)
        {
            var field = AccessTools.Field(typeof(UIPictureBodyData), "PCon");
            object controller = field?.GetValue(body);
            var property = controller == null ? null : AccessTools.Property(controller.GetType(), "is_position_right");
            if (property != null) return Convert.ToSingle(property.GetValue(controller, null));
            var side = controller == null ? null : AccessTools.Field(controller.GetType(), "is_position_right");
            return side == null ? 0f : Convert.ToSingle(side.GetValue(controller));
        }

        private static void ApplyClipping(SpineViewerNel viewer)
        {
            if (clipping == null || viewer == null) return;
            var texture = viewer.getSvTexture();
            bool enabled = texture != null && spineStates.TryGetValue(texture, out var state)
                && state.Current != null && state.Current.Composition.HasClipping;
            if (animator.GetValue(viewer) is SkeletonRenderer renderer) clipping.SetValue(renderer, enabled);
        }

        internal static void AfterSwitch(SpineViewerNel viewer)
        {
            ApplyClipping(viewer);
            Collect();
        }

        internal static void RegisterMti(MTIOneImage container, string assetKey, string imageKey)
        {
            if (!initialized || container == null || string.IsNullOrEmpty(assetKey) || mtiImage == null) return;
            if (!mtiRecords.TryGetValue(container, out var record))
            {
                record = new MtiRecord { Container = container };
                mtiRecords.Add(container, record);
            }
            record.AssetKey = assetKey;
            record.ImageKey = imageKey;
            TryApply(record);
        }

        private static void RetryMtiRecords()
        {
            foreach (var record in mtiRecords.Values.ToArray()) TryApply(record);
        }

        private static void TryApply(MtiRecord record)
        {
            if (selectionDelay.Waiting) return;
            if (record.Attempt == revision && record.Image != null && record.Pending == null) return;
            var image = mtiImage.GetValue(record.Container) as MImage;
            if (image == null || image.Tx == null) return;
            record.Image = image;
            if (record.Original == null) record.Original = image.Tx;
            var layer = Enabled ? selection.Texture("mti", record.AssetKey, record.ImageKey, null) : null;
            if (HasInvalidTextureLayer("mti", record.AssetKey, record.ImageKey, null)
                || (record.Source != null && HasUnidentifiedErrors(new[] { record.Source })))
            {
                record.Pending?.Dispose(); record.Pending = null;
                BLog.Warn("MTI replacement target is damaged: " + record.AssetKey + ".");
                if (record.Replacement == null || !CanRetain(new[] { record.Source }, record.SourceIdentity)) Restore(record);
                record.Attempt = revision;
                return;
            }
            if (layer == null)
            {
                record.Pending?.Dispose(); record.Pending = null;
                if (record.Replacement != null && CanRetain(new[] { record.Source }, record.SourceIdentity))
                {
                    record.Attempt = revision;
                    return;
                }
                Restore(record);
                record.Attempt = revision;
                return;
            }
            try
            {
                if (record.Replacement != null && !CanRetain(new[] { record.Source }, record.SourceIdentity)) Restore(record);
                if (record.Pending == null)
                {
                    record.Pending = PrepareTexture(layer);
                    record.Attempt = revision;
                }
                if (!record.Pending.IsCompleted || !ClaimUpload()) return;
                record.Pending.TryTake(out var bytes, out var error);
                record.Pending = null;
                if (error != null) throw error;
                ValidateCurrent(layer);
                record.Replacement = LoadStableTexture(bytes, record.Original, record.Replacement);
                record.Source = layer.Owner;
                record.SourceIdentity = layer.Identity;
                record.Image.Tx = record.Replacement;
                record.Attempt = revision;
                RefreshMtiUsers(record);
            }
            catch (Exception ex)
            {
                record.Pending?.Dispose(); record.Pending = null;
                BLog.Error("MTI replacement rejected: " + record.AssetKey, ex);
                if (record.Replacement == null || !CanRetain(new[] { record.Source }, record.SourceIdentity)) Restore(record);
                record.Attempt = revision;
            }
        }

        internal static Object ReplaceResource(string path, Type requestedType, Object original)
        {
            if (!initialized || original == null || string.IsNullOrEmpty(path)) return original;
            string objectType = requestedType == typeof(Sprite) || original is Sprite ? "Sprite"
                : requestedType == typeof(Texture2D) || original is Texture2D ? "Texture2D" : null;
            if (objectType == null) return original;
            string key = path + "\n" + objectType;
            if (!resourceRecords.TryGetValue(key, out var record))
            {
                record = new ResourceRecord { Path = path, ObjectType = objectType, Original = original };
                resourceRecords.Add(key, record);
            }
            ApplyResource(record, firstAccess: true);
            return record.Replacement ?? original;
        }

        private static void RefreshResourceRecords()
        {
            foreach (var record in resourceRecords.Values.ToArray()) ApplyResource(record);
        }

        private static void ApplyResource(ResourceRecord record, bool firstAccess = false)
        {
            if (selectionDelay.Waiting && !firstAccess) return;
            if (record.Attempt == revision && record.Pending == null) return;
            if (record.Original == null) { DisposeResource(record); record.Attempt = revision; return; }
            var layer = Enabled ? selection.Texture("resources", record.Path, null, record.ObjectType) : null;
            if (HasInvalidTextureLayer("resources", record.Path, null, record.ObjectType)
                || (record.Source != null && HasUnidentifiedErrors(new[] { record.Source })))
            {
                record.Pending?.Dispose(); record.Pending = null;
                BLog.Warn("Resources replacement target is damaged: " + record.Path + " (" + record.ObjectType + ").");
                if (record.Replacement == null || !CanRetain(new[] { record.Source }, record.SourceIdentity))
                    DisposeResource(record);
                record.Attempt = revision;
                return;
            }
            if (layer == null)
            {
                record.Pending?.Dispose(); record.Pending = null;
                if (record.Replacement != null && CanRetain(new[] { record.Source }, record.SourceIdentity))
                {
                    record.Attempt = revision;
                    return;
                }
                DisposeResource(record);
                record.Attempt = revision;
                return;
            }
            try
            {
                if (record.Replacement != null && !CanRetain(new[] { record.Source }, record.SourceIdentity)) DisposeResource(record);
                byte[] bytes;
                if (firstAccess && record.Replacement == null)
                {
                    // Resources.Load 的首次返回值会被调用方永久持有，不能先返回原对象再偷偷换引用。
                    // 这个入口保持同步首载；已有替换对象的刷新继续在后台准备并原位更新。
                    record.Pending?.Dispose(); record.Pending = null;
                    bytes = ReplacementPreparation.Texture(layer, PatchInfo.ReplaceImagePath,
                        PatchInfo.ReplaceSensitiveImagePath, selection.AllowSensitive, default(System.Threading.CancellationToken));
                }
                else
                {
                    if (record.Pending == null)
                    {
                        record.Pending = PrepareTexture(layer);
                        record.Attempt = revision;
                    }
                    if (!record.Pending.IsCompleted || !ClaimUpload()) return;
                    record.Pending.TryTake(out bytes, out var error);
                    record.Pending = null;
                    if (error != null) throw error;
                }
                ValidateCurrent(layer);
                Texture source = record.Original is Sprite sprite ? sprite.texture : (Texture)record.Original;
                record.Texture = LoadStableTexture(bytes, source, record.Texture);
                if (record.ObjectType == "Texture2D") record.Replacement = record.Texture;
                else if (!(record.Replacement is Sprite)) record.Replacement = CreateSprite((Sprite)record.Original, record.Texture);
                record.Source = layer.Owner;
                record.SourceIdentity = layer.Identity;
                record.Attempt = revision;
            }
            catch (Exception ex)
            {
                record.Pending?.Dispose(); record.Pending = null;
                BLog.Error("Resources replacement rejected: " + record.Path + " (" + record.ObjectType + ")", ex);
                if (record.Replacement == null || !CanRetain(new[] { record.Source }, record.SourceIdentity)) DisposeResource(record);
                record.Attempt = revision;
            }
        }

        private static ReplacementWork<byte[]> PrepareTexture(ReplacementTarget layer)
        {
            string root = PatchInfo.ReplaceImagePath, sensitive = PatchInfo.ReplaceSensitiveImagePath;
            bool allow = selection.AllowSensitive;
            return new ReplacementWork<byte[]>(token => ReplacementPreparation.Texture(layer, root, sensitive, allow, token));
        }

        private static void RefreshMtiUsers(MtiRecord record)
        {
            foreach (var texture in LiveViewers().Select(viewer => viewer.getSvTexture())
                .Where(texture => texture != null && texture.MtiImage0 == record.Container && !HasActive(texture)).Distinct())
            {
                try { texture.cleanExecute(); displayRevision++; }
                catch (Exception ex) { BLog.Error("Failed to refresh an MTI-backed Spine texture: " + texture.key, ex); }
            }
        }

        private static Texture2D LoadStableTexture(byte[] bytes, Texture source, Texture2D stable)
        {
            var candidate = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!candidate.LoadImage(bytes))
            {
                Object.Destroy(candidate);
                throw new InvalidDataException("PNG decoding failed.");
            }
            if (candidate.width != source.width || candidate.height != source.height)
            {
                Object.Destroy(candidate);
                throw new InvalidDataException("Replacement image dimensions must match the original resource.");
            }
            if (stable == null) stable = candidate;
            else
            {
                if (!stable.LoadImage(bytes))
                {
                    Object.Destroy(candidate);
                    throw new InvalidDataException("Stable texture refresh failed.");
                }
                Object.Destroy(candidate);
            }
            CopyTextureProperties(source, stable);
            return stable;
        }

        private static void CopyTextureProperties(Texture source, Texture destination)
        {
            destination.name = source.name;
            destination.filterMode = source.filterMode;
            destination.wrapMode = source.wrapMode;
            destination.wrapModeU = source.wrapModeU;
            destination.wrapModeV = source.wrapModeV;
            destination.wrapModeW = source.wrapModeW;
            destination.anisoLevel = source.anisoLevel;
            destination.mipMapBias = source.mipMapBias;
            destination.hideFlags = source.hideFlags;
        }

        private static Sprite CreateSprite(Sprite original, Texture2D texture)
        {
            Rect rect = original.rect;
            Vector2 pivot = new Vector2(original.pivot.x / rect.width, original.pivot.y / rect.height);
            var sprite = Sprite.Create(texture, rect, pivot, original.pixelsPerUnit, 0,
                SpriteMeshType.FullRect, original.border);
            sprite.name = original.name;
            sprite.hideFlags = original.hideFlags;
            return sprite;
        }

        private static void Restore(MtiRecord record)
        {
            record.Pending?.Dispose(); record.Pending = null;
            bool changed = record.Replacement != null;
            if (record.Image != null && record.Original != null) record.Image.Tx = record.Original;
            if (record.Replacement != null) Object.Destroy(record.Replacement);
            record.Replacement = null;
            record.Source = null;
            record.SourceIdentity = null;
            if (changed) RefreshMtiUsers(record);
        }

        private static void DisposeResource(ResourceRecord record)
        {
            record.Pending?.Dispose(); record.Pending = null;
            if (record.Replacement is Sprite sprite) Object.Destroy(sprite);
            if (record.Texture != null) Object.Destroy(record.Texture);
            record.Replacement = null;
            record.Texture = null;
            record.Source = null;
            record.SourceIdentity = null;
        }

        private static void RestoreOrdinary()
        {
            foreach (var record in mtiRecords.Values) Restore(record);
            foreach (var record in resourceRecords.Values) DisposeResource(record);
        }

        private static bool CanRetain(IEnumerable<ReplacementPackage> packages, string identity)
        {
            if (!Enabled || !selection.Authorizes(packages)) return false;
            foreach (var package in packages.Where(package => package != null).Distinct())
            {
                if (!File.Exists(package.ManifestPath)) return false;
                if (package.Sensitive && ConfigManager.EnableSensitivities?.Value != true) return false;
                try
                {
                    PortraitCatalog.Resolve(PatchInfo.ReplaceImagePath, Path.GetDirectoryName(package.ManifestPath),
                        Path.GetFileName(package.ManifestPath));
                }
                catch { return false; }
                var current = catalog.Packages.FirstOrDefault(candidate => candidate.Id == package.Id);
                if (current != null && !current.Targets.Any(target => target.Identity == identity)
                    && !current.InvalidTargetIdentities.Contains(identity)
                    && !current.HasUnidentifiedTargetErrors) return false;
            }
            return true;
        }

        private static void ValidateCurrent(ReplacementTarget target)
        {
            ReplacementPreparation.Validate(new[] { target }, PatchInfo.ReplaceImagePath,
                PatchInfo.ReplaceSensitiveImagePath, ConfigManager.EnableSensitivities?.Value == true);
        }

        private static bool HasUnidentifiedErrors(IEnumerable<ReplacementPackage> packages)
        {
            foreach (var source in packages.Where(package => package != null))
            {
                var current = catalog.Packages.FirstOrDefault(package => package.Id == source.Id);
                if (current != null && current.HasUnidentifiedTargetErrors) return true;
            }
            return false;
        }

        private static bool HasInvalidLayer(string identity)
        {
            return Enabled && selection.Invalid(identity);
        }

        private static bool HasInvalidTextureLayer(string loader, string key, string imageKey, string objectType)
        {
            return Enabled && selection.InvalidTexture(loader, key, imageKey, objectType);
        }

        private static List<ReplacementTarget> ActiveLayers(string identity)
        {
            if (!Enabled) return new List<ReplacementTarget>();
            return selection.Layers(identity).ToList();
        }

        private static bool HasLayers(string identity) => ActiveLayers(identity).Count > 0;
        private static string SpineIdentity(string key, string jsonKey) => "spine\n" + key + "\n" + jsonKey;

        private static IEnumerable<SpineViewerNel> LiveViewers()
        {
            viewers.RemoveAll(weak => !weak.IsAlive);
            return viewers.Select(weak => weak.Target).OfType<SpineViewerNel>().ToArray();
        }

        private static void Invalidate(BetobetoManager.SvTexture texture, SpineBundle old)
        {
            foreach (var viewer in LiveViewers().Where(item => item.getSvTexture() == texture))
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
            if (!spineStates.TryGetValue(texture, out var state)) return;
            Cancel(state);
            Invalidate(texture, state.Current);
            if (state.Current != null) retired.Add(state.Current);
            spineStates.Remove(texture);
            depth.SetValue(texture, null);
            Collect();
        }

        internal static void Collect()
        {
            if (retired.Count == 0) return;
            var live = LiveViewers().Select(viewer => animator.GetValue(viewer) as SkeletonAnimation)
                .Where(animation => animation != null).ToArray();
            for (int i = retired.Count - 1; i >= 0; i--)
            {
                if (live.Any(animation => animation.skeletonDataAsset == retired[i].Data)) continue;
                retired[i].Dispose();
                retired.RemoveAt(i);
            }
        }
    }
}
