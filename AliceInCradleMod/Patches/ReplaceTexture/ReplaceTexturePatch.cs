using HarmonyLib;
using System;
using UnityEngine;

namespace BetterExperience.Patches.ReplaceTexture
{
    internal partial class Patchs
    {
        [HarmonyPatch]
        private class ReplaceTexturePatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(AssetBundle), "LoadAsset", new Type[] { typeof(string), typeof(Type) })]
            static void LoadAssetPostfix(ref UnityEngine.Object __result, string name, Type type)
            {
                if (__result == null)
                    return;

                TextureManager.Instance.TryReplace(name, type, ref __result);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(AssetBundle), "LoadFromFile", new Type[] { typeof(string) })]
            static void LoadFromFilePostfix(string path, ref AssetBundle __result)
            {
                if (__result == null)
                    return;
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(AssetBundleRequest), "asset", MethodType.Getter)]
            static void AssetGetterPostfix(ref UnityEngine.Object __result)
            {
                if (__result == null)
                    return;

                TextureManager.Instance.TryReplace(__result.name, __result.GetType(), ref __result);
            }
        }
    }
}
