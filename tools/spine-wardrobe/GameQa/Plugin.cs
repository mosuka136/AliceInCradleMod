using BepInEx;
using BetterExperience.Patches.ReplaceTexture;
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
using System.Security.Cryptography;
using UnityEngine;
using XX;
using Object = UnityEngine.Object;

namespace Wardrobe.QA
{
    // Optional development helper. No patch to the installed BetterExperience assembly is needed.
    [BepInPlugin("local.aic.wardrobe.qa", "Wardrobe QA", "1.0.4")]
    public sealed partial class Plugin : BaseUnityPlugin
    {
        private static readonly List<WeakReference> Viewers = new List<WeakReference>();
        private static readonly FieldInfo Animator = AccessTools.Field(typeof(SpineViewer), "charaAnim");
        private readonly List<object> observations = new List<object>();
        private readonly Dictionary<string, object> screenshots = new Dictionary<string, object>();
        private readonly List<object> errors = new List<object>();
        private readonly HashSet<string> seen = new HashSet<string>();
        private readonly HashSet<string> rendered = new HashSet<string>();
        private Harmony harmony;
        private string root;
        private string currentId;
        private Dictionary<string, object> request;
        private float deadline;
        private float nextPoll;
        private float nextReload;
        private bool capturing;
        private int refreshes;
        private bool waitingForResources;

        private void Awake()
        {
            string bridge = Path.Combine(Paths.PluginPath, "WardrobeQA", "bridge.json");
            if (!File.Exists(bridge)) { Logger.LogInfo("No bridge.json; Wardrobe QA is inactive."); return; }
            root = Path.GetFullPath((string)PortraitJson.Parse(File.ReadAllText(bridge))["queueRoot"]);
            Directory.CreateDirectory(root);
            if (Animator == null) { Logger.LogError("Game viewer signature differs."); root = null; return; }
            harmony = new Harmony("local.aic.wardrobe.qa");
            harmony.Patch(AccessTools.Method(typeof(SpineViewerNel), "prepareAtlasAssets"),
                postfix: new HarmonyMethod(typeof(Plugin), nameof(Register)));
            Logger.LogInfo("Wardrobe QA queue: " + root);
        }

        private static void Register(SpineViewerNel __instance)
        {
            Viewers.RemoveAll(v => !v.IsAlive);
            if (!Viewers.Any(v => ReferenceEquals(v.Target, __instance))) Viewers.Add(new WeakReference(__instance));
        }

        private void Update()
        {
            if (root == null || Time.realtimeSinceStartup < nextPoll || capturing) return;
            nextPoll = Time.realtimeSinceStartup + 0.5f;
            try
            {
                string path = Path.Combine(root, "request.json");
                if (!File.Exists(path)) return;
                var next = PortraitJson.Parse(File.ReadAllText(path));
                string id = (string)next["id"];
                if (id != currentId)
                {
                    if (id.Length != 32 || id.Any(c => !Uri.IsHexDigit(c))) throw new InvalidDataException("Invalid request id.");
                    currentId = id;
                    // A development queue must not replay days-old requests on
                    // every normal game launch. Requeue explicitly to retry.
                    if (!next.ContainsKey("expiresUnixSeconds") ||
                        Convert.ToDouble(next["expiresUnixSeconds"]) <= DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                    {
                        request = null;
                        Logger.LogInfo("Skipped expired or undated QA request: " + id);
                        return;
                    }
                    request = next;
                    waitingForResources = false;
                    observations.Clear(); screenshots.Clear(); errors.Clear(); seen.Clear(); rendered.Clear(); refreshes = 0;
                    nextReload = Time.realtimeSinceStartup + 10;
                    deadline = Time.realtimeSinceStartup + Math.Min(1800, Math.Max(5, Convert.ToSingle(next["timeoutSeconds"])));
                    Directory.CreateDirectory(OutputDirectory());
                    WriteResult(false, "running");
                }
                if (request == null) return;
                if (request.ContainsKey("isolated") && Convert.ToBoolean(request["isolated"]))
                {
                    if (!EnsureIsolatedViewers())
                    {
                        if (!waitingForResources)
                        {
                            WriteResult(false, "waiting-for-game-resources; load a save to continue");
                            Logger.LogInfo("Wardrobe QA waiting for Nel game resources.");
                            waitingForResources = true;
                        }
                        if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() >= Convert.ToDouble(request["expiresUnixSeconds"]))
                        {
                            WriteResult(false, "expired-while-waiting-for-game-resources");
                            request = null;
                        }
                        return;
                    }
                    if (waitingForResources)
                    {
                        deadline = Time.realtimeSinceStartup + Math.Min(1800, Math.Max(5, Convert.ToSingle(request["timeoutSeconds"])));
                        waitingForResources = false;
                        WriteResult(false, "running");
                    }
                }
                string operation = (string)request["operation"];
                if (operation != "observe" && operation != "render" && operation != "refresh" && operation != "effects")
                    throw new InvalidDataException("Unknown QA operation.");
                if (operation == "effects")
                {
                    if (!request.ContainsKey("isolated") || !Convert.ToBoolean(request["isolated"]))
                        throw new InvalidOperationException("Effects QA requires isolated viewers.");
                    capturing = true;
                    StartCoroutine(CaptureEffects());
                    return;
                }
                if (operation == "refresh" && refreshes < 10 && Time.realtimeSinceStartup >= nextReload)
                {
                    var runtime = AccessTools.TypeByName("BetterExperience.Patches.ReplaceTexture.PortraitRuntime");
                    AccessTools.Method(runtime, "Reload").Invoke(null, null);
                    refreshes++;
                    foreach (var pair in isolatedViewers)
                    {
                        var currentAnimation = Animator.GetValue(pair.Value) as SkeletonAnimation;
                        if (currentAnimation == null || currentAnimation.GetComponent<MeshRenderer>() == null)
                            throw new InvalidOperationException("QA viewer was destroyed; requeue after the scene transition.");
                        pair.Value.clearAnim("stand", -1000, pair.Key == "stand_weak" ? "arm1A" : "default");
                        pair.Value.updateAnim(true, 0);
                    }
                    nextReload = Time.realtimeSinceStartup + 2;
                }
                if (Time.realtimeSinceStartup >= deadline)
                {
                    WriteResult(false, "observation-window-ended; acceptance requires explicit evidence review");
                    request = null;
                    return;
                }
                foreach (var weak in Viewers.ToArray())
                {
                    var viewer = weak.Target as SpineViewerNel;
                    var animation = viewer == null ? null : Animator.GetValue(viewer) as SkeletonAnimation;
                    if (animation == null || !viewer.enabled || !animation.gameObject.activeInHierarchy || animation.Skeleton == null) continue;
                    var wanted = PortraitJson.Array(request["targets"]).Select(PortraitJson.Object)
                        .FirstOrDefault(t => (string)t["target"] == viewer.getSvTexture().key);
                    if (wanted == null) continue;
                    var observation = Describe(viewer, animation);
                    if (!Equals(observation["activePackageId"], wanted["packageId"])) continue;
                    string signature = PortraitJson.Serialize(observation) + (operation == "refresh" ? "/" + refreshes : "");
                    if (!seen.Add(signature)) continue;
                    if (observations.Count >= 500) { WriteResult(false, "capture-limit"); request = null; break; }
                    capturing = true;
                    bool render = operation == "render" && rendered.Add((string)observation["target"]);
                    StartCoroutine(Capture(viewer, animation, observation, render));
                    break;
                }
            }
            catch (Exception ex)
            {
                errors.Add(ex.ToString());
                Logger.LogError(ex);
                if (request != null) WriteResult(false, "failed");
                request = null;
            }
        }

        private Dictionary<string, object> Describe(SpineViewerNel viewer, SkeletonAnimation animation)
        {
            var skins = new List<object>();
            if (animation.Skeleton.Skin != null) skins.Add(animation.Skeleton.Skin.Name);
            if (animation.Skeleton.SkinList != null)
                foreach (var skin in animation.Skeleton.SkinList)
                    if (skin != null && !skins.Contains(skin.Name)) skins.Add(skin.Name);
            var tracks = animation.AnimationState.Tracks.Where(t => t != null && t.Animation != null)
                .Select(t => (object)t.Animation.Name).ToList();
            string activeId = null;
            var runtime = AccessTools.TypeByName("BetterExperience.Patches.ReplaceTexture.PortraitRuntime");
            var states = AccessTools.Field(runtime, "states").GetValue(null) as IDictionary;
            if (states != null && states.Contains(viewer.getSvTexture()))
            {
                object state = states[viewer.getSvTexture()];
                object bundle = Field(state, "Current");
                // The game reuses one SkeletonAnimation for multiple cached
                // viewers. Attribute a frame only to the data actually bound.
                if (bundle != null && ReferenceEquals(Field(bundle, "Data"), animation.SkeletonDataAsset))
                    activeId = Field(Field(bundle, "Package"), "Id") as string;
            }
            var material = animation.GetComponent<MeshRenderer>().sharedMaterial;
            return Map("target", viewer.getSvTexture().key, "skins", skins, "animations", tracks,
                "scope", isolatedViewers.Values.Contains(viewer) ? "isolated-game-viewer" : "live-game-ui",
                "activePackageId", activeId, "dirtIndex", viewer.getSvTexture().dirt_index,
                "shader", material == null ? null : material.shader.name,
                "skeletonSha256", HashBytes(System.Text.Encoding.UTF8.GetBytes(animation.SkeletonDataAsset.skeletonJSON.text)));
        }

        private IEnumerator Capture(SpineViewerNel viewer, SkeletonAnimation animation, Dictionary<string, object> observation, bool render)
        {
            // Iterator exceptions are caught by a wrapper, since Unity otherwise only logs them.
            var operation = CaptureCore(viewer, animation, observation, render);
            while (true)
            {
                bool more;
                try { more = operation.MoveNext(); }
                catch (Exception ex)
                {
                    errors.Add(ex.ToString());
                    WriteResult(false, "capture-failed");
                    capturing = false;
                    request = null;
                    yield break;
                }
                if (!more) break;
                yield return operation.Current;
            }
            capturing = false;
        }

        private IEnumerator CaptureCore(SpineViewerNel viewer, SkeletonAnimation animation, Dictionary<string, object> observation, bool render, bool representative = false)
        {
            yield return new WaitForEndOfFrame();
            string prefix = observations.Count.ToString("D4");
            string screenshot = Path.Combine(OutputDirectory(), prefix + "-live.png");
            if (!isolatedViewers.Values.Contains(viewer))
            {
                SaveScreen(screenshot);
                observation["screenshot"] = screenshot;
            }
            observation["refreshIndex"] = refreshes;
            observation["textures"] = Resources.FindObjectsOfTypeAll<Texture>().Length;
            observation["materials"] = Resources.FindObjectsOfTypeAll<Material>().Length;
            observation["renderTextures"] = Resources.FindObjectsOfTypeAll<RenderTexture>().Length;
            observations.Add(observation);
            if (render)
            {
                // Render the same loaded candidate and game materials in an isolated camera.
                var clone = new GameObject("WardrobeQA-Isolated");
                var cameraObject = new GameObject("WardrobeQA-Camera");
                Object.DontDestroyOnLoad(clone);
                Object.DontDestroyOnLoad(cameraObject);
                var target = new RenderTexture(768, 768, 24);
                try
                {
                    clone.layer = 31;
                    clone.transform.position = new Vector3(100000, 100000, 0);
                    var renderer = SkeletonAnimation.AddToGameObject(clone, animation.SkeletonDataAsset, true);
                    renderer.enabled = false;
                    var meshRenderer = clone.GetComponent<MeshRenderer>();
                    meshRenderer.sharedMaterials = animation.GetComponent<MeshRenderer>().sharedMaterials;
                    var camera = cameraObject.AddComponent<Camera>();
                    camera.enabled = false;
                    camera.orthographic = true;
                    camera.cullingMask = 1 << 31;
                    camera.clearFlags = CameraClearFlags.SolidColor;
                    camera.backgroundColor = new Color(0.82f, 0.83f, 0.86f, 1);
                    camera.targetTexture = target;
                    camera.nearClipPlane = 0.01f;
                    camera.farClipPlane = 20;
                    var data = renderer.Skeleton.Data;
                    foreach (var skin in data.Skins)
                    {
                        var combined = new Skin("wardrobe-qa");
                        if (data.DefaultSkin != null) combined.AddSkin(data.DefaultSkin);
                        combined.AddSkin(skin);
                        renderer.Skeleton.SetSkin(combined, false);
                        int ai = 0;
                        foreach (var action in data.Animations)
                        {
                            if (representative && action.Name != "stand") { ai++; continue; }
                            for (int frame = 0; frame < (representative ? 1 : 9); frame++)
                            {
                                renderer.Skeleton.SetToSetupPose();
                                action.Apply(renderer.Skeleton, -1, action.Duration * frame / 8, false, null, 1, MixBlend.Replace, MixDirection.In);
                                renderer.Skeleton.UpdateWorldTransform();
                                renderer.LateUpdate();
                                var bounds = meshRenderer.bounds;
                                camera.transform.position = new Vector3(bounds.center.x, bounds.center.y, -10);
                                camera.orthographicSize = Math.Max(0.01f, Math.Max(bounds.extents.x, bounds.extents.y) * 1.1f);
                                camera.Render();
                                string filename = Path.Combine(OutputDirectory(), prefix + "-" + data.Skins.IndexOf(skin) + "-" + ai + "-" + frame + ".png");
                                SaveTexture(target, filename);
                                WriteJson(Path.ChangeExtension(filename, ".json"), Map("skin", skin.Name, "animation", action.Name,
                                    "time", action.Duration * frame / 8, "scope", "isolated-game-render-current-materials"));
                                yield return null;
                            }
                            ai++;
                        }
                    }
                }
                finally
                {
                    target.Release();
                    Object.Destroy(target);
                    Object.Destroy(clone);
                    Object.Destroy(cameraObject);
                }
            }
            WriteResult(false, "capturing; game effects and lifecycle still require review");
        }

        private void SaveScreen(string destination)
        {
            var previous = RenderTexture.active;
            Texture2D image = null;
            try
            {
                RenderTexture.active = null;
                image = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBA32, false);
                image.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
                image.Apply();
                File.WriteAllBytes(destination, image.EncodeToPNG());
                screenshots[destination] = HashFile(destination);
            }
            finally { RenderTexture.active = previous; if (image != null) Object.Destroy(image); }
        }

        private void SaveTexture(RenderTexture texture, string destination)
        {
            var previous = RenderTexture.active;
            var image = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
            try
            {
                RenderTexture.active = texture;
                image.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
                image.Apply();
                File.WriteAllBytes(destination, image.EncodeToPNG());
                screenshots[destination] = HashFile(destination);
            }
            finally { RenderTexture.active = previous; Object.Destroy(image); }
        }

        private string OutputDirectory() { return Path.Combine(root, currentId); }
        private static object Field(object instance, string name) { return AccessTools.Field(instance.GetType(), name).GetValue(instance); }
        private void WriteResult(bool complete, string status)
        {
            WriteJson(Path.Combine(OutputDirectory(), "result.json"), Map("requestId", currentId, "complete", complete,
                "status", status, "observations", observations, "screenshots", screenshots, "errors", errors,
                "refreshRequests", refreshes, "utc", DateTime.UtcNow.ToString("o")));
        }
        private static Dictionary<string, object> Map(params object[] values)
        {
            var result = new Dictionary<string, object>();
            for (int i = 0; i < values.Length; i += 2) result.Add((string)values[i], values[i + 1]);
            return result;
        }
        private static void WriteJson(string path, object value)
        {
            string temporary = path + ".tmp";
            File.WriteAllText(temporary, PortraitJson.Serialize(value), new System.Text.UTF8Encoding(false));
            if (File.Exists(path)) File.Delete(path);
            File.Move(temporary, path);
        }
        private static string HashBytes(byte[] bytes)
        {
            using (var hash = SHA256.Create()) return BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
        }
        private static string HashFile(string path) { return HashBytes(File.ReadAllBytes(path)); }
        private void OnDestroy()
        {
            ReleaseIsolatedViewers();
            if (harmony != null) harmony.UnpatchSelf();
        }
    }
}
