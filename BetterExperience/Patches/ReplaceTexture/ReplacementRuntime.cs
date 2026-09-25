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
        }

        private static readonly Dictionary<BetobetoManager.SvTexture, SpineState> spineStates =
            new Dictionary<BetobetoManager.SvTexture, SpineState>();
        private static readonly List<WeakReference> viewers = new List<WeakReference>();
        private static readonly List<SpineBundle> retired = new List<SpineBundle>();
        private static readonly Dictionary<MTIOneImage, MtiRecord> mtiRecords = new Dictionary<MTIOneImage, MtiRecord>();
        private static readonly Dictionary<string, ResourceRecord> resourceRecords =
            new Dictionary<string, ResourceRecord>(StringComparer.Ordinal);
        private static ReplacementCatalog catalog = new ReplacementCatalog();
        private static string settings;
        private static int revision;
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
                viewerTexture, animator, reserved
            }.All(member => member != null);
            if (!spineAvailable) BLog.Warn("Spine resource replacement disabled: game interfaces do not match.");
            if (mtiImage == null) BLog.Warn("MTI resource replacement disabled: image container interface does not match.");
            Directory.CreateDirectory(PatchInfo.ReplaceImagePath);
            Directory.CreateDirectory(PatchInfo.ReplaceSensitiveImagePath);
            Reload();
        }

        private static string Settings => stopped + "|" + ConfigManager.EnableResourceReplacement?.Value + "|"
            + string.Join("\n", EnabledIds()) + "|" + ConfigManager.EnableSensitivities?.Value;

        internal static void PollSettings()
        {
            if (!initialized) return;
            if (settings != Settings)
            {
                Reload();
                RefreshMtiSpineTextures();
                ReplayLiveSpines();
            }
            RetryMtiRecords();
            Collect();
        }

        internal static void Reload()
        {
            if (!initialized) return;
            settings = Settings;
            try
            {
                var scanned = ReplacementCatalog.Discover(PatchInfo.ReplaceImagePath,
                    PatchInfo.ReplaceSensitiveImagePath, ConfigManager.EnableSensitivities?.Value == true);
                SyncPackRows(scanned);
                catalog = scanned;
                foreach (string error in catalog.Errors) BLog.Warn("Replacement: " + error);
                settings = Settings;
            }
            catch (Exception ex)
            {
                BLog.Error("Replacement discovery failed; keeping the previous catalog.", ex);
            }
            revision++;
            RetryMtiRecords();
            RefreshResourceRecords();
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
            RefreshMtiSpineTextures();
            ReplayLiveSpines();
            Collect();
            BLog.Info("Resource replacements refreshed.");
        }

        private static void ReplayLiveSpines()
        {
            foreach (var viewer in LiveViewers())
            {
                try
                {
                    var texture = viewer.getSvTexture();
                    if (texture == null) continue;
                    string key = viewer.replace_json_key ?? texture.MtiText.default_json_key;
                    string identity = SpineIdentity(texture.key, key);
                    if (!HasActive(texture) && !HasLayers(identity)) continue;
                    string animation = viewer.getBaseAnimName();
                    if (animation == null) continue;
                    string[] skins = CaptureSkinNames(viewer.GetSkeleton()?.SkinList);
                    var entry = viewer.getTrack(0);
                    int loopFrame = entry?.Animation == null ? -1000 : viewer.getAnmLoopFrame(entry.Animation);
                    viewer.clearAnim(animation, loopFrame, skins.FirstOrDefault());
                    viewer.mergeSkins(skins);
                }
                catch (Exception ex) { BLog.Error("Replacement refresh failed for one Spine viewer.", ex); }
            }
        }

        internal static string[] CaptureSkinNames(IEnumerable<Skin> skins)
        {
            if (skins == null) return new string[0];
            return skins.Where(skin => skin != null && !string.IsNullOrEmpty(skin.Name))
                .Select(skin => skin.Name).Distinct(StringComparer.Ordinal).ToArray();
        }

        private static void RefreshMtiSpineTextures()
        {
            foreach (var texture in LiveViewers().Select(viewer => viewer.getSvTexture())
                .Where(texture => texture != null && !HasActive(texture)).Distinct())
            {
                if (!mtiRecords.TryGetValue(texture.MtiImage0, out var record) || record.Image == null) continue;
                try { texture.cleanExecute(); }
                catch (Exception ex) { BLog.Error("Failed to refresh an MTI-backed Spine texture: " + texture.key, ex); }
            }
        }

        internal static void Stop()
        {
            stopped = true;
            Reload();
            RestoreOrdinary();
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

        private static void Change(BetobetoManager.SvTexture texture, string key, Material material,
            SpineViewerNel switching)
        {
            if (!spineStates.TryGetValue(texture, out var state)) spineStates.Add(texture, state = new SpineState());
            if (state.Attempt == revision && state.JsonKey == key) return;
            if (material == null) return;
            var old = state.Current;
            var switchingAnimator = switching == null ? null : animator.GetValue(switching);
            if (LiveViewers().Any(viewer => viewer != switching && viewer.enabled && viewer.getSvTexture() == texture
                && !ReferenceEquals(animator.GetValue(viewer), switchingAnimator))) return;
            string identity = SpineIdentity(texture.key, key);
            var layers = ActiveLayers(identity);
            SpineBundle candidate = null;
            bool damaged = HasInvalidLayer(identity) || (old != null && HasUnidentifiedErrors(old.Sources));
            if (damaged)
            {
                BLog.Warn("Spine replacement target is damaged: " + texture.key + "/" + key + ".");
                if (old != null && state.JsonKey == key && CanRetain(old.Sources, identity))
                {
                    state.Attempt = revision;
                    return;
                }
            }
            else if (layers.Count > 0)
            {
                try { candidate = Build(texture, layers, material); }
                catch (Exception ex)
                {
                    BLog.Error("Spine replacement rejected for " + texture.key + "/" + key
                        + "; retaining the last usable composition when authorized.", ex);
                    if (old != null && state.JsonKey == key
                        && CanRetain(old.Sources, identity))
                    {
                        state.Attempt = revision;
                        return;
                    }
                }
            }
            else if (old != null && state.JsonKey == key
                && CanRetain(old.Sources, identity))
            {
                state.Attempt = revision;
                return;
            }
            state.Attempt = revision;
            state.JsonKey = key;
            if (old == null && candidate == null) return;
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
            BLog.Info(candidate == null ? "Spine replacement restored: " + texture.key
                : "Spine replacement activated: " + string.Join(" + ", candidate.Sources.Select(source => source.Id)));
        }

        private static SpineBundle Build(BetobetoManager.SvTexture texture, List<ReplacementTarget> layers,
            Material material)
        {
            var bundle = new SpineBundle();
            try
            {
                foreach (var layer in layers) ValidateCurrent(layer);
                foreach (var source in layers.Select(layer => layer.Owner).Distinct()) bundle.Sources.Add(source);
                texture.MtiText.addLoadKey("_SV");
                texture.MtiImage0.addLoadKey("_SV", false);
                SpineViewer.prepareAtlasAssetsS(texture.MtiText, out var originalAtlasAsset, out var originalData,
                    layers[0].JsonKey);
                string atlasText = LastPath(layers, layer => layer.AtlasPath) is string atlasPath
                    ? File.ReadAllText(atlasPath) : originalAtlasAsset.atlasFile.text;
                var metadataAtlas = PortraitCatalog.ReadAtlas(atlasText);
                if (metadataAtlas.Pages.Count != 1) throw new InvalidDataException("Only single-page Spine atlases are supported.");
                string imagePath = LastPath(layers, layer => layer.ImagePath);
                if (imagePath != null)
                {
                    byte[] bytes = File.ReadAllBytes(imagePath);
                    PortraitCatalog.ValidateImage(bytes, metadataAtlas);
                    var image = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    if (!image.LoadImage(bytes))
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
                bundle.Composition = SpineComposer.Compose(originalData.skeletonJSON.text, layers, metadataAtlas);
                if (bundle.Composition.DirtMode == "auto" && !bundle.Composition.DirtEnabled)
                    BLog.Info("Spine dirt effect disabled because the composed atlas has no compatible EM/ND region: "
                        + texture.key + "/" + layers[0].JsonKey);
                bundle.StagingMaterial = new Material(material) { mainTexture = bundle.Image };
                bundle.AtlasText = new TextAsset(atlasText);
                bundle.JsonText = new TextAsset(bundle.Composition.Json);
                bundle.Atlas = SpineAtlasAsset.CreateRuntimeInstance(bundle.AtlasText,
                    new[] { bundle.StagingMaterial }, false, asset => new MaterialLoader(bundle.StagingMaterial));
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
                if (bundle.Data.GetSkeletonData(false) == null)
                    throw new InvalidDataException("Game Spine parser rejected the composed resource.");
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
            if (record.Attempt == revision && record.Image != null) return;
            var image = mtiImage.GetValue(record.Container) as MImage;
            if (image == null || image.Tx == null) return;
            record.Image = image;
            if (record.Original == null) record.Original = image.Tx;
            var layer = ActiveTextureLayers("mti", record.AssetKey, record.ImageKey, null).LastOrDefault();
            if (HasInvalidTextureLayer("mti", record.AssetKey, record.ImageKey, null)
                || (record.Source != null && HasUnidentifiedErrors(new[] { record.Source })))
            {
                BLog.Warn("MTI replacement target is damaged: " + record.AssetKey + ".");
                if (record.Replacement == null || !CanRetain(new[] { record.Source }, record.SourceIdentity)) Restore(record);
                record.Attempt = revision;
                return;
            }
            if (layer == null)
            {
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
                ValidateCurrent(layer);
                record.Replacement = LoadStableTexture(layer.ImagePath, record.Original, record.Replacement);
                record.Source = layer.Owner;
                record.SourceIdentity = layer.Identity;
                record.Image.Tx = record.Replacement;
                record.Attempt = revision;
            }
            catch (Exception ex)
            {
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
            ApplyResource(record);
            return record.Replacement ?? original;
        }

        private static void RefreshResourceRecords()
        {
            foreach (var record in resourceRecords.Values.ToArray()) ApplyResource(record);
        }

        private static void ApplyResource(ResourceRecord record)
        {
            if (record.Attempt == revision) return;
            var layer = ActiveTextureLayers("resources", record.Path, null, record.ObjectType).LastOrDefault();
            if (HasInvalidTextureLayer("resources", record.Path, null, record.ObjectType)
                || (record.Source != null && HasUnidentifiedErrors(new[] { record.Source })))
            {
                BLog.Warn("Resources replacement target is damaged: " + record.Path + " (" + record.ObjectType + ").");
                if (record.Replacement == null || !CanRetain(new[] { record.Source }, record.SourceIdentity))
                    DisposeResource(record);
                record.Attempt = revision;
                return;
            }
            if (layer == null)
            {
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
                ValidateCurrent(layer);
                Texture source = record.Original is Sprite sprite ? sprite.texture : (Texture)record.Original;
                record.Texture = LoadStableTexture(layer.ImagePath, source, record.Texture);
                if (record.ObjectType == "Texture2D") record.Replacement = record.Texture;
                else if (!(record.Replacement is Sprite)) record.Replacement = CreateSprite((Sprite)record.Original, record.Texture);
                record.Source = layer.Owner;
                record.SourceIdentity = layer.Identity;
                record.Attempt = revision;
            }
            catch (Exception ex)
            {
                BLog.Error("Resources replacement rejected: " + record.Path + " (" + record.ObjectType + ")", ex);
                if (record.Replacement == null || !CanRetain(new[] { record.Source }, record.SourceIdentity)) DisposeResource(record);
                record.Attempt = revision;
            }
        }

        private static Texture2D LoadStableTexture(string path, Texture source, Texture2D stable)
        {
            byte[] bytes = File.ReadAllBytes(path);
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
            if (record.Image != null && record.Original != null) record.Image.Tx = record.Original;
            if (record.Replacement != null) Object.Destroy(record.Replacement);
            record.Replacement = null;
            record.Source = null;
            record.SourceIdentity = null;
        }

        private static void DisposeResource(ResourceRecord record)
        {
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
            if (!Enabled) return false;
            var enabled = new HashSet<string>(EnabledIds(), StringComparer.Ordinal);
            foreach (var package in packages.Where(package => package != null).Distinct())
            {
                if (!enabled.Contains(package.Id) || !File.Exists(package.ManifestPath)) return false;
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
            var paths = new[]
            {
                target.Owner.ManifestPath, target.ImagePath, target.AtlasPath, target.JsonPath
            }.Where(path => path != null);
            foreach (string path in paths)
            {
                if (!File.Exists(path)) throw new FileNotFoundException("Replacement dependency was removed.", path);
                PortraitCatalog.Resolve(PatchInfo.ReplaceImagePath, Path.GetDirectoryName(path), Path.GetFileName(path));
                if (PortraitCatalog.Within(PatchInfo.ReplaceSensitiveImagePath, path) != target.Owner.Sensitive)
                    throw new InvalidDataException("Replacement dependencies crossed the Sensitive boundary.");
                if (target.Owner.Sensitive && ConfigManager.EnableSensitivities?.Value != true)
                    throw new InvalidDataException("Sensitive content is disabled.");
            }
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
            if (!Enabled) return false;
            var packages = catalog.Packages.ToDictionary(package => package.Id, StringComparer.Ordinal);
            return EnabledIds().Any(id => packages.TryGetValue(id, out var package)
                && package.InvalidTargetIdentities.Contains(identity));
        }

        private static bool HasInvalidTextureLayer(string loader, string key, string imageKey, string objectType)
        {
            if (!Enabled) return false;
            string exact = loader == "mti" ? "texture\nmti\n" + key + "\n" + (imageKey ?? "")
                : "texture\nresources\n" + key + "\n" + objectType;
            string wildcard = loader == "mti" ? "texture\nmti\n" + key + "\n" : exact;
            var packages = catalog.Packages.ToDictionary(package => package.Id, StringComparer.Ordinal);
            return EnabledIds().Any(id => packages.TryGetValue(id, out var package)
                && (package.InvalidTargetIdentities.Contains(exact)
                    || package.InvalidTargetIdentities.Contains(wildcard)));
        }

        private static List<ReplacementTarget> ActiveLayers(string identity)
        {
            if (!Enabled) return new List<ReplacementTarget>();
            return catalog.Layers(identity, EnabledIds()).ToList();
        }

        private static IEnumerable<ReplacementTarget> ActiveTextureLayers(string loader, string key,
            string imageKey, string objectType)
        {
            if (!Enabled) yield break;
            var packages = catalog.Packages.ToDictionary(package => package.Id, StringComparer.Ordinal);
            foreach (string id in EnabledIds())
            {
                if (!packages.TryGetValue(id, out var package)) continue;
                foreach (var target in package.Targets)
                {
                    if (target.Type != "texture" || target.Loader != loader) continue;
                    if (loader == "mti" && target.AssetKey == key
                        && (target.ImageKey == null || target.ImageKey == imageKey)) yield return target;
                    if (loader == "resources" && target.ResourcePath == key && target.ObjectType == objectType)
                        yield return target;
                }
            }
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
