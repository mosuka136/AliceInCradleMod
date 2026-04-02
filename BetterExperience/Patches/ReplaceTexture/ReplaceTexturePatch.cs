using BetterExperience.BConfigManager;
using BetterExperience.Patches.ReplaceTexture;
using HarmonyLib;
using System;
using UnityEngine;
using XX;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        // 暂时废弃 [HarmonyPatch]
        public class ReplaceTexturePatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(AssetBundle), "LoadAsset", new Type[] { typeof(string), typeof(Type) })]
            static void LoadAssetPostfix(ref UnityEngine.Object __result, string name, Type type)
            {
                if (!ConfigManager.EnableReplaceTexture.Value)
                    return;

                if (__result == null)
                    return;

                TextureManager.Instance.TryReplace(name, type, ref __result);
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(MTIOneImage), "Image", MethodType.Getter)]
            static void MTIOneImageGetterPrefix(MTIOneImage __instance)
            {
                if (!ConfigManager.EnableReplaceTexture.Value)
                    return;

                TryReplace(__instance);
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(MTIOneImage), "MI", MethodType.Getter)]
            static void MTIOneImageMIGetterPrefix(MTIOneImage __instance)
            {
                if (!ConfigManager.EnableReplaceTexture.Value)
                    return;

                TryReplace(__instance);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(AssetBundleRequest), "asset", MethodType.Getter)]
            static void AssetGetterPostfix(ref UnityEngine.Object __result)
            {
                if (!ConfigManager.EnableReplaceTexture.Value)
                    return;

                if (__result == null)
                    return;

                TextureManager.Instance.TryReplace(__result.name, __result.GetType(), ref __result);
            }

            public static void TryReplace(MTIOneImage __instance)
            {
                if (__instance == null)
                    return;

                var replaceTexture = TextureManager.Instance.GetReplaceTexture(__instance.image_key);
                if (replaceTexture == null)
                    return;

                var limage = Traverse.Create(__instance).Field<MImage>("LImage_").Value;
                if (limage == null)
                    return;

                if (replaceTexture != limage.Tx)
                {
                    TextureManager.Instance.CopyTextureProperties(limage.Tx, replaceTexture);
                    limage.Tx = replaceTexture;
                }
                return;
            }
        }
    }
}
