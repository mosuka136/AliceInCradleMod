using BetterExperience.BConfigManager;
using BetterExperience.BLogSpace;
using BetterExperience.Patches.ReplaceTexture;
using HarmonyLib;
using nel;
using System;
using System.Reflection;
using UnityEngine;
using UnityModBase;
using UnityModBase.HClassAttribute;
using XX;
using Object = UnityEngine.Object;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        [HarmonyPatch]
        public static class ReplaceTexturePatch
        {
            private static bool initialized;

            [InitializeOnGameBoot]
            public static void Initialize()
            {
                if (initialized) return;
                FrameUpdateManager.OnFrameUpdate += Update;
                initialized = true;
                BLog.Debug(nameof(ReplaceTexturePatch) + " initialized.");
            }

            private static void Update()
            {
                if (ConfigManager.FlushTextureHotkey?.Value?.WasPressedThisFrame() != true) return;
                try { ReplacementRuntime.Refresh(); }
                catch (Exception ex) { BLog.Error("Unexpected error while refreshing resource replacements.", ex); }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(MTI), nameof(MTI.LoadContainerOneImage))]
            private static void LoadMti(MTIOneImage __result, string asset_key, string image_key)
            {
                try { ReplacementRuntime.RegisterMti(__result, asset_key, image_key); }
                catch (Exception ex) { BLog.Error("Unexpected error while registering an MTI image.", ex); }
            }
        }
    }

    [HarmonyPatch(typeof(Resources), nameof(Resources.Load), new[] { typeof(string), typeof(Type) })]
    internal static class ReplacementResourcesTypedPatch
    {
        [HarmonyPostfix]
        private static void Load(string __0, Type __1, ref Object __result)
        {
            __result = ReplacementRuntime.ReplaceResource(__0, __1, __result);
        }
    }

    [HarmonyPatch]
    internal static class ReplacementResourcesPatch
    {
        public static MethodBase TargetMethod()
        {
            foreach (var method in typeof(Resources).GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                if (method.Name != nameof(Resources.Load) || method.IsGenericMethod) continue;
                var parameters = method.GetParameters();
                if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string)) return method;
            }
            throw new MissingMethodException(typeof(Resources).FullName, nameof(Resources.Load) + "(string)");
        }

        [HarmonyPostfix]
        private static void Load(string __0, ref Object __result)
        {
            __result = ReplacementRuntime.ReplaceResource(__0, null, __result);
        }
    }

    [HarmonyPatch]
    internal static class ReplacementSpineResourcePatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(BetobetoManager.SvTexture), nameof(BetobetoManager.SvTexture.prepareAtlasAssets))]
        private static bool PrepareAtlas(BetobetoManager.SvTexture __instance,
            ref Spine.Unity.SpineAtlasAsset _SpAtlasAsset, ref Spine.Unity.SkeletonDataAsset _SpDataAsset,
            Material[] AMtr, string replace_json_key)
        {
            if (!ReplacementRuntime.Prepare(__instance, replace_json_key, AMtr, out var atlas, out var data)) return true;
            _SpAtlasAsset = atlas;
            _SpDataAsset = data;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BetobetoManager.SvTexture), nameof(BetobetoManager.SvTexture.cleanExecute))]
        private static bool Clean(BetobetoManager.SvTexture __instance, ref bool __result)
        {
            if (!ReplacementRuntime.HasActive(__instance)) return true;
            __result = ReplacementRuntime.Clean(__instance);
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BetobetoManager.SvTexture), nameof(BetobetoManager.SvTexture.releaseAtlasData))]
        private static void Release(BetobetoManager.SvTexture __instance) => ReplacementRuntime.Released(__instance);

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BetobetoManager.SvTexture), nameof(BetobetoManager.SvTexture.runBetobeto))]
        private static bool Dirt(BetobetoManager.SvTexture __instance, int cur_dirt, ref bool __result)
        {
            if (!ReplacementRuntime.HasActive(__instance) || ReplacementRuntime.DirtEnabled(__instance)) return true;
            __instance.prepareTexture();
            __instance.dirt_index = cur_dirt;
            __result = true;
            return false;
        }
    }

    [HarmonyPatch]
    internal static class ReplacementSpineViewerPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SpineViewerNel), "prepareAtlasAssets")]
        private static void Register(SpineViewerNel __instance) => ReplacementRuntime.Register(__instance);

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SpineViewerNel), nameof(SpineViewerNel.clearAnim))]
        private static void BeforeClear(SpineViewerNel __instance) => ReplacementRuntime.BeforeSwitch(__instance);

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SpineViewerNel), nameof(SpineViewerNel.clearAnim))]
        private static void AfterClear(SpineViewerNel __instance) => ReplacementRuntime.AfterSwitch(__instance);

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SpineViewer), nameof(SpineViewer.switchSkeletonJson))]
        private static void SwitchJson(SpineViewer __instance, string _jsonkey)
        {
            if (__instance is SpineViewerNel viewer) ReplacementRuntime.BeforeSwitch(viewer, _jsonkey);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SpineViewer), "FindBone")]
        private static void FindBone(SpineViewer __instance, ref string __0)
        {
            __0 = ReplacementRuntime.MapBone(__instance, __0);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SpineViewer), "existBone")]
        private static void ExistBone(SpineViewer __instance, ref string __0)
        {
            __0 = ReplacementRuntime.MapBone(__instance, __0);
        }
    }

    [HarmonyPatch]
    internal static class ReplacementDisplayPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIPictureBodySpine), "get_scale")]
        private static void Scale(UIPictureBodySpine __instance, ref float __result)
        {
            __result = ReplacementRuntime.ApplyDisplay(__instance, "scale", __result);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIPictureBodySpine), "get_shift_ux")]
        private static void OffsetX(UIPictureBodySpine __instance, ref float __result)
        {
            __result = ReplacementRuntime.ApplyDisplay(__instance, "shift_ux", __result);
            ReplacementRuntime.ApplyRightShift(__instance, ref __result);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIPictureBodySpine), "get_shift_uy")]
        private static void OffsetY(UIPictureBodySpine __instance, ref float __result)
        {
            __result = ReplacementRuntime.ApplyDisplay(__instance, "shift_uy", __result);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIPictureBodySpine), "get_base_swidth")]
        private static void Width(UIPictureBodySpine __instance, ref float __result)
        {
            __result = ReplacementRuntime.ApplyDisplay(__instance, "base_swidth", __result);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIPictureBodySpine), "get_base_sheight")]
        private static void Height(UIPictureBodySpine __instance, ref float __result)
        {
            __result = ReplacementRuntime.ApplyDisplay(__instance, "base_sheight", __result);
        }
    }
}
